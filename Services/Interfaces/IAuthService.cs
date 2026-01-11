// Services/Interfaces/IAuthService.cs
using System.Threading.Tasks;
using DayLog.Common;

namespace DayLog.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> HasPinAsync();
        Task<ServiceResult<bool>> SetPinAsync(string pin);
        Task<ServiceResult<bool>> VerifyPinAsync(string pin);
        Task<ServiceResult<bool>> RemovePinAsync();
    }
}
