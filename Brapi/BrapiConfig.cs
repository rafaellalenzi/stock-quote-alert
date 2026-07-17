using System.Text.Json;

public class BrapiConfig
{
    public string? BrapiToken { get; set; }

    public static BrapiConfig Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new InvalidOperationException($"Arquivo de configuração '{filePath}' não encontrado.");
        }

        string json = File.ReadAllText(filePath);

        try
        {
            return JsonSerializer.Deserialize<BrapiConfig>(json)
                ?? throw new InvalidOperationException("Configuração Brapi inválida no arquivo de configuração.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"O arquivo '{filePath}' não contém um JSON válido: {ex.Message}", ex);
        }
    }
}