using QuizTeamsMAUI.Views;
using QuizTeamsMAUI.Models;
using QuizTeamsMAUI.Services;

namespace QuizTeamsMAUI.Views;

public partial class TeamSetupPage : ContentPage
{
    private readonly QuestionService _questionService;
    private readonly List<TeamDraft> _drafts = new();

    public TeamSetupPage(QuestionService questionService)
    {
        InitializeComponent();

        _questionService = questionService;

        TeamCountPicker.ItemsSource = Enumerable
            .Range(2, 5)
            .Select(n => n.ToString())
            .ToList();

        TeamCountPicker.SelectedIndex = 0;
    }

    private void OnTeamCountChanged(object sender, EventArgs e)
    {
        if (TeamCountPicker.SelectedIndex < 0)
            return;

        int count = TeamCountPicker.SelectedIndex + 2;

        while (_drafts.Count < count)
        {
            int number = _drafts.Count + 1;

            _drafts.Add(new TeamDraft
            {
                Name = $"Команда {number}"
            });
        }

        while (_drafts.Count > count)
            _drafts.RemoveAt(_drafts.Count - 1);

        RenderTeams();
    }

    private void RenderTeams()
    {
        TeamCards.Children.Clear();

        if (_questionService.Topics.Count < 5)
        {
            TeamCards.Children.Add(new Label
            {
                Text = "Спочатку створи щонайменше п'ять тем у редакторі.",
                TextColor = Color.FromArgb("#C33E4D"),
                FontSize = 16
            });

            return;
        }

        for (int i = 0; i < _drafts.Count; i++)
        {
            var draft = _drafts[i];

            var card = new Frame
            {
                BackgroundColor = Colors.White,
                BorderColor = Color.FromArgb("#E0E5EF"),
                CornerRadius = 14,
                Padding = 18
            };

            var layout = new VerticalStackLayout
            {
                Spacing = 10
            };

            layout.Children.Add(new Label
            {
                Text = $"Команда {i + 1}",
                FontSize = 22,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#3157D5")
            });

            var nameEntry = new Entry
            {
                Text = draft.Name,
                Placeholder = "Назва команди",
                TextColor = Color.FromArgb("#172033"),
                PlaceholderColor = Color.FromArgb("#667085"),
                BackgroundColor = Colors.White
            };

            nameEntry.TextChanged += (_, args) =>
            {
                draft.Name = args.NewTextValue ?? string.Empty;
            };

            layout.Children.Add(nameEntry);

            var selectedLabel = new Label
            {
                Text = $"Обрано тем: {draft.Topics.Count}/5",
                FontSize = 15,
                TextColor = Color.FromArgb("#667085")
            };

            layout.Children.Add(selectedLabel);

            foreach (var topic in _questionService.Topics)
            {
                var checkBox = new CheckBox
                {
                    IsChecked = draft.Topics.Contains(topic),
                    Color = Color.FromArgb("#3157D5"),
                    VerticalOptions = LayoutOptions.Center
                };

                var topicLabel = new Label
                {
                    Text = $"{topic} ({_questionService.GetQuestionCount(topic)} зап.)",
                    TextColor = Color.FromArgb("#172033"),
                    VerticalOptions = LayoutOptions.Center
                };

                var row = new HorizontalStackLayout
                {
                    Spacing = 8,
                    Children = { checkBox, topicLabel }
                };

                checkBox.CheckedChanged += async (_, args) =>
                {
                    if (args.Value)
                    {
                        if (draft.Topics.Count >= 5)
                        {
                            checkBox.IsChecked = false;

                            await DisplayAlert(
                                "Ліміт тем",
                                "Для кожної команди можна обрати рівно п'ять тем.",
                                "OK");

                            return;
                        }

                        if (!draft.Topics.Contains(topic))
                            draft.Topics.Add(topic);
                    }
                    else
                    {
                        draft.Topics.Remove(topic);
                    }

                    selectedLabel.Text =
                        $"Обрано тем: {draft.Topics.Count}/5";
                };

                layout.Children.Add(row);
            }

            card.Content = layout;
            TeamCards.Children.Add(card);
        }
    }

    private async void OnSaveTeamsClicked(object sender, EventArgs e)
    {
        if (_questionService.Topics.Count < 5)
        {
            await DisplayAlert(
                "Недостатньо тем",
                "Створи щонайменше п'ять тем у редакторі.",
                "OK");
            return;
        }

        foreach (var draft in _drafts)
        {
            if (string.IsNullOrWhiteSpace(draft.Name))
            {
                await DisplayAlert(
                    "Помилка",
                    "У кожної команди має бути назва.",
                    "OK");
                return;
            }

            if (draft.Topics.Count != 5)
            {
                await DisplayAlert(
                    "Не всі теми вибрані",
                    $"Команда «{draft.Name}» повинна обрати рівно п'ять тем.",
                    "OK");
                return;
            }

            var emptyTopics = draft.Topics
                .Where(topic => _questionService.GetQuestionCount(topic) == 0)
                .ToList();

            if (emptyTopics.Count > 0)
            {
                await DisplayAlert(
                    "Немає запитань",
                    $"Додай хоча б одне запитання до тем: {string.Join(", ", emptyTopics)}.",
                    "OK");
                return;
            }
        }

        var teams = _drafts.Select(draft => new Team
        {
            Name = draft.Name.Trim(),
            Topics = draft.Topics.ToList()
        }).ToList();

        _questionService.SetTeams(teams);

        await DisplayAlert(
            "Готово!",
            "Команди готові. Починаємо гру!",
            "OK");

        await Navigation.PushAsync(new GamePage(_questionService));
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private class TeamDraft
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Topics { get; set; } = new();
    }
}
