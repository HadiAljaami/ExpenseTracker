using Application.DTOs.Users;
using Application.Interfaces;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;

    public UserService(IUserRepository userRepo) => _userRepo = userRepo;

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");
        return MapToDto(user);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        user.FullName = dto.FullName;
        user.Currency = dto.Currency;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepo.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        if (dto.NewPassword != dto.ConfirmNewPassword)
            throw new InvalidOperationException("New password and confirmation do not match.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepo.SaveChangesAsync();
    }

    public async Task DeleteAccountAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");
        await _userRepo.DeleteAsync(user);
        await _userRepo.SaveChangesAsync();
    }

    private static UserProfileDto MapToDto(Domain.Entities.User u) => new()
    {
        Id = u.Id, FullName = u.FullName, Email = u.Email,
        Role = u.Role, Currency = u.Currency, CreatedAt = u.CreatedAt
    };
}
