using System.Text;
using HotelSystem.BLL.DTOs;

namespace HotelSystem.BLL.Services;

public interface IReportService
{
    string GenerateOccupancyHtml(DateTime dateFrom, DateTime dateTo, IEnumerable<OccupancyReportRowDto> rows);
    string GenerateOccupancyCsv(IEnumerable<OccupancyReportRowDto> rows);
    string GenerateRevenueHtml(DateTime dateFrom, DateTime dateTo, IEnumerable<RevenueReportRowDto> rows);
    string GenerateRevenueCsv(IEnumerable<RevenueReportRowDto> rows);
}

public class ReportService : IReportService
{
    public string GenerateOccupancyHtml(DateTime dateFrom, DateTime dateTo, IEnumerable<OccupancyReportRowDto> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html><head><meta charset='utf-8'><title>Заполненность гостиницы</title>");
        sb.AppendLine("<style>body{font-family:sans-serif;}table{border-collapse:collapse;width:100%;}th,td{border:1px solid #ddd;padding:8px;text-align:left;}th{background:#f2f2f2;}</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine($"<h1>Отчет по заполненности гостиницы</h1><p>Период: {dateFrom:dd.MM.yyyy} - {dateTo:dd.MM.yyyy}</p>");
        sb.AppendLine("<table><tr><th>Дата</th><th>Занято номеров</th><th>Всего номеров</th><th>Заполненность, %</th></tr>");

        foreach (var row in rows)
        {
            sb.AppendLine($"<tr><td>{row.Date:dd.MM.yyyy}</td><td>{row.OccupiedRooms}</td><td>{row.TotalRooms}</td><td>{row.OccupancyPercent:N2}</td></tr>");
        }

        sb.AppendLine("</table></body></html>");
        return sb.ToString();
    }

    public string GenerateOccupancyCsv(IEnumerable<OccupancyReportRowDto> rows)
    {
        var sb = new StringBuilder();
        sb.Append('\uFEFF');
        sb.AppendLine("Дата;ЗанятоНомеров;ВсегоНомеров;ЗаполненностьПроцент");

        foreach (var row in rows)
        {
            sb.AppendLine($"{row.Date:dd.MM.yyyy};{row.OccupiedRooms};{row.TotalRooms};{row.OccupancyPercent}");
        }

        return sb.ToString();
    }

    public string GenerateRevenueHtml(DateTime dateFrom, DateTime dateTo, IEnumerable<RevenueReportRowDto> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html><head><meta charset='utf-8'><title>Выручка гостиницы</title>");
        sb.AppendLine("<style>body{font-family:sans-serif;}table{border-collapse:collapse;width:100%;}th,td{border:1px solid #ddd;padding:8px;text-align:left;}th{background:#f2f2f2;}</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine($"<h1>Отчет по выручке гостиницы</h1><p>Период: {dateFrom:dd.MM.yyyy} - {dateTo:dd.MM.yyyy}</p>");
        sb.AppendLine("<table><tr><th>Дата</th><th>Проживание</th><th>Услуги</th><th>Итого</th></tr>");

        foreach (var row in rows)
        {
            sb.AppendLine($"<tr><td>{row.Date:dd.MM.yyyy}</td><td>{row.AccommodationRevenue:N2}</td><td>{row.ServicesRevenue:N2}</td><td>{row.TotalRevenue:N2}</td></tr>");
        }

        sb.AppendLine("</table></body></html>");
        return sb.ToString();
    }

    public string GenerateRevenueCsv(IEnumerable<RevenueReportRowDto> rows)
    {
        var sb = new StringBuilder();
        sb.Append('\uFEFF');
        sb.AppendLine("Дата;Проживание;Услуги;Итого");

        foreach (var row in rows)
        {
            sb.AppendLine($"{row.Date:dd.MM.yyyy};{row.AccommodationRevenue};{row.ServicesRevenue};{row.TotalRevenue}");
        }

        return sb.ToString();
    }
}
