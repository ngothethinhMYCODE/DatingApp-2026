using System.Text;
using API.Data;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using API.Middleware;
using API.Services;
using API.SignalR;
using Humanizer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDBContext>(opt =>
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddCors();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPhotoService,PhotoService>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IMessageRespository, MessageRepository>();
builder.Services.AddScoped<ILikesRepository,LikesRepository>();
builder.Services.AddScoped<LogUserActivity>();
builder.Services.Configure<CloudinarySettings>(builder.Configuration.
    GetSection("CloudinarySettings"));
builder.Services.AddSignalR();
builder.Services.AddSingleton<PresenceTracker>();
builder.Services.AddIdentityCore<AppUser>(opt =>
{
   opt.Password.RequireNonAlphanumeric=false;
   opt.User.RequireUniqueEmail=true; 
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDBContext>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var tokenKey= builder.Configuration["TokenKey"] 
            ?? throw new Exception("Token key is not found - Program.cs");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey=true,
            IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
            ValidateIssuer=false,
            ValidateAudience=false
        };
        options.Events=new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken=context.Request.Query["access_token"];
                var path= context.HttpContext.Request.Path;
                if(!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token=accessToken;

                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAdminRole", policy=>policy.RequireRole("Admin"))
    .AddPolicy("ModeratePhotoRole", policy=>policy.RequireRole("Admin","Moderator"));
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors(x => x
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins("http://localhost:4200", "https://localhost:4200"));
app.UseAuthentication();
app.UseAuthorization(); 
app.MapControllers();

app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/messages");
using var scope= app.Services.CreateScope();
var service= scope.ServiceProvider;
try
{
    var context = service.GetRequiredService<AppDBContext>();
    var userManager=service.GetRequiredService<UserManager<AppUser>>();
    await context.Database.MigrateAsync();
    await context.Connections.ExecuteDeleteAsync();
    await Seed.SeedUsers(userManager);
}
catch (Exception ex)
{
    var logger=service.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migrations");
}
app.Run();
