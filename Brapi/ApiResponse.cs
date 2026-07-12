using System.Text.Json.Serialization;

public class Result
{
    [JsonPropertyName("regularMarketPrice")]
    public decimal? RegularMarketPrice { get; set; }

    [JsonPropertyName("regularMarketChangePercent")]
    public decimal? RegularMarketChangePercent { get; set; }

    [JsonPropertyName("regularMarketDayHigh")]
    public decimal? RegularMarketDayHigh { get; set; }

    [JsonPropertyName("regularMarketDayLow")]
    public decimal? RegularMarketDayLow { get; set; }

    [JsonPropertyName("regularMarketPreviousClose")]
    public decimal? RegularMarketPreviousClose { get; set; }

    [JsonPropertyName("fiftyTwoWeekLow")]
    public decimal? FiftyTwoWeekLow { get; set; }

    [JsonPropertyName("fiftyTwoWeekHigh")]
    public decimal? FiftyTwoWeekHigh { get; set; }
}

public class ApiResponse
{
    [JsonPropertyName("results")]
    public List<Result>? Results { get; set; }
}