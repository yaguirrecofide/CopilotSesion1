public class CalculadoraPrimaRecargoTests
{
    [Fact]
    public void PrimaAnualConRecargo_SinSiniestrosPrevios_AplicaPrimaBase()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var poliza = new Poliza("POL-001", "VIDA", 100m, hoy, hoy.AddYears(1), true);

        var resultado = CalculadoraPrima.PrimaAnualConRecargo(poliza);

        Assert.Equal(1200m, resultado);
        Assert.Equal(
            CalculadoraPrima.PrimaAnualConRecargo(poliza, siniestrosPrevios: 0),
            resultado);
    }
}