using System.Text;
using System.Text.Json;

namespace AegisCore.Services;

public class MlClientService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IConfiguration _configuration;
    public MlClientService(IHttpClientFactory clientFactory, IConfiguration configuration)
    {
        _clientFactory = clientFactory;
        _configuration = configuration;
    }

    public async Task<string> AnalyzeAsync(object payload)
    {
        var baseUrl = _configuration["MlService:BaseUrl"] ?? "http://127.0.0.1:8001";
        var client = _clientFactory.CreateClient();
        try
        {
            var json = JsonSerializer.Serialize(payload);
            var response = await client.PostAsync($"{baseUrl}/api/v1/analyze", new StringContent(json, Encoding.UTF8, "application/json"));
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { status = "offline", message = ex.Message });
        }
    }
}
