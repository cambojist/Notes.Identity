using IdentityModel;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Notes.Identity;
using Notes.Identity.Data;
using Notes.Identity.Models;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var connectionString = configuration.GetValue<string>("DbConnection");

builder.Services.AddDbContext<AuthDbContext>(opt =>
{
    opt.UseSqlite(connectionString);
});
builder.Services.AddIdentity<AppUser, IdentityRole>(cfg =>
{
    cfg.Password.RequiredLength = 4;
    cfg.Password.RequireDigit = false;
    cfg.Password.RequireNonAlphanumeric = false;
    cfg.Password.RequireUppercase = false;
})
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddIdentityServer()
    .AddAspNetIdentity<AppUser>()
    .AddInMemoryApiResources(Configuration.ApiResources)
    .AddInMemoryIdentityResources(Configuration.IdentityResources)
    .AddInMemoryApiScopes(Configuration.ApiScopes)
    .AddInMemoryClients(Configuration.Clients)
    .AddDeveloperSigningCredential();

builder.Services.ConfigureApplicationCookie(cfg =>
{
    cfg.Cookie.Name = "Notes.Identity.Cookie";
    cfg.LoginPath = "/Auth/Login";
    cfg.LogoutPath = "/Auth/Logout";
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    try
    {
        var context = serviceProvider.GetRequiredService<AuthDbContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception e)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(e, "An error occurred while app initialization");
    }
}

app.UseStaticFiles(new StaticFileOptions
{
FileProvider = new PhysicalFileProvider(
    Path.Combine(builder.Environment.ContentRootPath, "Styles")),
    RequestPath = "/styles"
});
app.UseRouting();
app.UseIdentityServer();
app.MapDefaultControllerRoute();
app.Run();