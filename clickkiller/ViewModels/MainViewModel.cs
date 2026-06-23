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
        private readonly Action _hideWindow;
        private string _application = string.Empty;
        private string _notes = string.Empty;
        private ObservableCollection<IssueViewModel> _issues = new ObservableCollection<IssueViewModel>();
        private bool _focusNotes;

        public ICommand ExitCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SaveAndHideCommand { get; }
        public ICommand FocusNotesCommand { get; }
        public ICommand DeleteIssueCommand { get; }
        public ICommand ToggleIssueDoneStatusCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DuplicateIssueCommand { get; }
        public ICommand SortCommand { get; }

        private string _updateMenuItemLabel;
        public string UpdateMenuItemLabel
        {
            get => _updateMenuItemLabel;
            set => this.RaiseAndSetIfChanged(ref _updateMenuItemLabel, value);
        }

        public MainViewModel(string appDataPath, Action exitApplication, Func<Task> updateApplication, string updateMenuItemLabel, Action hideWindow)
        {
            _databaseService = new DatabaseService(appDataPath);
            _hideWindow = hideWindow;
            ExitCommand = ReactiveCommand.Create(exitApplication);
            SaveCommand = ReactiveCommand.Create(SaveIssue);
            SaveAndHideCommand = ReactiveCommand.Create(SaveAndHide);
            FocusNotesCommand = ReactiveCommand.Create(() => FocusNotes = true);
            DeleteIssueCommand = ReactiveCommand.Create<IssueViewModel>(DeleteIssue);
            ToggleIssueDoneStatusCommand = ReactiveCommand.Create<IssueViewModel>(ToggleIssueDoneStatus);
            UpdateCommand = ReactiveCommand.CreateFromTask(updateApplication);
            DuplicateIssueCommand = ReactiveCommand.Create<IssueViewModel>(DuplicateIssue);
            SortCommand = ReactiveCommand.Create<string>(Sort);
            UpdateMenuItemLabel = updateMenuItemLabel;
            RefreshIssues();
            this.WhenAnyValue(x => x.Application, x => x.Notes)
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

        public ObservableCollection<IssueViewModel> Issues
        {
            get => _issues;
            private set => this.RaiseAndSetIfChanged(ref _issues, value);
        }

        private void SaveIssue()
        {
            TrySaveIssue();
        }

        private void SaveAndHide()
        {
            if (TrySaveIssue())
            {
                _hideWindow();
            }
        }

        private bool TrySaveIssue()
        {
            if (!string.IsNullOrWhiteSpace(Application) && !string.IsNullOrWhiteSpace(Notes))
            {
                _databaseService.SaveIssue(Application, Notes);
                Notes = string.Empty;
                RefreshIssues();
                return true;
            }

            return false;
        }

        private void RefreshIssues()
        {
            var issues = _databaseService.GetAllIssues(Application);
        
            var noteWords = !string.IsNullOrWhiteSpace(Notes) 
                ? Notes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                : Array.Empty<string>();

            var filteredIssues = issues.Where(i => 
                (string.IsNullOrWhiteSpace(Application) || i.Application.Contains(Application, StringComparison.OrdinalIgnoreCase)) &&
                (!i.IsDone || 
                 (i.IsDone && noteWords.Any() && 
                  noteWords.Any(word => i.Notes.Contains(word, StringComparison.OrdinalIgnoreCase))))
            ).ToList();
        
            var issueViewModels = new ObservableCollection<IssueViewModel>();

            IOrderedEnumerable<Issue> sortedIssues;
            switch (_sortColumn)
            {
                case "Application":
                    sortedIssues = _sortAscending 
                        ? filteredIssues.OrderBy(i => i.Application) 
                        : filteredIssues.OrderByDescending(i => i.Application);
                    break;
                case "Notes":
                    sortedIssues = _sortAscending 
                        ? filteredIssues.OrderBy(i => i.Notes) 
                        : filteredIssues.OrderByDescending(i => i.Notes);
                    break;
                case "Count":
                    sortedIssues = _sortAscending 
                        ? filteredIssues.OrderBy(i => _databaseService.GetDuplicateCount(i.Id)) 
                        : filteredIssues.OrderByDescending(i => _databaseService.GetDuplicateCount(i.Id));
                    break;
                case "Timestamp":
                default:
                    sortedIssues = _sortAscending 
                        ? filteredIssues.OrderBy(i => i.Timestamp) 
                        : filteredIssues.OrderByDescending(i => i.Timestamp);
                    break;
            }

            DateTime? lastDate = null;
            foreach (var issue in sortedIssues)
            {
                bool showDate = (_sortColumn == "Timestamp" && !_sortAscending) && (!lastDate.HasValue || issue.Timestamp.Date != lastDate.Value.Date);
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

        private string _sortColumn = "Timestamp";
        private bool _sortAscending = false;

        private void Sort(string column)
        {
            if (_sortColumn == column)
            {
                if (_sortAscending)
                {
                    // A-Z -> Z-A
                    _sortAscending = false;
                }
                else if (column != "Timestamp")
                {
                    // Z-A -> Reset to Timestamp Descending
                    _sortColumn = "Timestamp";
                    _sortAscending = false;
                }
                else
                {
                    // If it was already Timestamp Descending, toggle to Timestamp Ascending
                    _sortAscending = true;
                }
            }
            else
            {
                _sortColumn = column;
                _sortAscending = true;
            }
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
