using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using mkBoutiqueCaftan.Data;
using mkBoutiqueCaftan.Models;

namespace mkBoutiqueCaftan.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(string login, string password);
    Task<bool> RegisterAsync(string nomComplet, string login, string email, string password, int idRole, int idSociete, string? telephone);
}

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, ILogger<AuthService> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    private string GenerateJwtToken(User user, Role? role)
    {
        var jwtSecretKey = _configuration["Jwt:SecretKey"] 
            ?? "VotreCleSecreteSuperLongueEtSecuriseePourLaProductionChangezCetteCle";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "mkBoutiqueCaftan";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "mkBoutiqueCaftan";
        var jwtExpirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "1440"); // 24 heures par défaut

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.IdUtilisateur.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim("IdUtilisateur", user.IdUtilisateur.ToString()),
            new Claim("NomComplet", user.NomComplet),
            new Claim("IdRole", user.IdRole.ToString()),
            new Claim("IdSociete", user.IdSociete.ToString())
        };

        if (role != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.NomRole));
            claims.Add(new Claim("Role", role.NomRole));
        }

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<LoginResponse> LoginAsync(string login, string password)
    {
        try
        {
            _logger.LogInformation("Tentative de connexion pour le login: {Login}", login);
            // D'abord, essayer de trouver l'utilisateur sans Include pour éviter les erreurs de relation
            var user = await _context.Users
                .FirstOrDefaultAsync(u => 
                    u.Login.ToLower() == login.ToLower() && 
                    u.Actif);
            // Vérifier le mot de passe   _logger.LogWarning("Mot de passe incorrect. Hash utilisé : {hashOfInput}", hashOfInput); 
            if (user != null && !PasswordHelper.VerifyPassword(password, user.MotDePasseHash))
            {
                user = null;
            }

            if (user == null)
            {
                _logger.LogWarning("Tentative de connexion échouée pour le login: {Login}", login);
                return new LoginResponse
                {
                    Success = false,
                    Message = "Identifiant ou mot de passe incorrect, ou compte désactivé"
                };
            }

            _logger.LogInformation("Connexion réussie pour l'utilisateur: {UserId} ({Login})", user.IdUtilisateur, user.Login);

            // Charger le Role séparément si nécessaire
            Role? role = null;
            try
            {
                role = await _context.Roles.FindAsync(user.IdRole);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Impossible de charger le rôle pour l'utilisateur {UserId}", user.IdUtilisateur);
            }

            // Générer le token JWT
            var token = GenerateJwtToken(user, role);

            // Ne pas renvoyer le mot de passe dans la réponse - utiliser DTO pour éviter les cycles
            var userResponse = new UserDto
            {
                IdUtilisateur = user.IdUtilisateur,
                NomComplet = user.NomComplet,
                Login = user.Login,
                Email = user.Email,
                IdRole = user.IdRole,
                IdSociete = user.IdSociete,
                Telephone = user.Telephone,
                Actif = user.Actif,
                DateCreationCompte = user.DateCreationCompte,
                Token = token,
                Role = role != null ? new RoleDto
                {
                    IdRole = role.IdRole,
                    NomRole = role.NomRole,
                    Description = role.Description,
                    IdSociete = role.IdSociete,
                    Actif = role.Actif
                } : null
            };

            return new LoginResponse
            {
                Success = true,
                Message = "Connexion réussie",
                User = userResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la connexion pour le login: {Login}", login);
            
            // Vérifier si c'est une erreur de base de données
            if (ex.Message.Contains("Table") || ex.Message.Contains("doesn't exist") || ex.Message.Contains("Unknown column"))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Erreur de base de données. Vérifiez que les migrations ont été appliquées."
                };
            }
            
            return new LoginResponse
            {
                Success = false,
                Message = $"Erreur lors de la connexion: {ex.Message}"
            };
        }
    }

    public async Task<bool> RegisterAsync(string nomComplet, string login, string email, string password, int idRole, int idSociete, string? telephone)
    {
        _logger.LogInformation("Tentative d'inscription pour le login: {Login}", login);
        
        // Vérifier si le login existe déjà pour cette société
        var exists = await _context.Users
            .AnyAsync(u => 
                u.Login.ToLower() == login.ToLower() && u.IdSociete == idSociete);

        if (exists)
        {
            _logger.LogWarning("Tentative d'inscription avec un login déjà existant: {Login}", login);
            return false;
        }

        // Vérifier si l'email existe déjà pour cette société
        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower() && u.IdSociete == idSociete);

            if (emailExists)
            {
                _logger.LogWarning("Tentative d'inscription avec un email déjà existant: {Email}", email);
                return false;
            }
        }

        // Vérifier si le rôle existe
        var roleExists = await _context.Roles
            .AnyAsync(r => r.IdRole == idRole && r.Actif);

        if (!roleExists)
        {
            _logger.LogWarning("Tentative d'inscription avec un rôle inexistant ou inactif: {RoleId}", idRole);
            return false;
        }

        var newUser = new User
        {
            NomComplet = nomComplet,
            Login = login,
            Email = email,
            MotDePasseHash = PasswordHelper.HashPassword(password),
            IdRole = idRole,
            IdSociete = idSociete,
            Telephone = telephone,
            Actif = true,
            DateCreationCompte = DateTime.Now
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Inscription réussie pour l'utilisateur: {Login} (ID: {UserId})", login, newUser.IdUtilisateur);
        return true;
    }
}

