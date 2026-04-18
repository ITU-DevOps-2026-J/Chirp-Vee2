using Core.Interfaces;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Routing;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Prometheus;
using System.Diagnostics;
using Web.Monitoring;

namespace Web;

public class Program
{
    private static readonly Histogram RequestDuration = Metrics.CreateHistogram(
        "chirp_http_request_duration_seconds",
        "HTTP request duration grouped by method, path and status code.",
        new HistogramConfiguration
        {
            LabelNames = ["method", "path", "status_code"],
            Buckets = Histogram.ExponentialBuckets(0.005, 2, 12)
        });

    private static readonly Histogram FrontPageDuration = Metrics.CreateHistogram(
        "chirp_front_page_duration_seconds",
        "Front page GET request duration in seconds.",
        new HistogramConfiguration
        {
            LabelNames = ["status_code"],
            Buckets = Histogram.ExponentialBuckets(0.005, 2, 12)
        });

    private static string? GetMetricPath(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/monitoring/metrics"))
        {
            return null;
        }

        if (context.GetEndpoint() is RouteEndpoint routeEndpoint &&
            !string.IsNullOrWhiteSpace(routeEndpoint.RoutePattern.RawText))
        {
            return routeEndpoint.RoutePattern.RawText;
        }

        return NormalizePath(context.Request.Path);
    }

    private static string NormalizePath(PathString path)
    {
        if (!path.HasValue || string.IsNullOrWhiteSpace(path.Value))
        {
            return "unknown";
        }

        if (path == PathString.FromUriComponent("/"))
        {
            return "/";
        }

        var segments = path.Value!
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizePathSegment);

        return "/" + string.Join("/", segments);
    }

    private static string NormalizePathSegment(string segment)
    {
        if (Guid.TryParse(segment, out _))
        {
            return "{id}";
        }

        if (segment.All(char.IsDigit))
        {
            return "{id}";
        }

        if (segment.Length > 32)
        {
            return "{id}";
        }

        return segment;
    }

    //test
    /// <summary>
    /// Main Program to run
    /// </summary>
    /// <param name="args">Optional arguments</param>
    public static void Main(string[] args)
    {
        var app = BuildWebApplication(args);
        
        //Initialise Database
        if (!app.Environment.IsEnvironment("Testing"))
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        
            if (context.Database.IsRelational())
            {        
                context.Database.Migrate();
            }
        }

        app.Run();
    }

    /// <summary>
    /// Build a WebApplication depending on the runtime environment
    /// </summary>
    /// <param name="args">Optional arguments</param>
    /// <param name="environment">Optional environment to specify</param>
    /// <returns>Webapplication</returns>
    /// <exception cref="DirectoryNotFoundException">Cannot find Web directory</exception>
    public static WebApplication BuildWebApplication(string[]? args = null, string? environment = null)
    {
        var baseDir = AppContext.BaseDirectory;
        string webProjectPath;

        // Determine the correct content root path
        if (environment == "Testing")
        {
            var currentDir = new DirectoryInfo(baseDir);
            DirectoryInfo? solutionRoot = null;

            while (currentDir != null)
            {
                if (Directory.GetFiles(currentDir.FullName, "*.sln").Length > 0)
                {
                    solutionRoot = currentDir;
                    break;
                }

                var webCsprojPath = Path.Combine(currentDir.FullName, "src", "Web", "Web.csproj");
                if (File.Exists(webCsprojPath))
                {
                    solutionRoot = currentDir;
                    break;
                }

                currentDir = currentDir.Parent;
            }

            if (solutionRoot != null)
            {
                webProjectPath = Path.Combine(solutionRoot.FullName, "src", "Web");
                Console.WriteLine($"[Testing] Found Web project at: {webProjectPath}");
            }
            else
            {
                webProjectPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", "src", "Web"));
                Console.WriteLine($"[Testing] Using fallback path: {webProjectPath}");
            }

            if (!Directory.Exists(webProjectPath))
            {
                throw new DirectoryNotFoundException($"Web project directory not found at: {webProjectPath}");
            }

            var areasPath = Path.Combine(webProjectPath, "Areas");
            var pagesPath = Path.Combine(webProjectPath, "Pages");
            Console.WriteLine($"[Testing] Areas folder exists: {Directory.Exists(areasPath)}");
            Console.WriteLine($"[Testing] Pages folder exists: {Directory.Exists(pagesPath)}");
        }
        else
        {
            var currentDir = new DirectoryInfo(baseDir);
            DirectoryInfo? foundDir = null;

            while (currentDir != null)
            {
                var webCsprojDirect = Path.Combine(currentDir.FullName, "Web.csproj");
                var webCsprojInSrc = Path.Combine(currentDir.FullName, "src", "Web", "Web.csproj");

                if (File.Exists(webCsprojDirect))
                {
                    foundDir = currentDir;
                    break;
                }

                if (File.Exists(webCsprojInSrc))
                {
                    foundDir = new DirectoryInfo(Path.Combine(currentDir.FullName, "src", "Web"));
                    break;
                }

                currentDir = currentDir.Parent;
            }

            webProjectPath = foundDir?.FullName ?? baseDir;
        }

        string env;
        if (!args.IsNullOrEmpty())
        {
            env = args.Contains("--environment=Testing") ? "Testing" :
                args.Contains("--environment=Development") ? "Development" :
                args.Contains("--environment=Production") ? "Production" :
                environment ?? Environments.Development;
        }
        else env = environment ?? Environments.Development;
        
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
        {
            Args = args ?? Array.Empty<string>(),
            EnvironmentName = env,
            ContentRootPath = webProjectPath,
            WebRootPath = Path.Combine(webProjectPath, "wwwroot")
        });

        builder.Services.AddSession();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<DbCommandInterceptor, DbCommandMetricsInterceptor>();
        builder.Services.AddHostedService<SystemMetricsCollector>();



        // Configure database based on environment
        if (builder.Environment.IsEnvironment("Testing"))
        {
            builder.Services.AddDbContext<ChatDbContext>((serviceProvider, options) =>
                options
                    .UseSqlite("DataSource=TestDb;Mode=Memory;Cache=Shared")
                    .AddInterceptors(serviceProvider.GetRequiredService<DbCommandInterceptor>()));
        }
        else
        {
            string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.EnableDynamicJson();
            var dataSource = dataSourceBuilder.Build();

            builder.Services.AddDbContext<ChatDbContext>((serviceProvider, options) =>
                options
                    .UseNpgsql(dataSource)
                    .AddInterceptors(serviceProvider.GetRequiredService<DbCommandInterceptor>()));
        }

        // CRITICAL FIX: Use AddIdentity instead of AddDefaultIdentity
        // AddDefaultIdentity includes AddDefaultUI() which forces the RCL and prevents scaffolded pages from working
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
        {
            options.SignIn.RequireConfirmedAccount = true;
            // Add any other identity options you need
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;
        })
        .AddEntityFrameworkStores<ChatDbContext>()
        .AddDefaultTokenProviders();  // Required for email confirmation, password reset, etc.

        builder.Services.AddTransient<IEmailSender, NoOpEmailSender>();
        
        // Load User Secrets
        if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
        {
            builder.Configuration.AddUserSecrets<Program>(optional: true);
        }


        
        // Configure Razor Pages with runtime compilation
        var razorPagesBuilder = builder.Services.AddRazorPages(options =>
        {
            // Configure Razor Pages to allow Areas (required for Identity)
            options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
            options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
        });

        // Enable runtime compilation for Testing and Development
        if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
        {
            razorPagesBuilder.AddRazorRuntimeCompilation();
        }

        // Explicitly configure MVC to use the Web assembly
        builder.Services.AddMvc()
            .AddApplicationPart(typeof(Program).Assembly);

        builder.Services.AddScoped<ICheepService, CheepService>();
        builder.Services.AddScoped<ICheepRepository, CheepRepository>();
        builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
        builder.Services.AddScoped<ILatestsRepository, LatestsRepository>();
        
        builder.Services.AddControllers();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

            options.LoginPath = "/Identity/Account/Login";
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            options.SlidingExpiration = true;
        });
        builder.WebHost.UseUrls("http://0.0.0.0:8080");

        var app = builder.Build();

        // Log important paths in Testing environment
        if (app.Environment.IsEnvironment("Testing"))
        {
            Console.WriteLine($"[Testing] ContentRootPath: {app.Environment.ContentRootPath}");
            Console.WriteLine($"[Testing] WebRootPath: {app.Environment.WebRootPath}");
        }

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsEnvironment("Testing") && !app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
        {
            app.UseHttpsRedirection();
        }

        app.UseStaticFiles();

        app.UseRouting();
        app.Use(async (context, next) =>
        {
            var requestPath = GetMetricPath(context);
            var pathLabel = requestPath ?? "unknown";

            RequestMetricsContext.Path = pathLabel;
            var timer = Stopwatch.StartNew();

            try
            {
                await next();
            }
            finally
            {
                timer.Stop();
                var statusCode = context.Response.StatusCode.ToString();

                if (requestPath is not null)
                {
                    RequestDuration
                        .WithLabels(context.Request.Method, pathLabel, statusCode)
                        .Observe(timer.Elapsed.TotalSeconds);

                    if (context.Request.Method == HttpMethods.Get && pathLabel == "/")
                    {
                        FrontPageDuration.WithLabels(statusCode).Observe(timer.Elapsed.TotalSeconds);
                    }
                }

                RequestMetricsContext.Path = "unknown";
            }
        });
        app.MapControllers();
        app.MapMetrics("/monitoring/metrics");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseSession();
        app.MapRazorPages();

        return app;
    }
}
