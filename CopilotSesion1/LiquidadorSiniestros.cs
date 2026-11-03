/// <summary>
/// Calcula la indemnización de un siniestro aplicando el deducible,
/// el coaseguro y el ajuste por siniestros previos.
/// </summary>
public static class LiquidadorSiniestros
{
    /// <summary>
    /// Calcula el monto a liquidar después de aplicar las deducciones y ajustes correspondientes.
    /// </summary>
    /// <param name="montoReclamado">Monto reclamado por el asegurado.</param>
    /// <param name="deducible">Monto del deducible aplicable.</param>
    /// <param name="porcentajeCoaseguro">Porcentaje de coaseguro, entre 0 y 1.</param>
    /// <param name="siniestrosPrevios">Número de siniestros previos del asegurado.</param>
    /// <returns>El monto a liquidar, redondeado a dos decimales.</returns>
    public static decimal Calc(
        decimal montoReclamado,
        decimal deducible,
        decimal porcentajeCoaseguro,
        int siniestrosPrevios)
    {
        var r = montoReclamado - deducible;
        if (r < 0) r = 0;
        r = r * (1 - porcentajeCoaseguro);
        if (siniestrosPrevios > 2) r = r * 0.9m;
        return Math.Round(r, 2);
    }
}