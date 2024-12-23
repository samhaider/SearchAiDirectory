namespace SearchAiDirectory.Shared.Services;

public interface IAppLogServices
{
    Task AddAppLog(AppLog newLog);
    Task<IList<AppLog>> GetThisMonthAppLog();
}

public class AppLogServices(IDbContextFactory<ApplicationDataContext> dbContextFactory) : IAppLogServices
{
    public async Task AddAppLog(AppLog newLog)
    {
        using var db = dbContextFactory.CreateDbContext();
        await db.AppLogs.AddAsync(newLog);
        await db.SaveChangesAsync();
    }

    public async Task<IList<AppLog>> GetThisMonthAppLog()
    {
        using var db = dbContextFactory.CreateDbContext();
        return await db.AppLogs
            .Where(w => w.Created.Year == DateTime.UtcNow.Year && w.Created.Month == DateTime.UtcNow.Month)
            .ToListAsync();
    }
}