using DailyNotes.Blazor.Components;
using DailyNotes.Blazor.Services;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var azureAdConfig = builder.Configuration.GetSection("AzureAd");
if (builder.Environment.IsDevelopment() && (string.IsNullOrEmpty(azureAdConfig["ClientId"]) || azureAdConfig["ClientId"].Contains("[")))
{
    builder.Services.AddAuthentication("Demo")
        .AddScheme<AuthenticationSchemeOptions, MockAuthHandler>("Demo", null);
}
else
{
    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(azureAdConfig)
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddInMemoryTokenCaches();
}

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHeaderPropagation(options => options.Headers.Add("Authorization"));

builder.Services.AddHttpClient("DailyNotesApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5251/");
})
.AddMicrosoftIdentityUserAuthenticationHandler("DailyNotesApi", options =>
{
    options.Scopes = "api://b30d6c43-5055-4bf6-a71e-df0dd40ec946/access_as_user"; // Attempting to use a standard scope or the client ID scope
});

// Fallback plain client if needed
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("DailyNotesApi"));

builder.Services.AddScoped<ThemeService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.UseHeaderPropagation();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();

app.Run();
