using System;
using System.Collections.Generic;
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

        // Create (Add) TaskActivity
        public async Task<bool> AddTaskActivity(TaskActivity taskActivity)
        {
            try
            {
                return await _database.InsertAsync(taskActivity) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding TaskActivity: {ex.Message}");
                return false;
            }
        }

        // Read (Get) TaskActivity by ID
        public async Task<TaskActivity> GetTaskActivityAsync(int taskActivityId)
        {
            try
            {
                return await _database.FindAsync<TaskActivity>(taskActivityId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving TaskActivity with ID {taskActivityId}: {ex.Message}");
                return null;
            }
        }

        // Read (Get) all TaskActivities for a specific Task ID
        public async Task<List<TaskActivity>> GetTaskActivitiesAsyncByTaskId(int taskId)
        {
            try
            {
                return await _database.Table<TaskActivity>().Where(t => t.TaskId == taskId).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving TaskActivities for TaskID {taskId}: {ex.Message}");
                return new List<TaskActivity>();
            }
        }

        // Update TaskActivity
        public async Task<bool> UpdateTaskActivityAsync(TaskActivity taskActivity)
        {
            try
            {
                return await _database.UpdateAsync(taskActivity) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating TaskActivity with ID {taskActivity.Id}: {ex.Message}");
                return false;
            }
        }

        // Delete TaskActivity
        public async Task<bool> DeleteTaskActivityAsync(int taskActivityId)
        {
            try
            {
                var taskActivity = await _database.FindAsync<TaskActivity>(taskActivityId);
                if (taskActivity != null)
                {
                    return await _database.DeleteAsync(taskActivity) > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting TaskActivity with ID {taskActivityId}: {ex.Message}");
                return false;
            }
        }

        // Delete all activities associated with a deleted task (to avoid orphaned data)
        public async Task DeleteActivitiesByTaskIdAsync(int taskId)
        {
            try
            {
                var activities = await _database.Table<TaskActivity>().Where(a => a.TaskId == taskId).ToListAsync();
                foreach (var activity in activities)
                {
                    await _database.DeleteAsync(activity);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting TaskActivities for TaskID {taskId}: {ex.Message}");
            }
        }
    }
}
