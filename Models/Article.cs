namespace mkBoutiqueCaftan.Models;

public class Article
{
    public int IdArticle { get; set; }
    public string NomArticle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PrixLocationBase { get; set; }
    public decimal PrixAvanceBase { get; set; }
    public int? IdTaille { get; set; }
    public string? Couleur { get; set; }
    public string? Photo { get; set; }
    public int IdCategorie { get; set; }
    public int IdSociete { get; set; }
    public bool Actif { get; set; } = true;
    
    // Navigation properties
    public Taille? Taille { get; set; }
    public Categorie? Categorie { get; set; }
    public Societe? Societe { get; set; }
}

public class ArticleDto
{
    public int IdArticle { get; set; }
    public string NomArticle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PrixLocationBase { get; set; }
    public decimal PrixAvanceBase { get; set; }
    public int? IdTaille { get; set; }
    public string? Couleur { get; set; }
    public string? Photo { get; set; }
    public int IdCategorie { get; set; }
    public int IdSociete { get; set; }
    public bool Actif { get; set; }
    public TailleDto? Taille { get; set; }
    public CategorieDto? Categorie { get; set; }
}

public class CreateArticleRequest
{
    public string NomArticle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PrixLocationBase { get; set; }
    public decimal PrixAvanceBase { get; set; }
    public int? IdTaille { get; set; }
    public string? Couleur { get; set; }
    public string? Photo { get; set; }
    public int IdCategorie { get; set; }
    public bool Actif { get; set; } = true;
}

public class UpdateArticleRequest
{
    public string? NomArticle { get; set; }
    public string? Description { get; set; }
    public decimal? PrixLocationBase { get; set; }
    public decimal? PrixAvanceBase { get; set; }
    public int? IdTaille { get; set; }
    public string? Couleur { get; set; }
    public string? Photo { get; set; }
    public int? IdCategorie { get; set; }
    public bool? Actif { get; set; }
}

