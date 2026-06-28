namespace ITAssetManager.Models
{
    public class VwPurchaseRequest
    {
        public string? RequestNo { get; set; }
        public int? RequestDate { get; set; }
        public string? OrderNo { get; set; }
        public int? OrderDate { get; set; }
        public decimal? ConfirmedQuantity { get; set; }
        public decimal? Quantity { get; set; }
        public string? Seller { get; set; }
        public string? ItemCode { get; set; }
        public string? FarsiDesc { get; set; }
        public string? CurrentUnit { get; set; }
        public decimal? ReceivedQuantity { get; set; }
        public decimal? PayRate { get; set; }
        public decimal? PayPass { get; set; }
        public string? Serial { get; set; }
    }
}
