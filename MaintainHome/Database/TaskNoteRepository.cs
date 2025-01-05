using System;
using System.Collections.Generic;
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

        // Create (Add) TaskNote
        public async Task<bool> AddTaskNoteAsync(TaskNote taskNote)
        {
            try
            {
                return await _database.InsertAsync(taskNote) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding TaskNote: {ex.Message}");
                return false;
            }
        }

        // Read (Get) all TaskNote records that match the TaskId
        public async Task<List<TaskNote>> GetAllTaskNotesAsyncByTaskId(int taskId)
        {
            try
            {
                return await _database.Table<TaskNote>().Where(t => t.TaskId == taskId).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving TaskNotes for TaskID {taskId}: {ex.Message}");
                return new List<TaskNote>();
            }
        }

        // Read (Get) TaskNote by ID
        public async Task<TaskNote> GetTaskNoteAsync(int taskNoteId)
        {
            try
            {
                return await _database.FindAsync<TaskNote>(taskNoteId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving TaskNote with ID {taskNoteId}: {ex.Message}");
                return null;
            }
        }

        // Update TaskNote
        public async Task<bool> UpdateTaskNoteAsync(TaskNote taskNote)
        {
            try
            {
                return await _database.UpdateAsync(taskNote) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating TaskNote with ID {taskNote.NoteId}: {ex.Message}");
                return false;
            }
        }

        // Delete TaskNote
        public async Task<bool> DeleteTaskNoteAsync(int taskNoteId)
        {
            try
            {
                var taskNote = await _database.FindAsync<TaskNote>(taskNoteId);
                if (taskNote != null)
                {
                    return await _database.DeleteAsync(taskNote) > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting TaskNote with ID {taskNoteId}: {ex.Message}");
                return false;
            }
        }

        // Delete all notes associated with a deleted task (to avoid orphaned data)
        public async Task DeleteNotesByTaskIdAsync(int taskId)
        {
            try
            {
                var notes = await _database.Table<TaskNote>().Where(n => n.TaskId == taskId).ToListAsync();
                foreach (var note in notes)
                {
                    await _database.DeleteAsync(note);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting TaskNotes for TaskID {taskId}: {ex.Message}");
            }
        }
    }
}
