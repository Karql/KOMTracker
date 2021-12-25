namespace KomTracker.WEB.Models.User;

public class UserModel
{
    public int AthleteId { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Avatar { get; set; } = default!;
}
