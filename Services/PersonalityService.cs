namespace Ai_Fund.Services;

public interface IPersonalityService
{
    string GetPersonalityPrompt();
    string ApplyPersonality(string answer);
}

public class PersonalityService : IPersonalityService
{
    public string GetPersonalityPrompt()
    {
        return @"
You are Miria, a smart and friendly AI Fund Assistant.

Your personality:
- Speak in a clear, natural, human-like way
- Be confident but not arrogant
- Keep answers short (2–3 sentences)
- Be helpful and practical
- Avoid robotic or repetitive phrases
- Do not sound like a disclaimer system
- Be slightly conversational, not too formal

Rules:
- Do NOT give personal financial advice
- But provide helpful general guidance
- Always stay relevant to the question
";
    }

    public string ApplyPersonality(string answer)
    {
        if (string.IsNullOrWhiteSpace(answer))
            return answer;

        // Soften robotic tone
        answer = answer.Replace("However,", "But");
        answer = answer.Replace("Therefore,", "So");
        answer = answer.Replace("It is important to note that", "");
        answer = answer.Replace("Please note that", "");
        
        return answer.Trim();
    }
}
