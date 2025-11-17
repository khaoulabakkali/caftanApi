using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using mkBoutiqueCaftan.Data;
using mkBoutiqueCaftan.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] 
    ?? "VotreCleSecreteSuperLongueEtSecuriseePourLaProductionChangezCetteCle";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "mkBoutiqueCaftan";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "mkBoutiqueCaftan";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configure MariaDB connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=mkBoutiqueCaftan;User=root;Password=;Port=3306;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Utiliser une version fixe pour éviter les erreurs de détection
    // Vous pouvez modifier la version selon votre installation MariaDB
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 11, 0)));
});

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoleService, RoleService>();

// Configure CORS if needed
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Initialize default roles
try
{
    using (var scope = app.Services.CreateScope())
    {
        var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();
        await roleService.InitializeDefaultRolesAsync();
    }
}
catch (Exception ex)
{
    // Log l'erreur mais continue le démarrage
    // Cela peut arriver si les migrations n'ont pas encore été appliquées
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning(ex, "Impossible d'initialiser les rôles par défaut. Assurez-vous que les migrations ont été appliquées. Erreur: {ErrorMessage}", ex.Message);
}

// Configure the HTTP request pipeline
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
