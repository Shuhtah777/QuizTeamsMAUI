
namespace QuizTeamsMAUI.Models;

public class Question
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Topic { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public bool AnswerYes { get; set; }
}
