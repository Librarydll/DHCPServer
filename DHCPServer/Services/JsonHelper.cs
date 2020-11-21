using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Services
{
	public static class JsonHelper
	{
		public static T Parse<T>(string obj) where T: class
		{
			try
			{
				return (T)JsonConvert.DeserializeObject(obj);
			}
			catch (Exception e)
			{
				return null;
			}
		}
	}
}
