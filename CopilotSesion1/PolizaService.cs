public class PolizaService
{
    // Devuelve las pólizas activas y vigentes en la fecha indicada
    public List<Poliza> FiltrarVigentes(List<Poliza>? polizas, DateOnly fecha)
    {
        return polizas.Where(p => p.Activa && p.InicioVigencia <= fecha && p.FinVigencia >= fecha).ToList();
    }

    // Suma la prima mensual de las pólizas activas agrupadas por producto
    public Dictionary<string, decimal> SumarPrimaPorProducto(List<Poliza> polizas)
    {
        return polizas
            .Where(p => p.Activa)
            .GroupBy(p => p.Producto)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.PrimaMensualSoles));
    }
}