
namespace QuizTeamsMAUI.Models;

public class Team
{
    public string Name { get; set; } = string.Empty;

    public List<string> Topics { get; set; } = new();

    public int Correct { get; set; }

    public int Wrong { get; set; }
}
