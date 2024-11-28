using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaintainHome.Models;
using SQLite;

namespace MaintainHome.Database
{
    public class PartBuyRepository
    {
        private readonly SQLiteAsyncConnection _database;

        public PartBuyRepository()
        {
            _database = DatabaseConnection.GetConnectionAsync().Result;
        }

        public async Task<bool> AddPartBuyAsync(PartBuy partBuy)
        {
            return await _database.InsertAsync(partBuy) > 0;
        }

        public async Task<PartBuy> GetPartBuyAsync(int partBuyId)
        {
            return await _database.FindAsync<PartBuy>(partBuyId);
        }

        public async Task<bool> UpdatePartBuyAsync(PartBuy partBuy)
        {
            return await _database.UpdateAsync(partBuy) > 0;
        }

        public async Task<bool> DeletePartBuyAsync(int partBuyId)
        {
            var partsBuy = await _database.FindAsync<PartBuy>(partBuyId);
            if (partsBuy != null)
            {
                return await _database.DeleteAsync(partBuyId) > 0;
            }
            return false;
        }
    }
}
