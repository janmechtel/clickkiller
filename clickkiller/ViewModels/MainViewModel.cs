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

        public MainViewModel()
        {
            _databaseService = new DatabaseService();
            SaveCommand = ReactiveCommand.Create(SaveIssue);
            RefreshIssues();
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
