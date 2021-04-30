using System.Threading.Tasks;
using IdentitySample.Shared.Models;

namespace IdentitySample.BusinessLayer.Services
{
    public interface IIdentityService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);

        Task<AuthResponse> LoginAsync(LoginRequest request);
    }
}