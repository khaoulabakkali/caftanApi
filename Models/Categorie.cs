namespace mkBoutiqueCaftan.Models;

public class Categorie
{
    public int IdCategorie { get; set; }
    public string NomCategorie { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? OrdreAffichage { get; set; }
}

public class CategorieDto
{
    public int IdCategorie { get; set; }
    public string NomCategorie { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? OrdreAffichage { get; set; }
}

public class CreateCategorieRequest
{
    public string NomCategorie { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? OrdreAffichage { get; set; }
}

public class UpdateCategorieRequest
{
    public string NomCategorie { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? OrdreAffichage { get; set; }
}

