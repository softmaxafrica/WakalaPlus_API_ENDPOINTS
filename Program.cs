using Microsoft.EntityFrameworkCore;
using WakalaPlus.Shared;
using WakalaPlus.Shared.BgServices;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WakalaPlus.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddHostedService<NotificationService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySQL(builder.Configuration.GetConnectionString("DB_Connection"));
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", builder =>
    {
        builder.WithOrigins(
                "http://localhost",  // Add explicit frontend URLs
                "http://softmaxafrica-001-site1.gtempurl.com") // For IP-based access
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

// Configure JWT Authentication
var key = Encoding.ASCII.GetBytes("S0ftM@x@frica-W@kalaPlux.Key");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    });

var app = builder.Build();

// Enable Swagger UI
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

// Apply middleware in correct order
app.UseRouting();
app.UseCors("AllowLocalhost"); // Apply CORS after routing
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<SignalHub>("/SignalHub").RequireCors("AllowLocalhost"); // Apply CORS to the hub
    endpoints.MapControllers();
});

app.Run();
