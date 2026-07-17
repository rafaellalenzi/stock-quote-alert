using System.Net.Mail;
using System.Net;

public class Mailer: IMailer
{
    private readonly MailerConfig _config;

    public Mailer()
    {
        try
        {
            _config = MailerConfig.Load("configuration.json");
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException($"Erro ao carregar a configuração do Mailer: {ex.Message}", ex);
        }

        AssertConfiguration();
    }

    public async Task SendEmailAsync(string subject, string body)
    {
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

    private void AssertConfiguration()
    {
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
    }
}