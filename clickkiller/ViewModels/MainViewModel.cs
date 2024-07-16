using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using ReactiveUI;
using clickkiller.Data;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace clickkiller.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly DatabaseService _databaseService;
        private string _application = string.Empty;
        private string _notes = string.Empty;
        private ObservableCollection<IssueViewModel> _issues = new ObservableCollection<IssueViewModel>();
        private bool _focusNotes;
        private bool? _filterDoneStatus;

        public ICommand ExitCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand FocusNotesCommand { get; }
        public ICommand DeleteIssueCommand { get; }
        public ICommand ToggleIssueDoneStatusCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DuplicateIssueCommand { get; }

        private string _updateMenuItemLabel;
        public string UpdateMenuItemLabel
        {
            get => _updateMenuItemLabel;
            set => this.RaiseAndSetIfChanged(ref _updateMenuItemLabel, value);
        }

        public MainViewModel(string appDataPath, Action exitApplication, Func<Task> updateApplication, string updateMenuItemLabel)
        {
            _databaseService = new DatabaseService(appDataPath);
            ExitCommand = ReactiveCommand.Create(exitApplication);
            SaveCommand = ReactiveCommand.Create(SaveIssue);
            FocusNotesCommand = ReactiveCommand.Create(() => FocusNotes = true);
            DeleteIssueCommand = ReactiveCommand.Create<IssueViewModel>(DeleteIssue);
            ToggleIssueDoneStatusCommand = ReactiveCommand.Create<IssueViewModel>(ToggleIssueDoneStatus);
            UpdateCommand = ReactiveCommand.CreateFromTask(updateApplication);
            DuplicateIssueCommand = ReactiveCommand.Create<IssueViewModel>(DuplicateIssue);
            UpdateMenuItemLabel = updateMenuItemLabel;
            RefreshIssues();

            this.WhenAnyValue(x => x.Application, x => x.FilterDoneStatus)
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
            set
            {
                this.RaiseAndSetIfChanged(ref _notes, value);
                RefreshIssues();
            }
        }

        public bool? FilterDoneStatus
        {
            get => _filterDoneStatus;
            set => this.RaiseAndSetIfChanged(ref _filterDoneStatus, value);
        }

        public ObservableCollection<IssueViewModel> Issues
        {
            get => _issues;
            private set => this.RaiseAndSetIfChanged(ref _issues, value);
        }

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
        
            var fil teredIssues = issues.Where(i => 
                // TODO if an application is set, only show issues that match the aplication
                // TODO if notes are set, also include "done" issues if the are matching words from the notes otherwise don't show done issues
                // !i.IsDone || 
                // (i.IsDone && !string.IsNullOrWhiteSpace(Notes) && 
                //  (i.Application.Contains(Notes, StringComparison.OrdinalIgnoreCase) || 
                //   i.Notes.Contains(Notes, StringComparison.OrdinalIgnoreCase)))
            ).ToList();

            if (FilterDoneStatus.HasValue)
            {
                filteredIssues = filteredIssues.Where(i => i.IsDone == FilterDoneStatus.Value).ToList();
            }
        
            var issueViewModels = new ObservableCollection<IssueViewModel>();

            DateTime? lastDate = null;
            foreach (var issue in filteredIssues.OrderByDescending(i => i.Timestamp))
            {
                bool showDate = !lastDate.HasValue || issue.Timestamp.Date != lastDate.Value.Date;
                int duplicateCount = _databaseService.GetDuplicateCount(issue.Id);
                DateTime mostRecentTimestamp = _databaseService.GetMostRecentTimestamp(issue.Id);
                issueViewModels.Add(new IssueViewModel(issue, showDate, Notes, duplicateCount, mostRecentTimestamp));
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

        private void DuplicateIssue(IssueViewModel issueViewModel)
        {
            _databaseService.SaveIssue(issueViewModel.Application, issueViewModel.Notes, issueViewModel.Id);
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
        public string HighlightText { get; }
        public bool IsDuplicate { get; }

        public int DuplicateCount { get; }

        public DateTime MostRecentTimestamp { get; }

        public IssueViewModel(Issue issue, bool showDate, string highlightText, int duplicateCount, DateTime mostRecentTimestamp)
        {
            Id = issue.Id;
            Timestamp = issue.Timestamp;
            Application = issue.Application;
            Notes = issue.Notes;
            ShowDate = showDate;
            IsDone = issue.IsDone;
            HighlightText = highlightText;
            IsDuplicate = issue.DuplicateOf.HasValue;
            DuplicateCount = duplicateCount;
            MostRecentTimestamp = mostRecentTimestamp;

        }
    }
}
