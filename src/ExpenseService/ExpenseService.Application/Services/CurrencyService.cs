using ExpenseService.Application.DTOs;
using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseService.Application.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IExchangeRateRepository _exchangeRateRepository;
        private readonly IUserCurrencyPreferenceRepository _userPreferenceRepository;
        private readonly IGroupCurrencyPreferenceRepository _groupPreferenceRepository;

        public CurrencyService(
            ICurrencyRepository currencyRepository,
            IExchangeRateRepository exchangeRateRepository,
            IUserCurrencyPreferenceRepository userPreferenceRepository,
            IGroupCurrencyPreferenceRepository groupPreferenceRepository)
        {
            _currencyRepository = currencyRepository;
            _exchangeRateRepository = exchangeRateRepository;
            _userPreferenceRepository = userPreferenceRepository;
            _groupPreferenceRepository = groupPreferenceRepository;
        }

        #region Currency Methods

        public async Task<CurrencyDto> GetByIdAsync(int id)
        {
            var currency = await _currencyRepository.GetByIdAsync(id);
            if (currency == null)
            {
                throw new KeyNotFoundException($"Currency with ID {id} not found");
            }
            return MapToDto(currency);
        }

        public async Task<CurrencyDto> GetByCodeAsync(string code)
        {
            var currency = await _currencyRepository.GetByCodeAsync(code);
            if (currency == null)
            {
                throw new KeyNotFoundException($"Currency with code {code} not found");
            }
            return MapToDto(currency);
        }

        public async Task<IEnumerable<CurrencyDto>> GetAllAsync()
        {
            var currencies = await _currencyRepository.GetAllAsync();
            return currencies.Select(MapToDto);
        }

        public async Task<IEnumerable<CurrencyDto>> GetActiveAsync()
        {
            var currencies = await _currencyRepository.GetActiveAsync();
            return currencies.Select(MapToDto);
        }

        public async Task<CurrencyDto> GetDefaultAsync()
        {
            var defaultCurrency = await _currencyRepository.GetDefaultAsync();
            if (defaultCurrency == null)
            {
                throw new InvalidOperationException("No default currency is set");
            }
            return MapToDto(defaultCurrency);
        }

        public async Task<CurrencyDto> CreateAsync(CreateCurrencyRequest request)
        {
            var existingCurrency = await _currencyRepository.GetByCodeAsync(request.Code);
            if (existingCurrency != null)
            {
                throw new InvalidOperationException($"Currency with code {request.Code} already exists");
            }

            var currency = new Currency(request.Code, request.Name, request.Symbol);

            if (!request.IsActive)
            {
                currency.Deactivate();
            }

            await _currencyRepository.AddAsync(currency);
            await _currencyRepository.SaveChangesAsync();

            if (request.IsDefault)
            {
                await SetAsDefault(currency);
            }

            return MapToDto(currency);
        }

        public async Task<CurrencyDto> UpdateAsync(int id, UpdateCurrencyRequest request)
        {
            var currency = await _currencyRepository.GetByIdAsync(id);
            if (currency == null)
            {
                throw new KeyNotFoundException($"Currency with ID {id} not found");
            }

            currency.Update(request.Name, request.Symbol);

            if (request.IsActive)
            {
                currency.Activate();
            }
            else
            {
                currency.Deactivate();
            }

            _currencyRepository.Update(currency);
            await _currencyRepository.SaveChangesAsync();

            if (request.IsDefault)
            {
                await SetAsDefault(currency);
            }

            return MapToDto(currency);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var currency = await _currencyRepository.GetByIdAsync(id);
            if (currency == null)
            {
                return false;
            }

            if (currency.IsDefault)
            {
                throw new InvalidOperationException("Cannot delete the default currency");
            }

            // Check if the currency is used in any exchange rates
            var exchangeRatesFrom = await _exchangeRateRepository.GetByCurrencyPairAsync(currency.Code, "USD");
            var exchangeRatesTo = await _exchangeRateRepository.GetByCurrencyPairAsync("USD", currency.Code);

            if (exchangeRatesFrom.Any() || exchangeRatesTo.Any())
            {
                throw new InvalidOperationException("Cannot delete a currency that is used in exchange rates");
            }

            _currencyRepository.Delete(currency);
            await _currencyRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SetDefaultAsync(string currencyCode)
        {
            var currency = await _currencyRepository.GetByCodeAsync(currencyCode);
            if (currency == null)
            {
                return false;
            }

            await SetAsDefault(currency);
            return true;
        }

        private async Task SetAsDefault(Currency currency)
        {
            var currentDefault = await _currencyRepository.GetDefaultAsync();
            if (currentDefault != null && currentDefault.Id != currency.Id)
            {
                currentDefault.UnsetAsDefault();
                _currencyRepository.Update(currentDefault);
            }

            currency.SetAsDefault();
            _currencyRepository.Update(currency);
            await _currencyRepository.SaveChangesAsync();
        }

        private static CurrencyDto MapToDto(Currency currency)
        {
            return new CurrencyDto
            {
                Id = currency.Id,
                Code = currency.Code,
                Name = currency.Name,
                Symbol = currency.Symbol,
                IsActive = currency.IsActive,
                IsDefault = currency.IsDefault
            };
        }

        #endregion

        #region Exchange Rate Methods

        public async Task<ExchangeRateDto> GetExchangeRateByIdAsync(int id)
        {
            var exchangeRate = await _exchangeRateRepository.GetByIdAsync(id);
            if (exchangeRate == null)
            {
                throw new KeyNotFoundException($"Exchange rate with ID {id} not found");
            }
            return MapToDto(exchangeRate);
        }

        public async Task<ExchangeRateDto> GetLatestExchangeRateAsync(string fromCurrencyCode, string toCurrencyCode)
        {
            var exchangeRate = await _exchangeRateRepository.GetLatestRateAsync(fromCurrencyCode, toCurrencyCode);
            if (exchangeRate == null)
            {
                throw new KeyNotFoundException($"Exchange rate from {fromCurrencyCode} to {toCurrencyCode} not found");
            }
            return MapToDto(exchangeRate);
        }

        public async Task<IEnumerable<ExchangeRateDto>> GetAllExchangeRatesAsync()
        {
            var exchangeRates = await _exchangeRateRepository.GetAllAsync();
            return exchangeRates.Select(MapToDto);
        }

        public async Task<IEnumerable<ExchangeRateDto>> GetExchangeRatesByCurrencyPairAsync(string fromCurrencyCode, string toCurrencyCode)
        {
            var exchangeRates = await _exchangeRateRepository.GetByCurrencyPairAsync(fromCurrencyCode, toCurrencyCode);
            return exchangeRates.Select(MapToDto);
        }

        public async Task<ExchangeRateDto> CreateExchangeRateAsync(CreateExchangeRateRequest request)
        {
            // Validate currencies exist
            var fromCurrency = await _currencyRepository.GetByCodeAsync(request.FromCurrencyCode);
            if (fromCurrency == null)
            {
                throw new KeyNotFoundException($"Currency with code {request.FromCurrencyCode} not found");
            }

            var toCurrency = await _currencyRepository.GetByCodeAsync(request.ToCurrencyCode);
            if (toCurrency == null)
            {
                throw new KeyNotFoundException($"Currency with code {request.ToCurrencyCode} not found");
            }

            if (fromCurrency.Code == toCurrency.Code)
            {
                throw new InvalidOperationException("Cannot create exchange rate for the same currency");
            }

            // Check if there's already a rate for the same date
            var existingRates = await _exchangeRateRepository.GetByCurrencyPairAsync(request.FromCurrencyCode, request.ToCurrencyCode);
            var existingRate = existingRates.FirstOrDefault(r => r.EffectiveDate.Date == request.EffectiveDate.Date);

            if (existingRate != null)
            {
                throw new InvalidOperationException($"Exchange rate for {request.FromCurrencyCode} to {request.ToCurrencyCode} on {request.EffectiveDate.Date.ToShortDateString()} already exists");
            }

            var exchangeRate = new ExchangeRate(
                request.FromCurrencyCode,
                request.ToCurrencyCode,
                request.Rate,
                request.EffectiveDate,
                request.Source
            );

            await _exchangeRateRepository.AddAsync(exchangeRate);
            await _exchangeRateRepository.SaveChangesAsync();

            return MapToDto(exchangeRate);
        }

        public async Task<ExchangeRateDto> UpdateExchangeRateAsync(int id, UpdateExchangeRateRequest request)
        {
            var exchangeRate = await _exchangeRateRepository.GetByIdAsync(id);
            if (exchangeRate == null)
            {
                throw new KeyNotFoundException($"Exchange rate with ID {id} not found");
            }

            exchangeRate.UpdateRate(request.Rate, request.EffectiveDate, request.Source);

            _exchangeRateRepository.Update(exchangeRate);
            await _exchangeRateRepository.SaveChangesAsync();

            return MapToDto(exchangeRate);
        }

        public async Task<bool> DeleteExchangeRateAsync(int id)
        {
            var exchangeRate = await _exchangeRateRepository.GetByIdAsync(id);
            if (exchangeRate == null)
            {
                return false;
            }

            _exchangeRateRepository.Delete(exchangeRate);
            await _exchangeRateRepository.SaveChangesAsync();

            return true;
        }

        private static ExchangeRateDto MapToDto(ExchangeRate exchangeRate)
        {
            return new ExchangeRateDto
            {
                Id = exchangeRate.Id,
                FromCurrencyCode = exchangeRate.FromCurrencyCode,
                ToCurrencyCode = exchangeRate.ToCurrencyCode,
                Rate = exchangeRate.Rate,
                EffectiveDate = exchangeRate.EffectiveDate,
                Source = exchangeRate.Source
            };
        }

        #endregion

        #region User Preference Methods

        public async Task<UserCurrencyPreferenceDto> GetUserPreferenceAsync(int userId)
        {
            var preference = await _userPreferenceRepository.GetByUserIdAsync(userId);
            if (preference == null)
            {
                // Return default preference
                var defaultCurrency = await _currencyRepository.GetDefaultAsync();
                return new UserCurrencyPreferenceDto
                {
                    UserId = userId,
                    DefaultCurrencyCode = defaultCurrency?.Code ?? "USD",
                    AutoConvert = false
                };
            }

            return new UserCurrencyPreferenceDto
            {
                Id = preference.Id,
                UserId = preference.UserId,
                DefaultCurrencyCode = preference.DefaultCurrencyCode,
                AutoConvert = preference.AutoConvert
            };
        }

        public async Task<UserCurrencyPreferenceDto> SetUserPreferenceAsync(SetUserCurrencyPreferenceRequest request)
        {
            // Validate currency exists
            var currency = await _currencyRepository.GetByCodeAsync(request.DefaultCurrencyCode);
            if (currency == null)
            {
                throw new KeyNotFoundException($"Currency with code {request.DefaultCurrencyCode} not found");
            }

            var preference = await _userPreferenceRepository.GetByUserIdAsync(request.UserId);

            if (preference == null)
            {
                // Create new preference
                preference = new UserCurrencyPreference(request.UserId, request.DefaultCurrencyCode);
                preference.SetAutoConvert(request.AutoConvert);
                await _userPreferenceRepository.AddAsync(preference);
            }
            else
            {
                // Update existing preference
                preference.UpdateDefaultCurrency(request.DefaultCurrencyCode);
                preference.SetAutoConvert(request.AutoConvert);
                _userPreferenceRepository.Update(preference);
            }

            await _userPreferenceRepository.SaveChangesAsync();

            return new UserCurrencyPreferenceDto
            {
                Id = preference.Id,
                UserId = preference.UserId,
                DefaultCurrencyCode = preference.DefaultCurrencyCode,
                AutoConvert = preference.AutoConvert
            };
        }

        #endregion

        #region Group Preference Methods

        public async Task<GroupCurrencyPreferenceDto> GetGroupPreferenceAsync(int groupId)
        {
            var preference = await _groupPreferenceRepository.GetByGroupIdAsync(groupId);
            if (preference == null)
            {
                // Return default preference
                var defaultCurrency = await _currencyRepository.GetDefaultAsync();
                return new GroupCurrencyPreferenceDto
                {
                    GroupId = groupId,
                    DefaultCurrencyCode = defaultCurrency?.Code ?? "USD",
                    AutoConvertForMembers = false
                };
            }

            return new GroupCurrencyPreferenceDto
            {
                Id = preference.Id,
                GroupId = preference.GroupId,
                DefaultCurrencyCode = preference.DefaultCurrencyCode,
                AutoConvertForMembers = preference.AutoConvertForMembers
            };
        }

        public async Task<GroupCurrencyPreferenceDto> SetGroupPreferenceAsync(SetGroupCurrencyPreferenceRequest request)
        {
            // Validate currency exists
            var currency = await _currencyRepository.GetByCodeAsync(request.DefaultCurrencyCode);
            if (currency == null)
            {
                throw new KeyNotFoundException($"Currency with code {request.DefaultCurrencyCode} not found");
            }

            var preference = await _groupPreferenceRepository.GetByGroupIdAsync(request.GroupId);

            if (preference == null)
            {
                // Create new preference
                preference = new GroupCurrencyPreference(
                    request.GroupId,
                    request.DefaultCurrencyCode,
                    request.AutoConvertForMembers
                );
                await _groupPreferenceRepository.AddAsync(preference);
            }
            else
            {
                // Update existing preference
                preference.UpdateDefaultCurrency(request.DefaultCurrencyCode);
                preference.SetAutoConvertForMembers(request.AutoConvertForMembers);
                _groupPreferenceRepository.Update(preference);
            }

            await _groupPreferenceRepository.SaveChangesAsync();

            return new GroupCurrencyPreferenceDto
            {
                Id = preference.Id,
                GroupId = preference.GroupId,
                DefaultCurrencyCode = preference.DefaultCurrencyCode,
                AutoConvertForMembers = preference.AutoConvertForMembers
            };
        }

        #endregion
    }
}