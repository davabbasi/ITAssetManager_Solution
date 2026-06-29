namespace ITAssetManager.Models.DTOs
{
    public class PurchaseRequestViewModel
    {
        public List<VwPurchaseRequest> VwPurchaseRequests { get; set; }=new List<VwPurchaseRequest>();
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
    }
}
