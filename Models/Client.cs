namespace mkBoutiqueCaftan.Models;

public class Client
{
    public int IdClient { get; set; }
    public string NomComplet { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telephone { get; set; }
    public string? Adresse { get; set; }
    public int IdSociete { get; set; }
    public bool Actif { get; set; } = true;
    public DateTime DateCreation { get; set; } = DateTime.Now;
    
    // Navigation properties
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}

public class ClientDto
{
    public int IdClient { get; set; }
    public string NomComplet { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telephone { get; set; }
    public string? Adresse { get; set; }
    public int IdSociete { get; set; }
    public bool Actif { get; set; }
    public DateTime DateCreation { get; set; }
}

