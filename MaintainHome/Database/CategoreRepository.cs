using System;
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
            try
            {
                return await _database.InsertAsync(category) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding category: {ex.Message}");
                return false;
            }
        }

        // Read (Get) Category by ID
        public async Task<Category> GetCategoryAsync(int categoryId)
        {
            try
            {
                return await _database.FindAsync<Category>(categoryId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving category with ID {categoryId}: {ex.Message}");
                return null;
            }
        }

        // Update Category
        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            try
            {
                return await _database.UpdateAsync(category) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating category with ID {category.CategoryId}: {ex.Message}");
                return false;
            }
        }

        // Delete Category
        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            try
            {
                var category = await _database.FindAsync<Category>(categoryId);
                if (category != null)
                {
                    return await _database.DeleteAsync(category) > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting category with ID {categoryId}: {ex.Message}");
                return false;
            }
        }

        // Get all categories
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            try
            {
                return await _database.Table<Category>().ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving all categories: {ex.Message}");
                return new List<Category>();
            }
        }
    }
}

