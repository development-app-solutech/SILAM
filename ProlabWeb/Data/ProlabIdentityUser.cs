using Microsoft.AspNetCore.Identity;

namespace ProlabWeb.Data
{
    public class ProlabIdentityUser : IdentityUser
    {
        public bool MustChangePassword { get; set; }
        public string? TemporaryPassword { get; set; }
        public string? EmployeeId { get; set; }
    }
}
