using Microsoft.Data.SqlClient;

public class ClienteRepository
{
    private readonly string _connectionString;

    public ClienteRepository(string connectionString) => _connectionString = connectionString;

    // Busca un cliente por su número de documento recibido desde la pantalla de búsqueda
    public string? BuscarNombrePorDocumento(string documento)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();
        const string sql = "SELECT Nombre FROM Clientes WHERE Documento = @documento";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@documento", documento);
        return cmd.ExecuteScalar()?.ToString();
    }

    // Busca un cliente por documento usando SqlCommand con parámetros (@documento) para evitar inyección SQL
    public string? BuscarNombrePorDocumentoSeguro(string documento)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();
        var sql = "SELECT Nombre FROM Clientes WHERE Documento = @documento";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@documento", documento);
        var result = cmd.ExecuteScalar();
        return result as string;
    }
}
