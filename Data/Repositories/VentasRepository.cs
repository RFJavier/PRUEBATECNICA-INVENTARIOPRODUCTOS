using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using restapi.inventarios.Entities;

namespace restapi.inventarios.Data.Repositories
{
    public class VentasRepository
    {
        private readonly AppDbContext _db;
        public VentasRepository(AppDbContext db) { _db = db; }

        private static DataTable BuildDetallesTvp(IEnumerable<DetalleVenta> detalles)
        {
            var table = new DataTable();
            table.Columns.Add("fecha", typeof(DateTime));
            table.Columns.Add("idpro", typeof(int));
            table.Columns.Add("cantidad", typeof(int));
            table.Columns.Add("precio", typeof(decimal));
            table.Columns.Add("iva", typeof(decimal));
            table.Columns.Add("total", typeof(decimal));

            foreach (var d in detalles)
            {
                table.Rows.Add(d.Fecha, d.IdProducto, d.Cantidad, d.Precio, d.Iva, d.Total);
            }
            return table;
        }

        public async Task<int> CreateAsync(EncabezadoVenta enc, IEnumerable<DetalleVenta> detalles, CancellationToken ct = default)
        {
            await using var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "sp_Ventas_Create";
            cmd.CommandType = CommandType.StoredProcedure;

            var p1 = cmd.CreateParameter(); p1.ParameterName = "@fecha"; p1.Value = enc.Fecha; p1.DbType = DbType.DateTime2; cmd.Parameters.Add(p1);
            var p2 = cmd.CreateParameter(); p2.ParameterName = "@vendedor"; p2.Value = enc.Vendedor; p2.DbType = DbType.String; cmd.Parameters.Add(p2);
            var p3 = cmd.CreateParameter(); p3.ParameterName = "@total"; p3.Value = enc.Total; p3.DbType = DbType.Decimal; cmd.Parameters.Add(p3);

            var tvp = new SqlParameter("@Detalles", BuildDetallesTvp(detalles)) { SqlDbType = SqlDbType.Structured, TypeName = "TipoDetalleVenta" };
            cmd.Parameters.Add(tvp);

            var idventa = 0;
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                idventa = Convert.ToInt32(reader["idventa"]);
            }
            return idventa;
        }

        public async Task<(EncabezadoVenta? encabezado, List<DetalleVenta> detalles)> GetByIdAsync(int idventa, CancellationToken ct = default)
        {
            await using var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "sp_Ventas_GetById";
            cmd.CommandType = CommandType.StoredProcedure;
            var p1 = cmd.CreateParameter(); p1.ParameterName = "@idventa"; p1.Value = idventa; p1.DbType = DbType.Int32; cmd.Parameters.Add(p1);

            EncabezadoVenta? enc = null; var detalles = new List<DetalleVenta>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                enc = new EncabezadoVenta
                {
                    Id = reader.GetInt32(reader.GetOrdinal("idventa")),
                    Fecha = reader.GetDateTime(reader.GetOrdinal("fecha")),
                    Vendedor = reader.GetString(reader.GetOrdinal("vendedor")),
                    Total = reader.GetDecimal(reader.GetOrdinal("total"))
                };
            }
            if (await reader.NextResultAsync(ct))
            {
                while (await reader.ReadAsync(ct))
                {
                    var d = new DetalleVenta
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("idde")),
                        Fecha = reader.GetDateTime(reader.GetOrdinal("fecha")),
                        IdVenta = reader.GetInt32(reader.GetOrdinal("idventa")),
                        IdProducto = reader.GetInt32(reader.GetOrdinal("idpro")),
                        Cantidad = reader.GetInt32(reader.GetOrdinal("cantidad")),
                        Precio = reader.GetDecimal(reader.GetOrdinal("precio")),
                        Iva = reader.GetDecimal(reader.GetOrdinal("iva")),
                        Total = reader.GetDecimal(reader.GetOrdinal("total"))
                    };
                    detalles.Add(d);
                }
            }
            return (enc, detalles);
        }

        public async Task<List<EncabezadoVenta>> GetAllAsync(CancellationToken ct = default)
        {
            var list = new List<EncabezadoVenta>();
            await using var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "sp_Ventas_GetAll";
            cmd.CommandType = CommandType.StoredProcedure;
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                list.Add(new EncabezadoVenta
                {
                    Id = reader.GetInt32(reader.GetOrdinal("idventa")),
                    Fecha = reader.GetDateTime(reader.GetOrdinal("fecha")),
                    Vendedor = reader.GetString(reader.GetOrdinal("vendedor")),
                    Total = reader.GetDecimal(reader.GetOrdinal("total"))
                });
            }
            return list;
        }

        public async Task<bool> UpdateAsync(EncabezadoVenta enc, IEnumerable<DetalleVenta> detalles, CancellationToken ct = default)
        {
            await using var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "sp_Ventas_Update";
            cmd.CommandType = CommandType.StoredProcedure;

            var p0 = cmd.CreateParameter(); p0.ParameterName = "@idventa"; p0.Value = enc.Id; p0.DbType = DbType.Int32; cmd.Parameters.Add(p0);
            var p1 = cmd.CreateParameter(); p1.ParameterName = "@fecha"; p1.Value = enc.Fecha; p1.DbType = DbType.DateTime2; cmd.Parameters.Add(p1);
            var p2 = cmd.CreateParameter(); p2.ParameterName = "@vendedor"; p2.Value = enc.Vendedor; p2.DbType = DbType.String; cmd.Parameters.Add(p2);
            var p3 = cmd.CreateParameter(); p3.ParameterName = "@total"; p3.Value = enc.Total; p3.DbType = DbType.Decimal; cmd.Parameters.Add(p3);

            var tvp = new SqlParameter("@Detalles", BuildDetallesTvp(detalles)) { SqlDbType = SqlDbType.Structured, TypeName = "TipoDetalleVenta" };
            cmd.Parameters.Add(tvp);

            await cmd.ExecuteNonQueryAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int idventa, CancellationToken ct = default)
        {
            var rows = await _db.Database.ExecuteSqlRawAsync(
                "EXEC sp_Ventas_Delete @idventa",
                new SqlParameter("@idventa", idventa));
            return rows >= 0; // proc DELETE suele devolver -1
        }
    }
}
