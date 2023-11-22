using System.Text;
using BlogApi;
using BlogApi.Context;
using BlogApi.Data.Repositories;
using BlogApi.Data.Repositories.AddressRepository;
using BlogApi.Middlewares;
using BlogApi.Services.AddressService;
using BlogApi.Services.JwtService;
using BlogApi.Services.UserService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();

builder.Services.AddDbContext<BlogDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("BlogConnection"));
});
builder.Services.AddDbContext<GarDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("GarConnection"));
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenBlacklistRepository, TokenBlacklistRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddTransient<GlobalExceptionHandlingMiddleware>();
builder.Services.AddTransient<JwtMiddleware>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration.GetSection("Authentication:Issuer").Value,
        ValidateIssuer = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration.GetSection("Authentication:TokenSecret").Value!)
        ),
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration.GetSection("Authentication:Audience").Value,
        LifetimeValidator = (before, expires, _, _) =>
        {
            var utcNow = DateTime.UtcNow;
            return before <= utcNow && utcNow <= expires;
        }
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseMiddleware<JwtMiddleware>();

app.MapControllers();

app.Run();