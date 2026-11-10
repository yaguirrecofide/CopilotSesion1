/// <summary>
/// Provides validation and classification helpers for Peruvian RUC numbers.
/// </summary>
public static class ValidadorRuc
{
    /// <summary>
    /// Determina si un RUC es válido según estas tres reglas:
    /// <list type="number">
    /// <item><description>Contiene exactamente 11 caracteres.</description></item>
    /// <item><description>Todos sus caracteres son dígitos.</description></item>
    /// <item><description>Comienza con "10" o "20".</description></item>
    /// </list>
    /// </summary>
    /// <param name="ruc">El RUC que se va a validar.</param>
    /// <returns><see langword="true"/> si el RUC cumple las tres reglas; de lo contrario, <see langword="false"/>.</returns>
    public static bool EsValido(string? ruc)
    {
        if (string.IsNullOrEmpty(ruc))
            return false;

        if (ruc.Length != 11 || !ruc.All(char.IsDigit))
            return false;

        if (!ruc.StartsWith("10") && !ruc.StartsWith("20"))
            return false;

        return true;
    }

    public static void Validar(string? ruc)
    {
        if (!EsValido(ruc))
            throw new ArgumentException("El RUC proporcionado no es válido.", nameof(ruc));
    }

    /// <summary>
    /// Gets the person type indicated by the RUC prefix.
    /// </summary>
    /// <param name="ruc">The RUC to classify.</param>
    /// <returns>"PERSONA NATURAL", "PERSONA JURIDICA", or "DESCONOCIDO".</returns>
    public static string Tipo(string? ruc)
    {
        if (string.IsNullOrEmpty(ruc) || ruc.Length != 11 || !ruc.All(char.IsDigit))
            return "DESCONOCIDO";

        if (ruc.StartsWith("10"))
            return "PERSONA NATURAL";

        if (ruc.StartsWith("20"))
            return "PERSONA JURIDICA";

        return "DESCONOCIDO";
    }
}