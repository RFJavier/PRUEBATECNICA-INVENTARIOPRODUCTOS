using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WEBAPP.Models;

namespace WEBAPP.Services;

public class ReporteService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthService _authService;

    public ReporteService(IHttpClientFactory httpClientFactory, AuthService authService)
    {
        _httpClientFactory = httpClientFactory;
        _authService = authService;
    }

    private async Task<HttpClient> GetAuthenticatedClientAsync()
    {
        var client = _httpClientFactory.CreateClient("InventariosApi");
        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }

    public async Task<(bool Success, string Message, ReporteVentasInfo? Data)> GetReporteInfoAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        var client = await GetAuthenticatedClientAsync();
        var url = "/api/reportes/ventas/info";
        
        var queryParams = new List<string>();
        if (fechaInicio.HasValue)
            queryParams.Add($"fechaInicio={fechaInicio.Value:yyyy-MM-dd}");
        if (fechaFin.HasValue)
            queryParams.Add($"fechaFin={fechaFin.Value:yyyy-MM-dd}");
        
        if (queryParams.Count > 0)
            url += "?" + string.Join("&", queryParams);

        try
        {
            var response = await client.GetFromJsonAsync<ApiResponse<ReporteVentasInfo>>(url);
            return (response?.Success ?? false, response?.Message ?? "Error desconocido", response?.Data);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    public async Task<(bool Success, string Message, byte[]? Data, string? FileName)> DescargarExcelAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        return await DescargarReporteAsync("excel", fechaInicio, fechaFin);
    }

    public async Task<(bool Success, string Message, byte[]? Data, string? FileName)> DescargarPdfAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        return await DescargarReporteAsync("pdf", fechaInicio, fechaFin);
    }

    private async Task<(bool Success, string Message, byte[]? Data, string? FileName)> DescargarReporteAsync(string formato, DateTime? fechaInicio, DateTime? fechaFin)
    {
        var client = await GetAuthenticatedClientAsync();
        var url = $"/api/reportes/ventas/{formato}";
        
        var queryParams = new List<string>();
        if (fechaInicio.HasValue)
            queryParams.Add($"fechaInicio={fechaInicio.Value:yyyy-MM-dd}");
        if (fechaFin.HasValue)
            queryParams.Add($"fechaFin={fechaFin.Value:yyyy-MM-dd}");
        
        if (queryParams.Count > 0)
            url += "?" + string.Join("&", queryParams);

        try
        {
            var response = await client.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return (false, errorResponse?.Message ?? "Error al descargar el reporte", null, null);
                }
                catch
                {
                    return (false, $"Error {(int)response.StatusCode}: {response.ReasonPhrase}", null, null);
                }
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') 
                ?? $"reporte_ventas.{formato}";
            
            return (true, "Reporte descargado exitosamente", bytes, fileName);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null, null);
        }
    }
}
