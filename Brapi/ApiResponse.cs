using System.Text.Json.Serialization;

public class Result
{
    [JsonPropertyName("regularMarketPrice")]
    public decimal? RegularMarketPrice { get; set; }
}

public class ApiResponse
{
    [JsonPropertyName("results")]
    public List<Result>? Results { get; set; }
}