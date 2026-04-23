using PizzeriaAPI.DTOs.Auth;

namespace PizzeriaAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    }
}
