using InventoryAPI.Models;

namespace InventoryAPI.Services.Interfaces
{
    public interface IItemRepository
    {
        /// <summary>
        /// this method is used to get all items with pagination and optional search term
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<IEnumerable<Item>> GetAllAsync(string? searchTerm, int pageNumber, int pageSize);
        /// <summary>
        /// this method is used to get item by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Item?> GetByIdAsync(int id);
        /// <summary>
        /// this method is used to get item by item code
        /// </summary>
        /// <param name="itemCode"></param>
        /// <returns></returns>
        Task<Item?> GetByCodeAsync(string itemCode); // To check for unique ItemCode requirement
        /// <summary>
        /// this method is used to add new item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task AddAsync(Item item);
        /// <summary>
        /// this method is used to update existing item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task UpdateAsync(Item item);
        /// <summary>
        /// this method is used to delete item by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteAsync(int id);
        /// <summary>
        /// this method is used to save changes to the database
        /// </summary>
        /// <returns></returns>
        Task<bool> SaveChangesAsync();
        //ToDo : Add Controller methods and test for All API.
    }
}