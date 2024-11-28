using System.Collections.Generic;
using System.Threading.Tasks;
using MaintainHome.Models;
using SQLite;

namespace MaintainHome.Database
{
    public class CategoryRepository
    {
        private readonly SQLiteAsyncConnection _database;

        public CategoryRepository()
        {
            _database = DatabaseConnection.GetConnectionAsync().Result;
        }

        // Create (Add) Category
        public async Task<bool> AddCategoryAsync(Category category)
        {
            return await _database.InsertAsync(category) > 0;
        }

        // Read (Get) Category by ID
        public async Task<Category> GetCategoryAsync(int categoryId)
        {
            return await _database.FindAsync<Category>(categoryId);
        }

        // Update Category
        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            return await _database.UpdateAsync(category) > 0;
        }

        // Delete Category
        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            var category = await _database.FindAsync<Category>(categoryId);
            if (category != null)
            {
                return await _database.DeleteAsync(category) > 0;
            }
            return false;
        }

        // New method to get all categories
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _database.Table<Category>().ToListAsync();
        }
    }
}
