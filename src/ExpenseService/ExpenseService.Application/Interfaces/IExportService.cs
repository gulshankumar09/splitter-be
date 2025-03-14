using ExpenseService.Application.DTOs;

namespace ExpenseService.Application.Interfaces;

public interface IExportService
{
    byte[] ExportToCsv<T>(IEnumerable<T> data);
    byte[] ExportToPdf<T>(IEnumerable<T> data, string title, string description);
    byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName);
}