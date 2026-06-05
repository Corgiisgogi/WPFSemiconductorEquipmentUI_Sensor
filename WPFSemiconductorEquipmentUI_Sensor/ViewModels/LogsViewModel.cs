using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WPFSemiconductorEquipmentUI_Sensor.Models;
using WPFSemiconductorEquipmentUI_Sensor.Services;

namespace WPFSemiconductorEquipmentUI_Sensor.ViewModels
{
    public class LogsViewModel : ScreenViewModelBase
    {
        private const int DefaultLimit = 300;
        private readonly ActivityLogStore _activityLogStore;
        private readonly ActivityLogRepository _activityLogRepository;
        private readonly SensorSnapshotRepository _sensorSnapshotRepository;
        private ActivityLogItem _selectedLog;
        private SensorSnapshotRecord _selectedSensorSnapshot;
        private string _fromDateText;
        private string _toDateText;
        private string _searchKeyword;
        private string _selectedUserFilter;
        private string _selectedSourceFilter;
        private string _selectedSeverityFilter;
        private string _totalLogCountText;
        private string _warningCountText;
        private string _riskCountText;
        private string _lastShutdownText;
        private string _filterStatusText;
        private string _filterStatusTone;

        public LogsViewModel()
            : this(new ActivityLogStore(), null, null)
        {
        }

        public LogsViewModel(ActivityLogStore activityLogStore)
            : this(activityLogStore, null, null)
        {
        }

        public LogsViewModel(ActivityLogStore activityLogStore, ActivityLogRepository activityLogRepository, SensorSnapshotRepository sensorSnapshotRepository)
        {
            _activityLogStore = activityLogStore;
            _activityLogRepository = activityLogRepository;
            _sensorSnapshotRepository = sensorSnapshotRepository;
            Title = "활동 / 센서 로그";
            Description = "SQLite에 저장된 활동 로그와 센서 기록을 검색하고 상세 내용을 확인합니다.";

            UserFilterOptions = new ObservableCollection<string> { "전체 사용자" };
            SourceFilterOptions = new ObservableCollection<string> { "전체 유형" };
            SeverityFilterOptions = new ObservableCollection<string> { "전체" };
            SelectedUserFilter = "전체 사용자";
            SelectedSourceFilter = "전체 유형";
            SelectedSeverityFilter = "전체";
            FilterStatusText = _activityLogRepository == null ? "로컬 보기" : "DB 준비";
            FilterStatusTone = _activityLogRepository == null ? "Warning" : "Normal";
            SearchCommand = new RelayCommand(parameter => SearchLogs());
            ResetFilterCommand = new RelayCommand(parameter => ResetFilters());
            ExportCommand = new RelayCommand(parameter => MarkExportPending());

            ActivityLogs = activityLogStore == null ? new ObservableCollection<ActivityLogItem>() : activityLogStore.Logs;
            if (activityLogStore != null && activityLogStore.Logs != null)
            {
                activityLogStore.Logs.CollectionChanged += OnActivityLogStoreChanged;
            }

            RefreshFilterOptions();
            RefreshSummary();
            if (_activityLogRepository != null)
            {
                SearchLogs();
            }
            else if (ActivityLogs.Count > 0)
            {
                SelectedLog = ActivityLogs[0];
                UpdateSummaryFromCollection();
            }
        }

        public ObservableCollection<ActivityLogItem> ActivityLogs { get; private set; }
        public ObservableCollection<string> UserFilterOptions { get; private set; }
        public ObservableCollection<string> SourceFilterOptions { get; private set; }
        public ObservableCollection<string> SeverityFilterOptions { get; private set; }
        public ICommand SearchCommand { get; private set; }
        public ICommand ResetFilterCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }

        public string FromDateText
        {
            get { return _fromDateText; }
            set { SetField(ref _fromDateText, value); }
        }

        public string ToDateText
        {
            get { return _toDateText; }
            set { SetField(ref _toDateText, value); }
        }

        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { SetField(ref _searchKeyword, value); }
        }

        public string SelectedUserFilter
        {
            get { return _selectedUserFilter; }
            set { SetField(ref _selectedUserFilter, value); }
        }

        public string SelectedSourceFilter
        {
            get { return _selectedSourceFilter; }
            set { SetField(ref _selectedSourceFilter, value); }
        }

        public string SelectedSeverityFilter
        {
            get { return _selectedSeverityFilter; }
            set { SetField(ref _selectedSeverityFilter, value); }
        }

        public string TotalLogCountText
        {
            get { return _totalLogCountText; }
            private set { SetField(ref _totalLogCountText, value); }
        }

        public string WarningCountText
        {
            get { return _warningCountText; }
            private set { SetField(ref _warningCountText, value); }
        }

        public string RiskCountText
        {
            get { return _riskCountText; }
            private set { SetField(ref _riskCountText, value); }
        }

        public string LastShutdownText
        {
            get { return _lastShutdownText; }
            private set { SetField(ref _lastShutdownText, value); }
        }

        public string FilterStatusText
        {
            get { return _filterStatusText; }
            private set { SetField(ref _filterStatusText, value); }
        }

        public string FilterStatusTone
        {
            get { return _filterStatusTone; }
            private set { SetField(ref _filterStatusTone, value); }
        }

        public ActivityLogItem SelectedLog
        {
            get { return _selectedLog; }
            set
            {
                if (object.Equals(_selectedLog, value))
                {
                    return;
                }

                _selectedLog = value;
                LoadSelectedSensorSnapshot();
                OnPropertyChanged();
                NotifySelectedLogDetailsChanged();
            }
        }

        public string SelectedLogDetailIdText
        {
            get { return SelectedLog == null ? "선택 없음" : "로그 #" + SelectedLog.Id; }
        }

        public string SelectedLogDateTimeText
        {
            get { return SelectedLog == null || SelectedLog.OccurredAt == DateTime.MinValue ? "시간: --" : "시간: " + SelectedLog.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss"); }
        }

        public string SelectedLogEventText
        {
            get { return SelectedLog == null ? "상세 내용을 보려면 로그 행을 선택하세요." : SelectedLog.Event; }
        }

        public string SelectedLogSourceText
        {
            get { return "출처: " + (SelectedLog == null ? "--" : SelectedLog.Source); }
        }

        public string SelectedLogActorText
        {
            get { return "사용자: " + (SelectedLog == null ? "--" : SelectedLog.User); }
        }

        public string SelectedLogSeverityText
        {
            get { return "등급: " + (SelectedLog == null ? "--" : SelectedLog.Severity); }
        }

        public string SelectedLogSavedText
        {
            get { return "저장: " + (SelectedLog == null ? "--" : SelectedLog.Saved); }
        }

        public string SensorSnapshotTimeText
        {
            get { return _selectedSensorSnapshot == null ? "센서 스냅샷 없음" : "스냅샷: " + _selectedSensorSnapshot.CapturedAt.ToString("HH:mm:ss"); }
        }

        public string DetailPressureText
        {
            get { return _selectedSensorSnapshot == null ? "--" : _selectedSensorSnapshot.PressureValue.ToString("0.00") + " bar"; }
        }

        public string DetailVibrationText
        {
            get { return _selectedSensorSnapshot == null ? "--" : _selectedSensorSnapshot.VibrationValue.ToString("0.0"); }
        }

        public string DetailTemperatureText
        {
            get { return _selectedSensorSnapshot == null ? "--" : _selectedSensorSnapshot.TemperatureValue.ToString("0.0") + " C"; }
        }

        public string DetailHumidityText
        {
            get { return _selectedSensorSnapshot == null ? "--" : _selectedSensorSnapshot.HumidityValue.ToString("0.0") + " %"; }
        }

        public void Refresh()
        {
            RefreshFilterOptions();
            SearchLogs();
        }

        private void OnActivityLogStoreChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_activityLogRepository == null)
            {
                ActivityLogs = _activityLogStore == null ? ActivityLogs : _activityLogStore.Logs;
                OnPropertyChanged("ActivityLogs");
                UpdateSummaryFromCollection();
                return;
            }

            Refresh();
        }

        private void SearchLogs()
        {
            if (_activityLogRepository == null)
            {
                FilterStatusText = "로컬 전용";
                FilterStatusTone = "Warning";
                return;
            }

            try
            {
                var criteria = BuildCriteria();
                ActivityLogs = _activityLogRepository.Search(criteria);
                OnPropertyChanged("ActivityLogs");
                ApplySummary(_activityLogRepository.GetSummary(criteria));
                SelectedLog = ActivityLogs.Count > 0 ? ActivityLogs[0] : null;
                FilterStatusText = ActivityLogs.Count + "건";
                FilterStatusTone = "Blue";
                Description = "SQLite 활동 로그 검색을 완료했습니다.";
                OnPropertyChanged("Description");
            }
            catch (Exception ex)
            {
                FilterStatusText = "검색 오류";
                FilterStatusTone = "Danger";
                Description = ex.Message;
                OnPropertyChanged("Description");
            }
        }

        private void ResetFilters()
        {
            FromDateText = string.Empty;
            ToDateText = string.Empty;
            SearchKeyword = string.Empty;
            SelectedUserFilter = "전체 사용자";
            SelectedSourceFilter = "전체 유형";
            SelectedSeverityFilter = "전체";
            SearchLogs();
        }

        private void MarkExportPending()
        {
            FilterStatusText = "내보내기 보류";
            FilterStatusTone = "Warning";
            Description = "CSV 내보내기는 아직 구현되지 않았습니다. 현재 검색 결과는 표에서 확인할 수 있습니다.";
            OnPropertyChanged("Description");
        }

        private ActivityLogSearchCriteria BuildCriteria()
        {
            return new ActivityLogSearchCriteria
            {
                FromDate = ParseDate(FromDateText),
                ToDate = ParseDate(ToDateText),
                User = SelectedUserFilter,
                Source = SelectedSourceFilter,
                Severity = SelectedSeverityFilter,
                Keyword = SearchKeyword,
                Limit = DefaultLimit
            };
        }

        private void RefreshFilterOptions()
        {
            if (_activityLogRepository == null)
            {
                return;
            }

            UserFilterOptions = _activityLogRepository.LoadDistinctValues("user_id", "전체 사용자");
            SourceFilterOptions = _activityLogRepository.LoadDistinctValues("source", "전체 유형");
            SeverityFilterOptions = _activityLogRepository.LoadDistinctValues("severity", "전체");
            OnPropertyChanged("UserFilterOptions");
            OnPropertyChanged("SourceFilterOptions");
            OnPropertyChanged("SeverityFilterOptions");
        }

        private void RefreshSummary()
        {
            if (_activityLogRepository == null)
            {
                UpdateSummaryFromCollection();
                return;
            }

            ApplySummary(_activityLogRepository.GetSummary(BuildCriteria()));
        }

        private void ApplySummary(ActivityLogSummary summary)
        {
            if (summary == null)
            {
                TotalLogCountText = "0";
                WarningCountText = "0";
                RiskCountText = "0";
                LastShutdownText = "--:--";
                return;
            }

            TotalLogCountText = summary.TotalCount.ToString("N0");
            WarningCountText = summary.WarningCount.ToString("N0");
            RiskCountText = summary.RiskCount.ToString("N0");
            LastShutdownText = string.IsNullOrWhiteSpace(summary.LastShutdownTimeText) ? "--:--" : summary.LastShutdownTimeText;
        }

        private void UpdateSummaryFromCollection()
        {
            var warnings = 0;
            var risks = 0;
            foreach (var log in ActivityLogs)
            {
                if (string.Equals(log.Severity, "WARN", StringComparison.OrdinalIgnoreCase))
                {
                    warnings++;
                }
                else if (string.Equals(log.Severity, "RISK", StringComparison.OrdinalIgnoreCase))
                {
                    risks++;
                }
            }

            TotalLogCountText = ActivityLogs.Count.ToString("N0");
            WarningCountText = warnings.ToString("N0");
            RiskCountText = risks.ToString("N0");
            LastShutdownText = "--:--";
        }

        private void LoadSelectedSensorSnapshot()
        {
            _selectedSensorSnapshot = null;
            if (_sensorSnapshotRepository != null && SelectedLog != null && SelectedLog.OccurredAt != DateTime.MinValue)
            {
                try
                {
                    _selectedSensorSnapshot = _sensorSnapshotRepository.LoadLatestBefore(SelectedLog.OccurredAt);
                }
                catch
                {
                    _selectedSensorSnapshot = null;
                }
            }

            OnPropertyChanged("SensorSnapshotTimeText");
            OnPropertyChanged("DetailPressureText");
            OnPropertyChanged("DetailVibrationText");
            OnPropertyChanged("DetailTemperatureText");
            OnPropertyChanged("DetailHumidityText");
        }

        private void NotifySelectedLogDetailsChanged()
        {
            OnPropertyChanged("SelectedLogDetailIdText");
            OnPropertyChanged("SelectedLogDateTimeText");
            OnPropertyChanged("SelectedLogEventText");
            OnPropertyChanged("SelectedLogSourceText");
            OnPropertyChanged("SelectedLogActorText");
            OnPropertyChanged("SelectedLogSeverityText");
            OnPropertyChanged("SelectedLogSavedText");
        }

        private static DateTime? ParseDate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            DateTime parsed;
            return DateTime.TryParse(text.Trim(), out parsed) ? (DateTime?)parsed : null;
        }

        private void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged(propertyName);
        }
    }
}
