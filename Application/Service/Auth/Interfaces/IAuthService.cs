using Application.DTOs.Auth.Login;
using Application.DTOs.Auth.Password;
using Application.DTOs.Auth.Register;
using Domain.Entites.Models;


namespace Application.Service.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto input);
        Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto input);
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        Task ResetPassword(ResetPasswordDto input);
        Task<string> RefreshToken(string refreshToken);
    }
}
