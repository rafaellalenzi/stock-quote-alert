using System.Globalization;
using System.Text.Json;
using System.Net.Mail;
using System.Net;

if (args.Length != 3)
{
    Console.WriteLine("ERROR: uso incorreto. Esperado: stock-quote-alert.exe <ATIVO> <PRECO_VENDA> <PRECO_COMPRA>");
    return;
}

bool sellPriceValid = decimal.TryParse(args[1], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal sellPrice);
bool buyPriceValid = decimal.TryParse(args[2], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal buyPrice);

if (!sellPriceValid)
{
    Console.WriteLine($"ERROR: preço de venda inválido: '{args[1]}'");
    return;
}

if (!buyPriceValid)
{
    Console.WriteLine($"ERROR: preço de compra inválido: '{args[2]}'");
    return;
}

if (buyPrice >= sellPrice)
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
        throw new Exception("Arquivo de configuração inválido, verfique o arquivo configuration.json.example.");
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

bool sellAssetAlert = false;
bool buyAssetAlert = false;

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

    decimal currentPrice = result.RegularMarketPrice.Value;

    if (currentPrice >= sellPrice)
    {
        if (!sellAssetAlert)
        {
            var mensagem = new MailMessage
            {
                From = new MailAddress(config.Smtp.FromAddress, config.Smtp.FromName),
                Subject = "Alerta de venda do ativo " + args[0],
                Body = BuildEmailBody(args[0], currentPrice, sellPrice, result, "atingiu o preço de venda de referência", "Considere vender o ativo."),
                IsBodyHtml = false
            };
            mensagem.To.Add(config.AlertEmail);
            await smtpClient.SendMailAsync(mensagem);

            sellAssetAlert = true;
        }
    }
    else
    {
        sellAssetAlert = false;
    }

    if (currentPrice <= buyPrice)
    {
        if (!buyAssetAlert)
        {
            var mensagem = new MailMessage
            {
                From = new MailAddress(config.Smtp.FromAddress, config.Smtp.FromName),
                Subject = "Alerta de compra do ativo " + args[0],
                Body = BuildEmailBody(args[0], currentPrice, buyPrice, result, "atingiu o preço de compra de referência", "Considere comprar o ativo."),
                IsBodyHtml = false
            };
            mensagem.To.Add(config.AlertEmail);
            await smtpClient.SendMailAsync(mensagem);

            buyAssetAlert = true;
        }
    }
    else
    {
        buyAssetAlert = false;
    }

    await Task.Delay(TimeSpan.FromSeconds(30));
}

static string BuildEmailBody(string asset, decimal currentPrice, decimal limitPrice, Result result, string alertType, string suggestion)
{
    var range = result.RegularMarketChangePercent.HasValue
        ? $"{(result.RegularMarketChangePercent.Value >= 0 ? "+" : "")}{result.RegularMarketChangePercent.Value:0.00}%"
        : "Sem dados de variação disponíveis";

    return
        $"O ativo {asset} atingiu R$ {currentPrice:0.00}, {alertType} de R$ {limitPrice:0.00}.\n\n" +
        $"Variação no dia: {range}\n" +
        $"Fechamento anterior: R$ {result.RegularMarketPreviousClose:0.00}\n" +
        $"Faixa do dia: R$ {result.RegularMarketDayLow:0.00} - R$ {result.RegularMarketDayHigh:0.00}\n" +
        $"Faixa de 52 semanas: R$ {result.FiftyTwoWeekLow:0.00} - R$ {result.FiftyTwoWeekHigh:0.00}\n\n" +
        $"\nSugestão: {suggestion}";
}