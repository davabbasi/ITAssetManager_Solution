using ITAssetManager.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ITAssetManager.Pages.WarehouseManagements.Issues.CreateModel;

namespace ITAssetManager.Services
{
    public class InventoryService
    {
        private readonly ApplicationDbContext _context;

        public InventoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAvailableProductsAsync(int warehouseId)
        {
            var received = await _context.WarehouseReceiptItems
                .Where(x =>
                    x.Receipt != null &&
                    x.Receipt.WarehouseId == warehouseId)
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToListAsync();


            var issued = await _context.WarehouseIssueItems
                .Where(x =>
                    x.Issue.FromWarehouseId == warehouseId)
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToListAsync();


            var issuedDictionary = issued.ToDictionary(
                x => x.ProductId,
                x => x.Quantity
            );


            var stockList = received
                .Select(x =>
                {
                    var issuedQuantity =
                        issuedDictionary.TryGetValue(
                            x.ProductId,
                            out var value)
                            ? value
                            : 0;

                    return new
                    {
                        x.ProductId,

                        Stock = x.Quantity - issuedQuantity
                    };
                })
                .Where(x => x.Stock > 0)
                .ToList();


            var productIds =
                stockList.Select(x => x.ProductId).ToList();


            var products = await _context.Products
                .Where(x => productIds.Contains(x.Id))
                .Select(x => new
                {
                    ProductId = x.Id,
                    ProductName = x.ProductName
                })
                .OrderBy(x => x.ProductName)
                .ToListAsync();


            var result = products
                .Join(
                    stockList,
                    product => product.ProductId,
                    stock => stock.ProductId,
                    (product, stock) => new ProductStockDto
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        Stock = stock.Stock
                    })
                .ToList();


            return new JsonResult(result);
        }
    }
}