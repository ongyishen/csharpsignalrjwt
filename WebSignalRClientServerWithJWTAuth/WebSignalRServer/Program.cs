using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using WebSignalRServer;
using WebSignalRServer.Core;
using WebSignalRServer.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
//GET SYS CONFIG
GlobalContext.SystemConfig = builder.Configuration.GetSection("SystemConfig").Get<SystemConfig>();

//GENERATE JWT SECURITY KEY HERE
var key = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(GlobalContext.SystemConfig.JwtSecretKey));

builder.Services.AddAuthentication()
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			LifetimeValidator = (before, expires, token, parameters) =>
				expires > DateTime.UtcNow,
			ValidateAudience = false,
			ValidateIssuer = false,
			ValidateActor = false,
			ValidateLifetime = true,
			IssuerSigningKey = key,
			NameClaimType = ClaimTypes.NameIdentifier
		};
		options.Events = new JwtBearerEvents
		{
			OnMessageReceived = context =>
			{
				//BEARER
				var accessToken = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

				if (string.IsNullOrEmpty(accessToken) == false)
				{
					context.Token = accessToken;
				}
				return Task.CompletedTask;
			}
		};
	});

builder.Services.AddAuthorization(options =>
{
	options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
	{
		policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
		policy.RequireClaim(ClaimTypes.NameIdentifier);
	});
});

//FOR SIGNALR
builder.Services.AddSignalR();
//FOR SIGNALR
builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

/*
 Typically, browsers load connections from the same domain as the requested page. However, there are occasions when a connection to another domain is required.

When making cross domain requests, the client code must use an absolute URL instead of a relative URL. For cross domain requests, change .withUrl("/chathub") to .withUrl("https://{App domain name}/chathub").

To prevent a malicious site from reading sensitive data from another site, cross-origin connections are disabled by default. To allow a cross-origin request, enable CORS:

UseCors must be called before calling MapHub.
 */
// global cors policy
app.UseCors(x => x
	.AllowAnyOrigin()
	.AllowAnyMethod()
	.AllowAnyHeader()
	);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

//FOR SIGNALR
app.MapHub<ChatHub>("/chatHub");

app.Run();