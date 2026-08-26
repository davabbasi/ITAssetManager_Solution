using ITAssetManager.Models;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ITAssetManager.Convertor;

namespace ITAssetManager.Services.Pdf;

public class AssetPdfService
{
    static AssetPdfService()
    {
        FontManager.RegisterFontWithCustomName(
            "Vazirmatn",
            File.OpenRead("wwwroot/fonts/Vazirmatn-Regular.ttf"));

        FontManager.RegisterFontWithCustomName(
            "Vazirmatn-Bold",
            File.OpenRead("wwwroot/fonts/Vazirmatn-Bold.ttf"));
    }

    public byte[] Generate(List<Asset> assets,Dictionary<int, string>? componentLocations = null)
    {
        componentLocations ??= new Dictionary<int, string>();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                // A4 افقی
                page.Size(PageSizes.A4.Landscape());

                // حاشیه صفحه
                page.Margin(25);

                // فونت پیش‌فرض
                page.DefaultTextStyle(style =>
                    style
                        .FontFamily("Vazirmatn")
                        .FontSize(8));

                // ==========================
                // Header
                // ==========================
                var today = DateTime.Now.ToShamsi();

                page.Header()
                .AlignRight()
                .Column(column =>{column.Item().Text("گزارش تجهیزات")
                    .FontFamily("Vazirmatn-Bold")
                    .FontSize(18);
                    column.Item()
                    .PaddingTop(5)
                    .Text($"تاریخ گزارش: {today}")
                    .FontFamily("Vazirmatn")
                    .FontSize(10);});


                // ==========================
                // Content
                // ==========================

                page.Content()
      .PaddingTop(15)
      .Table(table =>
      {
          table.ColumnsDefinition(columns =>
          {
              // ترتیب ستون‌ها برای نمایش راست به چپ
              columns.RelativeColumn(1.1f);   // مکان قرارگیری
              columns.RelativeColumn(1.3f);   // اسمبل شده در
              columns.ConstantColumn(65);     // وضعیت
              columns.RelativeColumn(1.1f);   // پرسنل
              columns.RelativeColumn(1.0f);   // واحد
              columns.ConstantColumn(55);     // نوع تجهیز
              columns.RelativeColumn(1.1f);   // دسته‌بندی
              columns.RelativeColumn(1.5f);   // نام / مدل
              columns.ConstantColumn(65);     // برچسب اموال
              columns.ConstantColumn(40);     // کد تجهیز
          });

          table.Header(header =>
          {
              // راست‌ترین ستون
              header.Cell()
                  .Element(HeaderStyle)
                  .Text("مکان قرارگیری");

              header.Cell()
                  .Element(HeaderStyle)
                  .Text("اسمبل شده در");

              header.Cell()
                  .Element(HeaderStyle)
                  .Text("وضعیت");

              header.Cell()
                  .Element(HeaderStyle)
                  .Text("پرسنل");

              header.Cell()
                  .Element(HeaderStyle)
                  .Text("واحد");

              header.Cell()
                  .Element(HeaderStyle)
                  .Text("نوع تجهیز");

              header.Cell()
                  .Element(HeaderStyle)
                  .Text("دسته‌بندی");

              header.Cell()
                  .Element(HeaderStyle)
                  .Text("نام / مدل");

              header.Cell()
                  .Element(HeaderStyle)
                  .Text("برچسب اموال");

              // چپ‌ترین ستون
              header.Cell()
                  .Element(HeaderStyle)
                  .Text("کد تجهیز");
          });

          foreach (var asset in assets)
          {
              string assemblyLocation = "-";

              if (componentLocations.TryGetValue(
                  asset.Id,
                  out var location))
              {
                  assemblyLocation = location;
              }

              var nameModel = asset.Name;

              

              var assetType = GetCategoryTypeText(
                  asset.Category?.Type);


              // ==========================================
              // ترتیب ثبت Cell ها از راست به چپ
              // ==========================================

              // مکان قرارگیری
              table.Cell()
                  .Element(CellStyle)
                  .Text(asset.Location ?? "-");

              // اسمبل شده در
              table.Cell()
                  .Element(CellStyle)
                  .Text(assemblyLocation);

              // وضعیت
              table.Cell()
                  .Element(CellStyleCenter)
                  .Text(GetStatusText(asset.Status));

              // پرسنل
              table.Cell()
                  .Element(CellStyle)
                  .Text(asset.EmployeeName ?? "-");

              // واحد
              table.Cell()
                  .Element(CellStyle)
                  .Text(asset.DepartmentName ?? "-");

              // نوع تجهیز
              table.Cell()
                  .Element(CellStyleCenter)
                  .Text(assetType);

              // دسته‌بندی
              table.Cell()
                  .Element(CellStyle)
                  .Text(asset.Category?.Name ?? "-");

              // نام / مدل
              table.Cell()
                  .Element(CellStyle)
                  .Text(nameModel ?? "-");

              // برچسب اموال
              table.Cell()
                  .Element(CellStyleCenter)
                  .Text(asset.PropertyTag ?? "-");

              // کد تجهیز
              table.Cell()
                  .Element(CellStyleCenter)
                  .Text(asset.Id.ToString());
          }
      });

                // ==========================
                // Footer
                // ==========================

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("تعداد تجهیزات: ");

                        text.Span(
                            assets.Count.ToString())
                            .Bold();
                    });
            });
        });
        return document.GeneratePdf();
    }

    // =====================================================
    // وضعیت تجهیز
    // =====================================================

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
            AssetStatus.Installed => "اسمبل شده",
            _ => "نامشخص"
        };
    }

    // =====================================================
    // نوع دسته‌بندی
    // =====================================================

    private static string GetCategoryTypeText(
        AssetCategoryType? type)
    {
        return type switch
        {
            AssetCategoryType.Installed => "قطعه",
            AssetCategoryType.Tagged_Installed=>"قطعه/مستقل",
            AssetCategoryType.Tagged=>"مستقل",
            AssetCategoryType.Consumable => "مصرفی",

            // اگر enum پروژه‌ات مقادیر دیگری دارد
            // بعداً اینجا اضافه می‌کنیم.

            _ => "-"
        };
    }

    // =====================================================
    // استایل Header
    // =====================================================

    private static IContainer HeaderStyle(
        IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten2)
            .Border(1)
            .Padding(4)
            .AlignCenter()
            .AlignMiddle()
            .DefaultTextStyle(
                x => x
                    .FontFamily("Vazirmatn-Bold")
                    .FontSize(8));
    }

    // =====================================================
    // استایل سلول
    // =====================================================

    private static IContainer CellStyle(
        IContainer container)
    {
        return container
            .Border(1)
            .Padding(4)
            .AlignRight()
            .AlignMiddle();
    }

    // =====================================================
    // استایل سلول وسط‌چین
    // =====================================================

    private static IContainer CellStyleCenter(
        IContainer container)
    {
        return container
            .Border(1)
            .Padding(4)
            .AlignCenter()
            .AlignMiddle();
    }
}