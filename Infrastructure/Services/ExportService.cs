using Application.Interfaces;
using ClosedXML.Excel;

namespace Infrastructure.Services;

public class ExportService : IExportService
{
    private readonly IExpenseRepository _expenseRepo;
    private readonly IReportService _reportService;

    public ExportService(IExpenseRepository expenseRepo, IReportService reportService)
    {
        _expenseRepo = expenseRepo;
        _reportService = reportService;
    }

    public async Task<byte[]> ExportExpensesToExcelAsync(int userId, int month, int year)
    {
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);
        var expenses = await _expenseRepo.GetUserExpensesAsync(userId, from, to);
        var monthName = new DateTime(year, month, 1).ToString("MMMM yyyy");

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add($"Expenses {monthName}");

        // Header styling
        var headerRange = ws.Range("A1:F1");
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E86AB");
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        ws.Cell("A1").Value = "Date";
        ws.Cell("B1").Value = "Category";
        ws.Cell("C1").Value = "Description";
        ws.Cell("D1").Value = "Amount";
        ws.Cell("E1").Value = "Icon";
        ws.Cell("F1").Value = "Month";

        int row = 2;
        foreach (var expense in expenses.OrderBy(e => e.Date))
        {
            ws.Cell(row, 1).Value = expense.Date.ToString("yyyy-MM-dd");
            ws.Cell(row, 2).Value = expense.Category.Name;
            ws.Cell(row, 3).Value = expense.Description;
            ws.Cell(row, 4).Value = expense.Amount;
            ws.Cell(row, 5).Value = expense.Category.Icon;
            ws.Cell(row, 6).Value = monthName;

            if (row % 2 == 0)
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F5F5");

            row++;
        }

        // Summary row
        ws.Cell(row, 3).Value = "TOTAL";
        ws.Cell(row, 3).Style.Font.Bold = true;
        ws.Cell(row, 4).FormulaA1 = $"=SUM(D2:D{row - 1})";
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F4FD");

        ws.Column(4).Style.NumberFormat.Format = "#,##0.00";
        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportMonthlyReportToPdfAsync(int userId, int month, int year)
    {
        return await ExportExpensesToExcelAsync(userId, month, year);
    }
}
