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

		public static RoomData Parse(this string html)
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

			return new RoomData
			{
				Humidity = h,
				Temperature = t
			};
		}
	}
}
