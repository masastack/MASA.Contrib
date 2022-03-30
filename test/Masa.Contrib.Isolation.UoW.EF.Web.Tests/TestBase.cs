namespace Masa.Contrib.Isolation.UoW.EF.Web.Tests;

public class TestBase : IDisposable
{
    protected readonly string _connectionString = "DataSource=:memory:";
    protected readonly SqliteConnection Connection;

    protected TestBase()
    {
        Connection = new SqliteConnection(_connectionString);
        Connection.Open();
    }

    public void Dispose()
    {
        Connection.Close();
    }
}
