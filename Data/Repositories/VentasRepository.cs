using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using restapi.inventarios.Entities;
using restapi.inventarios.Services;

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

            // Parámetro OUTPUT para obtener el ID generado
            var pIdVenta = new SqlParameter("@idventa", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(pIdVenta);

            await cmd.ExecuteNonQueryAsync(ct);
            
            var idventa = pIdVenta.Value is int i ? i : Convert.ToInt32(pIdVenta.Value);
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
            cmd.CommandText = "sp_Ventas_GetAll_Detalles";
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

            // Verificar si existe en la misma conexión
            await using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(1) FROM EncabezadoVentas WHERE idventa = @idventa";
            checkCmd.CommandType = CommandType.Text;
            var pCheck = checkCmd.CreateParameter(); pCheck.ParameterName = "@idventa"; pCheck.Value = enc.Id; pCheck.DbType = DbType.Int32; checkCmd.Parameters.Add(pCheck);
            var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync(ct)) > 0;
            if (!exists) return false;

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
            await using var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "sp_Ventas_Delete";
            cmd.CommandType = CommandType.StoredProcedure;

            var p1 = cmd.CreateParameter(); p1.ParameterName = "@idventa"; p1.Value = idventa; p1.DbType = DbType.Int32; cmd.Parameters.Add(p1);
            var pRet = cmd.CreateParameter(); pRet.ParameterName = "@RETURN_VALUE"; pRet.Direction = ParameterDirection.ReturnValue; pRet.DbType = DbType.Int32; cmd.Parameters.Add(pRet);

            await cmd.ExecuteNonQueryAsync(ct);
            var estado = (pRet.Value is int i) ? i : Convert.ToInt32(pRet.Value);
            return estado == 1;
        }

        public async Task<List<object>> GetAllWithDetallesAsync(CancellationToken ct = default)
        {
            var result = new List<object>();
            await using var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "sp_Ventas_GetAll_Detalles";
            cmd.CommandType = CommandType.StoredProcedure;

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var idventa = reader.GetInt32(reader.GetOrdinal("idventa"));
                var fecha = reader.GetDateTime(reader.GetOrdinal("fecha"));
                var vendedor = reader.GetString(reader.GetOrdinal("vendedor"));
                var total = reader.GetDecimal(reader.GetOrdinal("total"));
                var detallesJson = reader.IsDBNull(reader.GetOrdinal("detalles")) ? "[]" : reader.GetString(reader.GetOrdinal("detalles"));
                result.Add(new { idventa, fecha, vendedor, total, detalles = System.Text.Json.JsonSerializer.Deserialize<object>(detallesJson) });
            }
            return result;
        }

        public async Task<List<VentaReporteItem>> GetAllWithDetallesForReportAsync(CancellationToken ct = default)
        {
            var result = new List<VentaReporteItem>();
            await using var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);

            // Obtener encabezados
            await using var cmdEnc = conn.CreateCommand();
            cmdEnc.CommandText = @"
                SELECT e.idventa, e.fecha, e.vendedor, e.total
                FROM EncabezadoVentas e
                ORDER BY e.fecha DESC";
            cmdEnc.CommandType = CommandType.Text;

            var ventasDict = new Dictionary<int, VentaReporteItem>();
            await using (var reader = await cmdEnc.ExecuteReaderAsync(ct))
            {
                while (await reader.ReadAsync(ct))
                {
                    var venta = new VentaReporteItem
                    {
                        IdVenta = reader.GetInt32(reader.GetOrdinal("idventa")),
                        Fecha = reader.GetDateTime(reader.GetOrdinal("fecha")),
                        Vendedor = reader.GetString(reader.GetOrdinal("vendedor")),
                        Total = reader.GetDecimal(reader.GetOrdinal("total"))
                    };
                    ventasDict[venta.IdVenta] = venta;
                    result.Add(venta);
                }
            }

            if (result.Count == 0) return result;

            // Obtener detalles con nombre de producto
            await using var cmdDet = conn.CreateCommand();
            cmdDet.CommandText = @"
                SELECT d.idventa, d.idpro, p.producto, d.cantidad, d.precio, d.iva, d.total
                FROM DetalleVentas d
                LEFT JOIN Productos p ON d.idpro = p.idpro
                ORDER BY d.idventa";
            cmdDet.CommandType = CommandType.Text;

            await using (var reader = await cmdDet.ExecuteReaderAsync(ct))
            {
                while (await reader.ReadAsync(ct))
                {
                    var idventa = reader.GetInt32(reader.GetOrdinal("idventa"));
                    if (ventasDict.TryGetValue(idventa, out var venta))
                    {
                        venta.Detalles.Add(new DetalleReporteItem
                        {
                            IdProducto = reader.GetInt32(reader.GetOrdinal("idpro")),
                            NombreProducto = reader.IsDBNull(reader.GetOrdinal("producto")) ? null : reader.GetString(reader.GetOrdinal("producto")),
                            Cantidad = reader.GetInt32(reader.GetOrdinal("cantidad")),
                            Precio = reader.GetDecimal(reader.GetOrdinal("precio")),
                            Iva = reader.GetDecimal(reader.GetOrdinal("iva")),
                            Total = reader.GetDecimal(reader.GetOrdinal("total"))
                        });
                    }
                }
            }

            return result;
        }
    }
}
