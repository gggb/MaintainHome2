using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaintainHome.Models;
using SQLite;

namespace MaintainHome.Database
{
    public class TaskNoteRepository
    {
        private readonly SQLiteAsyncConnection _database;

        public TaskNoteRepository()
        {
            _database = DatabaseConnection.GetConnectionAsync().Result;
        }

        // Create (Add) TaskNote
        public async Task<bool> AddTaskNoteAsync(TaskNote taskNote)
        {
            return await _database.InsertAsync(taskNote) > 0;
        }

        // Read (Get) TaskNote by ID
        public async Task<TaskNote> GetTaskNoteAsync(int taskNoteId)
        {
            return await _database.FindAsync<TaskNote>(taskNoteId);
        }

        // Update TaskNote
        public async Task<bool> UpdateTaskNoteAsync(TaskNote taskNote)
        {
            return await _database.UpdateAsync(taskNote) > 0;
        }

        // Delete TaskNote
        public async Task<bool> DeleteTaskNoteAsync(int taskNoteId)
        {
            var taskNote = await _database.FindAsync<TaskNote>(taskNoteId);
            if (taskNote != null)
            {
                return await _database.DeleteAsync(taskNote) > 0;
            }
            return false;
        }
    }
}
