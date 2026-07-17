using System.Text.Json;

class Brapi : IBrapi
{
    private readonly HttpClient _httpClient;
    private readonly BrapiConfig _config;

    public Brapi()
    {
        _httpClient = new HttpClient();
        try
        {
            _config = BrapiConfig.Load("configuration.json");
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException($"Erro ao carregar a configuração do Brapi: {ex.Message}");
        }
    }

    public async Task<BrapiResponse?> GetStockQuoteAsync(string symbol)
    {
        HttpResponseMessage ?response = null;
        try
        {
            if (!string.IsNullOrWhiteSpace(_config.BrapiToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.BrapiToken);
            }

            response = await _httpClient.GetAsync($"https://brapi.dev/api/quote/{symbol}");
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<BrapiResponse>(jsonResponse, options);
        }
        catch (Exception ex)
        {
            if (response?.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine($"ERROR: Ativo '{symbol}' não encontrado.");
            }
            else if (response?.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine($"ERROR: Token de autenticação inválido ou ausente para o Brapi.");
            }
            else
            {
                Console.WriteLine($"ERROR: Falha ao obter a cotação do ativo '{symbol}'. Detalhes: {ex.Message}");
            }
            return null;
        }
    }
}