using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DayLog.Data;
using DayLog.Models;
using System.Collections.Generic;

namespace DayLog.Services
{
    public class EntryService
    {
        private readonly AppDbContext _db;
        public EntryService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Entry> GetByDateAsync(DateTime date)
        {
            var day = date.Date;
            return await _db.Entries.FirstOrDefaultAsync(e => e.Date == day);
        }

        public async Task<List<Entry>> GetAllAsync()
        {
            return await _db.Entries.OrderByDescending(e => e.Date).ToListAsync();
        }

        public async Task SaveAsync(Entry entry)
        {
            if (entry.Id == 0)
            {
                entry.CreatedAt = DateTime.UtcNow;
                entry.UpdatedAt = DateTime.UtcNow;
                entry.Date = entry.Date.Date;
                _db.Entries.Add(entry);
            }
            else
            {
                entry.UpdatedAt = DateTime.UtcNow;
                _db.Entries.Update(entry);
            }
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var e = await _db.Entries.FindAsync(id);
            if (e != null)
            {
                _db.Entries.Remove(e);
                await _db.SaveChangesAsync();
            }
        }
    }
}
