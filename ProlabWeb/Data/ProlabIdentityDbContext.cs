using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ProlabWeb.Data;

public class ProlabIdentityDbContext : IdentityDbContext<ProlabIdentityUser>
{
    public ProlabIdentityDbContext(DbContextOptions<ProlabIdentityDbContext> options)
        : base(options)
    {
    }
}
