using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DHCPServer.Dialogs
{
    public class CloseReportViewModel : DialogViewModelBase
    {
        private bool _canClose = false;
        private ICollection<int> _closedReportsId = new List<int>();
        private readonly IReportRepository _reportRepository;


        private ObservableCollection<Report> _reportsCollection;
        public ObservableCollection<Report> ReportsCollection
        {
            get { return _reportsCollection; }
            set { SetProperty(ref _reportsCollection, value); }
        }

        private Report _selectedReport;
        public Report SelectedReport
        {
            get { return _selectedReport; }
            set { SetProperty(ref _selectedReport, value); }
        }

        public DelegateCommand CloseReportCommand => new DelegateCommand(async () => await ExecuteCloseReport());


        public CloseReportViewModel(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;

            Title = "Закрытие архива";
            Task.Run(async () =>
            {
                var reports = await _reportRepository.GetActiveReports();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ReportsCollection = new ObservableCollection<Report>(reports);
                });
            });

        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            _closedReportsId = parameters.GetValue<ICollection<int>>("model");
        }

        private async Task ExecuteCloseReport()
        {
            if (SelectedReport == null) return;

            var msg = MessageBox.Show($"Вы действительно хотите закрыть архив {SelectedReport.Title}", "Закрытие арзива", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (msg == MessageBoxResult.No) return;

            var result =  await _reportRepository.TryCloseReport(SelectedReport.Id);

            if (result)
            {
                MessageBox.Show("Успешно закрыт архив");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _closedReportsId.Add(SelectedReport.Id);

                    ReportsCollection.Remove(SelectedReport);
                });
            }
        }

        
    }
}
