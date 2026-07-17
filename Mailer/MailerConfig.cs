using System.Text.Json;

public class SmtpConfig
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Stock Quote Alert";
}

public class MailerConfig
{
    public string AlertEmail { get; set; } = string.Empty;
    public SmtpConfig Smtp { get; set; } = new();

    public static MailerConfig Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new InvalidOperationException($"Arquivo de configuração '{filePath}' não encontrado.");
        }

        string json = File.ReadAllText(filePath);

        var config = JsonSerializer.Deserialize<MailerConfig>(json);
        if (config == null)
        {
            throw new InvalidOperationException("O arquivo configuration.json está vazio ou inválido.");
        }

        return config;
    }
}
