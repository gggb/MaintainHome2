using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaintainHome.Models;

namespace MaintainHome.Database
{
    public class TaskHelpRepository
    {
        private readonly SQLiteAsyncConnection _database;

        public TaskHelpRepository()
        {
            _database = DatabaseConnection.GetConnectionAsync().Result;
        }

        // Create (Add) TaskHelp
        public async Task<bool> AddTaskHelpAsync(TaskHelp taskHelp)
        {
            return await _database.InsertAsync(taskHelp) > 0;
        }

        // Read (Get) TaskHelp by ID
        public async Task<TaskHelp> GetTaskHelpAsync(int taskHelpId)
        {
            return await _database.FindAsync<TaskHelp>(taskHelpId);
        }

        // Update TaskHelp
        public async Task<bool> UpdateTaskHelpAsync(TaskHelp taskHelp)
        {
            return await _database.UpdateAsync(taskHelp) > 0;
        }

        // Delete TaskHelp
        public async Task<bool> DeleteTaskHelpAsync(int taskHelpId)
        {
            var taskHelp = await _database.FindAsync<TaskHelp>(taskHelpId);
            if (taskHelp != null)
            {
                return await _database.DeleteAsync(taskHelp) > 0;
            }
            return false;
        }
    }
}
