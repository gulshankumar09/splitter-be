using SharedLibrary.Domain;

namespace ExpenseService.Domain.Entities;

public class UserCurrencyPreference : BaseEntity
{
    public int UserId { get; private set; }
    public string DefaultCurrencyCode { get; private set; }
    public bool AutoConvert { get; private set; }

    // For EF Core
    private UserCurrencyPreference() { }

    public UserCurrencyPreference(int userId, string defaultCurrencyCode)
    {
        UserId = userId;
        DefaultCurrencyCode = defaultCurrencyCode;
        AutoConvert = true;
    }

    public void UpdateDefaultCurrency(string currencyCode)
    {
        DefaultCurrencyCode = currencyCode;
    }

    public void SetAutoConvert(bool autoConvert)
    {
        AutoConvert = autoConvert;
    }
}