using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtService _jwtService;
    private readonly IRevokedTokenRepository _revokedTokenRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IPasswordResetRepository _passwordResetRepo;
    private readonly IEmailService _emailService;

    public AuthService(
        IUserRepository userRepo, IJwtService jwtService,
        IRevokedTokenRepository revokedTokenRepo, IRefreshTokenRepository refreshTokenRepo,
        IPasswordResetRepository passwordResetRepo, IEmailService emailService)
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
        _revokedTokenRepo = revokedTokenRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _passwordResetRepo = passwordResetRepo;
        _emailService = emailService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existing = await _userRepo.GetByEmailAsync(dto.Email.ToLower());
        if (existing != null)
            throw new ConflictException("Email already registered.");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Currency = "SAR",
            Role = UserRoles.User
        };

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        _ = _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);
        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email.ToLower());
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new BusinessException("Invalid email or password.");

        if (user.IsSuspended)
            throw new ForbiddenException("Your account has been suspended. Please contact support.");

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var token = await _refreshTokenRepo.GetByTokenAsync(refreshToken);

        if (token == null || token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
            throw new BusinessException("Invalid or expired refresh token.");

        var user = await _userRepo.GetByIdAsync(token.UserId)
            ?? throw new NotFoundException("User not found.");

        if (user.IsSuspended)
            throw new ForbiddenException("Your account has been suspended.");

        token.IsRevoked = true;
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        token.ReplacedByToken = newRefreshToken;

        await _refreshTokenRepo.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = _jwtService.GetRefreshTokenExpiry()
        });
        await _refreshTokenRepo.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = _jwtService.GenerateAccessToken(user),
            RefreshToken = newRefreshToken,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Currency = user.Currency,
            AccessTokenExpiresAt = _jwtService.GetAccessTokenExpiry(),
            RefreshTokenExpiresAt = _jwtService.GetRefreshTokenExpiry()
        };
    }

    public async Task LogoutAsync(string accessToken, string refreshToken)
    {
        await _revokedTokenRepo.AddAsync(new RevokedToken
        {
            Token = accessToken,
            ExpiresAt = _jwtService.GetAccessTokenExpiry()
        });

        var token = await _refreshTokenRepo.GetByTokenAsync(refreshToken);
        if (token != null)
        {
            token.IsRevoked = true;
            await _refreshTokenRepo.SaveChangesAsync();
        }

        await _revokedTokenRepo.CleanupExpiredAsync();
        await _revokedTokenRepo.SaveChangesAsync();
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _userRepo.GetByEmailAsync(email.ToLower());
        if (user == null) return; // Prevent email enumeration

        var resetToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        await _passwordResetRepo.AddAsync(new PasswordResetToken
        {
            UserId = user.Id,
            Token = resetToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });
        await _passwordResetRepo.SaveChangesAsync();

        await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, resetToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmNewPassword)
            throw new BusinessException("Passwords do not match.");

        var token = await _passwordResetRepo.GetByTokenAsync(dto.Token);
        if (token == null || token.IsUsed || token.ExpiresAt < DateTime.UtcNow)
            throw new BusinessException("Invalid or expired reset token.");

        var user = await _userRepo.GetByIdAsync(token.UserId)
            ?? throw new NotFoundException("User not found.");

        if (user.Email.ToLower() != dto.Email.ToLower())
            throw new BusinessException("Invalid reset token.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        token.IsUsed = true;

        await _userRepo.SaveChangesAsync();
        await _passwordResetRepo.SaveChangesAsync();
    }

    private async Task<AuthResponseDto> BuildAuthResponseAsync(User user)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        await _refreshTokenRepo.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = _jwtService.GetRefreshTokenExpiry()
        });
        await _refreshTokenRepo.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Currency = user.Currency,
            AccessTokenExpiresAt = _jwtService.GetAccessTokenExpiry(),
            RefreshTokenExpiresAt = _jwtService.GetRefreshTokenExpiry()
        };
    }
}
