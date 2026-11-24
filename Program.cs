using AegisCore.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Регистрация ML-сервисов
builder.Services.AddSingleton<MLPipeline>();
builder.Services.AddSingleton<ComplianceCalculator>();

// Регистрация сервисов
builder.Services.AddSingleton<SystemCollectorService>();
builder.Services.AddSingleton<LocalScanAdapter>();
builder.Services.AddSingleton<JsonDataStore>();
builder.Services.AddSingleton<AuditService>();
builder.Services.AddSingleton<ScannerService>();
builder.Services.AddSingleton<FstecImportService>();
builder.Services.AddSingleton<AssessmentService>();
builder.Services.AddSingleton<ReportService>();
builder.Services.AddSingleton<MlClientService>();
builder.Services.AddSingleton<SystemMonitoringSettings>();
builder.Services.AddSingleton<SystemMonitoringBackgroundService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<SystemMonitoringBackgroundService>());

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();
builder.Services.AddHostedService<SystemMonitoringBackgroundService>();

var app = builder.Build();

// Правильная логика!
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Маршрут для Areas
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();