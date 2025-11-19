namespace mkBoutiqueCaftan.Models;

public class Paiement
{
    public int IdPaiement { get; set; }
    public int IdReservation { get; set; }
    public decimal Montant { get; set; }
    public DateTime DatePaiement { get; set; } = DateTime.Now;
    public string? MethodePaiement { get; set; } // Espèces, Carte, Chèque, etc.
    public string? Reference { get; set; } // Numéro de transaction, référence chèque, etc.
    
    // Navigation property
    public Reservation? Reservation { get; set; }
}

public class PaiementDto
{
    public int IdPaiement { get; set; }
    public int IdReservation { get; set; }
    public decimal Montant { get; set; }
    public DateTime DatePaiement { get; set; }
    public string? MethodePaiement { get; set; }
    public string? Reference { get; set; }
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
