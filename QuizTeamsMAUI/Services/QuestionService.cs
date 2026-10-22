
using System.Text.Json;
using QuizTeamsMAUI.Models;

namespace QuizTeamsMAUI.Services;

public class QuestionService
{
    private readonly string _filePath;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public List<string> Topics { get; private set; } = new();
    public List<Team> Teams { get; private set; } = new();
    public List<Question> Questions { get; private set; } = new();

    public QuestionService()
    {
        _filePath = Path.Combine(
            FileSystem.AppDataDirectory,
            "quiz-data.json");

        Load();
    }

    public void Load()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                var data = JsonSerializer.Deserialize<QuizData>(json);

                if (data != null)
                {
                    Topics = data.Topics ?? new();
                    Questions = data.Questions ?? new();
                    Teams = data.Teams ?? new();
                    return;
                }
            }
        }
        catch
        {
        }

        CreateDefaultData();
        Save();
    }

    public void SetTeams(IEnumerable<Team> teams)
    {
        Teams = teams.ToList();
        Save();
    }

    public void Save()
    {
        var data = new QuizData
        {
            Topics = Topics,
            Questions = Questions,
            Teams = Teams
        };

        var json = JsonSerializer.Serialize(data, _jsonOptions);

        Directory.CreateDirectory(
            Path.GetDirectoryName(_filePath)!);

        File.WriteAllText(_filePath, json);
    }

    public bool AddTopic(string topic)
    {
        topic = topic.Trim();

        if (string.IsNullOrWhiteSpace(topic))
            return false;

        if (Topics.Any(t =>
            string.Equals(
                t,
                topic,
                StringComparison.OrdinalIgnoreCase)))
            return false;

        Topics.Add(topic);
        Save();

        return true;
    }

    public bool AddQuestion(
        string topic,
        string text,
        bool answerYes)
    {
        topic = topic.Trim();
        text = text.Trim();

        if (string.IsNullOrWhiteSpace(topic) ||
            string.IsNullOrWhiteSpace(text))
            return false;

        if (!Topics.Contains(topic))
            return false;

        Questions.Add(new Question
        {
            Topic = topic,
            Text = text,
            AnswerYes = answerYes
        });

        Save();

        return true;
    }

    public bool RemoveQuestion(string questionId)
    {
        var question = Questions.FirstOrDefault(
            q => q.Id == questionId);

        if (question == null)
            return false;

        Questions.Remove(question);
        Save();

        return true;
    }

    public bool RemoveTopic(string topic)
    {
        if (Questions.Any(q => q.Topic == topic))
            return false;

        var removed = Topics.Remove(topic);

        if (removed)
            Save();

        return removed;
    }

    public List<Question> GetQuestionsForTopics(
        IEnumerable<string> topics)
    {
        var selectedTopics = topics.ToHashSet(
            StringComparer.OrdinalIgnoreCase);

        return Questions
            .Where(q => selectedTopics.Contains(q.Topic))
            .ToList();
    }

    public Question? GetRandomQuestion(
        IEnumerable<string> topics,
        IEnumerable<string>? usedQuestionIds = null)
    {
        var selectedTopics = topics.ToHashSet(
            StringComparer.OrdinalIgnoreCase);

        var usedIds = (usedQuestionIds ?? Enumerable.Empty<string>())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var available = Questions
            .Where(q => selectedTopics.Contains(q.Topic))
            .Where(q => !usedIds.Contains(q.Id))
            .ToList();

        if (available.Count == 0)
            return null;

        return available[Random.Shared.Next(available.Count)];
    }

    public int GetQuestionCount(string topic)
    {
        return Questions.Count(q => q.Topic == topic);
    }

    private void CreateDefaultData()
    {
        Topics = new List<string>
        {
            "Загальні знання",
            "Наука",
            "Історія",
            "Географія",
            "Технології"
        };

        Questions = new List<Question>
        {
            new()
            {
                Topic = "Загальні знання",
                Text = "Київ є столицею України?",
                AnswerYes = true
            },
            new()
            {
                Topic = "Загальні знання",
                Text = "У тижні вісім днів?",
                AnswerYes = false
            },
            new()
            {
                Topic = "Наука",
                Text = "Вода за нормального тиску кипить приблизно при 100 °C?",
                AnswerYes = true
            },
            new()
            {
                Topic = "Наука",
                Text = "Сонце є планетою?",
                AnswerYes = false
            },
            new()
            {
                Topic = "Історія",
                Text = "Україна проголосила незалежність у 1991 році?",
                AnswerYes = true
            },
            new()
            {
                Topic = "Історія",
                Text = "Першу людину висадили на Місяць у 1965 році?",
                AnswerYes = false
            },
            new()
            {
                Topic = "Географія",
                Text = "Найбільший океан Землі — Тихий?",
                AnswerYes = true
            },
            new()
            {
                Topic = "Географія",
                Text = "Єгипет розташований у Південній Америці?",
                AnswerYes = false
            },
            new()
            {
                Topic = "Технології",
                Text = "HTML використовують для структурування вебсторінок?",
                AnswerYes = true
            },
            new()
            {
                Topic = "Технології",
                Text = "Оперативна пам'ять зазвичай зберігає дані після вимкнення ПК?",
                AnswerYes = false
            }
        };
    }
}
