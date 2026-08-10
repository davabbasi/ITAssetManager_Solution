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

    }
}