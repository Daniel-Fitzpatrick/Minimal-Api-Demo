namespace Minimal.Api.Common.Model;

public class User
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Id { get; set; } = default!;
    public string? Skills { get; set; }
    public int YearsOfExperience { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Email { get; set; } = default!;
}
