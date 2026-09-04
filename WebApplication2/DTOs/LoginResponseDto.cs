namespace WebApplication2.DTOs;
public class LoginResponseDto
{
    public string Token { get; set; } = "";
    public int UserId { get; set; }
    public string Role { get; set; } = "";
}
