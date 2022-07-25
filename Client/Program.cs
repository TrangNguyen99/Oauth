using Microsoft.AspNetCore.Authentication.OAuth;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = "ClientCookie";
    config.DefaultSignInScheme = "ClientCookie";
    config.DefaultChallengeScheme = "MyServer";
})
    .AddCookie("ClientCookie")
    .AddOAuth("MyServer", config =>
    {
        config.ClientId = "client_id";
        config.ClientSecret = "client_secret";
        config.CallbackPath = "/oauth/callback";
        config.AuthorizationEndpoint = "https://localhost:7016/oauth/authorize";
        config.TokenEndpoint = "https://localhost:7016/oauth/token";

        config.SaveTokens = true;

        config.Events = new OAuthEvents
        {
            OnCreatingTicket = context =>
            {
                var accessToken = context.AccessToken!;
                var base64Payload = accessToken.Split('.')[1];
                var bytes = Convert.FromBase64String(base64Payload);
                var jsonPayload = Encoding.UTF8.GetString(bytes);
                var claims = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonPayload)!;

                foreach (var claim in claims)
                {
                    context.Identity?.AddClaim(new Claim(claim.Key, claim.Value));
                }

                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
