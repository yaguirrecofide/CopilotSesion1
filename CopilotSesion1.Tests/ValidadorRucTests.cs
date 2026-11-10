public class ValidadorRucTests
{
    [Theory]
    [InlineData("20123456789", true)]
    [InlineData("10987654321", true)]
    [InlineData("2012345678", false)]
    [InlineData("201234567890", false)]
    [InlineData("2012345678A", false)]
    [InlineData(" 20123456789 ", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void EsValido_DebeValidarSegunLongitudCaracteresYPrefijo(string? ruc, bool esperado)
    {
        Assert.Equal(esperado, ValidadorRuc.EsValido(ruc));
    }

    [Fact]
    public void Validar_RucInvalido_LanzaExcepcionConMensajeDescriptivo()
    {
        var exception = Assert.Throws<ArgumentException>(() => ValidadorRuc.Validar("2012345678A"));

        Assert.Equal("El RUC proporcionado no es válido. (Parameter 'ruc')", exception.Message);
    }
}