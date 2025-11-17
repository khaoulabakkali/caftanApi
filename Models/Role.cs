using System.Text.Json.Serialization;

namespace mkBoutiqueCaftan.Models;

public class Role
{
    public int IdRole { get; set; }
    public string NomRole { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Actif { get; set; } = true;
    
    // Navigation property - ignorée lors de la sérialisation JSON pour éviter les cycles
    [JsonIgnore]
    public ICollection<User> Users { get; set; } = new List<User>();
}

