namespace Core.DTO;

public class RegisterRequest
{
    public required string Email { get; set; }
    public required string Pwd  { get; set; }
    public required string Username { get; set; }
}