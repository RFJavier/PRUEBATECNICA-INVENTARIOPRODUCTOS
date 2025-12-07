using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using restapi.inventarios.Entities;

namespace restapi.inventarios.Data.Repositories
{
    public class ProductoRepository
    {
        private readonly AppDbContext _db;
        public ProductoRepository(AppDbContext db) { _db = db; }

        public async Task<int> CreateAsync(string codigo, string producto, decimal precio, CancellationToken ct = default)
        {
            await using var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Productos (codigo, producto, precio) VALUES (@codigo, @producto, @precio); SELECT SCOPE_IDENTITY();";
            cmd.CommandType = CommandType.Text;

            var p1 = cmd.CreateParameter(); p1.ParameterName = "@codigo"; p1.Value = codigo; p1.DbType = DbType.String; cmd.Parameters.Add(p1);
            var p2 = cmd.CreateParameter(); p2.ParameterName = "@producto"; p2.Value = producto; p2.DbType = DbType.String; cmd.Parameters.Add(p2);
            var p3 = cmd.CreateParameter(); p3.ParameterName = "@precio"; p3.Value = precio; p3.DbType = DbType.Decimal; cmd.Parameters.Add(p3);

            var result = await cmd.ExecuteScalarAsync(ct);
            return Convert.ToInt32(result);
        }

        public async Task<Producto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            await using var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT idpro, codigo, producto, precio FROM Productos WHERE idpro = @idpro";
            cmd.CommandType = CommandType.Text;

            var p1 = cmd.CreateParameter();
            p1.ParameterName = "@idpro";
            p1.Value = id;
            p1.DbType = DbType.Int32;
            cmd.Parameters.Add(p1);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new Producto
                {
                    Id = reader.GetInt32(reader.GetOrdinal("idpro")),
                    Codigo = reader.GetString(reader.GetOrdinal("codigo")),
                    Nombre = reader.GetString(reader.GetOrdinal("producto")),
                    Precio = reader.GetDecimal(reader.GetOrdinal("precio"))
                };
            }
            return null;
        }

        public async Task<List<Producto>> GetAllAsync(CancellationToken ct = default)
        {
            await using var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT idpro, codigo, producto, precio FROM Productos ORDER BY idpro";
            cmd.CommandType = CommandType.Text;

            var result = new List<Producto>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                result.Add(new Producto
                {
                    Id = reader.GetInt32(reader.GetOrdinal("idpro")),
                    Codigo = reader.GetString(reader.GetOrdinal("codigo")),
                    Nombre = reader.GetString(reader.GetOrdinal("producto")),
                    Precio = reader.GetDecimal(reader.GetOrdinal("precio"))
                });
            }
            return result;
        }

        public async Task<bool> UpdateAsync(int id, string codigo, string producto, decimal precio, CancellationToken ct = default)
        {
            await using var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Productos SET codigo = @codigo, producto = @producto, precio = @precio WHERE idpro = @idpro";
            cmd.CommandType = CommandType.Text;

            var p1 = cmd.CreateParameter(); p1.ParameterName = "@idpro"; p1.Value = id; p1.DbType = DbType.Int32; cmd.Parameters.Add(p1);
            var p2 = cmd.CreateParameter(); p2.ParameterName = "@codigo"; p2.Value = codigo; p2.DbType = DbType.String; cmd.Parameters.Add(p2);
            var p3 = cmd.CreateParameter(); p3.ParameterName = "@producto"; p3.Value = producto; p3.DbType = DbType.String; cmd.Parameters.Add(p3);
            var p4 = cmd.CreateParameter(); p4.ParameterName = "@precio"; p4.Value = precio; p4.DbType = DbType.Decimal; cmd.Parameters.Add(p4);

            var rows = await cmd.ExecuteNonQueryAsync(ct);
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            await using var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Productos WHERE idpro = @idpro";
            cmd.CommandType = CommandType.Text;

            var p1 = cmd.CreateParameter();
            p1.ParameterName = "@idpro";
            p1.Value = id;
            p1.DbType = DbType.Int32;
            cmd.Parameters.Add(p1);

            var rows = await cmd.ExecuteNonQueryAsync(ct);
            return rows > 0;
        }

        public async Task<List<Producto>> SearchByCodigoAsync(string codigo, CancellationToken ct = default)
        {
            await using var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT idpro, codigo, producto, precio FROM Productos WHERE codigo LIKE @codigo";
            cmd.CommandType = CommandType.Text;

            var p1 = cmd.CreateParameter();
            p1.ParameterName = "@codigo";
            p1.Value = $"%{codigo}%";
            p1.DbType = DbType.String;
            cmd.Parameters.Add(p1);

            var result = new List<Producto>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                result.Add(new Producto
                {
                    Id = reader.GetInt32(reader.GetOrdinal("idpro")),
                    Codigo = reader.GetString(reader.GetOrdinal("codigo")),
                    Nombre = reader.GetString(reader.GetOrdinal("producto")),
                    Precio = reader.GetDecimal(reader.GetOrdinal("precio"))
                });
            }
            return result;
        }

        public async Task<Producto?> GetByCodigoExactoAsync(string codigo, CancellationToken ct = default)
        {
            await using var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT TOP 1 idpro, codigo, producto, precio FROM Productos WHERE codigo = @codigo";
            cmd.CommandType = CommandType.Text;

            var p1 = cmd.CreateParameter();
            p1.ParameterName = "@codigo";
            p1.Value = codigo;
            p1.DbType = DbType.String;
            cmd.Parameters.Add(p1);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new Producto
                {
                    Id = reader.GetInt32(reader.GetOrdinal("idpro")),
                    Codigo = reader.GetString(reader.GetOrdinal("codigo")),
                    Nombre = reader.GetString(reader.GetOrdinal("producto")),
                    Precio = reader.GetDecimal(reader.GetOrdinal("precio"))
                };
            }
            return null;
        }
    }
}
