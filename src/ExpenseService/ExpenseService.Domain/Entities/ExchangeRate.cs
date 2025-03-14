using SharedLibrary.Domain;
using System;

namespace ExpenseService.Domain.Entities;

public class ExchangeRate : BaseEntity
{
    public string FromCurrencyCode { get; private set; }
    public string ToCurrencyCode { get; private set; }
    public decimal Rate { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    public string? Source { get; private set; }

    // For EF Core
    private ExchangeRate() { }

    public ExchangeRate(string fromCurrencyCode, string toCurrencyCode, decimal rate, DateTime effectiveDate, string? source = null)
    {
        FromCurrencyCode = fromCurrencyCode;
        ToCurrencyCode = toCurrencyCode;
        Rate = rate;
        EffectiveDate = effectiveDate;
        Source = source;
    }

    public void UpdateRate(decimal rate, DateTime effectiveDate, string? source = null)
    {
        Rate = rate;
        EffectiveDate = effectiveDate;
        Source = source;
    }

    public decimal ConvertAmount(decimal amount)
    {
        return amount * Rate;
    }
}