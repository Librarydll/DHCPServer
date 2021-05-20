using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using DHCPServer.Models.DTO;
using DHCPServer.Models.Infrastructure;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DHCPServer.ViewModels
{
	public class ReportViewModel : BindableBase
	{
		private readonly IReportRepository _reportRepository;
		private readonly IDialogService _dialogService;
		private string _searchingString;
		public string SearchingString
		{
			get { return _searchingString; }
			set { SetProperty(ref _searchingString, value); }
		}

		private ObservableCollection<ReportDTO> _reportsCollection;

		public ObservableCollection<ReportDTO> ReportsCollection
		{
			get { return _reportsCollection; }
			set { SetProperty(ref _reportsCollection, value); }
		}

		public DelegateCommand FilterCommand { get; set; }
		public DelegateCommand<ReportDTO> OpenGraphCommand { get; set; }

		public ReportViewModel(IReportRepository reportRepository,IDialogService dialogService)
		{
			FilterCommand = new DelegateCommand( async()=>await Filter());
			OpenGraphCommand = new DelegateCommand<ReportDTO>(OpenGraph);
			_reportRepository = reportRepository;
			_dialogService = dialogService;
		}

		private void OpenGraph(ReportDTO reportDTO)
		{
			var room = new RoomLineGraphInfo(reportDTO.ActiveDevice,false);
			var dialogParametr = new DialogParameters
			{
				{ "model", room },
				{"id",reportDTO.Report.Id }
			};

			_dialogService.Show("ArchiveGraphView", dialogParametr, x =>
			{
			});
		}

		private async Task Filter()
		{
			if (string.IsNullOrWhiteSpace(SearchingString)) return;


			var collection = await _reportRepository.GetReportsByString(SearchingString);
			var result = ReportDTO.Map(collection);
			ReportsCollection = new ObservableCollection<ReportDTO>(result);

		}


	}
}
