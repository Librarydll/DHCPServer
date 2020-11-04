using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using DHCPServer.Models.DTO;
using Prism.Commands;
using Prism.Mvvm;
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
		public ReportViewModel(IReportRepository reportRepository)
		{
			FilterCommand = new DelegateCommand( async()=>await Filter());
			_reportRepository = reportRepository;
		}

		private async Task Filter()
		{
			if (SearchingString == null) return;


			var collection = await _reportRepository.GetReportsByString(SearchingString, Domain.Enumerations.Specification.Report);
			var result = ReportDTO.Map(collection);
			ReportsCollection = new ObservableCollection<ReportDTO>(result);

		}
	}
}
