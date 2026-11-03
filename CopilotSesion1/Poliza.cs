public record Poliza(
    string Numero,
    string Producto,          // "SALUD", "VIDA", "VEHICULAR"
    decimal PrimaMensualSoles,
    DateOnly InicioVigencia,
    DateOnly FinVigencia,
    bool Activa);
