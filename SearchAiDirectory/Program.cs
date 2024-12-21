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

        builder.Services.AddDbContext<ApplicationDataContext>(options =>
            options.UseSqlServer(config.GetConnectionString("Default"),
            sqlServerOptionsAction: sqlOptions => sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(1), errorNumbersToAdd: null))
            .UseLazyLoadingProxies(false)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        builder.Services.AddTransient<IUserService, UserService>();
        builder.Services.AddTransient<IToolService, ToolService>();
        builder.Services.AddTransient<IEmbeddingService, EmbeddingService>();
        builder.Services.AddSingleton(new JWTokenService(builder.Configuration["WebsiteURL"]));
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