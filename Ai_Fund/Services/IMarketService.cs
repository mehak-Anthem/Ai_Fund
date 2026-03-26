using System.Threading.Tasks;
using System.Collections.Generic;

namespace Ai_Fund.Services;

public interface IMarketService
{
    Task<object> GetMarketOverviewAsync();
    Task<object> FetchLiveIndexAsync(string symbol);
    Task<List<double?>> GetIndexChartAsync(string symbol, string range);
    Task<object> GetYahooNewsAsync(string query);
}


