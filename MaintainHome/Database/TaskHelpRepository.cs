using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MaintainHome.Models;

namespace MaintainHome.Database
{
    public class TaskHelpRepository
    {
        private readonly SQLiteAsyncConnection _database;

        public TaskHelpRepository()
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

        // Create (Add) TaskHelp
        public async Task<bool> AddTaskHelpAsync(TaskHelp taskHelp)
        {
            try
            {
                return await _database.InsertAsync(taskHelp) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding TaskHelp: {ex.Message}");
                return false;
            }
        }

        // Read (Get) all TaskHelp records that match the TaskId
        public async Task<List<TaskHelp>> GetAllTaskHelpsAsyncByTaskId(int taskId)
        {
            try
            {
                return await _database.Table<TaskHelp>().Where(t => t.TaskId == taskId).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving TaskHelps for TaskID {taskId}: {ex.Message}");
                return new List<TaskHelp>();
            }
        }

        // Read (Get) TaskHelp by ID
        public async Task<TaskHelp> GetTaskHelpAsync(int taskHelpId)
        {
            try
            {
                return await _database.FindAsync<TaskHelp>(taskHelpId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving TaskHelp with ID {taskHelpId}: {ex.Message}");
                return null;
            }
        }

        // Update TaskHelp
        public async Task<bool> UpdateTaskHelpAsync(TaskHelp taskHelp)
        {
            try
            {
                return await _database.UpdateAsync(taskHelp) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating TaskHelp with ID {taskHelp.TaskHelpsId}: {ex.Message}");
                return false;
            }
        }

        // Delete TaskHelp
        public async Task<bool> DeleteTaskHelpAsync(int taskHelpId)
        {
            try
            {
                var taskHelp = await _database.FindAsync<TaskHelp>(taskHelpId);
                if (taskHelp != null)
                {
                    return await _database.DeleteAsync(taskHelp) > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting TaskHelp with ID {taskHelpId}: {ex.Message}");
                return false;
            }
        }

        // Delete all help (tips) associated with a deleted task (to avoid orphaned data)
        public async Task DeleteHelpsByTaskIdAsync(int taskId)
        {
            try
            {
                var helps = await _database.Table<TaskHelp>().Where(h => h.TaskId == taskId).ToListAsync();
                foreach (var help in helps)
                {
                    await _database.DeleteAsync(help);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting TaskHelps for TaskID {taskId}: {ex.Message}");
            }
        }
    }
}
