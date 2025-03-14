using ExpenseService.Application.DTOs;
using ExpenseService.Application.Interfaces;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ExpenseService.Application.Services;

public class ExportService : IExportService
{
    public byte[] ExportToCsv<T>(IEnumerable<T> data)
    {
        if (data == null || !data.Any())
        {
            return Encoding.UTF8.GetBytes(string.Empty);
        }

        var stringBuilder = new StringBuilder();
        var properties = typeof(T).GetProperties();

        // Add headers
        stringBuilder.AppendLine(string.Join(",", properties.Select(p => $"\"{GetDisplayName(p)}\"")));

        // Add rows
        foreach (var item in data)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                return value == null ? "\"\"" : $"\"{FormatValue(value)}\"";
            });
            stringBuilder.AppendLine(string.Join(",", values));
        }

        return Encoding.UTF8.GetBytes(stringBuilder.ToString());
    }

    public byte[] ExportToPdf<T>(IEnumerable<T> data, string title, string description)
    {
        // This would normally use a PDF library like iTextSharp, PDFsharp, or DocX
        // For simplicity, we'll just create a formatted text representation
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"PDF REPORT: {title}");
        stringBuilder.AppendLine(description);
        stringBuilder.AppendLine(new string('-', 80));
        stringBuilder.AppendLine();

        if (data != null && data.Any())
        {
            var properties = typeof(T).GetProperties();

            // Add headers
            stringBuilder.AppendLine(string.Join(" | ", properties.Select(p => GetDisplayName(p).PadRight(15))));
            stringBuilder.AppendLine(new string('-', properties.Length * 18));

            // Add rows
            foreach (var item in data)
            {
                var values = properties.Select(p =>
                {
                    var value = p.GetValue(item);
                    return value == null ? "".PadRight(15) : FormatValue(value).PadRight(15);
                });
                stringBuilder.AppendLine(string.Join(" | ", values));
            }
        }
        else
        {
            stringBuilder.AppendLine("No data available for this report.");
        }

        return Encoding.UTF8.GetBytes(stringBuilder.ToString());
    }

    public byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName)
    {
        // This would normally use a library like EPPlus, ClosedXML, or NPOI
        // For simplicity, we'll just create a CSV with Excel metadata
        var csv = ExportToCsv(data);

        // In a real implementation, this would create a proper Excel file
        return csv;
    }

    #region Helper Methods

    private string GetDisplayName(PropertyInfo property)
    {
        var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
        if (displayAttribute != null && !string.IsNullOrEmpty(displayAttribute.Name))
        {
            return displayAttribute.Name;
        }

        var descriptionAttribute = property.GetCustomAttribute<DescriptionAttribute>();
        if (descriptionAttribute != null && !string.IsNullOrEmpty(descriptionAttribute.Description))
        {
            return descriptionAttribute.Description;
        }

        return property.Name;
    }

    private string FormatValue(object value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (value is DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }

        if (value is decimal decimalValue)
        {
            return decimalValue.ToString("0.00", CultureInfo.InvariantCulture);
        }

        return value.ToString().Replace("\"", "\"\""); // Escape quotes for CSV
    }

    #endregion
}