namespace clickkiller.ViewModels;

using System.Collections.ObjectModel;

public class MainViewModel : ViewModelBase
{
    private ObservableCollection<string> _suggestions;

    public MainViewModel()
    {
        _suggestions = new ObservableCollection<string>
        {
            "Suggestion1",
            "Suggestion2",
            "Suggestion3"
        };
    }

    public ObservableCollection<string> Suggestions
    {
        get => _suggestions;
    }

    public static string ButtonContent => "Report Problem";
}
