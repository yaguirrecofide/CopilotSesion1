using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

public class ReporteServiceTests
{
    private readonly ReporteService _reporteService;

    public ReporteServiceTests()
    {
        _reporteService = new ReporteService();
    }

    private List<Poliza> ObtenerPolizasEjemplo()
    {
        return new List<Poliza>
        {
            new("POL-001", "SALUD", 150.50m, new DateOnly(2023, 01, 15), new DateOnly(2025, 01, 15), true),
            new("POL-002", "SALUD", 200.00m, new DateOnly(2023, 06, 01), new DateOnly(2025, 06, 01), true),
            new("POL-003", "VIDA", 300.75m, new DateOnly(2023, 03, 10), new DateOnly(2025, 03, 10), true),
            new("POL-004", "VEHICULAR", 250.00m, new DateOnly(2023, 02, 20), new DateOnly(2025, 02, 20), true),
            new("POL-005", "VEHICULAR", 180.50m, new DateOnly(2024, 05, 01), new DateOnly(2026, 05, 01), false),
            new("POL-006", "SALUD", 120.00m, new DateOnly(2024, 08, 15), new DateOnly(2026, 08, 15), true),
        };
    }

    // Tests para ResumenPorProducto
    [Fact]
    public void ResumenPorProducto_SinFiltro_DebeAgruparPorProducto()
    {
        // Arrange
        var polizas = ObtenerPolizasEjemplo();

        // Act
        var resumen = _reporteService.ResumenPorProducto(polizas);

        // Assert
        Assert.Equal(3, resumen.Count);
        var salud = resumen.First(r => r.Producto == "SALUD");
        Assert.Equal(3, salud.CantidadPolizasActivas);
        Assert.Equal(470.50m, salud.SumaPrimasMensuales);

        var vida = resumen.First(r => r.Producto == "VIDA");
        Assert.Equal(1, vida.CantidadPolizasActivas);
        Assert.Equal(300.75m, vida.SumaPrimasMensuales);

        var vehicular = resumen.First(r => r.Producto == "VEHICULAR");
        Assert.Equal(1, vehicular.CantidadPolizasActivas);
        Assert.Equal(250.00m, vehicular.SumaPrimasMensuales);
    }

    [Fact]
    public void ResumenPorProducto_ConFiltroInicioVigencia_DebeFiltrrarPorFecha()
    {
        // Arrange
        var polizas = ObtenerPolizasEjemplo();
        var fechaFiltro = new DateOnly(2024, 01, 01);

        // Act
        var resumen = _reporteService.ResumenPorProducto(polizas, inicioVigencia: fechaFiltro);

        // Assert
        Assert.Single(resumen);
        var salud = resumen.First(r => r.Producto == "SALUD");
        Assert.Equal(1, salud.CantidadPolizasActivas);
        Assert.Equal(120.00m, salud.SumaPrimasMensuales);
    }

    [Fact]
    public void ResumenPorProducto_ConFiltroRangoFechas_DebeFiltrrarPorRango()
    {
        // Arrange
        var polizas = ObtenerPolizasEjemplo();
        var inicio = new DateOnly(2023, 01, 01);
        var fin = new DateOnly(2023, 06, 30);

        // Act
        var resumen = _reporteService.ResumenPorProducto(polizas, inicioVigencia: inicio, finVigencia: fin);

        // Assert
        Assert.Equal(3, resumen.Count);
        var salud = resumen.First(r => r.Producto == "SALUD");
        Assert.Equal(2, salud.CantidadPolizasActivas);
    }

    [Fact]
    public void ResumenPorProducto_ListaVacia_DebeRetornarListaVacia()
    {
        // Act
        var resumen = _reporteService.ResumenPorProducto(new List<Poliza>());

        // Assert
        Assert.Empty(resumen);
    }

    [Fact]
    public void ResumenPorProducto_ListaNula_DebeRetornarListaVacia()
    {
        // Act
        var resumen = _reporteService.ResumenPorProducto(null);

        // Assert
        Assert.Empty(resumen);
    }

    // Tests para ExportarCsv
    [Fact]
    public void ExportarCsv_ConPolizas_DebeCrearaArchivoConDatos()
    {
        // Arrange
        var polizas = ObtenerPolizasEjemplo();
        var rutaTemporal = Path.Combine(Path.GetTempPath(), $"polizas_{Guid.NewGuid()}.csv");

        try
        {
            // Act
            _reporteService.ExportarCsv(polizas, rutaTemporal);

            // Assert
            Assert.True(File.Exists(rutaTemporal));
            var lineas = File.ReadAllLines(rutaTemporal);
            Assert.Equal(7, lineas.Length); // 1 encabezado + 6 datos
            Assert.Contains("Número", lineas[0]);
            Assert.Contains("Producto", lineas[0]);
        }
        finally
        {
            if (File.Exists(rutaTemporal))
                File.Delete(rutaTemporal);
        }
    }

    [Fact]
    public void ExportarCsv_ConListaVacia_DebeCrearArchivoConSoloEncabezados()
    {
        // Arrange
        var rutaTemporal = Path.Combine(Path.GetTempPath(), $"polizas_{Guid.NewGuid()}.csv");

        try
        {
            // Act
            _reporteService.ExportarCsv(new List<Poliza>(), rutaTemporal);

            // Assert
            Assert.True(File.Exists(rutaTemporal));
            var lineas = File.ReadAllLines(rutaTemporal);
            Assert.Single(lineas); // Solo encabezado
        }
        finally
        {
            if (File.Exists(rutaTemporal))
                File.Delete(rutaTemporal);
        }
    }

    [Fact]
    public void ExportarCsv_ConRutaVacia_DebeLanzarArgumentException()
    {
        // Arrange
        var polizas = ObtenerPolizasEjemplo();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _reporteService.ExportarCsv(polizas, string.Empty));
    }

    [Fact]
    public void ExportarCsv_ConRutaNula_DebeLanzarArgumentException()
    {
        // Arrange
        var polizas = ObtenerPolizasEjemplo();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _reporteService.ExportarCsv(polizas, null));
    }

    // Tests para ExportarJson
    [Fact]
    public void ExportarJson_ConPolizas_DebeCrearArchivoConDatos()
    {
        // Arrange
        var polizas = ObtenerPolizasEjemplo();
        var rutaTemporal = Path.Combine(Path.GetTempPath(), $"polizas_{Guid.NewGuid()}.json");

        try
        {
            // Act
            _reporteService.ExportarJson(polizas, rutaTemporal);

            // Assert
            Assert.True(File.Exists(rutaTemporal));
            var contenido = File.ReadAllText(rutaTemporal);
            Assert.Contains("POL-001", contenido);
            Assert.Contains("SALUD", contenido);

            var polizasDesde = JsonSerializer.Deserialize<List<Poliza>>(contenido);
            Assert.Equal(6, polizasDesde.Count);
        }
        finally
        {
            if (File.Exists(rutaTemporal))
                File.Delete(rutaTemporal);
        }
    }

    [Fact]
    public void ExportarJson_ConListaVacia_DebeCrearArchivoConArrayVacio()
    {
        // Arrange
        var rutaTemporal = Path.Combine(Path.GetTempPath(), $"polizas_{Guid.NewGuid()}.json");

        try
        {
            // Act
            _reporteService.ExportarJson(new List<Poliza>(), rutaTemporal);

            // Assert
            Assert.True(File.Exists(rutaTemporal));
            var contenido = File.ReadAllText(rutaTemporal);
            var polizas = JsonSerializer.Deserialize<List<Poliza>>(contenido);
            Assert.Empty(polizas);
        }
        finally
        {
            if (File.Exists(rutaTemporal))
                File.Delete(rutaTemporal);
        }
    }

    [Fact]
    public void ExportarJson_ConListaNula_DebeCrearArchivoConArrayVacio()
    {
        // Arrange
        var rutaTemporal = Path.Combine(Path.GetTempPath(), $"polizas_{Guid.NewGuid()}.json");

        try
        {
            // Act
            _reporteService.ExportarJson(null, rutaTemporal);

            // Assert
            Assert.True(File.Exists(rutaTemporal));
            var contenido = File.ReadAllText(rutaTemporal);
            var polizas = JsonSerializer.Deserialize<List<Poliza>>(contenido);
            Assert.Empty(polizas);
        }
        finally
        {
            if (File.Exists(rutaTemporal))
                File.Delete(rutaTemporal);
        }
    }

    [Fact]
    public void ExportarJson_ConRutaVacia_DebeLanzarArgumentException()
    {
        // Arrange
        var polizas = ObtenerPolizasEjemplo();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _reporteService.ExportarJson(polizas, string.Empty));
    }

    [Fact]
    public void ExportarJson_ConRutaNula_DebeLanzarArgumentException()
    {
        // Arrange
        var polizas = ObtenerPolizasEjemplo();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _reporteService.ExportarJson(polizas, null));
    }
}
