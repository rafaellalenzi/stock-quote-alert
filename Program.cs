using System.Globalization;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine("ERROR: Uso incorreto. Esperado: stock-quote-alert.exe <ATIVO> <PRECO_VENDA> <PRECO_COMPRA>");
            return;
        }

        if (string.IsNullOrWhiteSpace(args[0]))
        {
            Console.WriteLine("ERROR: O ativo não pode ser vazio.");
            return;
        }

        bool sellPriceValid = decimal.TryParse(args[1], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal sellPrice);
        bool buyPriceValid = decimal.TryParse(args[2], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal buyPrice);
        // CultureInfo.InvariantCulture é usado para garantir que o ponto seja entendido como separador decimal

        if (!sellPriceValid)
        {
            Console.WriteLine($"ERROR: O preço de venda inválido: '{args[1]}'");
            return;
        }

        if (!buyPriceValid)
        {
            Console.WriteLine($"ERROR: O preço de compra inválido: '{args[2]}'");
            return;
        }

        if (buyPrice >= sellPrice)
        {
            Console.WriteLine("ERROR: O preço de compra deve ser menor que o preço de venda.");
            return;
        }

        if (buyPrice < 0 || sellPrice < 0)
        {
            Console.WriteLine("ERROR: Os preços não podem ser negativos.");
            return;
        }

        if (!File.Exists("configuration.json"))
        {
            Console.WriteLine("ERROR: Arquivo de configuração não encontrado. Verifique se o arquivo configuration.json está presente.");
            return;
        }

        IBrapi brapi = new Brapi();
        IMailer mailer = new Mailer();

        bool sellAssetAlert = false;
        bool buyAssetAlert = false;

        while (true)
        {
            var apiResponse = await brapi.GetStockQuoteAsync(args[0]);

            var result = apiResponse?.Results?.FirstOrDefault();

            if (result == null || result.RegularMarketPrice == null)
            {
                Console.WriteLine("ERROR: Não foi possível obter o preço do ativo.");
                return;
            }

            decimal currentPrice = result.RegularMarketPrice.Value;

            if (currentPrice >= sellPrice)
            {
                if (!sellAssetAlert)
                {
                    var Subject = "Alerta de venda do ativo " + args[0];
                    var Body = BuildEmailBody(
                        args[0], 
                        currentPrice, 
                        sellPrice, 
                        result, 
                        "atingiu o preço de venda de referência", 
                        "Considere vender o ativo."
                    );

                    await mailer.SendEmailAsync(Subject, Body);

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
                    var Subject = "Alerta de compra do ativo " + args[0];
                    var Body = BuildEmailBody(
                        args[0], 
                        currentPrice, 
                        buyPrice, 
                        result, 
                        "atingiu o preço de compra de referência", 
                        "Considere comprar o ativo."
                    );

                    await mailer.SendEmailAsync(Subject, Body);

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
                $"Sugestão: {suggestion}";
        }
    }
}