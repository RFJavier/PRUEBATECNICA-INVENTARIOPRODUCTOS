namespace restapi.inventarios.Models
{
    /// <summary>
    /// Respuesta estándar de la API con mensaje de notificación
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Operación exitosa")
        {
            return new ApiResponse<T> { Success = true, Message = message, Data = data };
        }

        public static ApiResponse<T> Created(T data, string message = "Recurso creado exitosamente")
        {
            return new ApiResponse<T> { Success = true, Message = message, Data = data };
        }

        public static ApiResponse<T> Fail(string message, List<string>? errors = null)
        {
            return new ApiResponse<T> { Success = false, Message = message, Errors = errors };
        }
    }

    /// <summary>
    /// Respuesta estándar sin datos
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string>? Errors { get; set; }

        public static ApiResponse Ok(string message = "Operación exitosa")
        {
            return new ApiResponse { Success = true, Message = message };
        }

        public static ApiResponse Fail(string message, List<string>? errors = null)
        {
            return new ApiResponse { Success = false, Message = message, Errors = errors };
        }
    }
}
