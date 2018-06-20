using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicWeatherQuery.Menus
{
	public class ManualMenu
	{
		public Tuple<string, string> Run()
		{
			Console.WriteLine();
			Console.Write("Choose a state or region: ");
			string stateOrRegion = Console.ReadLine();
			Console.Write("Choose a city to query weather: ");
			string city = Console.ReadLine();
	
			return new Tuple<string, string>(stateOrRegion, city);
		}

	}
}
