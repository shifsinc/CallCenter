using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class ChangeWorkerDialogViewModel :INotifyPropertyChanged
    {
        private Window _view;

        private RequestServiceImpl.RequestService _requestService;
        private int _requestId;
        private ObservableCollection<WorkerDto> _workerList;
        private WorkerDto _selectedWorker;
        private ObservableCollection<WorkerHistoryDto> _workerHistoryList;
        private int? _oldMasterId;

        public ChangeWorkerDialogViewModel(RequestServiceImpl.RequestService requestService,int requestId)
        {
            _requestService = requestService;
            _requestId = requestId;
            WorkerList = new ObservableCollection<WorkerDto>();
            WorkerList.Add(new WorkerDto()
            {
                Id = -1,
                SurName = "��� �������"
            });
            foreach (var master in _requestService.GetMasters(null))
            {
                WorkerList.Add(master);
            }
            var request = _requestService.GetRequest(_requestId);
            _oldMasterId = request.MasterId;
            SelectedWorker = WorkerList.SingleOrDefault(w => w.Id == (request.MasterId??-1));
            Refresh(null);
        }
        public string WorkerTitle { get; set; }
        public void SetView(Window view)
        {
            _view = view;
        }

        private ICommand _refreshCommand;
        public ICommand RefreshCommand { get { return _refreshCommand ?? (_refreshCommand = new RelayCommand(Refresh)); } }
        private ICommand _saveCommand;
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }

        public DateTime FromTime { get; set; }

        private void Save(object sender)
        {
            var t = FromTime;
            if (_oldMasterId == SelectedWorker.Id)
                return;
            _requestService.AddNewMaster(_requestId, (SelectedWorker.Id == -1) ? (int?)null : SelectedWorker.Id);
            _oldMasterId = SelectedWorker.Id;
            _view.DialogResult = true;
        }

        public int? MasterId => _oldMasterId;

        public ObservableCollection<WorkerHistoryDto> WorkerHistoryList
        {
            get { return _workerHistoryList; }
            set { _workerHistoryList = value; OnPropertyChanged(nameof(WorkerHistoryList));}
        }

        public ObservableCollection<WorkerDto> WorkerList
        {
            get { return _workerList; }
            set { _workerList = value; OnPropertyChanged(nameof(WorkerList)); }
        }

        public void Refresh(object sender)
        {
            WorkerHistoryList = new ObservableCollection<WorkerHistoryDto>(_requestService.GetMasterHistoryByRequest(_requestId));
        }

        public WorkerDto SelectedWorker
        {
            get { return _selectedWorker; }
            set { _selectedWorker = value; OnPropertyChanged(nameof(SelectedWorker)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}