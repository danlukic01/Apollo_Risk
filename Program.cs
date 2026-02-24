using AACS.Risk.Web.Components;
using AACS.Risk.Web.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Configure logging explicitly
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Radzen services
builder.Services.AddRadzenComponents();

// Database connection factory for RiskService
builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

// Register RiskService with database connection
builder.Services.AddScoped<IRiskService, RiskService>();

// Register ChatService with OpenAI integration
builder.Services.AddHttpClient<IChatService, ChatService>();

// ============================================================================
// AZURE AD AUTHENTICATION
// ============================================================================
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddAuthorization(options =>
{
    // Policy for AD admin group
    options.AddPolicy("AdminsOnly", policy =>
        policy.RequireRole("RiskMgmtApp-Admins-Dev"));

    // Require authentication for all pages by default
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Add cascading authentication state for Blazor
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Log startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("=== Application Starting ===");

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map Microsoft Identity UI controllers (for login/logout)
app.MapControllers();

app.Run();
