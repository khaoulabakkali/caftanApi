namespace mkBoutiqueCaftan.Models;

public class Paiement
{
    public int IdPaiement { get; set; }
    public int IdReservation { get; set; }
    public decimal Montant { get; set; }
    public DateTime DatePaiement { get; set; } = DateTime.Now;
    public string? MethodePaiement { get; set; } // Espèces, Carte, Chèque, etc.
    public string? Reference { get; set; } // Numéro de transaction, référence chèque, etc.
    public int IdSociete { get; set; }
    
    // Navigation properties
    public Reservation? Reservation { get; set; }
    public Societe? Societe { get; set; }
}

public class PaiementDto
{
    public int IdPaiement { get; set; }
    public int IdReservation { get; set; }
    public decimal Montant { get; set; }
    public DateTime DatePaiement { get; set; }
    public string? MethodePaiement { get; set; }
    public string? Reference { get; set; }
    public int IdSociete { get; set; }
}

public class CreatePaiementRequest
{
    public int IdReservation { get; set; }
    public decimal Montant { get; set; }
    public string? MethodePaiement { get; set; }
    public string? Reference { get; set; }
}

public class UpdatePaiementRequest
{
    public int? IdReservation { get; set; }
    public decimal? Montant { get; set; }
    public string? MethodePaiement { get; set; }
    public string? Reference { get; set; }
}
