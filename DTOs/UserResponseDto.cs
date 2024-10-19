using Microsoft.AspNetCore.Identity;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
}

public class LoginResponseDto
{
    public SignInResult Result {get;set;}
    public string Token {get;set;}
}