using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoogleMaps.LocationServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenWeatherMap_Current;
using System.Runtime.InteropServices;
using OpenWeatherMap_FiveDay;
using BasicWeatherQuery.Menus;

namespace BasicWeatherQuery
{
	class Program
	{
		static string serverData { get; set; }

		const string UNDERLINE = "\x1B[4m";
		const string RESET = "\x1B[0m";
		const int STD_OUTPUT_HANDLE = -11;
		const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4;

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport("kernel32.dll")]
		static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

		[DllImport("kernel32.dll")]
		static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

		private static void Main(string[] args)
		{
			CreateConditionsForUnderline();

			MainMenu menu = new MainMenu();

			GetTempScale getTempScale = new GetTempScale();

			string userChoice = menu.Run();

			while (true)
			{
				if (userChoice == "1")
				{
					string tempScale = getTempScale.Run();

					GettingWeatherAnimation(true);

					var location = GetCurrentCoordinates();
					var lat = location.Item1;
					var lng = location.Item2;

					GetCurrentForecast(lat, lng, tempScale);

					FiveDayPrompt();
				}

				if (userChoice == "2")
				{
					ManualMenu manualMenu = new ManualMenu();

					var location = manualMenu.Run();

					var stateOrRegion = location.Item1;
					var city = location.Item2;

					string tempScale = getTempScale.Run();

					MapPoint point = GetGeocoordinatesFromGoogle(city, stateOrRegion);

					float lat = (float)point.Latitude;
					float lng = (float)point.Longitude;

					GettingWeatherAnimation(false);

					GetCurrentForecast(lat, lng, tempScale);

					FiveDayPrompt();
				}
				else
				{
					break;
				}
			}

		}

		private static void GettingWeatherAnimation(bool isCurrentLocation)
		{
			Console.WriteLine();

			if (isCurrentLocation)
			{
				Console.Write("Getting weather for current location");
			}
			else
			{
				Console.Write("Getting weather");
			}

			Thread.Sleep(300);
			Console.Write(".");
			Thread.Sleep(300);
			Console.Write(".");
			Thread.Sleep(300);
			Console.Write(".");
			Thread.Sleep(300);
			Console.Write(".\n");

		}

		static MapPoint GetGeocoordinatesFromGoogle(string city, string stateOrRegion)
		{
			string address = $"{city}, {stateOrRegion}";
			GoogleLocationService locationService = new GoogleLocationService();
			MapPoint point = locationService.GetLatLongFromAddress(address);
			return point;
		}

		static void Get5DayForecast(float lat, float lng, string tempScale)
		{
			WebRequest request = WebRequest.Create("http://api.openweathermap.org/data/2.5/forecast?lat=" + lat + "&lon=" + lng + "&appID=408e6e270720aed460c1c391408063f5");
			WebResponse response = request.GetResponse();
			Stream apiText = response.GetResponseStream();
			StreamReader reader = new StreamReader(apiText);
			serverData = reader.ReadToEnd();

			MainClass tmp = JsonConvert.DeserializeObject<MainClass>((JObject.Parse(serverData)["list"]).ToString());

			OpenWeatherMap_FiveDay.Weather condition =
			JsonConvert.DeserializeObject<OpenWeatherMap_FiveDay.Weather>
			((JObject.Parse(serverData)["list"]).ToString());

			Console.Write("TEMPERATURE: ");
			if (tempScale == "F")
			{
				Console.WriteLine(Math.Round((double)tmp.Temp * 1.8 - 459.67, 1) + "°F");
			}
			if (tempScale == "C")
			{
				Console.WriteLine(Math.Round((double)tmp.Temp - 273.15) + "°C");
			}

			Console.WriteLine("CONDITIONS: " + condition.Description.ToUpper());
			Console.WriteLine();

			reader.Close();
			response.Close();
		}

		static void GetCurrentForecast(float lat, float lng, string tempScale)
		{
			try
			{
				// Plugs google response into openweathermap API
				WebRequest request = WebRequest.Create("http://api.openweathermap.org/data/2.5/weather?lat=" + lat + "&lon=" + lng + "&APPID=408e6e270720aed460c1c391408063f5");
				WebResponse response = request.GetResponse();
				Stream apiText = response.GetResponseStream();
				StreamReader reader = new StreamReader(apiText);
				serverData = reader.ReadToEnd();

				// Pulls current temperature in Kelvin
				Main tmp = JsonConvert.DeserializeObject<Main>((JObject.Parse(serverData)["main"]).ToString());
				OpenWeatherMap_Current.Weather condition = JsonConvert.DeserializeObject<OpenWeatherMap_Current.Weather>((JObject.Parse(serverData)["weather"][0]).ToString());

				// Invokes logkeeper to track each query
				InvokeCurrentTempLogkeeper(tmp);

				Console.WriteLine();
				Console.WriteLine("   " + UNDERLINE + "               " + RESET);
				Console.WriteLine("***" + UNDERLINE + "CURRENT WEATHER" + RESET + "***");
				Console.WriteLine();
				// Converts temperature in Kelvin to Farenheit or Celsius,
				// dependent on user choice

				DisplayTemp(tempScale, tmp);

				Console.WriteLine("CONDITIONS: " + condition.Description.ToUpper());
				Console.WriteLine();

				// Closes StreamReader
				reader.Close();
				response.Close();
			}
			catch (System.Net.WebException)
			{
				Console.WriteLine("You may be over the request limit! Try again soon.");
			}

		}

		private static void DisplayTemp(string tempScale, Main tmp)
		{
			Console.Write("TEMPERATURE: ");
			if (tempScale == "F")
			{
				Console.WriteLine(Math.Round((double)tmp.Temp * 1.8 - 459.67, 1) + "°F");
			}
			if (tempScale == "C")
			{
				Console.WriteLine(Math.Round((double)tmp.Temp - 273.15) + "°C");
			}
		}

		/// <summary>
		/// ADD LOCATION
		//  Write to log with current time and temperature,
		//  Average of all temperatures so far
		//  (will have to parse each previous line in log to get temp, and then divide by number of lines)
		//  (with datetime, may also be able to average by season, time of day)
		//  (**make sure to organize by unique location**)
		/// </summary>
		/// <param name="tmp">Takes JSON-parsed temperature</param>
		private static void InvokeCurrentTempLogkeeper(Main tmp)
		{
			StreamWriter logKeeper = new StreamWriter("tempLog.txt", true);

			logKeeper.WriteLine(DateTime.Now.ToString().PadRight(25) +
								(Math.Round(tmp.Temp, 1)) + "°K".PadRight(5) +
								(Math.Round((tmp.Temp * 1.8 - 459.67), 1) + "°F".PadRight(5) +
								(Math.Round((tmp.Temp - 273.15), 1) + "°C")));

			logKeeper.Close();
		}

		static void CreateConditionsForUnderline()
		{
			// Not sure what this block does--
			// Found on stackoverflow, enables underlined console text
			var handle = GetStdHandle(STD_OUTPUT_HANDLE);
			uint mode;
			GetConsoleMode(handle, out mode);
			mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
			SetConsoleMode(handle, mode);
			// Below is model for code:
			// Console.WriteLine("Some " + UNDERLINE + "underlined" + RESET + " text");
		}

		// Allows us to wat for something
		static ManualResetEvent waitHandle = new ManualResetEvent(false);

		static Tuple<float, float> GetCurrentCoordinates()
		{
			GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();
			float lat = 0, lng = 0;
			watcher.PositionChanged += (sender, currentLocation) =>
			{
				GeoCoordinate coordinate = currentLocation.Position.Location;
				lat = (float)coordinate.Latitude;
				lng = (float)coordinate.Longitude;
				Console.WriteLine();
				Console.WriteLine("DETECTED CURRENT LOCATION!");
				Thread.Sleep(800);
				Console.WriteLine($"CURRENT LATITUDE: {lat}");
				Thread.Sleep(800);
				Console.WriteLine($"CURRENT LONGITUDE: {lng}");
				Thread.Sleep(800);

				watcher.Stop();
				waitHandle.Set(); //trigger the signal so that the waithandle no longer blocks and resumes
			};

			watcher.Start();
			waitHandle.WaitOne();

			return new Tuple<float, float>(lat, lng);
		}

		static void FiveDayPrompt()
		{
			Console.WriteLine("Would you like to get a five day forecast? (Y/N)");
			string userChoice = Console.ReadLine().ToUpper();

			while (userChoice != "Y" && userChoice != "N")
			{
				Console.WriteLine("Please choose Y or N!");
				userChoice = Console.ReadLine().ToUpper();
			}

			Console.WriteLine();
		}
	}

	

}

// Can split some of these off to classes --
// "API requests" , "Logkeepers" , "Menus"
// Goal is readability -- make the program as clear as possible for anyone who wants to manipulate it