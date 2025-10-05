using barefoot_travel.Common;
using barefoot_travel.Common.Exceptions;
using barefoot_travel.DTOs.Auth;
using barefoot_travel.Models;
using barefoot_travel.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace barefoot_travel.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IConfiguration _configuration;

        public AuthService(
            IAccountRepository accountRepository,
            IConfiguration configuration)
        {
            _accountRepository = accountRepository;
            _configuration = configuration;
        }

        public async Task<TokenDto> LoginAsync(string username, string password)
        {
            // Validation in Service layer
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new BadRequestException("Username is required");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new BadRequestException("Password is required");
            }

            if (username.Length > 50)
            {
                throw new BadRequestException("Username cannot exceed 50 characters");
            }

            if (password.Length > 255)
            {
                throw new BadRequestException("Password cannot exceed 255 characters");
            }

            var user = await _accountRepository.GetUserByUsernameAsync(username);
            if (user == null)
            {
                throw new UnauthorizedException("Invalid credentials");
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                throw new UnauthorizedException("Invalid credentials");
            }

            // Get role name from RoleId
            var roleName = GetRoleName(user.RoleId);
            if (string.IsNullOrEmpty(roleName))
            {
                throw new UnauthorizedException("User has no assigned role");
            }

            var accessToken = GenerateAccessToken(user.Id, new List<string> { roleName });
            var refreshToken = GenerateRefreshToken();

            // Store refresh token in memory or database (simplified approach)
            // In production, you might want to store this in a database or cache

            return new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<TokenDto> RefreshTokenAsync(string refreshToken)
        {
            // Validation in Service layer
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new BadRequestException("Refresh token is required");
            }

            // Simplified refresh token validation
            // In production, you might want to store and validate refresh tokens in database
            throw new NotImplementedException("Refresh token functionality needs to be implemented based on your requirements");
        }

        public async Task LogoutAsync(string refreshToken)
        {
            // Validation in Service layer
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new BadRequestException("Refresh token is required");
            }

            // Simplified logout
            // In production, you might want to invalidate refresh tokens in database
            await Task.CompletedTask;
        }

        private string GenerateAccessToken(int userId, List<string> roles)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
            var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, userId.ToString()),
                new Claim("sub", userId.ToString())
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString() + Guid.NewGuid().ToString();
        }

        public async Task<ApiResponse> RegisterAsync(RegisterDto dto)
        {
            // Validation in Service layer
            if (string.IsNullOrWhiteSpace(dto.Username))
            {
                throw new BadRequestException("Username is required");
            }

            if (string.IsNullOrWhiteSpace(dto.FullName))
            {
                throw new BadRequestException("Full name is required");
            }

            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                throw new BadRequestException("Password is required");
            }

            if (dto.Username.Length > 50)
            {
                throw new BadRequestException("Username cannot exceed 50 characters");
            }

            if (dto.FullName.Length > 255)
            {
                throw new BadRequestException("Full name cannot exceed 255 characters");
            }

            if (dto.Password.Length > 255)
            {
                throw new BadRequestException("Password cannot exceed 255 characters");
            }

            if (!string.IsNullOrEmpty(dto.Email) && dto.Email.Length > 255)
            {
                throw new BadRequestException("Email cannot exceed 255 characters");
            }

            if (!string.IsNullOrEmpty(dto.Phone) && dto.Phone.Length > 20)
            {
                throw new BadRequestException("Phone cannot exceed 20 characters");
            }

            if (dto.RoleId <= 0)
            {
                throw new BadRequestException("Invalid role ID");
            }

            // Check if username already exists
            var existingUser = await _accountRepository.GetUserByUsernameAsync(dto.Username);
            if (existingUser != null)
            {
                throw new BadRequestException("Username already exists");
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // Create new account
            var account = new Account
            {
                Username = dto.Username,
                FullName = dto.FullName,
                PasswordHash = passwordHash,
                Email = dto.Email,
                Phone = dto.Phone,
                RoleId = dto.RoleId,
                CreatedTime = DateTime.UtcNow,
                Active = true
            };

            var createdAccount = await _accountRepository.CreateAsync(account);

            // Get role name for response
            var roleName = GetRoleName(createdAccount.RoleId);

            var userProfile = new UserProfileDto
            {
                UserId = createdAccount.Id,
                Username = createdAccount.Username,
                Name = createdAccount.FullName,
                Email = createdAccount.Email,
                Phone = createdAccount.Phone,
                Roles = new List<string> { roleName }
            };

            return new ApiResponse(true, "Account created successfully", userProfile);
        }

        private string GetRoleName(int roleId)
        {
            // Map RoleId to RoleName based on Database-schema.md
            // Id = 1: Admin, Id = 2: User
            return roleId switch
            {
                1 => "Admin",
                2 => "User",
                _ => string.Empty
            };
        }
    }
}
