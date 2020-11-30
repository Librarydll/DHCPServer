using DHCPServer.Domain.Models;
using DHCPServer.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DHCPServer.Services
{
	public static class HtmlHelper
	{
		public static readonly string pattern = "[\r\n\t ]";
		public static readonly string TemperatureId = "tid";
		public static readonly string HumidityId = "hid";
		public static readonly string TMid = "tMid";
		public static readonly string HMid = "hMid";
		public static readonly string TNord = "tNord";
		public static readonly string HNord = "hNord";
		public static readonly string TProcess = "tProcess";
		public static readonly string HProcess = "hProcess";
		public static Regex regex = new Regex(pattern);

		public static RoomInfo ParseHtml(this string html)
		{
			Regex regex = new Regex(pattern);

			HtmlDocument htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(html);
			string temp = string.Empty;
			string humidity = string.Empty;
			var tempNode = htmlDoc.GetElementbyId(TemperatureId);
			var humidityNode = htmlDoc.GetElementbyId(HumidityId);
			if (tempNode?.InnerText!=null)
			{
				temp = regex.Replace(tempNode.InnerText, "");
			}
			if (humidityNode?.InnerText != null)
			{
				humidity = regex.Replace(humidityNode.InnerText, "");
			}

			double h=-1, t=-1;
			if (double.TryParse(temp.Replace(".",","), out double tem)) t = tem;
			if (double.TryParse(humidity.Replace(".", ","), out double hum)) h = hum;

			return new RoomInfo
			{
				Humidity = h,
				Temperature = t
			};
		}

		public static RoomInfo ParseMultiHtml(this string html)
		{

			HtmlDocument htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(html);

			var tempNode = htmlDoc.GetElementbyId(TemperatureId);
			var humidityNode = htmlDoc.GetElementbyId(HumidityId);

			var tMidNode = htmlDoc.GetElementbyId(TMid);
			var hMidNode = htmlDoc.GetElementbyId(HMid);

			var tNordNode = htmlDoc.GetElementbyId(TNord);
			var hNordNode = htmlDoc.GetElementbyId(HNord);

			var TProcessNode = htmlDoc.GetElementbyId(TProcess);
			var HProcessNode = htmlDoc.GetElementbyId(HProcess);

			var f1 = GetHAndT(tempNode, humidityNode);
			string temp = f1.Item1;
			string humidity = f1.Item2;


			var f2 = GetHAndT(tMidNode, hMidNode);
			string midt = f2.Item1;
			string midh = f2.Item2;
			var f3 = GetHAndT(tNordNode, hNordNode);
			string nordt = f3.Item1;
			string nordh = f3.Item2;
			var f4 = GetHAndT(TProcessNode, HProcessNode);
			string processt = f4.Item1;
			string processh = f4.Item2;

			double t;
			double.TryParse(temp.Replace(".", ","), out t);
			double h;
			double.TryParse(humidity.Replace(".", ","), out h);
			double mt;
			double.TryParse(midt.Replace(".", ","), out mt);
			double mh;
			double.TryParse(midh.Replace(".", ","), out mh);
			double nt;
			double.TryParse(nordt.Replace(".", ","), out nt);
			double nh;
			double.TryParse(nordh.Replace(".", ","), out nh);
			double pt;
			double.TryParse(processt.Replace(".", ","), out pt);
			double ph;
			double.TryParse(processh.Replace(".", ","), out ph);


			return new MultiRoomInfo
			{
				Humidity = h,
				Temperature = t,
				HumidityMiddle =mh,
				TemperatureMiddle=mt,
				HumidityProcess=ph,
				TemperatureProcess=pt,
				HumidityNord =nh,
				TemperatureNord=nt
			};
		}

		private static Tuple<string,string> GetHAndT(HtmlNode h, HtmlNode t)
		{
			string t1= string.Empty;
			string h1= string.Empty;

			if (t?.InnerText != null)
			{
				t1 = regex.Replace(t.InnerText, "");
			}
			if (h?.InnerText != null)
			{
				h1 = regex.Replace(h.InnerText, "");
			}
			return Tuple.Create(t1, h1);
		}
	}
}
