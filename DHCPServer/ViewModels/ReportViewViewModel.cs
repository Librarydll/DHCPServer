using DHCPServer.Domain.Interfaces;
using DHCPServer.Domain.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DHCPServer.ViewModels
{
	public class ReportViewViewModel : BindableBase
	{
		private readonly IReportRepository _reportRepository;

		private string _searchingString;
		public string SearchingString
		{
			get { return _searchingString; }
			set { SetProperty(ref _searchingString, value); }
		}

		private ObservableCollection<Report> _reportsCollection;

		public ObservableCollection<Report> ReportsCollection
		{
			get { return _reportsCollection; }
			set { SetProperty(ref _reportsCollection, value); }
		}

		public DelegateCommand FilterCommand { get; set; }
		public ReportViewViewModel(IReportRepository reportRepository)
		{
			FilterCommand = new DelegateCommand(Filter);
			_reportRepository = reportRepository;
		}

		private void Filter()
		{
			if (SearchingString == null) return;
			
			
			

		}
	}
}
