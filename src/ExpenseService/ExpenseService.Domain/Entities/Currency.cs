using SharedLibrary.Domain;

namespace ExpenseService.Domain.Entities;

public class Currency : BaseEntity
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string Symbol { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDefault { get; private set; }

    // For EF Core
    private Currency() { }

    public Currency(string code, string name, string symbol)
    {
        Code = code;
        Name = name;
        Symbol = symbol;
        IsActive = true;
        IsDefault = false;
    }

    public void Update(string name, string symbol)
    {
        Name = name;
        Symbol = symbol;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void SetAsDefault() => IsDefault = true;

    public void UnsetAsDefault() => IsDefault = false;
}