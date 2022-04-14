namespace KomTracker.API.Models.User;

public class UserModel
{
    public int AthleteId { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Avatar { get; set; } = default!;
    public string? Email { get; set; }
}
