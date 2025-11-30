namespace mkBoutiqueCaftan.Models;

public class Client
{
    public int IdClient { get; set; }
    public string NomClient { get; set; } = string.Empty;
    public string PrenomClient { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? AdressePrincipale { get; set; }
    public int IdSociete { get; set; }
    public int TotalCommandes { get; set; } = 0;
    public DateTime DateCreationFiche { get; set; } = DateTime.Now;
    public bool Actif { get; set; } = true;
    public string? PhotoCIN { get; set; }
    
    // Navigation properties
    public Societe? Societe { get; set; }
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}

public class ClientDto
{
    public int IdClient { get; set; }
    public string NomClient { get; set; } = string.Empty;
    public string PrenomClient { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? AdressePrincipale { get; set; }
    public int IdSociete { get; set; }
    public int TotalCommandes { get; set; }
    public DateTime DateCreationFiche { get; set; }
    public bool Actif { get; set; }
    public string? PhotoCIN { get; set; }
}

public class CreateClientRequest
{
    public string NomClient { get; set; } = string.Empty;
    public string PrenomClient { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? AdressePrincipale { get; set; }
    public bool Actif { get; set; } = true;
    public string? PhotoCIN { get; set; }
}

public class UpdateClientRequest
{
    public string? NomClient { get; set; }
    public string? PrenomClient { get; set; }
    public string? Telephone { get; set; }
    public string? Email { get; set; }
    public string? AdressePrincipale { get; set; }
    public int? TotalCommandes { get; set; }
    public bool? Actif { get; set; }
    public string? PhotoCIN { get; set; }
}
