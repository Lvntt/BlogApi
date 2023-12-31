using System.Text;
using BlogApi.Data.DbContext;
using BlogApi.Mappers;
using BlogApi.Middlewares;
using BlogApi.Services.AddressService;
using BlogApi.Services.AuthorService;
using BlogApi.Services.CommentService;
using BlogApi.Services.CommunityService;
using BlogApi.Services.JwtService;
using BlogApi.Services.PostService;
using BlogApi.Services.TagService;
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

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommunityService, CommunityService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<ICommentService, CommentService>();

builder.Services.AddTransient<GlobalExceptionHandlingMiddleware>();

builder.Services.AddAutoMapper(typeof(AutoMappingProfile));

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
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var jwtService = context.HttpContext.RequestServices.GetRequiredService<IJwtService>();
            var userService =
                context.HttpContext.RequestServices.GetRequiredService<IUserService>();
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var userId = jwtService.ValidateToken(token);
            var isTokenInBlacklist = await userService.IsTokenInvalid(token);
            if (userId == null || isTokenInBlacklist)
            {
                context.Fail("Unauthorized");
            }
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

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var blogDbContext = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
    if (blogDbContext.Database.GetPendingMigrations().Any())
    {
        blogDbContext.Database.Migrate();
    }
}

app.Run();