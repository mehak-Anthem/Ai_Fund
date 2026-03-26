namespace Ai_Fund.Services;

public interface IMarketService
{
    Task<object> GetMarketOverviewAsync();
    Task<object> FetchLiveIndexAsync(string symbol);
}
