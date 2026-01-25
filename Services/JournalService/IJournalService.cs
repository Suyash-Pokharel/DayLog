// Services/Interfaces/IJournalService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DayLog.Common;
using DayLog.Models;

namespace DayLog.Services.JournalService
{
    public interface IJournalService
    {
        Task<ServiceResult<EntryDisplayModel>> GetByIdAsync(int id);
        Task<ServiceResult<EntryDisplayModel>> GetByDateAsync(DateTime date);
        Task<ServiceResult<List<EntryDisplayModel>>> GetAllAsync();
        Task<ServiceResult<(List<EntryDisplayModel> Items, int TotalCount)>> SearchAsync(
            string? query,
            int pageIndex = 0,
            int pageSize = 20,
            DateTime? from = null,
            DateTime? to = null,
            int[]? moods = null,
            string[]? tags = null);

        Task<ServiceResult<EntryDisplayModel>> SaveAsync(EntryViewModel model);
        Task<ServiceResult<bool>> DeleteAsync(int id);
        Task<ServiceResult<int>> DeduplicateAsync();
    }
}
