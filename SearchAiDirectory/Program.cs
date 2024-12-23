namespace SearchAiDirectory;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var config = builder.Configuration;

        // Add services to the container.
        builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

        builder.Services.AddMemoryCache();
        builder.Services.AddOutputCache(options =>
            options.AddPolicy("GlobalCachePolicy", builder =>
            {
                builder.Tag("global-cache");
                builder.SetVaryByQuery(["*"]);
                builder.Expire(TimeSpan.FromSeconds(60));
            }));

        builder.Services.AddDistributedMemoryCache();
        builder.Services.Configure<CookieTempDataProviderOptions>(options =>
        {
            options.Cookie.Name = "TempDataCookie";
            options.Cookie.Expiration = TimeSpan.FromMinutes(30);
            options.Cookie.IsEssential = true;
        });
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.MaxAge = TimeSpan.FromMinutes(32);
            options.Cookie.IsEssential = true;
        });
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = CookieAuthenticationDefaults.AuthenticationScheme;
                options.Cookie.IsEssential = true;
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
                options.AccessDeniedPath = "/login";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
            });
        builder.Services.AddCors(options => options.AddDefaultPolicy(builder =>
        {
            builder
            .WithOrigins(config["WebsiteURL"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        }));

        //Adding services to the container
        builder.Services.AddHttpContextAccessor();


        builder.Services.AddDbContextFactory<ApplicationDataContext>(options =>
        {
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("Default"),
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(1), errorNumbersToAdd: null);// Retry on failure for transient faults
                    sqlOptions.MaxBatchSize(100);// Max batch size for better performance with bulk operations
                    sqlOptions.CommandTimeout(30); // Command timeout for long-running queries in seconds
                });

            options.UseLazyLoadingProxies(false);// Disable lazy loading to avoid N+1 query issues
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);// Globally disable tracking for better read performance on large queries
        });

        builder.Services.AddTransient<IUserService, UserService>();
        builder.Services.AddTransient<IUserAuthenticator, UserAuthenticator>();
        builder.Services.AddTransient<IEmbeddingService, EmbeddingService>();
        builder.Services.AddTransient<IToolService, ToolService>();
        builder.Services.AddTransient<ICategoryService, CategoryService>();
        builder.Services.AddTransient<ILikeService, LikeService>();
        builder.Services.AddTransient<ICommentService, CommentService>();
        builder.Services.AddTransient<INewsService, NewsService>();
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();
        else app.UseMiddleware<ErrorHandlerMiddleware>();

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseOutputCache();
        app.UseCookiePolicy();
        app.UseSession();
        app.UseCors();

        app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapAreaControllerRoute(name: "Website", areaName: "Website", pattern: "Website/{controller=Home}/{action=Index}/{id?}");
        app.Run();
    }
}