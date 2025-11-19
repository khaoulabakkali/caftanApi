namespace mkBoutiqueCaftan.Models;

public class Taille
{
    public int IdTaille { get; set; }
    public string Libelle { get; set; } = string.Empty;
    public int IdSociete { get; set; }
}

public class TailleDto
{
    public int IdTaille { get; set; }
    public string Taille { get; set; } = string.Empty;
    public int IdSociete { get; set; }
}

public class CreateTailleRequest
{
    public string Taille { get; set; } = string.Empty;
}

public class UpdateTailleRequest
{
    public string Taille { get; set; } = string.Empty;
}

