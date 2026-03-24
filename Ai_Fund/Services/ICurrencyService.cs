namespace Ai_Fund.Services;

public interface ICurrencyService
{
    Task<double> GetUsdToInrRateAsync();
}
