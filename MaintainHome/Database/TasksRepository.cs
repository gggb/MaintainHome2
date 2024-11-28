using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaintainHome.Models;
using SQLite;

namespace MaintainHome.Database
{
    public class TasksRepository
    {
        private readonly SQLiteAsyncConnection _database;

        public TasksRepository()
        {
            _database = DatabaseConnection.GetConnectionAsync().Result;
        }

        // Create (Add) Task
        public async Task<bool> AddTaskAsync(Tasks tasks)       //AddTaskAsync
        {
            return await _database.InsertAsync(tasks) > 0;
        }

        // Read (Get) Task by ID
        public async Task<Tasks> GetTaskAsync(int taskId)
        {
            return await _database.FindAsync<Tasks>(taskId);
        }

        // Get all Tasks
        public async Task<List<Tasks>> GetAllOpenTasksAsync()
        {
            try
            {
                // Fetch all tasks where status is not "closed" using raw SQL query
                return await _database.QueryAsync<Tasks>("SELECT * FROM Tasks WHERE Status != ?", "closed");
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine($"Error fetching open tasks: {ex.Message}");
                return new List<Tasks>();
            }
        }




        // Update Task
        public async Task<bool> UpdateTaskAsync(Tasks task)
        {
            return await _database.UpdateAsync(task) > 0;
        }

        // Delete Task
        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            var task = await _database.FindAsync<Tasks>(taskId);
            if (task != null)
            {
                return await _database.DeleteAsync(taskId) > 0;
            }
            return false;
        }
    }
}
