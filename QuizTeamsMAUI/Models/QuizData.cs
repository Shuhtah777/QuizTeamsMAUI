
namespace QuizTeamsMAUI.Models;

public class QuizData
{
    public List<string> Topics { get; set; } = new();

    public List<Question> Questions { get; set; } = new();

    public List<Team> Teams { get; set; } = new();
}
