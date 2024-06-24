using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ReactiveUI;
using clickkiller.Data;

namespace clickkiller.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly DatabaseService _databaseService;
        private string _application;
        private string _notes;
        private ObservableCollection<Issue> _issues;

        private bool _focusNotes;

        public MainViewModel()
        {
            _databaseService = new DatabaseService();
            SaveCommand = ReactiveCommand.Create(SaveIssue);
            FocusNotesCommand = ReactiveCommand.Create(() => FocusNotes = true);
            RefreshIssues();
        }

        public bool FocusNotes
        {
            get => _focusNotes;
            set => this.RaiseAndSetIfChanged(ref _focusNotes, value);
        }

        public string Application
        {
            get => _application;
            set => this.RaiseAndSetIfChanged(ref _application, value);
        }

        public string Notes
        {
            get => _notes;
            set => this.RaiseAndSetIfChanged(ref _notes, value);
        }

        public ObservableCollection<Issue> Issues
        {
            get => _issues;
            private set => this.RaiseAndSetIfChanged(ref _issues, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand FocusNotesCommand { get; }

        private void SaveIssue()
        {
            if (!string.IsNullOrWhiteSpace(Application) && !string.IsNullOrWhiteSpace(Notes))
            {
                _databaseService.SaveIssue(Application, Notes);
                Application = string.Empty;
                Notes = string.Empty;
                RefreshIssues();
            }
        }

        private void RefreshIssues()
        {
            Issues = new ObservableCollection<Issue>(_databaseService.GetAllIssues());
        }
    }
}
