using basic_kanban_api.Data;
using basic_kanban_api.Models;
using basic_kanban_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add database context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<KanbanDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add Identity
builder.Services.AddIdentity<User, Role>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<KanbanDbContext>()
.AddDefaultTokenProviders();

// Add services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IEndpointService, EndpointService>();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured.");
var issuer = jwtSettings["Issuer"] ?? "KanbanAPI";
var audience = jwtSettings["Audience"] ?? "KanbanClients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization();

// Configure CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:4200", "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", corsPolicyBuilder =>
    {
        corsPolicyBuilder
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Apply migrations automatically with retry/backoff to tolerate DB cold-starts
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<KanbanDbContext>();

    var maxAttempts = 8;
    var attempt = 0;
    var succeeded = false;

    while (!succeeded && ++attempt <= maxAttempts)
    {
        try
        {
            logger.LogInformation("Applying migrations (attempt {Attempt}/{Max})...", attempt, maxAttempts);
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied.");
            succeeded = true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Migration attempt {Attempt} failed.", attempt);
            if (attempt == maxAttempts)
            {
                logger.LogError(ex, "Maximum migration attempts reached — rethrowing.");
                throw;
            }

            var delayMs = Math.Min(1000 * attempt, 10000);
            logger.LogInformation("Waiting {Delay}ms before next attempt...", delayMs);
            await Task.Delay(delayMs);
        }
    }

    // Create default roles after migrations applied
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    var defaultRoles = new[] { "User", "Manager", "Admin" };

    foreach (var roleName in defaultRoles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var result = await roleManager.CreateAsync(new Role
            {
                Name = roleName,
                Description = $"{roleName} role",
                CreatedAt = DateTime.UtcNow
            });

            if (!result.Succeeded)
            {
                logger.LogWarning("Creating role {Role} returned errors: {Errors}", roleName, string.Join(';', result.Errors.Select(e => e.Description)));
            }
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
