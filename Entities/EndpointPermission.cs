namespace restapi.inventarios.Entities
{
    public class EndpointPermission
    {
        public int Id { get; set; }
        public string Route { get; set; } = string.Empty; // ejemplo: "/api/productos"
        public string HttpMethod { get; set; } = string.Empty; // GET/POST/PUT/DELETE
        public string AllowedRolesCsv { get; set; } = "*"; // "*" => cualquiera; lista separada por coma
        public bool IsEnabled { get; set; } = true; // desactivado => no accesible
    }
}
