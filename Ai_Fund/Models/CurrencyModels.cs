using System.Text.Json.Serialization;

namespace Ai_Fund.Models;

public class CurrencyResponse
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("usd")]
    public Dictionary<string, double> Usd { get; set; } = new();
}
