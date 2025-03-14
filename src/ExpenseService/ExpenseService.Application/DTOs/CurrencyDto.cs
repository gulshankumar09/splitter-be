using System;
using System.Collections.Generic;

namespace ExpenseService.Application.DTOs;

public class CurrencyDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
}

public class ExchangeRateDto
{
    public int Id { get; set; }
    public string FromCurrencyCode { get; set; } = string.Empty;
    public string ToCurrencyCode { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? Source { get; set; }
}

public class UserCurrencyPreferenceDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string DefaultCurrencyCode { get; set; } = string.Empty;
    public bool AutoConvert { get; set; }
}

public class GroupCurrencyPreferenceDto
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string DefaultCurrencyCode { get; set; } = string.Empty;
    public bool AutoConvertForMembers { get; set; }
}

public class CurrencyConversionDto
{
    public string FromCurrencyCode { get; set; } = string.Empty;
    public string ToCurrencyCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal ConvertedAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public DateTime ConversionDate { get; set; }
}

public class CreateCurrencyRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
}

public class UpdateCurrencyRequest
{
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
}

public class CreateExchangeRateRequest
{
    public string FromCurrencyCode { get; set; } = string.Empty;
    public string ToCurrencyCode { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    public string? Source { get; set; }
}

public class UpdateExchangeRateRequest
{
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    public string? Source { get; set; }
}

public class SetUserCurrencyPreferenceRequest
{
    public int UserId { get; set; }
    public string DefaultCurrencyCode { get; set; } = string.Empty;
    public bool AutoConvert { get; set; } = true;
}

public class SetGroupCurrencyPreferenceRequest
{
    public int GroupId { get; set; }
    public string DefaultCurrencyCode { get; set; } = string.Empty;
    public bool AutoConvertForMembers { get; set; } = true;
}

public class ConvertCurrencyRequest
{
    public string FromCurrencyCode { get; set; } = string.Empty;
    public string ToCurrencyCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}