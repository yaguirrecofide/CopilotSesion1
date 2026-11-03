public static class Textos
{
    // Invierte una cadena de texto
    public static string Invertir(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return texto;

        char[] caracteres = texto.ToCharArray();
        Array.Reverse(caracteres);
        return new string(caracteres);
    }

    // Cuenta cuántas vocales tiene un texto, sin distinguir mayúsculas ni tildes
    public static int ContarVocales(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return 0;

        int contador = 0;
        string vocales = "aeiouáéíóú";
        foreach (char c in texto.ToLower())
        {
            if (vocales.Contains(c))
                contador++;
        }
        return contador;
    }
}