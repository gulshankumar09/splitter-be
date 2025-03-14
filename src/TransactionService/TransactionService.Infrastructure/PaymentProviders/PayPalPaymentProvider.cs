using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TransactionService.Application.DTOs;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.PaymentProviders;

public class PayPalPaymentProvider : IPaymentProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PayPalPaymentProvider> _logger;
    private readonly PayPalOptions _options;
    private string _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public string Id => "paypal";
    public string Name => "PayPal";
    public string LogoUrl => "https://www.paypalobjects.com/webstatic/mktg/logo/pp_cc_mark_37x23.jpg";
    public IEnumerable<string> SupportedPaymentMethods => new[] { "credit_card", "paypal_account", "bank_account" };
    public bool IsEnabled => true;
    public decimal TransactionFeePercentage => 2.9m;
    public decimal FixedTransactionFee => 0.30m;

    public PayPalPaymentProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<PayPalOptions> options,
        ILogger<PayPalPaymentProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> CreatePaymentMethodTokenAsync(CreatePaymentMethodRequest request)
    {
        try
        {
            // Ensure access token is valid
            await EnsureValidAccessTokenAsync();

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Create the payment source request based on method type
            var requestObject = new
            {
                payment_source = request.MethodType switch
                {
                    "credit_card" => new
                    {
                        card = new
                        {
                            number = request.AccountNumber,
                            expiry = request.ExpiryDate?.ToString("yyyy-MM"),
                            name = "Card Holder",
                            billing_address = new
                            {
                                address_line_1 = "123 Main St",
                                admin_area_2 = "City",
                                admin_area_1 = "State",
                                postal_code = "12345",
                                country_code = "US"
                            }
                        }
                    },
                    "bank_account" => new
                    {
                        bank_account = new
                        {
                            account_number = request.AccountNumber,
                            account_type = "CHECKING",
                            routing_number = "123456789",
                            country_code = "US"
                        }
                    },
                    _ => throw new NotSupportedException($"Payment method type {request.MethodType} not supported by PayPal")
                },
                customer = new
                {
                    id = request.UserId.ToString()
                }
            };

            var response = await client.PostAsJsonAsync(
                $"{_options.ApiUrl}/v1/vault/payment-tokens",
                requestObject
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error creating PayPal payment token: {ErrorContent}", errorContent);
                throw new Exception($"Error creating PayPal payment token: {response.StatusCode}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<PayPalTokenResponse>();
            return tokenResponse?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment method token with PayPal");
            throw;
        }
    }

    public async Task<PaymentResultDto> ProcessPaymentAsync(ProcessPaymentRequest request)
    {
        try
        {
            // Ensure access token is valid
            await EnsureValidAccessTokenAsync();

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Create the payment request
            var paymentRequest = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        amount = new
                        {
                            currency_code = request.CurrencyCode ?? "USD",
                            value = request.Amount.ToString("0.00")
                        },
                        description = request.Description,
                        reference_id = request.ExternalReferenceId ?? request.TransactionId.ToString()
                    }
                },
                payment_source = new
                {
                    token = new
                    {
                        id = request.PaymentMethodId?.ToString() ?? "",
                        type = "PAYMENT_METHOD_TOKEN"
                    }
                }
            };

            var response = await client.PostAsJsonAsync(
                $"{_options.ApiUrl}/v2/checkout/orders",
                paymentRequest
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error processing PayPal payment: {ErrorContent}", errorContent);
                throw new Exception($"Error processing PayPal payment: {response.StatusCode}");
            }

            var paymentResponse = await response.Content.ReadFromJsonAsync<PayPalPaymentResponse>();

            // Capture the payment (in a real scenario, this might be a separate step)
            var captureResponse = await client.PostAsync(
                $"{_options.ApiUrl}/v2/checkout/orders/{paymentResponse.Id}/capture",
                new StringContent("{}", Encoding.UTF8, "application/json")
            );

            if (!captureResponse.IsSuccessStatusCode)
            {
                var errorContent = await captureResponse.Content.ReadAsStringAsync();
                _logger.LogError("Error capturing PayPal payment: {ErrorContent}", errorContent);
                throw new Exception($"Error capturing PayPal payment: {captureResponse.StatusCode}");
            }

            var captureResult = await captureResponse.Content.ReadFromJsonAsync<PayPalCaptureResponse>();

            // Map the response to our DTO
            return new PaymentResultDto
            {
                TransactionId = request.TransactionId,
                PaymentProviderId = Id,
                ProviderTransactionId = captureResult.Id,
                Status = TransactionStatus.Completed,
                Amount = request.Amount,
                Fee = CalculateFee(request.Amount),
                ProcessedAt = DateTime.UtcNow,
                ReceiptUrl = captureResult.Links.FirstOrDefault(l => l.Rel == "receipt")?.Href,
                Metadata = new Dictionary<string, string>
                {
                    { "paypal_order_id", captureResult.Id },
                    { "payment_status", captureResult.Status }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment with PayPal");
            throw;
        }
    }

    public async Task<PaymentResultDto> GetPaymentStatusAsync(string providerTransactionId)
    {
        try
        {
            // Ensure access token is valid
            await EnsureValidAccessTokenAsync();

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.GetAsync($"{_options.ApiUrl}/v2/checkout/orders/{providerTransactionId}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error getting PayPal payment status: {ErrorContent}", errorContent);
                throw new Exception($"Error getting PayPal payment status: {response.StatusCode}");
            }

            var orderDetails = await response.Content.ReadFromJsonAsync<PayPalOrderDetailsResponse>();

            var status = orderDetails.Status switch
            {
                "COMPLETED" => TransactionStatus.Completed,
                "SAVED" => TransactionStatus.Pending,
                "APPROVED" => TransactionStatus.Pending,
                "VOIDED" => TransactionStatus.Cancelled,
                _ => TransactionStatus.Failed
            };

            var amount = decimal.Parse(orderDetails.PurchaseUnits[0].Amount.Value);

            return new PaymentResultDto
            {
                TransactionId = Guid.Empty, // This would be filled in by the service layer
                PaymentProviderId = Id,
                ProviderTransactionId = providerTransactionId,
                Status = status,
                Amount = amount,
                Fee = CalculateFee(amount),
                ProcessedAt = orderDetails.UpdateTime,
                ReceiptUrl = null, // Would need to generate this
                Metadata = new Dictionary<string, string>
                {
                    { "paypal_order_id", providerTransactionId },
                    { "payment_status", orderDetails.Status }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment status from PayPal");
            throw;
        }
    }

    public async Task<bool> CancelPaymentAsync(string providerTransactionId)
    {
        try
        {
            // Ensure access token is valid
            await EnsureValidAccessTokenAsync();

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Check if the payment is in a cancellable state
            var statusResponse = await client.GetAsync($"{_options.ApiUrl}/v2/checkout/orders/{providerTransactionId}");

            if (!statusResponse.IsSuccessStatusCode)
            {
                return false;
            }

            var orderDetails = await statusResponse.Content.ReadFromJsonAsync<PayPalOrderDetailsResponse>();

            // Only certain statuses can be cancelled
            if (orderDetails.Status != "CREATED" && orderDetails.Status != "SAVED" && orderDetails.Status != "APPROVED")
            {
                return false;
            }

            // Cancel the order
            var response = await client.PostAsync(
                $"{_options.ApiUrl}/v2/checkout/orders/{providerTransactionId}/cancel",
                new StringContent("{}", Encoding.UTF8, "application/json")
            );

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling payment with PayPal");
            return false;
        }
    }

    public async Task<string> GenerateReceiptUrlAsync(string providerTransactionId)
    {
        // For PayPal, we would typically redirect to the PayPal receipt page
        // This is a simplified version that would just construct the URL
        return $"https://www.paypal.com/activity/payment/{providerTransactionId}";
    }

    #region Helper Methods

    private async Task EnsureValidAccessTokenAsync()
    {
        if (string.IsNullOrEmpty(_accessToken) || DateTime.UtcNow >= _tokenExpiry)
        {
            await RefreshAccessTokenAsync();
        }
    }

    private async Task RefreshAccessTokenAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            // Set Basic authentication header with client ID and secret
            var authString = $"{_options.ClientId}:{_options.ClientSecret}";
            var encodedAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes(authString));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedAuth);

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var response = await client.PostAsync($"{_options.ApiUrl}/v1/oauth2/token", content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to get access token: {errorContent}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<PayPalTokenAuthResponse>();
            _accessToken = tokenResponse.AccessToken;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // Expire 1 minute early to be safe
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing PayPal access token");
            throw;
        }
    }

    private decimal CalculateFee(decimal amount)
    {
        return (amount * (TransactionFeePercentage / 100)) + FixedTransactionFee;
    }

    #endregion
}

#region PayPal Response Classes

public class PayPalTokenResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public string Id { get; set; }
}

public class PayPalTokenAuthResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

public class PayPalPaymentResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public string Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("status")]
    public string Status { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("links")]
    public List<PayPalLink> Links { get; set; }
}

public class PayPalCaptureResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public string Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("status")]
    public string Status { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("links")]
    public List<PayPalLink> Links { get; set; }
}

public class PayPalLink
{
    [System.Text.Json.Serialization.JsonPropertyName("href")]
    public string Href { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("rel")]
    public string Rel { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("method")]
    public string Method { get; set; }
}

public class PayPalOrderDetailsResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public string Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("status")]
    public string Status { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("update_time")]
    public DateTime UpdateTime { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("purchase_units")]
    public List<PayPalPurchaseUnit> PurchaseUnits { get; set; }
}

public class PayPalPurchaseUnit
{
    [System.Text.Json.Serialization.JsonPropertyName("reference_id")]
    public string ReferenceId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("amount")]
    public PayPalAmount Amount { get; set; }
}

public class PayPalAmount
{
    [System.Text.Json.Serialization.JsonPropertyName("currency_code")]
    public string CurrencyCode { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("value")]
    public string Value { get; set; }
}

#endregion

public class PayPalOptions
{
    public string ApiUrl { get; set; } = "https://api-m.sandbox.paypal.com";
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}