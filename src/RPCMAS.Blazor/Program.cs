using RPCMAS.Blazor.Api;
using RPCMAS.Blazor.Components;
using RPCMAS.Blazor.State;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<CurrentUserState>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<UserHeaderHandler>();

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5080";

builder.Services.AddHttpClient<ApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<UserHeaderHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
