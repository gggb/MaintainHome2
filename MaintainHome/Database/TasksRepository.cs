using System;
using System.Collections.Generic;
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
            try
            {
                _database = DatabaseConnection.GetConnectionAsync().Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database connection: {ex.Message}");
                throw;
            }
        }

        // Create (Add) Task
        public async Task<bool> AddTaskAsync(Tasks tasks)
        {
            try
            {
                return await _database.InsertAsync(tasks) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding task: {ex.Message}");
                return false;
            }
        }

        // Read (Get) Task by ID
        public async Task<Tasks> GetTaskAsync(int taskId)
        {
            try
            {
                return await _database.FindAsync<Tasks>(taskId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving task with ID {taskId}: {ex.Message}");
                return null;
            }
        }

        // Get all Tasks
        public async Task<List<Tasks>> GetTasksAsync()
        {
            try
            {
                // Fetch all tasks where status is not "closed" using raw SQL query
                //return await _database.QueryAsync<Tasks>("SELECT * FROM Tasks WHERE Status != ?", "closed");
                return await _database.QueryAsync<Tasks>("SELECT * FROM Tasks");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching open tasks: {ex.Message}");
                return new List<Tasks>();
            }
        }

        // Update Task
        public async Task<bool> UpdateTaskAsync(Tasks task)
        {
            try
            {
                return await _database.UpdateAsync(task) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating task with ID {task.Id}: {ex.Message}");
                return false;
            }
        }

        // Delete Task
        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            try
            {
                var task = await _database.FindAsync<Tasks>(taskId);
                if (task != null)
                {
                    return await _database.DeleteAsync(task) > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting task with ID {taskId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Tasks>> SearchTasksAsync(string status, string priority, int? userId, int? categoryId, DateTime? dueDateStart, DateTime? dueDateEnd)
        {
            try
            {
                var parameters = new List<object>();
                var query = "SELECT * FROM Tasks WHERE 1=1";
                if (!string.IsNullOrEmpty(status) && status != "None")
                {
                    query += " AND Status = ?"; parameters.Add(status);
                }
                if (!string.IsNullOrEmpty(priority) && priority != "None")
                {
                    query += " AND Priority = ?"; parameters.Add(priority);
                }
                if (userId.HasValue)
                {
                    query += " AND UserId = ?"; parameters.Add(userId);
                }
                if (categoryId.HasValue)
                {
                    query += " AND CategoryId = ?"; parameters.Add(categoryId);
                }
                if (dueDateStart.HasValue)
                {
                    query += " AND DueDate >= ?"; parameters.Add(dueDateStart.Value);
                }
                if (dueDateEnd.HasValue)
                {
                    query += " AND DueDate <= ?"; parameters.Add(dueDateEnd.Value);
                }
                return await _database.QueryAsync<Tasks>(query, parameters.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching tasks: {ex.Message}");
                return new List<Tasks>();
            }
        }
    }
}
