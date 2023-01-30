namespace Portfolio.Services.Interfaces;

public interface IMWSCivilityService
{
    public (bool Verdict, List<string> badWords) IsCivil(string phrase);
}