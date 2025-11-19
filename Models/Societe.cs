namespace mkBoutiqueCaftan.Models;

public class Societe
{
    public int IdSociete { get; set; }
    public string NomSociete { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Adresse { get; set; }
    public string? Telephone { get; set; }
    public string? Email { get; set; }
    public string? SiteWeb { get; set; }
    public string? Logo { get; set; }
    public bool Actif { get; set; } = true;
    public DateTime DateCreation { get; set; } = DateTime.Now;
}

public class SocieteDto
{
    public int IdSociete { get; set; }
    public string NomSociete { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Adresse { get; set; }
    public string? Telephone { get; set; }
    public string? Email { get; set; }
    public string? SiteWeb { get; set; }
    public string? Logo { get; set; }
    public bool Actif { get; set; }
    public DateTime DateCreation { get; set; }
}

public class CreateSocieteRequest
{
    public string NomSociete { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Adresse { get; set; }
    public string? Telephone { get; set; }
    public string? Email { get; set; }
    public string? SiteWeb { get; set; }
    public string? Logo { get; set; }
    public bool Actif { get; set; } = true;
}

public class UpdateSocieteRequest
{
    public string? NomSociete { get; set; }
    public string? Description { get; set; }
    public string? Adresse { get; set; }
    public string? Telephone { get; set; }
    public string? Email { get; set; }
    public string? SiteWeb { get; set; }
    public string? Logo { get; set; }
    public bool? Actif { get; set; }
}

