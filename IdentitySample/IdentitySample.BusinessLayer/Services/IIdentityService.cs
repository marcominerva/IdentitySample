using System.Threading.Tasks;
using IdentitySample.Shared.Models;

namespace IdentitySample.BusinessLayer.Services
{
    public interface IIdentityService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }
}