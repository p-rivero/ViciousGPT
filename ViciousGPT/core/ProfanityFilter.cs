namespace ViciousGPT;

public enum ProfanityFilter
{
    None = 0,
    Allowed = 1,
    Encouraged = 2,
    Mandatory = 3
}

public static class ProfanityFilterExtensions
{
    public static string GetPrompt(this ProfanityFilter filter)
    {
        return filter switch
        {
            ProfanityFilter.None => "You are not allowed to use profanity.",
            ProfanityFilter.Allowed => "You are allowed to use profanity occasionally.",
            ProfanityFilter.Encouraged => "You can use profanity if you feel like it.",
            ProfanityFilter.Mandatory => "You must use profanity in your response.",
            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
        };
    }
}