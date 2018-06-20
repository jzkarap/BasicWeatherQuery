using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicWeatherQuery.Menus
{
	public class GetTempScale
	{
		public string Run()
		{
			Console.Write("Temperate in (C)elcius or (F)arenheit: ");
			string tempScale = Console.ReadLine().ToUpper();

			while (tempScale != "F" && tempScale != "C")
			{
				Console.WriteLine("Please select C or F!");
				tempScale = Console.ReadLine().ToUpper();
			}

			return tempScale;
		}
	}
}
