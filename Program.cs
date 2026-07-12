using System.Globalization;
using System.Text.Json;
using System.Net.Mail;
using System.Net;

if (args.Length != 3)
{
    Console.WriteLine("ERROR: uso incorreto. Esperado: stock-quote-alert.exe <ATIVO> <PRECO_VENDA> <PRECO_COMPRA>");
    return;
}

bool preçoVenda = decimal.TryParse(args[1], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal precoVenda);
bool preçoCompra = decimal.TryParse(args[2], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal precoCompra);

if (!preçoVenda)
{
    Console.WriteLine($"ERROR: preço de venda inválido: '{args[1]}'");
    return;
}

if (!preçoCompra)
{
    Console.WriteLine($"ERROR: preço de compra inválido: '{args[2]}'");
    return;
}

if (precoCompra >= precoVenda)
{
    Console.WriteLine("ERROR: preço de compra deve ser menor que o preço de venda.");
    return;
}

AppConfig? config;
try
{
    string json = File.ReadAllText("configuration.json");
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    config = JsonSerializer.Deserialize<AppConfig>(json, options);

    if (config == null)
    {
        throw new Exception("Arquivo de configuração inválido, check configuration.json.example.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: erro ao carregar a configuração: {ex.Message}");
    return;
}

var httpClient = new HttpClient();
string url = $"https://brapi.dev/api/quote/{args[0]}";

var smtpClient = new SmtpClient(config.Smtp.Host, config.Smtp.Port)
{
    EnableSsl = config.Smtp.EnableSsl,
    Credentials = new NetworkCredential(config.Smtp.Username, config.Smtp.Password)
};

bool alertaVendaAtivo = false;
bool alertaCompraAtivo = false;

while (true)
{
    string responseBody = await httpClient.GetStringAsync(url);
    Console.WriteLine($"{responseBody}");

    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseBody);

    var result = apiResponse?.Results?.FirstOrDefault();

    if (result == null || result.RegularMarketPrice == null)
    {
        Console.WriteLine("ERROR: não foi possível obter o preço do ativo.");
        return;
    }

    decimal precoAtual = result.RegularMarketPrice.Value;

    if (precoAtual >= precoVenda)
    {
        if (!alertaVendaAtivo)
        {
            var mensagem = new MailMessage
            {
                From = new MailAddress(config.Smtp.FromAddress, config.Smtp.FromName),
                Subject = "Alerta de venda do ativo " + args[0],
                Body = buildEmailBody(args[0], precoAtual, precoVenda, result, "atingiu o preço de venda de referência", "Considere vender o ativo."),
                IsBodyHtml = false
            };
            mensagem.To.Add(config.AlertEmail);
            await smtpClient.SendMailAsync(mensagem);

            alertaVendaAtivo = true;
        }
    }
    else
    {
        alertaVendaAtivo = false;
    }

    if (precoAtual <= precoCompra)
    {
        if (!alertaCompraAtivo)
        {
            var mensagem = new MailMessage
            {
                From = new MailAddress(config.Smtp.FromAddress, config.Smtp.FromName),
                Subject = "Alerta de compra do ativo " + args[0],
                Body = buildEmailBody(args[0], precoAtual, precoCompra, result, "atingiu o preço de compra de referência", "Considere comprar o ativo."),
                IsBodyHtml = false
            };
            mensagem.To.Add(config.AlertEmail);
            await smtpClient.SendMailAsync(mensagem);

            alertaCompraAtivo = true;
        }
    }
    else
    {
        alertaCompraAtivo = false;
    }

    await Task.Delay(TimeSpan.FromSeconds(30));
}

static string buildEmailBody(string ativo, decimal precoAtual, decimal precoLimite, Result result, string tipoAlerta, string sugestao)
{
    var variacao = result.RegularMarketChangePercent.HasValue
        ? $"{(result.RegularMarketChangePercent.Value >= 0 ? "+" : "")}{result.RegularMarketChangePercent.Value:0.00}%"
        : "Sem dados de variação disponíveis";

    return
        $"O ativo {ativo} atingiu R$ {precoAtual:0.00}, {tipoAlerta} de R$ {precoLimite:0.00}.\n\n" +
        $"Variação no dia: {variacao}\n" +
        $"Fechamento anterior: R$ {result.RegularMarketPreviousClose:0.00}\n" +
        $"Faixa do dia: R$ {result.RegularMarketDayLow:0.00} - R$ {result.RegularMarketDayHigh:0.00}\n" +
        $"Faixa de 52 semanas: R$ {result.FiftyTwoWeekLow:0.00} - R$ {result.FiftyTwoWeekHigh:0.00}\n\n" +
        $"\nSugestão: {sugestao}";
}