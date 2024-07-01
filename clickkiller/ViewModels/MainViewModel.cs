using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using ReactiveUI;
using clickkiller.Data;
using System.Reactive.Linq;

namespace clickkiller.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly DatabaseService _databaseService;
        private string _application = string.Empty;
        private string _notes = string.Empty;
        private ObservableCollection<IssueViewModel> _issues = [];

        private bool _focusNotes;

        public MainViewModel(string appDataPath)
        {
            _databaseService = new DatabaseService(appDataPath);
            SaveCommand = ReactiveCommand.Create(SaveIssue);
            FocusNotesCommand = ReactiveCommand.Create(() => FocusNotes = true);
            DeleteIssueCommand = ReactiveCommand.Create<IssueViewModel>(DeleteIssue);
            ToggleIssueDoneStatusCommand = ReactiveCommand.Create<IssueViewModel>(ToggleIssueDoneStatus);
            RefreshIssues();

            this.WhenAnyValue(x => x.Application)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .Subscribe(_ => RefreshIssues());
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

        public ObservableCollection<IssueViewModel> Issues
        {
            get => _issues;
            private set => this.RaiseAndSetIfChanged(ref _issues, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand FocusNotesCommand { get; }
        public ICommand DeleteIssueCommand { get; }
        public ICommand ToggleIssueDoneStatusCommand { get; }

        private void SaveIssue()
        {
            if (!string.IsNullOrWhiteSpace(Application) && !string.IsNullOrWhiteSpace(Notes))
            {
                _databaseService.SaveIssue(Application, Notes);
                Notes = string.Empty;
                RefreshIssues();
            }
        }

        private void RefreshIssues()
        {
            var issues = _databaseService.GetAllIssues(Application);
            var issueViewModels = new ObservableCollection<IssueViewModel>();

            DateTime? lastDate = null;
            foreach (var issue in issues.OrderByDescending(i => i.Timestamp))
            {
                bool showDate = !lastDate.HasValue || issue.Timestamp.Date != lastDate.Value.Date;
                issueViewModels.Add(new IssueViewModel(issue, showDate));
                lastDate = issue.Timestamp;
            }

            Issues = new ObservableCollection<IssueViewModel>(issueViewModels);
        }

        private void DeleteIssue(IssueViewModel issueViewModel)
        {
            _databaseService.DeleteIssue(issueViewModel.Id);
            RefreshIssues();
        }

        private void ToggleIssueDoneStatus(IssueViewModel issueViewModel)
        {
            _databaseService.ToggleIssueDoneStatus(issueViewModel.Id);
            RefreshIssues();
        }
    }

    public class IssueViewModel : ViewModelBase
    {
        public int Id { get; }
        public DateTime Timestamp { get; }
        public string Application { get; }
        public string Notes { get; }
        public bool ShowDate { get; }
        public bool IsDone { get; }

        public IssueViewModel(Issue issue, bool showDate)
        {
            Id = issue.Id;
            Timestamp = issue.Timestamp;
            Application = issue.Application;
            Notes = issue.Notes;
            ShowDate = showDate;
            IsDone = issue.IsDone;
        }
    }
}
