using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MaintainHome.Models;
using SQLite;

namespace MaintainHome.Database
{
    public class PartInfoRepository
    {
        private readonly SQLiteAsyncConnection _database;

        public PartInfoRepository()
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

        // Create (Add) PartsInfo
        public async Task<bool> AddPartsInfoAsync(PartInfo partInfo)
        {
            try
            {
                return await _database.InsertAsync(partInfo) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding PartInfo: {ex.Message}");
                return false;
            }
        }

        // Read (Get) PartsInfo by ID
        public async Task<PartInfo> GetPartsInfoAsync(int partInfoId)
        {
            try
            {
                return await _database.FindAsync<PartInfo>(partInfoId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving PartInfo with ID {partInfoId}: {ex.Message}");
                return null;
            }
        }

        // Read (Get) all TaskNot records ethat match the TaskId
        public async Task<List<PartInfo>> GetAllPartsInfoAsyncByTaskId(int taskId)
        {
            try
            {
                return await _database.Table<PartInfo>().Where(t => t.TaskId == taskId).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving PartInfo for TaskID {taskId}: {ex.Message}");
                return new List<PartInfo>();
            }
        }

        // Update PartsInfo
        public async Task<bool> UpdatePartsInfoAsync(PartInfo partInfo)
        {
            try
            {
                return await _database.UpdateAsync(partInfo) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating PartInfo with ID {partInfo.PartInfoId}: {ex.Message}");
                return false;
            }
        }

        // Delete PartsInfo
        public async Task<bool> DeletePartsInfoAsync(int partInfoId)
        {
            try
            {
                var partInfo = await _database.FindAsync<PartInfo>(partInfoId);
                if (partInfo != null)
                {
                    return await _database.DeleteAsync(partInfo) > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting PartInfo with ID {partInfoId}: {ex.Message}");
                return false;
            }
        }

        // Deleting all Part data belonging to a deleted task (to avoid orphaned data)
        public async Task DeletePartsByTaskIdAsync(int taskId)
        {
            try
            {
                var parts = await _database.Table<PartInfo>().Where(p => p.TaskId == taskId).ToListAsync();
                foreach (var part in parts)
                {
                    await _database.DeleteAsync(part);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting PartInfo for TaskID {taskId}: {ex.Message}");
            }
        }
    }
}
