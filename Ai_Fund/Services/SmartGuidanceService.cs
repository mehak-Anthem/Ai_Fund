using System.Text.RegularExpressions;

namespace Ai_Fund.Services;

public interface ISmartGuidanceService
{
    bool IsPersonalQuery(string query);
    Task<string> GenerateGuidedAnswerAsync(string query, string context);
    int ExtractAmount(string query);
    int ExtractYears(string query);
    string ExtractInvestmentType(string query);
    Task<string> GenerateInvestmentAdviceAsync(int amount);
    bool IsReturnsQuery(string query);
    bool IsFDQuery(string query);
    Task<string> GenerateReturnsGuidanceAsync();
    Task<string> GenerateReturnsForAmountAsync(int monthlyAmount, int years);
    Task<string> GenerateFDReturnsAsync(int amount, int years);
    Task<string> GenerateUniversalReturnsAsync(string investmentType, int amount, int years);
    Task<string> CompareInvestmentsAsync(string type1, string type2, int amount, int years);
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
               query.Contains("if i invest") ||
               query.Contains("what will be") ||
               query.Contains("what will i get") ||
               query.Contains("how much will i get") ||
               query.Contains("can i invest") ||
               query.Contains("should i go for") ||
               query.Contains("is it good for me") ||
               (query.Contains("i") && query.Contains("invest") && query.Contains("where")) ||
               (query.Contains("i") && query.Contains("invest") && query.Contains("how much")) ||
               (query.Contains("i") && query.Contains("invest") && query.Contains("return"));
    }

    public int ExtractAmount(string query)
    {
        query = query.ToLower().Replace(",", "");
        
        // Handle "X lakh"
        var lakhMatch = Regex.Match(query, @"(\d+(\.\d+)?)\s*lakh");
        if (lakhMatch.Success)
        {
            return (int)(double.Parse(lakhMatch.Groups[1].Value) * 100000);
        }
        
        // Handle "X crore" or "X cr"
        var croreMatch = Regex.Match(query, @"(\d+(\.\d+)?)\s*(crore|cr)");
        if (croreMatch.Success)
        {
            return (int)(double.Parse(croreMatch.Groups[1].Value) * 10000000);
        }
        
        // Handle "X k" or "X thousand"
        var thousandMatch = Regex.Match(query, @"(\d+(\.\d+)?)\s*(k|thousand)");
        if (thousandMatch.Success)
        {
            return (int)(double.Parse(thousandMatch.Groups[1].Value) * 1000);
        }
        
        // Handle pure numbers (e.g., 100000)
        var match = Regex.Match(query, @"\d+");
        return match.Success ? int.Parse(match.Value) : 0;
    }

    public int ExtractYears(string query)
    {
        query = query.ToLower();
        
        // Look for "X year" or "X years" (e.g., "10 year", "5 years")
        var yearMatch = Regex.Match(query, @"(\d+)\s*years?");
        if (yearMatch.Success)
        {
            return int.Parse(yearMatch.Groups[1].Value);
        }
        
        // Look for written numbers (e.g., "one year", "ten years")
        var writtenNumbers = new Dictionary<string, int>
        {
            { "one", 1 }, { "two", 2 }, { "three", 3 }, { "four", 4 }, { "five", 5 },
            { "six", 6 }, { "seven", 7 }, { "eight", 8 }, { "nine", 9 }, { "ten", 10 }
        };
        
        foreach (var num in writtenNumbers)
        {
            if (query.Contains($"{num.Key} year"))
                return num.Value;
        }
        
        // Default to 1 year if asking about returns but no timeframe specified
        if (query.Contains("in year") || query.Contains("per year") || query.Contains("a year"))
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
    
    public async Task<string> GenerateUniversalReturnsAsync(string investmentType, int amount, int years)
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
        
        var prompt = $@"
You are Miria, a smart and helpful financial assistant.
The user wants to know about returns for investing ₹{amount:N0} in {returns.Name} for {years} year{(years > 1 ? "s" : "")}.
Use these exact factual calculations in your response:
- Expected value roughly: ₹{maturityAmount:N0}
- Estimated profit: ₹{profit:N0}
- Annual return rate used: ~{returns.Rate * 100:F1}%
- Risk level: {returns.Risk}
- Investment type: {returns.Type}

Provide a conversational, easy-to-read response that shares these numbers natively.
Add a note that {(returns.Type == "Market-linked" ? "returns may vary based on market performance." : "returns are relatively stable.")}
Do not mention the prompt context. Be friendly!

CRITICAL INSTRUCTIONS:
- You must answer DIRECTLY. 
- Do NOT generate a fake dialogue or transcript (e.g., NEVER use 'User:' or 'Miria:').
- Do NOT introduce yourself. Just provide the answer.
";
        return await _llmService.GenerateStructuredAsync(prompt);
    }
    
    public async Task<string> CompareInvestmentsAsync(string type1, string type2, int amount, int years)
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
        
        var prompt = $@"
You are Miria, a smart and helpful financial assistant.
You need to compare investing ₹{amount:N0} over {years} year{(years > 1 ? "s" : "")} in {type1} vs {type2}.
Use these exact factual calculations:

{type1} Details:
- Expected Profit: ₹{profit1:N0} (at {returns1.Rate * 100:F1}%)
- Risk Level: {returns1.Risk}
- Investment Type: {returns1.Type}

{type2} Details:
- Expected Profit: ₹{profit2:N0} (at {returns2.Rate * 100:F1}%)
- Risk Level: {returns2.Risk}
- Investment Type: {returns2.Type}

Difference Insight: They could earn roughly ₹{difference:N0} more in profit with {better}.

Write a natural, conversational response sharing this comparison. Highlight the profitability difference, but also mention the difference in risk. Conclude with a helpful tip emphasizing they should choose based on their risk appetite and investment goals.

CRITICAL INSTRUCTIONS:
- You must answer DIRECTLY. 
- Do NOT generate a fake dialogue or transcript (e.g., NEVER use 'User:' or 'Miria:').
- Do NOT introduce yourself. Just provide the answer.
";
        return await _llmService.GenerateStructuredAsync(prompt);
    }

    public async Task<string> GenerateFDReturnsAsync(int amount, int years)
    {
        // FD typically gives 6-7% annual interest
        var interestRate = 0.065; // 6.5% average
        var maturityAmount = amount * Math.Pow(1 + interestRate, years);
        var interest = maturityAmount - amount;

        var prompt = $@"
You are Miria, a smart and helpful financial assistant.
The user wants to know about returns for a Fixed Deposit (FD) of ₹{amount:N0} over {years} year{(years > 1 ? "s" : "")}.
Use these exact calculations:
- Maturity amount: ₹{maturityAmount:N0} (approx)
- Interest earned: ₹{interest:N0}
- Interest rate assumed: ~6.5% annually

Present this information naturally and conversationally.
End with a note that FDs provide fixed, guaranteed returns, which is good for safety and short-term goals.

CRITICAL INSTRUCTIONS:
- You must answer DIRECTLY. 
- Do NOT generate a fake dialogue or transcript (e.g., NEVER use 'User:' or 'Miria:').
- Do NOT introduce yourself. Just provide the answer.
";
        return await _llmService.GenerateStructuredAsync(prompt);
    }

    public async Task<string> GenerateReturnsGuidanceAsync()
    {
        var prompt = @"
You are Miria, a smart and helpful financial assistant.
The user is asking generally about what kind of returns they can expect from investments.
Explain dynamically that returns depend on market conditions.
Give a realistic example: ""A ₹5,000/month SIP for 10 years typically grows to roughly ₹10-12 lakh (assuming a historical return of ~12% annually).""
Conclude with a tip that long-term investing gives better compounding results and SIP works best for 5+ years.

CRITICAL INSTRUCTIONS:
- You must answer DIRECTLY. 
- Do NOT generate a fake dialogue or transcript (e.g., NEVER use 'User:' or 'Miria:').
- Do NOT introduce yourself. Just provide the answer.
";
        return await _llmService.GenerateStructuredAsync(prompt);
    }

    public async Task<string> GenerateReturnsForAmountAsync(int monthlyAmount, int years)
    {
        // Simple compound interest calculation for SIP
        // Using ~12% annual return assumption
        var monthlyRate = 0.12 / 12;
        var months = years * 12;
        var futureValue = monthlyAmount * (((Math.Pow(1 + monthlyRate, months) - 1) / monthlyRate) * (1 + monthlyRate));
        var totalInvested = monthlyAmount * months;
        var returns = futureValue - totalInvested;

        var prompt = $@"
You are Miria, a smart and helpful financial assistant.
The user wants to invest ₹{monthlyAmount:N0} per month (SIP) for {years} year{(years > 1 ? "s" : "")}.
Use these exact mathematical facts in your response:
- Total amount invested: ₹{totalInvested:N0}
- Expected total value: ₹{futureValue:N0} (approx)
- Estimated returns/profit: ₹{returns:N0}
- Historical annual return rate assumed: ~12%

Present this cleanly and dynamically. Mention that returns depend on the market but this is a realistic estimate based on historical averages.
End with a tip that longer investment periods give substantially better compounding results.

CRITICAL INSTRUCTIONS:
- You must answer DIRECTLY. 
- Do NOT generate a fake dialogue or transcript (e.g., NEVER use 'User:' or 'Miria:').
- Do NOT introduce yourself. Just provide the answer.
";
        return await _llmService.GenerateStructuredAsync(prompt);
    }

    public async Task<string> GenerateInvestmentAdviceAsync(int amount)
    {
        var prompt = $@"
You are Miria, a friendly and knowledgeable financial assistant.
The user wants advice on how to invest ₹{amount:N0}. 

Instead of a generic 50/50 split, provide a DYNAMIC and personalized investment strategy.
Consider the amount:
- If small (under 10k), suggest starting with a single SIP and consistency.
- If medium (10k - 1 Lakh), suggest a mix of Large Cap Mutual Funds and perhaps a safe FD or Liquid Fund.
- If large (above 1 Lakh), suggest a diversified portfolio including:
    - 50-60% in Multi-cap or Index Funds for growth.
    - 20-30% in Debt/FDs for stability.
    - 10-20% in Gold or International Funds for diversification.

Format your response engagingly and practically. Explain WHY this allocation works for ₹{amount:N0}.
End with a practical next step (e.g., ""Start your first SIP this month"").

CRITICAL INSTRUCTIONS:
- You must answer DIRECTLY. 
- Do NOT generate a fake dialogue or transcript (e.g., NEVER use 'User:' or 'Miria:').
- Do NOT introduce yourself. Just provide the answer.
- Focus on the total amount: ₹{amount:N0}.
";
        return await _llmService.GenerateStructuredAsync(prompt);
    }

    public async Task<string> GenerateGuidedAnswerAsync(string query, string context)
    {
        var prompt = $@"
You are Miria, a smart and helpful financial assistant.

User Question: {query}

Knowledge Base Context:
{context}

Provide a helpful, accurate, and practical answer based on the context provided.

GUIDELINES:
- Use the context to give accurate information
- Be conversational and friendly
- Give practical examples with numbers when relevant
- Keep it concise (4-8 lines)
- If asking about best/top funds, provide general categories and characteristics
- For comparison queries, highlight key differences clearly
- For returns queries, give realistic expectations with disclaimers
- End with a practical tip or next step

IMPORTANT:
- Do NOT say 'I cannot give advice' - instead provide general guidance
- Do NOT repeat the question
- Do NOT include these instructions in your answer
- Base your answer on the provided context
- Speak DIRECTLY to the user. Do NOT describe what you are doing.
- Do NOT introduce yourself or your role (e.g., never start with ""As a financial assistant"").

Answer:";

        return await _llmService.GenerateStructuredAsync(prompt);
    }
}
