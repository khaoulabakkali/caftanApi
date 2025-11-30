namespace mkBoutiqueCaftan.Models;

public class Configuration
{
    public int IdConfiguration { get; set; }
    public int IdSociete { get; set; }
    public string Cle { get; set; } = string.Empty;
    public string Data { get; set; } = "{}"; // JSON string
    public DateTime DateCreation { get; set; } = DateTime.Now;
    public DateTime? DateModification { get; set; }
    
    // Navigation property
    public Societe? Societe { get; set; }
}

public class ConfigurationDto
{
    public int IdConfiguration { get; set; }
    public int IdSociete { get; set; }
    public string Cle { get; set; } = string.Empty;
    public string Data { get; set; } = "{}";
    public DateTime DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
}

public class CreateConfigurationRequest
{
    public string Cle { get; set; } = string.Empty;
    public string Data { get; set; } = "{}";
}

public class UpdateConfigurationRequest
{
    public string? Cle { get; set; }
    public string? Data { get; set; }
}