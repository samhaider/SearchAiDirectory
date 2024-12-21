namespace SearchAiDirectory.Shared.Services;

public interface IAppLogServices
{
    Task AddAppLog(AppLog newLog);
    Task<IList<AppLog>> GetThisMonthAppLog();
}

public class AppLogServices(ApplicationDataContext db) : IAppLogServices
{
    public async Task AddAppLog(AppLog newLog)
    {
        await db.AppLogs.AddAsync(newLog);
        await db.SaveChangesAsync();
    }

    public async Task<IList<AppLog>> GetThisMonthAppLog()
        => await db.AppLogs
        .Where(w => w.Created.Year == DateTime.UtcNow.Year && w.Created.Month == DateTime.UtcNow.Month)
        .ToListAsync();
}