#region Imports

using Portfolio.Services.Interfaces;

#endregion

namespace Portfolio.Services;

public class MWSCivilityService : IMWSCivilityService
{
    private readonly IWebHostEnvironment _hostEnvironment;

    public MWSCivilityService(IWebHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }

    public (bool Verdict, List<string> badWords) IsCivil(string phrase)
    {
        //get phrase to check
        var phraseInLowerCase = phrase.Trim().ToLower();

        //create new profanity filter.
        var filter = new ProfanityFilter.ProfanityFilter();

        //check for bad words.
        var badWordList = filter.DetectAllProfanities(phraseInLowerCase);

        //return result based on the number of bad words.
        if (badWordList.Count > 0) return (false, badWordList.ToList());
        return (true, new List<string>());
    }
}