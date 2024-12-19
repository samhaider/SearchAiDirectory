namespace SearchAiDirectory.Shared.Data;

public class ApplicationDataContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserCode> UserCodes { get; set; }

    public DbSet<Tool> Tools => Set<Tool>();
    public DbSet<ToolCategory> ToolCategories => Set<ToolCategory>();

    public DbSet<AppLog> AppLogs => Set<AppLog>();
}
