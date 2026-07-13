using System.Text.Json;

class Brapi : IBrapi
{
    private readonly HttpClient _httpClient;

    public Brapi()
    {
        _httpClient = new HttpClient();
    }

    public async Task<BrapiResponse?> GetStockQuoteAsync(string symbol)
    {
        try
        {
            var response = await _httpClient.GetAsync($"https://brapi.dev/api/quote/{symbol}");
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<BrapiResponse>(jsonResponse, options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Falha ao obter a cotação do ativo '{symbol}'. Detalhes: {ex.Message}");
            return null;
        }
    }
}