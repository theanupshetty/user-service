using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Identity;

public class UserService : IUserService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtTokenService _jwtTokenService;

    private readonly HttpClient _httpClient;

    public UserService(UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    JwtTokenService jwtTokenService,
    IConfiguration configuration
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
        _httpClient = new HttpClient{BaseAddress = new Uri(configuration["EmailService:Url"])};
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto model)
    {
        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            var token = _jwtTokenService.GenerateToken(user);
            return new LoginResponseDto { Result = result, Token = token };
        }
        throw new Exception("Invalid login attempt.");
    }

    public async Task<IdentityResult> RegisterAsync(UserRegisterDto model)
    {
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };
        var result = await _userManager.CreateAsync(user, model.Password);
        return result;
    }

    public async Task<IdentityResult> ChangePasswordAsync(ChangePasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        return result;
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            throw new Exception("User not found.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return token;
    }

    public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        return result;
    }

    public async Task<HttpResponseMessage> ForgotPasswordAsync(ForgotPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
        {
            throw new Exception("User not found.");
        }
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var emailData = new
        {
            To = user.Email,
            TemplateName = "ForgotPassword",
            TemplateData = new
            {
                Name = user.FirstName,
                ResetLink = _configuration["Admin:Url"] + "/reset-password/" + HttpUtility.UrlEncode(user.Email) + "/" + HttpUtility.UrlEncode(token)
            },
            Subject = "Password Reset Request",
            Content = "",
            IsBodyHtml = true
        };

        var content = new StringContent(JsonSerializer.Serialize(emailData), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/email/send-email", content);

        return response;
        
    }
}