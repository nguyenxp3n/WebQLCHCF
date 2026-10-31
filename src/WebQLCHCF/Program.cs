using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Data;
using WebQLCHCF.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IChamCongService, ChamCongService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
var app = builder.Build();

await WarmUpEfCoreAsync(app);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapControllerRoute(name: "controller-default", pattern: "{controller}/{action=Index}/{id?}");
app.MapControllerRoute(name: "default", pattern: "{controller=Account}/{action=Login}/{id?}");
app.Run();

static async Task WarmUpEfCoreAsync(WebApplication app)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        _ = context.Model.GetEntityTypes().Count();
        await context.Database.CanConnectAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Không thể warm-up EF Core.");
    }
}
