using HtmlAgilityPack;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DHCPServer.Services
{
	public static class JsonHelper
	{
		public static T ParseJson<T>(this string html) where T: class
		{

			HtmlDocument htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(html);
			var body = htmlDoc.DocumentNode.Descendants("body")
				.FirstOrDefault();
			try
			{
				var text = body.InnerText;
				return JsonConvert.DeserializeObject<T>(text);
			}
			catch (Exception e)
			{
				Log.Logger.Error("while parse exception {0}", e.Message);
				Log.Logger.Error("while parse exception {0}", e?.InnerException?.Message);
				return null;
			}
		}
	}
}
