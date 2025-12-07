using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace restapi.inventarios.Migrations
{
    public partial class EndpointPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EndpointPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Route = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HttpMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AllowedRolesCsv = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EndpointPermissions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EndpointPermissions_Route_HttpMethod",
                table: "EndpointPermissions",
                columns: new[] { "Route", "HttpMethod" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "EndpointPermissions");
        }
    }
}
