
using QuizTeamsMAUI.Views;

namespace QuizTeamsMAUI;

public partial class App : Application
{
    public App(QuestionEditorPage questionEditorPage)
    {
        InitializeComponent();

        MainPage = new NavigationPage(questionEditorPage);
    }
}
