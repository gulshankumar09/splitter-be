using SharedLibrary.Domain;

namespace ExpenseService.Domain.Entities;

public class GroupCurrencyPreference : BaseEntity
{
    public int GroupId { get; private set; }
    public string DefaultCurrencyCode { get; private set; }
    public bool AutoConvertForMembers { get; private set; }

    // For EF Core
    private GroupCurrencyPreference() { }

    public GroupCurrencyPreference(int groupId, string defaultCurrencyCode, bool autoConvertForMembers = true)
    {
        GroupId = groupId;
        DefaultCurrencyCode = defaultCurrencyCode;
        AutoConvertForMembers = autoConvertForMembers;
    }

    public void UpdateDefaultCurrency(string currencyCode)
    {
        DefaultCurrencyCode = currencyCode;
    }

    public void SetAutoConvertForMembers(bool autoConvert)
    {
        AutoConvertForMembers = autoConvert;
    }
}