
using QuizTeamsMAUI.Models;
using QuizTeamsMAUI.Services;

namespace QuizTeamsMAUI.Views;

public partial class GamePage : ContentPage
{
    private readonly QuestionService _questionService;
    private int _teamIndex;
    private int _secondsRemaining;
    private int _questionNumber;
    private bool _roundActive;
    private Question? _currentQuestion;
    private readonly HashSet<string> _usedQuestionIds = new();

    private Team CurrentTeam => _questionService.Teams[_teamIndex];

    public GamePage(QuestionService questionService)
    {
        InitializeComponent();

        _questionService = questionService;

        foreach (var team in _questionService.Teams)
        {
            team.Correct = 0;
            team.Wrong = 0;
        }

        _questionService.SetTeams(_questionService.Teams);

        if (_questionService.Teams.Count == 0)
        {
            ReadyTeamLabel.Text = "Спочатку налаштуйте команди";
            StartButton.IsEnabled = false;
            return;
        }

        ShowReadyPanel();
    }

    private void ShowReadyPanel()
    {
        ReadyPanel.IsVisible = true;
        QuestionPanel.IsVisible = false;
        ResultsPanel.IsVisible = false;

        ReadyTeamLabel.Text =
            $"Наступні грають: {CurrentTeam.Name}";

        StartButton.Text = "Почати раунд";
        StartButton.IsEnabled = true;
    }

    private void OnStartClicked(object sender, EventArgs e)
    {
        if (_roundActive || _teamIndex >= _questionService.Teams.Count)
            return;

        _roundActive = true;
        _secondsRemaining = 60;
        _questionNumber = 0;
        _usedQuestionIds.Clear();

        ReadyPanel.IsVisible = false;
        QuestionPanel.IsVisible = true;
        ResultsPanel.IsVisible = false;

        CurrentTeamLabel.Text = CurrentTeam.Name;
        UpdateTimerLabel();
        UpdateScoreLabel();
        ShowNextQuestion();

        Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            if (!_roundActive)
                return false;

            _secondsRemaining--;
            UpdateTimerLabel();

            if (_secondsRemaining <= 0)
            {
                EndRound();
                return false;
            }

            return true;
        });
    }

    private void UpdateTimerLabel()
    {
        var time = TimeSpan.FromSeconds(
            Math.Max(0, _secondsRemaining));

        TimerLabel.Text = time.ToString(@"mm\:ss");
    }

    private void ShowNextQuestion()
    {
        var question = _questionService.GetRandomQuestion(
            CurrentTeam.Topics,
            _usedQuestionIds);

        if (question == null)
        {
            _usedQuestionIds.Clear();

            question = _questionService.GetRandomQuestion(
                CurrentTeam.Topics,
                _usedQuestionIds);
        }

        if (question == null)
        {
            QuestionLabel.Text = "У вибраних тем немає запитань.";
            EndRound();
            return;
        }

        _currentQuestion = question;
        _usedQuestionIds.Add(question.Id);
        _questionNumber++;

        QuestionNumberLabel.Text = $"Запитання №{_questionNumber}";
        QuestionLabel.Text = question.Text;
    }

    private void OnYesClicked(object sender, EventArgs e)
    {
        CheckAnswer(true);
    }

    private void OnNoClicked(object sender, EventArgs e)
    {
        CheckAnswer(false);
    }

    private void CheckAnswer(bool answer)
    {
        if (!_roundActive || _currentQuestion == null)
            return;

        if (answer == _currentQuestion.AnswerYes)
            CurrentTeam.Correct++;
        else
            CurrentTeam.Wrong++;

        UpdateScoreLabel();
        ShowNextQuestion();
    }

    private void UpdateScoreLabel()
    {
        ScoreLabel.Text =
            $"Правильно: {CurrentTeam.Correct}   |   " +
            $"Неправильно: {CurrentTeam.Wrong}";
    }

    private async void EndRound()
    {
        if (!_roundActive)
            return;

        _roundActive = false;
        QuestionPanel.IsVisible = false;

        if (_teamIndex + 1 < _questionService.Teams.Count)
        {
            _teamIndex++;

            await DisplayAlert(
                "Раунд завершено",
                $"Час команди завершився. " +
                $"Правильних відповідей: {_questionService.Teams[_teamIndex - 1].Correct}.\n\n" +
                $"Передайте пристрій команді «{CurrentTeam.Name}».",
                "Добре");

            ShowReadyPanel();
        }
        else
        {
            ShowResults();
        }
    }

    private void ShowResults()
    {
        ReadyPanel.IsVisible = false;
        QuestionPanel.IsVisible = false;
        ResultsPanel.IsVisible = true;

        ResultsList.Children.Clear();

        var orderedTeams = _questionService.Teams
            .OrderByDescending(t => t.Correct)
            .ThenBy(t => t.Wrong)
            .ToList();

        var winner = orderedTeams[0];

        var tiedTeams = orderedTeams
            .Where(t => t.Correct == winner.Correct &&
                        t.Wrong == winner.Wrong)
            .ToList();

        WinnerLabel.Text = tiedTeams.Count > 1
            ? $"Нічия між командами: {string.Join(", ", tiedTeams.Select(t => t.Name))}!"
            : $"Переможець: {winner.Name}!";

        for (int i = 0; i < orderedTeams.Count; i++)
        {
            var team = orderedTeams[i];

            var card = new Frame
            {
                BackgroundColor = Colors.White,
                BorderColor = Color.FromArgb("#E0E5EF"),
                CornerRadius = 12,
                Padding = 16
            };

            card.Content = new VerticalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    new Label
                    {
                        Text = $"{i + 1}. {team.Name}",
                        FontSize = 21,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#3157D5")
                    },
                    new Label
                    {
                        Text = $"Правильні відповіді: {team.Correct}",
                        FontSize = 17,
                        TextColor = Color.FromArgb("#17834A")
                    },
                    new Label
                    {
                        Text = $"Неправильні відповіді: {team.Wrong}",
                        FontSize = 17,
                        TextColor = Color.FromArgb("#C33E4D")
                    }
                }
            };

            ResultsList.Children.Add(card);
        }
    }

    private async void OnFinishClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
