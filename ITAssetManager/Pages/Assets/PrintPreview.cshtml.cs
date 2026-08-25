using System.Threading.Tasks;
using ITAssetManager.Data;
using ITAssetManager.Models;
using ITAssetManager.Services.Pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Stimulsoft.Report;
using Stimulsoft.Report.Mvc;
using Stimulsoft.Report.Web;

namespace ITAssetManager.Pages.Assets;

[Authorize]
public class PrintPreviewModel : PageModel
{
    public IActionResult OnPostGetReport()
    {
        var report = new StiReport();

        return StiNetCoreDesigner.GetReportResult(this, report);
    }

    public IActionResult OnPostSaveReport()
    {
        var report = StiNetCoreDesigner.GetReportObject(this);

        var fileName = "AssetReport.mrt";

        var filePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Reports",
            fileName
        );

        report.Save(filePath);

        return StiNetCoreDesigner.SaveReportResult(
            this,
            "گزارش با موفقیت ذخیره شد."
        );
    }

    public IActionResult OnGetDesignerEvent()
    {
        return StiNetCoreDesigner.DesignerEventResult(this);
    }

    public IActionResult OnPostDesignerEvent()
    {
        return StiNetCoreDesigner.DesignerEventResult(this);
    }


}