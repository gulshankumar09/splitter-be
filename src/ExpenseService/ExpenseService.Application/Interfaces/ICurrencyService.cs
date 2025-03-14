using ExpenseService.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseService.Application.Interfaces;

public interface ICurrencyService
{
    Task<CurrencyDto> GetByIdAsync(int id);
    Task<CurrencyDto> GetByCodeAsync(string code);
    Task<IEnumerable<CurrencyDto>> GetAllAsync();
    Task<IEnumerable<CurrencyDto>> GetActiveAsync();
    Task<CurrencyDto> GetDefaultAsync();
    Task<CurrencyDto> CreateAsync(CreateCurrencyRequest request);
    Task<CurrencyDto> UpdateAsync(int id, UpdateCurrencyRequest request);
    Task<bool> DeleteAsync(int id);
    Task<bool> SetDefaultAsync(string currencyCode);

    // Exchange rate methods
    Task<ExchangeRateDto> GetExchangeRateByIdAsync(int id);
    Task<ExchangeRateDto> GetLatestExchangeRateAsync(string fromCurrencyCode, string toCurrencyCode);
    Task<IEnumerable<ExchangeRateDto>> GetAllExchangeRatesAsync();
    Task<IEnumerable<ExchangeRateDto>> GetExchangeRatesByCurrencyPairAsync(string fromCurrencyCode, string toCurrencyCode);
    Task<ExchangeRateDto> CreateExchangeRateAsync(CreateExchangeRateRequest request);
    Task<ExchangeRateDto> UpdateExchangeRateAsync(int id, UpdateExchangeRateRequest request);
    Task<bool> DeleteExchangeRateAsync(int id);

    // User preferences methods
    Task<UserCurrencyPreferenceDto> GetUserPreferenceAsync(int userId);
    Task<UserCurrencyPreferenceDto> SetUserPreferenceAsync(SetUserCurrencyPreferenceRequest request);

    // Group preferences methods
    Task<GroupCurrencyPreferenceDto> GetGroupPreferenceAsync(int groupId);
    Task<GroupCurrencyPreferenceDto> SetGroupPreferenceAsync(SetGroupCurrencyPreferenceRequest request);
}