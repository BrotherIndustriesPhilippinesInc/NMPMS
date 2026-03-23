using Npgsql;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public class dbconfig
{
    private readonly IConfiguration _configuration;

    public dbconfig(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // PostgreSQL connection
    public NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(
            _configuration.GetConnectionString("conn"));
    }

    // MSSQL connection
    public SqlConnection GetSqlServerConnection()
    {
        return new SqlConnection(
            _configuration.GetConnectionString("conn2"));
    }
}