using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicWeatherQuery.Menus
{
	public class MainMenu
	{
		public string Run()
		{
			Console.WriteLine("Would you like to get your weather" +
								"\n" +
								"(1) Automatically" +
								"\n" +
								"(2) By request" +
								"\n\n" +
								"(Q to quit)" +
								"\n");

			string userChoice = Console.ReadLine().ToUpper();

			while (userChoice != "1" && userChoice != "2" &&
				   userChoice != "Q")
			{
				int currentLineCursor = Console.CursorTop;
				Console.SetCursorPosition(0, Console.CursorTop);
				Console.Write(new string(' ', Console.WindowWidth));
				Console.SetCursorPosition(0, currentLineCursor);
				Console.WriteLine("Please choose a valid option!");
				userChoice = Console.ReadLine();
				Console.WriteLine();
				
			}

			return userChoice;
		}
	}
}
