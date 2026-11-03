Console.WriteLine("Hello, World!");

Console.WriteLine(Calculadora.Sumar(7, 3));
Console.WriteLine(Calculadora.Restar(7, 3));
Console.WriteLine(Calculadora.Multiplicar(7, 3));
//Console.WriteLine(Calculadora.Dividir(7, 0));


// Probar ValidadorRuc.EsValido con: null, "123", "20123456789", "10987654321", "30123456789", "2012345678A"
Console.WriteLine(ValidadorRuc.EsValido(null));
Console.WriteLine(ValidadorRuc.EsValido("123"));
Console.WriteLine(ValidadorRuc.EsValido("20123456789"));
Console.WriteLine(ValidadorRuc.EsValido("10987654321"));
Console.WriteLine(ValidadorRuc.EsValido("30123456789"));
Console.WriteLine(ValidadorRuc.EsValido("2012345678A"));

Console.WriteLine(new PolizaService().FiltrarVigentes(null, DateOnly.FromDateTime(DateTime.Today)));
