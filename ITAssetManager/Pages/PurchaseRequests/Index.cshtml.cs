using ITAssetManager.Data;
using ITAssetManager.Models;
using ITAssetManager.Models.DTOs;
using ITAssetManager.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.PurchaseRequests;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public IndexModel(ApplicationDbContext context) => _context = context;
    public PurchaseRequestViewModel Result { get; set; }= new PurchaseRequestViewModel();
    public int Take { get; set; } = 50;
    public int TotalRecord { get; set; } = 0;
    public async Task OnGetAsync(int currentPage = 1 ,string? orderNo = null,string? serial = null, string? description = null)
    {

        IQueryable<VwPurchaseRequest> purchaseRequests = _context.VwPurchaseRequests;

        if (!string.IsNullOrEmpty(orderNo))
        {
            purchaseRequests= purchaseRequests.Where(r=>r.OrderNo.Contains( orderNo));
        }
        if(!string.IsNullOrEmpty(serial))
        {
            purchaseRequests = purchaseRequests.Where(r => r.Serial.Contains(serial));
        }
        if(!string.IsNullOrEmpty(description))
        {
            purchaseRequests = purchaseRequests.Where(r => r.FarsiDesc.Contains(description));
        }

        
        int skip = (currentPage - 1) * Take;

        PurchaseRequestViewModel result = new PurchaseRequestViewModel();
        result.CurrentPage = currentPage;
        TotalRecord = purchaseRequests.Count();
        result.PageCount = purchaseRequests.Count() / Take;
        result.VwPurchaseRequests=await purchaseRequests.OrderBy(r=>r.OrderDate).Skip(skip).Take(Take).ToListAsync();

        Result = result;



        //Page = page < 1 ? 1 : page;
        //OrderNo = orderNo;
        //Serial = serial;
        //Description = description;

        //var query = _context.VwPurchaseRequests.AsQueryable();

        //if (!string.IsNullOrEmpty(OrderNo))
        //    query = query.Where(x => x.OrderNo != null && x.OrderNo.Contains(OrderNo));

        //if (!string.IsNullOrEmpty(Serial))
        //    query = query.Where(x => x.Serial != null && x.Serial.Contains(Serial));

        //if (!string.IsNullOrEmpty(Description))
        //    query = query.Where(x => x.FarsiDesc != null && x.FarsiDesc.Contains(Description));

        //TotalCount = await query.CountAsync();
        //TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
        //Result = await query
        //    .OrderByDescending(x => x.RequestDate)
        //    .ToPagedResultAsync(Page, PageSize);

        //Items = await query.OrderByDescending(x => x.RequestDate).ToListAsync();
    }
}