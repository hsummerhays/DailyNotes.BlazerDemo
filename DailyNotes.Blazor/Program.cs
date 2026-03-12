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
    var azureAdSection = builder.Configuration.GetSection("AzureAd");
    Console.WriteLine($"DEBUG: AzureAd:ClientSecret length: {builder.Configuration["AzureAd:ClientSecret"]?.Length ?? 0}");
    
    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(options => 
        {
            azureAdSection.Bind(options);
            options.ClientSecret = builder.Configuration["AzureAd:ClientSecret"];
        })
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddInMemoryTokenCaches();
    builder.Services.AddMicrosoftIdentityConsentHandler();
}

builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();
builder.Services.AddControllersWithViews();

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
    var apiBaseAddress = builder.Configuration["DailyNotesApi:BaseAddress"] ?? "http://localhost:5251/";
    client.BaseAddress = new Uri(apiBaseAddress);
})
.AddMicrosoftIdentityUserAuthenticationHandler("DailyNotesApi", options =>
{
    options.Scopes = "api://b30d6c43-5055-4bf6-a71e-df0dd40ec946/access_as_user"; 
});

// Provide simpler injection for components
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
app.MapRazorPages();

app.Run();
