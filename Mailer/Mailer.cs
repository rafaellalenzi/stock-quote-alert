using System.Net.Mail;
using System.Net;
using System.Text.Json;

public class Mailer: IMailer
{
    private readonly MailerConfig? _config;
    public Mailer()
    {
        MailerConfig? config;
        try
        {
            string json = File.ReadAllText("configuration.json");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            config = JsonSerializer.Deserialize<MailerConfig>(json, options);

            _config = config;
            AssertConfiguration();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Erro ao carregar a configuração: {ex.Message}");
            Console.WriteLine("Verifique se o arquivo configuration.json está no formato correto e contém todas as informações necessárias como em configuration.json.example.");
            return;
        }
    }

    public async Task SendEmailAsync(string subject, string body)
    {
        if (_config == null)
        {
            Console.WriteLine("ERROR: A configuração do e-mail não foi carregada.");
            return;
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            Console.WriteLine("ERROR: O assunto do e-mail não pode ser vazio.");
            return;
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            Console.WriteLine("ERROR: O corpo do e-mail não pode ser vazio.");
            return;
        }

        using (var client = new SmtpClient(_config.Smtp.Host, _config.Smtp.Port))
        {
            client.EnableSsl = _config.Smtp.EnableSsl;
            client.Credentials = new NetworkCredential(_config.Smtp.Username, _config.Smtp.Password);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config.Smtp.FromAddress, _config.Smtp.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            mailMessage.To.Add(_config.AlertEmail);

            await client.SendMailAsync(mailMessage);
        }
    }

    private MailerConfig AssertConfiguration()
    {
        if (_config == null)
        {
            throw new InvalidOperationException("A configuração do MailerService não foi carregada corretamente.");
        }

        if (string.IsNullOrWhiteSpace(_config.AlertEmail))
        {
            throw new InvalidOperationException("O endereço de e-mail de alerta não está configurado.");
        }

        if (string.IsNullOrWhiteSpace(_config.Smtp.Host) || _config.Smtp.Port <= 0)
        {
            throw new InvalidOperationException("Configuração SMTP inválida. Verifique o host e a porta.");
        }

        if (string.IsNullOrWhiteSpace(_config.Smtp.Username) || string.IsNullOrWhiteSpace(_config.Smtp.Password))
        {
            throw new InvalidOperationException("Nome de usuário ou senha SMTP não configurados.");
        }

        if (string.IsNullOrWhiteSpace(_config.Smtp.FromAddress))
        {
            throw new InvalidOperationException("O endereço de e-mail do remetente não está configurado.");
        }

        if (string.IsNullOrWhiteSpace(_config.Smtp.FromName))
        {
            throw new InvalidOperationException("O nome do remetente não está configurado.");
        }

        return _config;
    }
}