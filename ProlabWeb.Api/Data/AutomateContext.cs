using Microsoft.EntityFrameworkCore;
using ProlabWeb.Api.Models;

namespace ProlabWeb.Api.Data;

public class AutomateContext : DbContext
{
    public AutomateContext(DbContextOptions<AutomateContext> options) : base(options)
    {
    }

    public DbSet<LabResult> LabResults { get; set; }
    public DbSet<TestResult> TestResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ... existing configuration
    }
}