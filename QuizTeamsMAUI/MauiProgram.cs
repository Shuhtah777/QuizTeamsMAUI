
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuizTeamsMAUI.Services;
using QuizTeamsMAUI.Views;

namespace QuizTeamsMAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder.UseMauiApp<App>();

        builder.Services.AddSingleton<QuestionService>();
        builder.Services.AddTransient<QuestionEditorPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
