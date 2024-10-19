using Microsoft.AspNetCore.Identity;

public interface IUserService
{
    Task<IdentityResult> RegisterAsync(UserRegisterDto user);
    Task<LoginResponseDto> LoginAsync(LoginDto user);
    Task<IdentityResult> ChangePasswordAsync(ChangePasswordDto user);
    Task<HttpResponseMessage> ForgotPasswordAsync(ForgotPasswordDto user);
    Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto user);
    Task<string> GeneratePasswordResetTokenAsync(string email);
}