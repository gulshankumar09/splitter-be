using ExpenseService.Application.DTOs;
using ExpenseService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

namespace ExpenseService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportingService _reportingService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportingService reportingService, ILogger<ReportsController> logger)
    {
        _reportingService = reportingService;
        _logger = logger;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<Result<ExpenseSummaryDto>>> GetExpenseSummary([FromQuery] ExpenseReportRequestDto request)
    {
        request.ReportType = ReportType.Summary;
        var result = await _reportingService.GetExpenseSummaryAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("category-breakdown")]
    public async Task<ActionResult<Result<List<CategoryBreakdownDto>>>> GetCategoryBreakdown([FromQuery] ExpenseReportRequestDto request)
    {
        request.ReportType = ReportType.CategoryBreakdown;
        var result = await _reportingService.GetCategoryBreakdownAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("spending-trend")]
    public async Task<ActionResult<Result<SpendingTrendDto>>> GetSpendingTrend([FromQuery] ExpenseReportRequestDto request)
    {
        request.ReportType = ReportType.SpendingTrend;
        var result = await _reportingService.GetSpendingTrendAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("export")]
    public async Task<ActionResult> ExportReport([FromQuery] ExpenseReportRequestDto request)
    {
        if (!request.ExportFormat.HasValue)
        {
            return BadRequest("Export format must be specified");
        }

        var result = await _reportingService.ExportReportAsync(request);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        string contentType;
        string fileName;

        switch (request.ExportFormat.Value)
        {
            case ExportFormat.Csv:
                contentType = "text/csv";
                fileName = $"expense-report-{DateTime.Now:yyyyMMdd}.csv";
                break;
            case ExportFormat.Pdf:
                contentType = "application/pdf";
                fileName = $"expense-report-{DateTime.Now:yyyyMMdd}.pdf";
                break;
            case ExportFormat.Excel:
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileName = $"expense-report-{DateTime.Now:yyyyMMdd}.xlsx";
                break;
            default:
                return BadRequest("Invalid export format");
        }

        return File(result.Data, contentType, fileName);
    }
}