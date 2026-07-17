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

        return JsonSerializer.Deserialize<BrapiConfig>(json)
            ?? throw new InvalidOperationException("Configuração Brapi inválida no arquivo de configuração.");
    }
}