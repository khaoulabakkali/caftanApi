namespace mkBoutiqueCaftan.Models;

public enum StatutReservation
{
    EnAttente = 0,      // Réservation créée mais non confirmée
    Confirmee = 1,      // Réservation confirmée
    EnCours = 2,        // Location en cours
    Terminee = 3,       // Location terminée
    Annulee = 4         // Réservation annulée
}

