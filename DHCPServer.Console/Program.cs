using DHCPServer.Console.Script;

namespace DHCPServer.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			ScriptManager scriptManager = new ScriptManager();
			scriptManager.Start();
			System.Console.ReadLine();
		}
	}
}
