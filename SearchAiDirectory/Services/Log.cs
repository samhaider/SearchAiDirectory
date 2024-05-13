namespace SearchAiDirectory.Services;

public interface ILogServices
{
    Task Log(Log newLog);
    Task<IList<Log>> GetThisMonthLog();
}

public class LogServices(ApplicationDataContext db) : ILogServices
{
    public async Task Log(Log newLog)
    {
        await db.Logs.AddAsync(newLog);
        await db.SaveChangesAsync();
    }

    public async Task<IList<Log>> GetThisMonthLog()
        => await db.Logs
        .Where(w => w.Created.Year == DateTime.UtcNow.Year && w.Created.Month == DateTime.UtcNow.Month)
        .ToListAsync();
}