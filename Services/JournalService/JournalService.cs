// Services/JournalService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DayLog.Common;
using DayLog.Data;
using DayLog.Entities;
using DayLog.Models;
using DayLog.Helpers;

namespace DayLog.Services.JournalService
{
    public class JournalService : IJournalService
    {
        private readonly AppDbContext _db;

        private record ConflictInfo(int? ConflictId, int RemovedCount);

        public JournalService(AppDbContext db)
        {
            _db = db;
            // For prototype, ensure DB exists. For production, prefer migrations and Migrate().
            _db.Database.EnsureCreated();
        }

        // -----------------------
        // Mapping helpers
        // -----------------------

        // Convert entity -> display model (short preview + core fields)
        private static EntryDisplayModel ToDisplay(LogEntry e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));

            // Get plain-text preview by stripping HTML; fall back to raw content if helper not present
            string plain = e.ContentHtml ?? string.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(plain))
                {
                    // HtmlUtils.StripHtml should return plain text without tags
                    plain = HtmlUtils.StripHtml(plain);
                }
            }
            catch
            {
                // If HtmlUtils isn't available or fails, keep original HTML stripped crudely
                plain = StripTagsFallback(e.ContentHtml ?? string.Empty);
            }

            // Truncate to 300 characters for preview
            var previewText = plain.Length > 300 ? plain.Substring(0, 300) + "�" : plain;

            return new EntryDisplayModel
            {
                Id = e.Id,
                EntryDate = e.EntryDate,
                Title = e.Title,
                // PreviewHtml field holds a short plain-text preview (you can rename to PreviewText if preferred)
                PreviewHtml = previewText,
                PrimaryMoodId = e.PrimaryMoodId,
                TagsCsv = e.TagsCsv
            };
        }

        // Convert viewmodel -> entity (for insert/update)
        private static LogEntry FromViewModel(EntryViewModel vm, LogEntry? existing = null)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));

            // If updating, copy into existing entity; otherwise create new
            if (existing == null)
            {
                return new LogEntry
                {
                    EntryDate = vm.EntryDate.Date,
                    Title = vm.Title,
                    ContentHtml = vm.ContentHtml,
                    PrimaryMoodId = vm.PrimaryMoodId,
                    TagsCsv = vm.TagsCsv,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    WordCount = ComputeWordCount(vm.ContentHtml)
                };
            }
            else
            {
                existing.EntryDate = vm.EntryDate.Date;
                existing.Title = vm.Title;
                existing.ContentHtml = vm.ContentHtml;
                existing.PrimaryMoodId = vm.PrimaryMoodId;
                existing.TagsCsv = vm.TagsCsv;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.WordCount = ComputeWordCount(vm.ContentHtml);
                return existing;
            }
        }

        // -----------------------
        // Utility helpers
        // -----------------------

        // Count words from HTML content (strip tags then split on whitespace)
        private static int ComputeWordCount(string? html)
        {
            if (string.IsNullOrWhiteSpace(html)) return 0;

            string plain;
            try
            {
                plain = HtmlUtils.StripHtml(html);
            }
            catch
            {
                plain = StripTagsFallback(html);
            }

            var words = plain.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Length;
        }

        // Safe fallback for stripping tags if HtmlUtils is not available
        private static string StripTagsFallback(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            var array = new char[input.Length];
            var arrayIndex = 0;
            var inside = false;

            for (int i = 0; i < input.Length; i++)
            {
                char let = input[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }

        // -----------------------
        // Service methods
        // -----------------------

        public async Task<ServiceResult<EntryDisplayModel>> GetByIdAsync(int id)
        {
            var e = await _db.LogEntries.FindAsync(id);
            if (e == null) return ServiceResult<EntryDisplayModel>.Fail("Not found");
            return ServiceResult<EntryDisplayModel>.Ok(ToDisplay(e));
        }

        public async Task<ServiceResult<EntryDisplayModel>> GetByDateAsync(DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);
            var e = await _db.LogEntries.FirstOrDefaultAsync(x => x.EntryDate >= start && x.EntryDate < end);
            if (e == null) return ServiceResult<EntryDisplayModel>.Fail("Not found");
            return ServiceResult<EntryDisplayModel>.Ok(ToDisplay(e));
        }

        public async Task<ServiceResult<List<EntryDisplayModel>>> GetAllAsync()
        {
            var items = await _db.LogEntries
                .OrderByDescending(x => x.EntryDate)
                .ToListAsync();

            var mapped = items.Select(ToDisplay).ToList();
            return ServiceResult<List<EntryDisplayModel>>.Ok(mapped);
        }

        public async Task<ServiceResult<(List<EntryDisplayModel> Items, int TotalCount)>> SearchAsync(
            string? query,
            int pageIndex = 0,
            int pageSize = 20,
            DateTime? from = null,
            DateTime? to = null,
            int[]? moods = null,
            string[]? tags = null)
        {
            var q = _db.LogEntries.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                // use EF.Functions.Like for SQLite translation
                var like = $"%{query}%";
                q = q.Where(e => EF.Functions.Like(e.Title ?? "", like) || EF.Functions.Like(e.ContentHtml ?? "", like));
            }

            if (from.HasValue) q = q.Where(e => e.EntryDate >= from.Value.Date);
            if (to.HasValue) q = q.Where(e => e.EntryDate < to.Value.Date.AddDays(1)); // exclusive

            if (moods != null && moods.Length > 0) q = q.Where(e => moods.Contains(e.PrimaryMoodId));

            if (tags != null && tags.Length > 0)
            {
                foreach (var tag in tags)
                {
                    var pattern = $"%{tag}%";
                    q = q.Where(e => EF.Functions.Like(e.TagsCsv ?? "", pattern));
                }
            }

            var total = await q.CountAsync();
            var page = await q.OrderByDescending(e => e.EntryDate)
                              .Skip(pageIndex * pageSize)
                              .Take(pageSize)
                              .ToListAsync();

            var mapped = page.Select(ToDisplay).ToList();
            return ServiceResult<(List<EntryDisplayModel>, int)>.Ok((mapped, total));
        }

        public async Task<ServiceResult<EntryDisplayModel>> SaveAsync(EntryViewModel model)
        {
            try
            {
                if (model == null) return ServiceResult<EntryDisplayModel>.Fail("Invalid model");

                LogEntry entity;

                var entryDateStart = model.EntryDate.Date;
                var entryDateEnd = entryDateStart.AddDays(1);

                if (model.Id == 0)
                {
                    // New entry: ensure there's no existing entry for the same calendar day
                    var exists = await _db.LogEntries.AnyAsync(e => e.EntryDate >= entryDateStart && e.EntryDate < entryDateEnd);
                    if (exists)
                    {
                        return ServiceResult<EntryDisplayModel>.Fail("An entry for this date already exists. Edit or delete that entry before creating a new one.");
                    }

                    // New entry
                    entity = FromViewModel(model, null);
                    // CreatedAt and UpdatedAt have been set in FromViewModel for new entity
                    _db.LogEntries.Add(entity);
                }
                else
                {
                    // Update existing: ensure moving date doesn't collide with another entry
                    var existing = await _db.LogEntries.FindAsync(model.Id);
                    if (existing == null) return ServiceResult<EntryDisplayModel>.Fail("Not found");

                    var conflict = await _db.LogEntries
                        .Where(e => e.Id != model.Id && e.EntryDate >= entryDateStart && e.EntryDate < entryDateEnd)
                        .AnyAsync();
                    if (conflict)
                    {
                        return ServiceResult<EntryDisplayModel>.Fail("Another entry already exists for the selected date. Choose a different date or delete the other entry.");
                    }

                    entity = FromViewModel(model, existing);
                    _db.LogEntries.Update(entity);
                }

                try
                {
                    await _db.SaveChangesAsync();

                    // Refresh entity from DB to ensure all fields (e.g., Id) are populated
                    var saved = await _db.LogEntries.FindAsync(entity.Id);
                    return ServiceResult<EntryDisplayModel>.Ok(ToDisplay(saved!));
                }
                catch (DbUpdateException)
                {
                    // Database constraint violated (unique index on EntryDate) — attempt to provide a friendly response.
                    try
                    {
                        // First, try to clean obvious duplicates and report how many were removed.
                        var dedupeRes = await DeduplicateAsync();
                        int removed = 0;
                        if (dedupeRes.Success && dedupeRes.Data >= 0)
                        {
                            removed = dedupeRes.Data;
                        }

                        // Find existing entry for the conflicting date to provide an actionable id
                        var existing = await _db.LogEntries.FirstOrDefaultAsync(e => e.EntryDate >= entryDateStart && e.EntryDate < entryDateEnd);
                        if (existing != null)
                        {
                            var msg = removed > 0
                                ? $"Conflict: {removed} duplicate row(s) were removed. An entry for this date already exists (id:{existing.Id}). You can edit or delete that entry."
                                : $"An entry for this date already exists (id:{existing.Id}). You can edit or delete that entry.";
                            var info = new ConflictInfo(existing.Id, removed);
                            return ServiceResult<EntryDisplayModel>.Fail(msg, info);
                        }
                        else
                        {
                            var msg = removed > 0
                                ? $"Conflict detected and {removed} duplicate row(s) were removed, but an entry for this date still prevents creating a new one."
                                : "An entry for this date already exists; please edit or delete it before creating a new one.";
                            var info = new ConflictInfo(null, removed);
                            return ServiceResult<EntryDisplayModel>.Fail(msg, info);
                        }
                    }
                    catch
                    {
                        // If anything goes wrong while recovering, surface a generic friendly message
                        return ServiceResult<EntryDisplayModel>.Fail("Unable to save entry: an entry for this date already exists. Edit or delete the existing entry.");
                    }
                }
            }
            catch (Exception ex)
            {
                return ServiceResult<EntryDisplayModel>.Fail(ex.Message);
            }
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var e = await _db.LogEntries.FindAsync(id);
            if (e == null) return ServiceResult<bool>.Fail("Not found");
            _db.LogEntries.Remove(e);
            await _db.SaveChangesAsync();
            return ServiceResult<bool>.Ok(true);
        }

        // Deduplicate existing entries according to rules described by product:
        // 1) For groups with the same Id:
        //    - If CreatedAt values are equal, keep the row with the newest UpdatedAt.
        //    - If CreatedAt values differ, keep the row with the earliest CreatedAt.
        // 2) After resolving same-Id groups, for remaining rows that share the same CreatedAt,
        //    keep the row with the smallest Id and delete the rest.
        public async Task<ServiceResult<int>> DeduplicateAsync()
        {
            // Load all entries
            var entries = await _db.LogEntries.OrderBy(e => e.Id).ToListAsync();
            var toDelete = new List<LogEntry>();

            // Step 1: handle groups by Id (defensive: although Id should be unique)
            var byId = entries.GroupBy(e => e.Id).Where(g => g.Count() > 1);
            foreach (var g in byId)
            {
                var list = g.ToList();
                var createdDistinctCount = list.Select(x => x.CreatedAt).Distinct().Count();
                if (createdDistinctCount == 1)
                {
                    // same CreatedAt: keep newest UpdatedAt
                    var keep = list.OrderByDescending(x => x.UpdatedAt).First();
                    toDelete.AddRange(list.Where(x => x != keep));
                }
                else
                {
                    // different CreatedAt: keep earliest CreatedAt
                    var keep = list.OrderBy(x => x.CreatedAt).First();
                    toDelete.AddRange(list.Where(x => x != keep));
                }
            }

            // Step 2: among remaining (not already marked) rows, handle same CreatedAt groups
            var remaining = entries.Except(toDelete).ToList();
            var byCreated = remaining.GroupBy(e => e.CreatedAt).Where(g => g.Count() > 1);
            foreach (var g in byCreated)
            {
                var list = g.OrderBy(x => x.Id).ToList();
                var keep = list.First(); // smallest Id
                toDelete.AddRange(list.Where(x => x != keep));
            }

            if (toDelete.Count == 0)
            {
                return ServiceResult<int>.Ok(0);
            }

            _db.LogEntries.RemoveRange(toDelete);
            await _db.SaveChangesAsync();
            return ServiceResult<int>.Ok(toDelete.Count);
        }
    }
}
