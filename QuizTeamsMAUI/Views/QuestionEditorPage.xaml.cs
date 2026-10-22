using QuizTeamsMAUI.Views;
using QuizTeamsMAUI.Models;
using QuizTeamsMAUI.Services;

namespace QuizTeamsMAUI.Views;

public partial class QuestionEditorPage : ContentPage
{
    private readonly QuestionService _questionService;

    public QuestionEditorPage(QuestionService questionService)
    {
        InitializeComponent();

        _questionService = questionService;

        AnswerPicker.SelectedIndex = 0;

        RefreshTopics();
        RefreshQuestions();
    }

    private void RefreshTopics()
    {
        var selectedTopic = TopicPicker.SelectedItem?.ToString();

        TopicPicker.ItemsSource = _questionService.Topics.ToList();

        if (selectedTopic != null &&
            _questionService.Topics.Contains(selectedTopic))
        {
            TopicPicker.SelectedItem = selectedTopic;
        }
        else if (_questionService.Topics.Count > 0)
        {
            TopicPicker.SelectedIndex = 0;
        }
    }

    private async void OnAddTopicClicked(
        object sender,
        EventArgs e)
    {
        var topic = TopicEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(topic))
        {
            await DisplayAlert(
                "Помилка",
                "Введи назву теми.",
                "OK");
            return;
        }

        if (!_questionService.AddTopic(topic))
        {
            await DisplayAlert(
                "Помилка",
                "Тема вже існує або назва порожня.",
                "OK");
            return;
        }

        TopicEntry.Text = string.Empty;

        RefreshTopics();
        RefreshQuestions();

        await DisplayAlert(
            "Готово",
            $"Тему «{topic}» додано.",
            "OK");
    }
    private async void OnTeamSetupClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TeamSetupPage(_questionService));
    }

    private async void OnAddQuestionClicked(
        object sender,
        EventArgs e)
    {
        var topic = TopicPicker.SelectedItem?.ToString();
        var text = QuestionEntry.Text?.Trim();

        if (topic == null ||
            string.IsNullOrWhiteSpace(text) ||
            AnswerPicker.SelectedIndex < 0)
        {
            await DisplayAlert(
                "Не всі поля заповнені",
                "Оберіть тему, введи запитання та правильну відповідь.",
                "OK");
            return;
        }

        var answerYes = AnswerPicker.SelectedIndex == 0;

        if (!_questionService.AddQuestion(
            topic,
            text,
            answerYes))
        {
            await DisplayAlert(
                "Помилка",
                "Не вдалося зберегти запитання.",
                "OK");
            return;
        }

        QuestionEntry.Text = string.Empty;
        RefreshQuestions();

        await DisplayAlert(
            "Готово",
            "Запитання додано до вікторини!",
            "OK");
    }

    private void RefreshQuestions()
    {
        QuestionsList.Children.Clear();

        foreach (var question in _questionService.Questions
                     .OrderBy(q => q.Topic)
                     .ThenBy(q => q.Text))
        {
            var card = new VerticalStackLayout
            {
                Padding = 12,
                Spacing = 7,
                BackgroundColor = Colors.White
            };

            card.Children.Add(new Label
            {
                Text = question.Topic,
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#3157D5")
            });

            card.Children.Add(new Label
            {
                Text = question.Text,
                FontSize = 16,
                TextColor = Color.FromArgb("#172033")
            });

            card.Children.Add(new Label
            {
                Text = $"Правильна відповідь: {(question.AnswerYes ? "Так" : "Ні")}",
                FontSize = 14,
                TextColor = Color.FromArgb("#17834A")
            });

            var deleteButton = new Button
            {
                Text = "Видалити запитання",
                BackgroundColor = Color.FromArgb("#C33E4D"),
                TextColor = Colors.White,
                FontSize = 13,
                CornerRadius = 8
            };

            deleteButton.Clicked += async (_, _) =>
            {
                var confirmed = await DisplayAlert(
                    "Видалення",
                    "Точно видалити це запитання?",
                    "Так",
                    "Ні");

                if (!confirmed)
                    return;

                _questionService.RemoveQuestion(question.Id);
                RefreshQuestions();
            };

            card.Children.Add(deleteButton);
            QuestionsList.Children.Add(card);
        }

        if (_questionService.Questions.Count == 0)
        {
            QuestionsList.Children.Add(new Label
            {
                Text = "Поки що запитань немає. Час створити щось цікаве!",
                FontSize = 15,
                TextColor = Color.FromArgb("#667085")
            });
        }
    }

    private async void OnBackClicked(
        object sender,
        EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
