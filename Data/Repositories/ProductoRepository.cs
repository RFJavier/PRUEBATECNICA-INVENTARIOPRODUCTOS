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

        public async Task<int> CreateAsync(string producto, decimal precio)
        {
            await using var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "sp_Producto_Create";
            cmd.CommandType = CommandType.StoredProcedure;
            var p1 = cmd.CreateParameter(); p1.ParameterName = "@producto"; p1.Value = producto; p1.DbType = DbType.String; cmd.Parameters.Add(p1);
            var p2 = cmd.CreateParameter(); p2.ParameterName = "@precio"; p2.Value = precio; p2.DbType = DbType.Decimal; cmd.Parameters.Add(p2);

            var id = 0;
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                // sp devuelve SELECT SCOPE_IDENTITY() AS idpro
                id = Convert.ToInt32(reader[0]);
            }
            return id;
        }

        public async Task<Producto?> GetByIdAsync(int id)
        {
            var list = await _db.Productos.FromSqlRaw("EXEC sp_Producto_GetById @idpro",
                new Microsoft.Data.SqlClient.SqlParameter("@idpro", id)).ToListAsync();
            return list.FirstOrDefault();
        }

        public async Task<List<Producto>> GetAllAsync()
        {
            return await _db.Productos.FromSqlRaw("EXEC sp_Producto_GetAll").ToListAsync();
        }

        public async Task<bool> UpdateAsync(int id, string producto, decimal precio)
        {
            var rows = await _db.Database.ExecuteSqlRawAsync(
                "EXEC sp_Producto_Update @idpro, @producto, @precio",
                new Microsoft.Data.SqlClient.SqlParameter("@idpro", id),
                new Microsoft.Data.SqlClient.SqlParameter("@producto", producto),
                new Microsoft.Data.SqlClient.SqlParameter("@precio", precio)
            );
            return rows >= 0; // UPDATE vía proc suele devolver -1
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var rows = await _db.Database.ExecuteSqlRawAsync(
                "EXEC sp_Producto_Delete @idpro",
                new Microsoft.Data.SqlClient.SqlParameter("@idpro", id)
            );
            return rows >= 0;
        }
    }
}
