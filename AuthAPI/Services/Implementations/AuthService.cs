using AuthAPI.Data;
using AuthAPI.Models;
using AuthAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;
using Shared.Helpers.Interfaces;

namespace AuthAPI.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtTokenGenerator _jwtGenerator;

        public AuthService(AppDbContext context, IJwtTokenGenerator jwtGenerator)
        {
            _context = context;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return new AuthResponseDto { Success = false, Message = "Email already exists." };

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == request.RoleName)
                       ?? await _context.Roles.FirstAsync(r => r.Name == "User");

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                RoleId = role.Id,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password) // Hashing!
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new AuthResponseDto { Success = true, Message = "User registered successfully." };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return new AuthResponseDto { Success = false, Message = "Invalid username or password." };

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                RoleName = user.Role.Name,
                IsActive = user.IsActive
            };

            var token = _jwtGenerator.GenerateToken(userDto);

            return new AuthResponseDto
            {
                Success = true,
                Token = token,
                User = userDto,
                Message = "Login successful."
            };
        }
    }
}