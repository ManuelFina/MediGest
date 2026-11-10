
namespace MediGest.Web.ViewModels.Users;

public class UserListViewModel
{
    public IEnumerable<UserViewModel> Users { get; set; } =
        new List<UserViewModel>();

    public string? SearchTerm { get; set; }

    public int TotalUsers { get; set; }

    public int ActiveUsers { get; set; }

    public int InactiveUsers { get; set; }
}

public class UserViewModel
{
    public string Id { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
