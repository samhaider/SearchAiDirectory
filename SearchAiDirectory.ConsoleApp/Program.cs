using Microsoft.EntityFrameworkCore;
using SearchAiDirectory.Shared.Data;

namespace SearchAiDirectory.ConsoleApp;

public class Program
{
    public static async Task Main()
    {
        await ScrapeWebsite.Scrape();
    }
}

public static class BgUtil
{
    public static ServiceProvider GetServices()
    {
        return new ServiceCollection()
            .AddDbContextFactory<ApplicationDataContext>(options => options.UseSqlServer("Server=tcp:searchaidirectory.database.windows.net,1433;Initial Catalog=sad_db;Persist Security Info=False;User ID=sadDBuser;Password=_2PSP2EE&R@?r2V1#hr;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
            .AddTransient<IToolService, ToolService>()
            .BuildServiceProvider();
    }
}