namespace SearchAiDirectory.Function;

public class Program
{
    private static void Main()
    {
        var builder = FunctionsApplication.CreateBuilder(null);
        builder.ConfigureFunctionsWebApplication();

        var connectionString = "Server=tcp:searchaidirectory.database.windows.net,1433;Initial Catalog=sad_db;Persist Security Info=False;User ID=sadDBuser;Password=_2PSP2EE&R@?r2V1#hr;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        // Register DbContext  
        builder.Services.AddDbContextFactory<ApplicationDataContext>(options =>
            options.UseSqlServer(connectionString,
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                }));

        // Register other services  
        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<INewsService, NewsService>();

        builder.Build().Run();
    }
}
