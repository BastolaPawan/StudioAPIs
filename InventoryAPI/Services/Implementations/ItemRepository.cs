using InventoryAPI.Data;
using InventoryAPI.Models;
using InventoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Services.Implementations
{
    public class ItemRepository(InventoryDbContext context) : IItemRepository
    {
        public async Task<IEnumerable<Item>> GetAllAsync(string? searchTerm, int pageNumber, int pageSize)
        {
            var query = context.Items
                .Include(i => i.ItemGroup)
                .Include(i => i.UnitOfMeasure)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(i => i.Name.Contains(searchTerm) || i.ItemCode.Contains(searchTerm));
            }

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Item?> GetByIdAsync(int id) =>
            await context.Items.Include(i => i.ItemGroup).FirstOrDefaultAsync(i => i.Id == id);

        public async Task AddAsync(Item item) => await context.Items.AddAsync(item);

        public async Task UpdateAsync(Item item) => context.Items.Update(item);

        public async Task DeleteAsync(int id)
        {
            var item = await context.Items.FindAsync(id);
            if (item != null) context.Items.Remove(item);
        }

        public async Task<bool> SaveChangesAsync() => await context.SaveChangesAsync() > 0;

        public async Task<Item?> GetByCodeAsync(string itemCode) =>
            await context.Items.FirstOrDefaultAsync(i => i.ItemCode == itemCode);
    }
}