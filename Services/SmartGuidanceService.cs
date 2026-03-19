using System.Text.RegularExpressions;

namespace Ai_Fund.Services;

public interface ISmartGuidanceService
{
    bool IsPersonalQuery(string query);
    Task<string> GenerateGuidedAnswerAsync(string query, string context);
    int ExtractAmount(string query);
    int ExtractYears(string query);
    string ExtractInvestmentType(string query);
    string GenerateInvestmentAdvice(int amount);
    bool IsReturnsQuery(string query);
    bool IsFDQuery(string query);
    string GenerateReturnsGuidance();
    string GenerateReturnsForAmount(int monthlyAmount, int years);
    string GenerateFDReturns(int amount, int years);
    string GenerateUniversalReturns(string investmentType, int amount, int years);
    string CompareInvestments(string type1, string type2, int amount, int years);
}

public class SmartGuidanceService : ISmartGuidanceService
{
    private readonly ILLMService _llmService;

    public SmartGuidanceService(ILLMService llmService)
    {
        _llmService = llmService;
    }

    public bool IsPersonalQuery(string query)
    {
        query = query.ToLower();

        return query.Contains("i have") ||
               query.Contains("my money") ||
               query.Contains("should i invest") ||
               query.Contains("i should invest") ||
               query.Contains("where should i") ||
               query.Contains("where i should") ||
               query.Contains("how much should i") ||
               query.Contains("how much i should") ||
               query.Contains("what should i do") ||
               query.Contains("i want to invest") ||
               query.Contains("what will be") ||
               query.Contains("what will i get") ||
               (query.Contains("i") && query.Contains("invest") && query.Contains("where")) ||
               (query.Contains("i") && query.Contains("invest") && query.Contains("how much"));
    }

    public int ExtractAmount(string query)
    {
        var match = Regex.Match(query, @"\d+");
        return match.Success ? int.Parse(match.Value) : 0;
    }

    public int ExtractYears(string query)
    {
        query = query.ToLower();
        
        // Look for "X year" or "X years"
        var yearMatch = Regex.Match(query, @"(\d+)\s*year");
        if (yearMatch.Success)
        {
            return int.Parse(yearMatch.Groups[1].Value);
        }
        
        // Default to 1 year if asking about returns but no timeframe specified
        if (query.Contains("one year") || query.Contains("1 year"))
            return 1;
        
        return 0;
    }

    public bool IsReturnsQuery(string query)
    {
        query = query.ToLower();
        return query.Contains("return") || 
               query.Contains("profit") || 
               query.Contains("gain") ||
               query.Contains("earn") ||
               query.Contains("how much") ||
               (query.Contains("what") && query.Contains("get")) ||
               (query.Contains("what about") && query.Contains("return"));
    }

    public bool IsFDQuery(string query)
    {
        query = query.ToLower();
        return query.Contains("fd") || query.Contains("fixed deposit");
    }
    
    public string ExtractInvestmentType(string query)
    {
        query = query.ToLower();
        
        if (query.Contains("sip")) return "SIP";
        if (query.Contains("fd") || query.Contains("fixed deposit")) return "FD";
        if (query.Contains("mutual fund")) return "Mutual Fund";
        if (query.Contains("gold")) return "Gold";
        if (query.Contains("stock") || query.Contains("equity")) return "Stock";
        if (query.Contains("ppf")) return "PPF";
        if (query.Contains("nps")) return "NPS";
        if (query.Contains("bond")) return "Bond";
        if (query.Contains("real estate") || query.Contains("property")) return "Real Estate";
        
        return string.Empty;
    }
    
    public string GenerateUniversalReturns(string investmentType, int amount, int years)
    {
        // Define typical returns for different investment types
        var returns = investmentType switch
        {
            "FD" => (Rate: 0.065, Name: "Fixed Deposit", Risk: "Low", Type: "Fixed"),
            "SIP" => (Rate: 0.12, Name: "SIP (Mutual Funds)", Risk: "Medium", Type: "Market-linked"),
            "Mutual Fund" => (Rate: 0.12, Name: "Mutual Fund", Risk: "Medium", Type: "Market-linked"),
            "Gold" => (Rate: 0.08, Name: "Gold", Risk: "Low-Medium", Type: "Commodity"),
            "Stock" => (Rate: 0.15, Name: "Stocks", Risk: "High", Type: "Market-linked"),
            "PPF" => (Rate: 0.071, Name: "PPF", Risk: "Very Low", Type: "Government-backed"),
            "NPS" => (Rate: 0.10, Name: "NPS", Risk: "Medium", Type: "Pension scheme"),
            "Bond" => (Rate: 0.07, Name: "Bonds", Risk: "Low", Type: "Fixed income"),
            "Real Estate" => (Rate: 0.09, Name: "Real Estate", Risk: "Medium", Type: "Property"),
            _ => (Rate: 0.10, Name: "Investment", Risk: "Medium", Type: "General")
        };
        
        var maturityAmount = amount * Math.Pow(1 + returns.Rate, years);
        var profit = maturityAmount - amount;
        
        return $@"With ₹{amount:N0} in {returns.Name} for {years} year{(years > 1 ? "s" : "")}:

📊 Returns:
Invested: ₹{amount:N0}
Expected value: ₹{maturityAmount:N0} (approx)
Estimated profit: ₹{profit:N0}

(assuming ~{returns.Rate * 100:F1}% annual return)

🎯 Note:
Risk level: {returns.Risk}
Type: {returns.Type}
{(returns.Type == "Market-linked" ? "Returns may vary based on market performance." : "Returns are relatively stable.")}";
    }
    
    public string CompareInvestments(string type1, string type2, int amount, int years)
    {
        // Universal investment data
        var investmentData = new Dictionary<string, (double Rate, string Risk, string Type)>
        {
            { "FD", (0.065, "Low", "Fixed") },
            { "Fixed Deposit", (0.065, "Low", "Fixed") },
            { "SIP", (0.12, "Medium", "Market-linked") },
            { "Mutual Fund", (0.12, "Medium", "Market-linked") },
            { "Gold", (0.08, "Low-Medium", "Commodity") },
            { "Stock", (0.15, "High", "Market-linked") },
            { "Stocks", (0.15, "High", "Market-linked") },
            { "PPF", (0.071, "Very Low", "Government-backed") },
            { "NPS", (0.10, "Medium", "Pension") },
            { "Bond", (0.07, "Low", "Fixed income") },
            { "Bonds", (0.07, "Low", "Fixed income") },
            { "Real Estate", (0.09, "Medium", "Property") }
        };
        
        // Get data for both investment types
        if (!investmentData.TryGetValue(type1, out var returns1))
        {
            returns1 = (0.10, "Medium", "General");
        }
        
        if (!investmentData.TryGetValue(type2, out var returns2))
        {
            returns2 = (0.10, "Medium", "General");
        }
        
        var profit1 = amount * (Math.Pow(1 + returns1.Rate, years) - 1);
        var profit2 = amount * (Math.Pow(1 + returns2.Rate, years) - 1);
        var difference = Math.Abs(profit2 - profit1);
        var better = profit1 > profit2 ? type1 : type2;
        var betterProfit = Math.Max(profit1, profit2);
        
        return $@"Comparison for ₹{amount:N0} invested for {years} year{(years > 1 ? "s" : "")}:

📊 {type1}:
• Profit: ₹{profit1:N0} ({returns1.Rate * 100:F1}%)
• Risk: {returns1.Risk}
• Type: {returns1.Type}

📊 {type2}:
• Profit: ₹{profit2:N0} ({returns2.Rate * 100:F1}%)
• Risk: {returns2.Risk}
• Type: {returns2.Type}

💡 Difference: ₹{difference:N0} more with {better}

🎯 Tip:
Choose based on your risk appetite and investment goals.";
    }

    public string GenerateFDReturns(int amount, int years)
    {
        // FD typically gives 6-7% annual interest
        var interestRate = 0.065; // 6.5% average
        var maturityAmount = amount * Math.Pow(1 + interestRate, years);
        var interest = maturityAmount - amount;

        return $@"With ₹{amount:N0} in FD for {years} year{(years > 1 ? "s" : "")}, you'll get:

📊 Returns:
Invested: ₹{amount:N0}
Maturity amount: ₹{maturityAmount:N0} (approx)
Interest earned: ₹{interest:N0}

(assuming ~6.5% annual interest)

🎯 Note:
FD provides fixed, guaranteed returns. Good for safety and short-term goals.";
    }

    public string GenerateReturnsGuidance()
    {
        return @"Returns depend on market conditions, but here's a realistic idea:

📊 Example:
₹5,000/month SIP for 10 years → ~₹10-12 lakh (approx)

(assuming ~12% annual return)

🎯 Tip:
Long-term investing gives better results than short-term. SIP works best for 5+ years.";
    }

    public string GenerateReturnsForAmount(int monthlyAmount, int years)
    {
        // Simple compound interest calculation for SIP
        // Using ~12% annual return assumption
        var monthlyRate = 0.12 / 12;
        var months = years * 12;
        var futureValue = monthlyAmount * (((Math.Pow(1 + monthlyRate, months) - 1) / monthlyRate) * (1 + monthlyRate));
        var totalInvested = monthlyAmount * months;
        var returns = futureValue - totalInvested;

        return $@"Returns depend on market, but here's a realistic estimate:

📊 Your scenario:
₹{monthlyAmount:N0}/month for {years} year{(years > 1 ? "s" : "")}

Total invested: ₹{totalInvested:N0}
Expected value: ₹{futureValue:N0} (approx)
Estimated returns: ₹{returns:N0}

(assuming ~12% annual return)

🎯 Tip:
Longer investment periods give better compounding results.";
    }

    public string GenerateInvestmentAdvice(int amount)
    {
        if (amount >= 10000)
        {
            var half = amount / 2;
            return $@"Good question. With ₹{amount:N0}, you can balance safety and growth.

💡 Simple approach:
• ₹{half:N0} in FD → safe, stable returns
• ₹{half:N0} in SIP → growth over time

🎯 Tip:
Use FD for short-term needs, and SIP for long-term wealth building.";
        }
        else if (amount >= 5000)
        {
            return $@"With ₹{amount:N0}, SIP is a good starting option for long-term growth.

💡 Simple idea:
• Start with ₹{amount:N0}/month SIP
• Stay invested for 5-10 years

🎯 Tip:
Consistency matters more than amount in SIP.";
        }
        else if (amount > 0)
        {
            return $@"With ₹{amount:N0}, you can start small with SIP.

💡 Simple approach:
• Begin with ₹{amount:N0}/month
• Increase gradually as income grows

🎯 Tip:
Starting early is more important than starting big.";
        }

        return string.Empty;
    }

    public async Task<string> GenerateGuidedAnswerAsync(string query, string context)
    {
        var prompt = $@"
You are Miria, a smart financial assistant.

User asked: {query}

Context: {context}

Give a helpful, practical answer in 4-6 lines.

RULES:
- Start with Good question. It depends on your goals/risk level/time horizon
- Give 2-3 options with bullet points
- Include ONE simple example with numbers
- End with a practical tip
- Do NOT include prompt instructions in your answer
- Do NOT say I cannot give advice

Answer directly without repeating these instructions:";

        return await _llmService.GenerateStructuredAsync(prompt);
    }
}
