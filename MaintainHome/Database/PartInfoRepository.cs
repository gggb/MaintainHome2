using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaintainHome.Models;
using SQLite;
//using static Android.Media.Midi.MidiDeviceInfo;

namespace MaintainHome.Database
{
    public class PartInfoRepository
    {
        private readonly SQLiteAsyncConnection _database;

        public PartInfoRepository()
        {
            _database = DatabaseConnection.GetConnectionAsync().Result;
        }

        // Create (Add) PartsInfo
        public async Task<bool> AddPartsInfoAsync(PartInfo partInfo)
        {
            return await _database.InsertAsync(partInfo) > 0;
        }
        





        // Read (Get) PartsInfo by ID
        public async Task<PartInfo> GetPartsInfoAsync(int partInfoId)
        {
            return await _database.FindAsync<PartInfo>(partInfoId);
        }

        // Read (Get) all TaskNot records ethat match the TaskId
        public async Task<List<PartInfo>> GetAllPartsInfoAsyncByTaskId(int taskId)
        {
            return await _database.Table<PartInfo>().Where(t => t.TaskId == taskId).ToListAsync();
        }







        // Update PartsInfo
        public async Task<bool> UpdatePartsInfoAsync(PartInfo partInfo)
        {
            return await _database.UpdateAsync(partInfo) > 0;
        }

        // Delete PartsInfo
        public async Task<bool> DeletePartsInfoAsync(int partInfoId)
        {
            var partInfo = await _database.FindAsync<PartInfo>(partInfoId);
            if (partInfo != null)
            {
                return await _database.DeleteAsync(partInfo) > 0;
            }
            return false;
        }
    }
}
