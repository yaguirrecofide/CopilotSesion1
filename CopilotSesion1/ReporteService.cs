using System.Text.Json;
using System.Text.Encodings.Web;

public record ResumenProducto(
    string Producto,
    int CantidadPolizasActivas,
    decimal SumaPrimasMensuales);

public class ReporteService
{
    public ReporteService()
    {
    }

    /// <summary>
    /// Genera un resumen de pólizas activas y suma de primas mensuales por producto.
    /// </summary>
    /// <param name="polizas">Lista de pólizas a procesar</param>
    /// <param name="inicioVigencia">Filtro opcional: fecha mínima de inicio de vigencia</param>
    /// <param name="finVigencia">Filtro opcional: fecha máxima de inicio de vigencia</param>
    /// <returns>Lista de resúmenes por producto</returns>
    public List<ResumenProducto> ResumenPorProducto(
        List<Poliza> polizas,
        DateOnly? inicioVigencia = null,
        DateOnly? finVigencia = null)
    {
        if (polizas == null || polizas.Count == 0)
            return [];

        var polizasFiltradas = polizas;

        // Aplicar filtro de rango de fechas de inicio de vigencia si se proporcionan
        if (inicioVigencia.HasValue)
        {
            polizasFiltradas = polizasFiltradas
                .Where(p => p.InicioVigencia >= inicioVigencia.Value)
                .ToList();
        }

        if (finVigencia.HasValue)
        {
            polizasFiltradas = polizasFiltradas
                .Where(p => p.InicioVigencia <= finVigencia.Value)
                .ToList();
        }

        // Agrupar por producto y contar pólizas activas
        var resumen = polizasFiltradas
            .Where(p => p.Activa)
            .GroupBy(p => p.Producto)
            .Select(g => new ResumenProducto(
                Producto: g.Key,
                CantidadPolizasActivas: g.Count(),
                SumaPrimasMensuales: g.Sum(p => p.PrimaMensualSoles)))
            .OrderBy(r => r.Producto)
            .ToList();

        return resumen;
    }

    /// <summary>
    /// Exporta la lista de pólizas a un archivo CSV.
    /// </summary>
    /// <param name="polizas">Lista de pólizas a exportar</param>
    /// <param name="ruta">Ruta del archivo CSV de destino</param>
    public void ExportarCsv(List<Poliza> polizas, string ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta))
            throw new ArgumentException("La ruta no puede estar vacía", nameof(ruta));

        if (polizas == null)
            polizas = [];

        try
        {
            using (var writer = new StreamWriter(ruta, false, System.Text.Encoding.UTF8))
            {
                // Encabezados
                writer.WriteLine("Número,Producto,Prima Mensual (S/),Inicio Vigencia,Fin Vigencia,Activa");

                // Datos
                foreach (var poliza in polizas)
                {
                    var fila = $"{EscaparCsv(poliza.Numero)},{EscaparCsv(poliza.Producto)},{poliza.PrimaMensualSoles},{poliza.InicioVigencia:yyyy-MM-dd},{poliza.FinVigencia:yyyy-MM-dd},{poliza.Activa}";
                    writer.WriteLine(fila);
                }
            }
        }
        catch (IOException ex)
        {
            throw new IOException($"Error al exportar a CSV en la ruta '{ruta}'", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new IOException($"Acceso denegado al escribir en '{ruta}'", ex);
        }
    }

    /// <summary>
    /// Exporta la lista de pólizas a un archivo JSON.
    /// </summary>
    /// <param name="polizas">Lista de pólizas a exportar</param>
    /// <param name="ruta">Ruta del archivo JSON de destino</param>
    public void ExportarJson(List<Poliza> polizas, string ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta))
            throw new ArgumentException("La ruta no puede estar vacía", nameof(ruta));

        if (polizas == null)
            polizas = [];

        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(polizas, options);
            File.WriteAllText(ruta, json, System.Text.Encoding.UTF8);
        }
        catch (IOException ex)
        {
            throw new IOException($"Error al exportar a JSON en la ruta '{ruta}'", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new IOException($"Acceso denegado al escribir en '{ruta}'", ex);
        }
    }

    /// <summary>
    /// Escapa caracteres especiales para CSV, incluyendo fórmulas de Excel.
    /// </summary>
    private static string EscaparCsv(string valor)
    {
        if (string.IsNullOrEmpty(valor))
            return string.Empty;

        // Escapar fórmulas de Excel (=, +, -, @)
        if (valor.StartsWith("=") || valor.StartsWith("+") || valor.StartsWith("-") || valor.StartsWith("@"))
        {
            valor = "'" + valor;
        }

        if (valor.Contains(",") || valor.Contains("\"") || valor.Contains("\n"))
        {
            return $"\"{valor.Replace("\"", "\"\"")}\"";
        }

        return valor;
    }
}
