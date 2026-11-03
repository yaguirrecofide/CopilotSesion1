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
}