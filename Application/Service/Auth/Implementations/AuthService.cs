using Application.DTOs.Auth;
using Application.DTOs.Auth.Login;
using Application.DTOs.Auth.Password;
using Application.DTOs.Auth.Register;
using Application.Repositories.Interfaces;
using Application.Service.Auth.Interfaces;
using Domain.Entites.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace Application.Service.Auth.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<Student> _studentRepo;
        private readonly IGenericRepository<RefreshToken> _refreshTokenRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IConfiguration config, IGenericRepository<User> userRepo, IHttpContextAccessor httpContextAccessor, IGenericRepository<RefreshToken> refreshTokenRepo, IGenericRepository<Student> studentRepo)
        {
            _config = config;
            _userRepo = userRepo;
            _httpContextAccessor = httpContextAccessor;
            _refreshTokenRepo = refreshTokenRepo;
            _studentRepo = studentRepo;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto input)
        {
            var user = await _userRepo.GetAll()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.Trim().ToLower() == input.Email.Trim().ToLower());

            if (user == null)   return null;

            var passwordHasher = new PasswordHasher<User>();
            var passowrdResult = passwordHasher.VerifyHashedPassword(user, user.Password, input.Password);

            if (passowrdResult == PasswordVerificationResult.Failed)   return null;
            
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            await _refreshTokenRepo.Insert(new RefreshToken
            {
                Token = refreshToken,
                UserId = user.UserId,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return new LoginResponseDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                RoleId = user.RoleId,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
        }

        public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto input)
        {
            var existingUser = await _userRepo.GetAll()
                .FirstOrDefaultAsync(u => u.Email!.Trim().ToLower() == input.Email.Trim().ToLower());

            if (existingUser != null)

                throw new InvalidOperationException("A user with this email already exists.");

            var passwordHasher = new PasswordHasher<User>();

            var newUser = new User
            {
                FullName = input.FullName,
                Email = input.Email,
                PhoneNumber = input.PhoneNumber,
                RoleId = 2 // 2 is RoleId for users or students
            };
            if(input.Password != input.ConfirmedPassword)
            {
                throw new InvalidOperationException("Password and Confirm Password do not match.");
            }
            newUser.Password = passwordHasher.HashPassword(newUser, input.Password);

            await _userRepo.Insert(newUser);
            await _userRepo.SaveChanges();

            var newStudent = new Student
            {
                UserId = newUser.UserId,
                BirthDate= input.DOB,
                UnivercityName= input.UniName,
            };

           
            await _studentRepo.Insert(newStudent);
            await _userRepo.SaveChanges(); 

            var accessToken = GenerateAccessToken(newUser);
            var refreshToken = GenerateRefreshToken();

            await _refreshTokenRepo.Insert(new RefreshToken
            {
                Token = refreshToken,
                UserId = newUser.UserId,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            await _refreshTokenRepo.SaveChanges();

            return new RegisterResponseDto
            {
                UserId = newUser.UserId,
                FullName = newUser.FullName,
                Email = newUser.Email,
                RoleId = newUser.RoleId,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task ResetPassword(ResetPasswordDto input)
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Convert.ToInt32(userIdClaim);
            var user = await _userRepo.GetById(userId);

            var passwordHasher = new PasswordHasher<User>();
            var passowrdResult = passwordHasher.VerifyHashedPassword(user, user.Password, input.OldPassword);

            if (passowrdResult == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException("Old password is incorrect.");
            }

            user.Password = passwordHasher.HashPassword(user, input.NewPassword);
            _userRepo.Update(user);
            await _userRepo.SaveChanges();
        }

        //=========================================================================================== For Token Generation

        public string GenerateAccessToken(User user)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.RoleName),
            };



            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                Issuer = jwtSection["Issuer"],
                Audience = jwtSection["Audience"],
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }
        public string GenerateRefreshToken()
        {
            var random = new byte[64];
            RandomNumberGenerator.Fill(random);
            return Convert.ToBase64String(random);
        }
        public async Task<string> RefreshToken(string refreshToken)
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Convert.ToInt32(userIdClaim);

            var storedToken = _refreshTokenRepo.GetAll()
                .FirstOrDefault(rt => rt.UserId == userId && rt.Token == refreshToken && rt.Expires > DateTime.UtcNow);
            if (storedToken == null)
            {
                throw new SecurityTokenException("Invalid refresh token.");
            }
            var user = await _userRepo.GetById(storedToken.UserId);
            return GenerateAccessToken(user);
        }
    }
}
