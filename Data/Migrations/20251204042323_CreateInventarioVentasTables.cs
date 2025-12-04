using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRUEBATECNICA.STEFFANINIGROUP.INVENTARIO.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateInventarioVentasTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EncabezadoVentas",
                columns: table => new
                {
                    idventa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    vendedor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    total = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncabezadoVentas", x => x.idventa);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    idpro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    producto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.idpro);
                });

            migrationBuilder.CreateTable(
                name: "DetalleVentas",
                columns: table => new
                {
                    idde = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    idventa = table.Column<int>(type: "int", nullable: false),
                    idpro = table.Column<int>(type: "int", nullable: false),
                    cantidad = table.Column<int>(type: "int", nullable: false),
                    precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    iva = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    total = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleVentas", x => x.idde);
                    table.ForeignKey(
                        name: "FK_DetalleVentas_EncabezadoVentas_idventa",
                        column: x => x.idventa,
                        principalTable: "EncabezadoVentas",
                        principalColumn: "idventa",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetalleVentas_Productos_idpro",
                        column: x => x.idpro,
                        principalTable: "Productos",
                        principalColumn: "idpro",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetalleVentas_idpro",
                table: "DetalleVentas",
                column: "idpro");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleVentas_idventa",
                table: "DetalleVentas",
                column: "idventa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalleVentas");

            migrationBuilder.DropTable(
                name: "EncabezadoVentas");

            migrationBuilder.DropTable(
                name: "Productos");
        }
    }
}
