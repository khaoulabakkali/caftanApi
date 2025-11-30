namespace mkBoutiqueCaftan.Models;

public class User
{
    public int IdUtilisateur { get; set; }
    public string NomComplet { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MotDePasseHash { get; set; } = string.Empty;
    public int IdRole { get; set; }
    public int IdSociete { get; set; }
    public string? Telephone { get; set; }
    public bool Actif { get; set; } = true;
    public DateTime DateCreationCompte { get; set; } = DateTime.Now;
    
    // Navigation properties
    public Role? Role { get; set; }
    public Societe? Societe { get; set; }
}

public class LoginRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UserDto
{
    public int IdUtilisateur { get; set; }
    public string NomComplet { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int IdRole { get; set; }
    public int IdSociete { get; set; }
    public string? Telephone { get; set; }
    public bool Actif { get; set; }
    public DateTime DateCreationCompte { get; set; }
    public string? Token { get; set; }
    public RoleDto? Role { get; set; }
}

public class RoleDto
{
    public int IdRole { get; set; }
    public int IdSociete { get; set; }
    public string NomRole { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Actif { get; set; }
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public UserDto? User { get; set; }
}

public class RegisterRequest
{
    public string NomComplet { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int IdRole { get; set; }
    public int IdSociete { get; set; }
    public string? Telephone { get; set; }
}

public class CreateUserRequest
{
    public string NomComplet { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int IdRole { get; set; }
    public int IdSociete { get; set; }
    public string? Telephone { get; set; }
    public bool Actif { get; set; } = true;
}

public class UpdateUserRequest
{
    public string? NomComplet { get; set; }
    public string? Login { get; set; }
    public string? Email { get; set; }
    public int? IdRole { get; set; }
    public int? IdSociete { get; set; }
    public string? Telephone { get; set; }
    public bool? Actif { get; set; }
}

public class ChangePasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}

