using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using LibraryPlatform.Models;

namespace LibraryPlatform.Services;

public class ReportService
{
    public byte[] GenerateExcel(List<LibEntry> libraries)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Libraries");

        var headers = new[] { "ID", "Name", "Version", "Language", "License", "Stars", "Open Issues", "Last Updated", "Repository" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.SteelBlue;
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        for (int r = 0; r < libraries.Count; r++)
        {
            var lib = libraries[r];
            ws.Cell(r + 2, 1).Value = lib.Id;
            ws.Cell(r + 2, 2).Value = lib.Name;
            ws.Cell(r + 2, 3).Value = lib.Version;
            ws.Cell(r + 2, 4).Value = lib.Language;
            ws.Cell(r + 2, 5).Value = lib.LicenseType;
            ws.Cell(r + 2, 6).Value = lib.GitHubStars ?? 0;
            ws.Cell(r + 2, 7).Value = lib.GitHubOpenIssues ?? 0;
            ws.Cell(r + 2, 8).Value = lib.GitHubLastUpdated?.ToString("yyyy-MM-dd") ?? "N/A";
            ws.Cell(r + 2, 9).Value = lib.RepositoryLink;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] GeneratePdf(List<LibEntry> libraries)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Text("LibraryPlatform — Full Library Report")
                    .SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(30);
                        cols.RelativeColumn(3);
                        cols.RelativeColumn(1);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(1);
                        cols.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        foreach (var h in new[] { "#", "Name", "Version", "Language", "License", "Stars", "Issues" })
                        {
                            header.Cell().Background(Colors.Blue.Darken2)
                                .Padding(4).Text(h).FontColor(Colors.White).SemiBold();
                        }
                    });

                    foreach (var lib in libraries)
                    {
                        var bg = libraries.IndexOf(lib) % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                        table.Cell().Background(bg).Padding(4).Text(lib.Id.ToString());
                        table.Cell().Background(bg).Padding(4).Text(lib.Name);
                        table.Cell().Background(bg).Padding(4).Text(lib.Version);
                        table.Cell().Background(bg).Padding(4).Text(lib.Language);
                        table.Cell().Background(bg).Padding(4).Text(lib.LicenseType);
                        table.Cell().Background(bg).Padding(4).Text((lib.GitHubStars ?? 0).ToString());
                        table.Cell().Background(bg).Padding(4).Text((lib.GitHubOpenIssues ?? 0).ToString());
                    }
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Generated on ");
                    t.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")).SemiBold();
                    t.Span("  |  Page ");
                    t.CurrentPageNumber();
                    t.Span(" of ");
                    t.TotalPages();
                });
            });
        });

        using var ms = new MemoryStream();
        document.GeneratePdf(ms);
        return ms.ToArray();
    }
}
