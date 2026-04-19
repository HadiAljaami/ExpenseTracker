using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtService _jwtService;
    private readonly IRevokedTokenRepository _revokedTokenRepo;

    public AuthService(IUserRepository userRepo, IJwtService jwtService, IRevokedTokenRepository revokedTokenRepo)
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
        _revokedTokenRepo = revokedTokenRepo;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existing = await _userRepo.GetByEmailAsync(dto.Email.ToLower());
        if (existing != null)
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Currency = "SAR"
        };

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = _jwtService.GenerateToken(user),
            FullName = user.FullName,
            Email = user.Email,
            ExpiresAt = _jwtService.GetExpiryDate()
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email.ToLower());
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return new AuthResponseDto
        {
            Token = _jwtService.GenerateToken(user),
            FullName = user.FullName,
            Email = user.Email,
            ExpiresAt = _jwtService.GetExpiryDate()
        };
    }

    public async Task LogoutAsync(string token)
    {
        await _revokedTokenRepo.AddAsync(new RevokedToken
        {
            Token = token,
            ExpiresAt = _jwtService.GetExpiryDate()
        });
        await _revokedTokenRepo.CleanupExpiredAsync();
        await _revokedTokenRepo.SaveChangesAsync();
    }
}
