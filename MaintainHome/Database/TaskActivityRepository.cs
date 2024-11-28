using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaintainHome.Models;
using SQLite;

namespace MaintainHome.Database
{
    public class TaskActivityRepository
    {
        private readonly SQLiteAsyncConnection _database;

        public TaskActivityRepository()
        {
            _database = DatabaseConnection.GetConnectionAsync().Result;
        }

        // Create (Add) TaskActivity
        public async Task<bool> AddTaskActivity(TaskActivity taskActivity)
        {
            return await _database.InsertAsync(taskActivity) > 0;
        }

        // Read (Get) TaskActivity by ID
        public async Task<TaskActivity> GetTaskActivityAsync(int taskActivityId)
        {
            return await _database.FindAsync<TaskActivity>(taskActivityId);
        }

        // Update TaskActivity
        public async Task<bool> UpdateTaskActivityAsync(TaskActivity taskActivity)
        {
            return await _database.UpdateAsync(taskActivity) > 0;
        }

        // Delete TaskActivity
        public async Task<bool> DeleteTaskActivityAsync(int taskActivityId)
        {
            var taskActivity = await _database.FindAsync<TaskActivity>(taskActivityId);
            if (taskActivity != null)
            {
                return await _database.DeleteAsync(taskActivity) > 0;
            }
            return false;
        }
    }
}
