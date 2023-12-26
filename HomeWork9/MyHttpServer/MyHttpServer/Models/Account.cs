namespace MyHttpServer.Models;

public class Account
{
    public int Id { get; init; }
    public string? Login { get; set; }
    public string? Password { get; set; }
}