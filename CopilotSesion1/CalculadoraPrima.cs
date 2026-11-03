public static class CalculadoraPrima
{
    public static int CalcularDiasVigenciaRestantes(Poliza poliza)
    {
        int diasRestantes = poliza.FinVigencia.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;
        return Math.Max(0, diasRestantes);
    }

    /// <summary>
    /// Calcula la prima anual aplicando estas tres reglas:
    /// 1. La prima mensual no puede ser negativa.
    /// 2. La prima anual base es la prima mensual multiplicada por 12.
    /// 3. Si el producto es SALUD, se aplica un descuento del 5 %.
    /// </summary>
    /// <param name="poliza">Póliza cuya prima se calculará.</param>
    /// <returns>La prima anual calculada, con el descuento correspondiente si aplica.</returns>
    public static decimal CalcularPrimaAnual(Poliza poliza)
    {
        if (poliza.PrimaMensualSoles < 0)
            throw new ArgumentException("La prima mensual no puede ser negativa.", nameof(poliza));

        decimal primaAnual = poliza.PrimaMensualSoles * 12;

        return string.Equals(poliza.Producto, "SALUD", StringComparison.OrdinalIgnoreCase)
            ? primaAnual * 0.95m
            : primaAnual;
    }
}