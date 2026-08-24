using ITAssetManager.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;

namespace ITAssetManager.Services.Pdf;

public class AssetPdfService
{
    static AssetPdfService()
    {
        FontManager.RegisterFontWithCustomName(
            "Vazirmatn",
            File.OpenRead("wwwroot/fonts/Vazirmatn-Regular.ttf"));

        FontManager.RegisterFontWithCustomName(
            "Vazirmatn",
            File.OpenRead("wwwroot/fonts/Vazirmatn-Bold.ttf"));

        FontManager.RegisterFontWithCustomName(
    "Vazirmatn",
    File.OpenRead("wwwroot/fonts/Vazirmatn-Regular.ttf"));

        FontManager.RegisterFontWithCustomName(
            "Vazirmatn-Bold",
            File.OpenRead("wwwroot/fonts/Vazirmatn-Bold.ttf"));
    }
    public byte[] Generate(List<Asset> assets)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(style =>style.FontFamily("Vazirmatn").FontSize(9));
                page.Header()
                    .AlignRight()
                    .Text("گزارش تجهیزات")
                    .FontFamily("Vazirmatn-Bold")
                    .FontSize(18);
                page.Content()
                    .PaddingTop(20)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(45);       // وضعیت
                            columns.RelativeColumn(1.5f);     // سریال
                            columns.RelativeColumn(1.5f);     // برچسب
                            columns.RelativeColumn(1.5f);     // دسته‌بندی
                            columns.RelativeColumn(2.5f);     // نام تجهیز
                            columns.ConstantColumn(40);        // ردیف
                        });

                        table.Header(header =>
                        {
                            header.Cell()
                            .Element(HeaderStyle)
                            .Text("وضعیت");

                            header.Cell()
                                .Element(HeaderStyle)
                                .Text("سریال");

                            header.Cell()
                                .Element(HeaderStyle)
                                .Text("برچسب اموال");

                            header.Cell()
                                .Element(HeaderStyle)
                                .Text("دسته‌بندی");

                            header.Cell()
                                .Element(HeaderStyle)
                                .Text("نام تجهیز");

                            header.Cell()
                                .Element(HeaderStyle)
                                .Text("ردیف");
                        });

                        var row = 1;

                        foreach (var asset in assets)
                        {
                            table.Cell()
                            .Element(CellStyle)
                            .Text(GetStatusText(asset.Status));

                            table.Cell()
                                .Element(CellStyleCenter)
                                .Text(asset.SerialNumber ?? "-");

                            table.Cell()
                                .Element(CellStyle)
                                .Text(asset.PropertyTag ?? "-");

                            table.Cell()
                                .Element(CellStyle)
                                .Text(asset.Category?.Name ?? "-");

                            table.Cell()
                                .Element(CellStyle)
                                .Text(asset.Name ?? "-");

                            table.Cell()
                                .Element(CellStyleCenter)
                                .Text(row.ToString());

                            row++;
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("تعداد تجهیزات: ");
                        text.Span(assets.Count.ToString()).Bold();
                    });
            });
        });

        return document.GeneratePdf();
    }

    private static string GetStatusText(AssetStatus status)
    {
        return status switch
        {
            AssetStatus.Active => "فعال",
            AssetStatus.Faulty => "معیوب",
            AssetStatus.UnderRepair => "در تعمیر",
            AssetStatus.Scrapped => "اسقاط",
            AssetStatus.Waste => "ضایعات",
            AssetStatus.InStorage => "در انبار",
            AssetStatus.Assigned => "تحویل داده شده",
            _ => "نامشخص"
        };
    }

    private static IContainer HeaderStyle(IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten2)
            .Border(1)
            .Padding(5)
            .AlignRight();
    }
    private static IContainer CellStyle(IContainer container)
    {
        return container
            .Border(1)
            .Padding(5)
            .AlignRight();
    }

    private static IContainer CellStyleCenter(IContainer container)
    {
        return container
            .Border(1)
            .Padding(5)
            .AlignCenter();
    }
}