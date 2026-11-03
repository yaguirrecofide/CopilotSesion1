public class CalculadoraPrimaTests
{
    [Theory]
    [InlineData("SALUD", 1140)]
    [InlineData("VIDA", 1200)]
    [InlineData("salud", 1140)]
    public void CalcularPrimaAnual_DebeAplicarDescuentoSegunProducto(
        string producto,
        decimal primaAnualEsperada)
    {
        var poliza = CrearPoliza(producto, 100m);

        var resultado = CalculadoraPrima.CalcularPrimaAnual(poliza);

        Assert.Equal(primaAnualEsperada, resultado);
    }

    [Fact]
    public void CalcularPrimaAnual_DebeRechazarPrimaMensualNegativa()
    {
        var poliza = CrearPoliza("VIDA", -1m);

        Assert.Throws<ArgumentException>(() => CalculadoraPrima.CalcularPrimaAnual(poliza));
    }

    private static Poliza CrearPoliza(string producto, decimal primaMensualSoles)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        return new Poliza("POL-001", producto, primaMensualSoles, hoy, hoy.AddYears(1), true);
    }
}
