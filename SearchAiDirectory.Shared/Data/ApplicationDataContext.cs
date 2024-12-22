namespace SearchAiDirectory.Shared.Data;

public class ApplicationDataContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserCode> UserCodes => Set<UserCode>();
    
    public DbSet<Tool> Tools => Set<Tool>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Embedding> Embeddings => Set<Embedding>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Comment> Comments => Set<Comment>();


    public DbSet<AppLog> AppLogs => Set<AppLog>();
}
