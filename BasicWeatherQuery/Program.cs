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

namespace BasicWeatherQuery
{
	class Program
	{
		static string serverData { get; set; }

		private static void Main(string[] args)
		{
			Console.WriteLine("Would you like to get your weather\n(1) Automatically (**ONLY RETURNS LAT/LNG RIGHT NOW**)\n(2) By request");
			string userChoice = Console.ReadLine();
			while (userChoice != "1" && userChoice != "2")
			{
				Console.WriteLine("Please choose 1 or 2!");
				userChoice = Console.ReadLine();
			}

			if (userChoice == "1")
			{
				Console.Write("Getting current weather");
				Thread.Sleep(300);
				Console.Write(".");
				Thread.Sleep(300);
				Console.Write(".");
				Thread.Sleep(300);
				Console.Write(".");
				Thread.Sleep(300);
				Console.Write(".\n");

				GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();

				watcher.PositionChanged += (sender, currentLocation) =>
				{
					GeoCoordinate coordinate = currentLocation.Position.Location;
					float currentLat = (float)coordinate.Latitude;
					float currentLng = (float)coordinate.Longitude;
					Console.WriteLine($"Current latitude: {currentLat}");
					Console.WriteLine($"Current Longitude: {currentLng}");

					watcher.Stop();
				};

				watcher.Start();
				Thread.Sleep(800);
			}

			else
			{
				try
				{
					Console.Write("Choose a state: ");
					string state = Console.ReadLine();
					Console.Write("Choose a city to query weather: ");
					string city = Console.ReadLine();
					Console.Write("Temperate in (C)elcius or (F)arenheit: ");
					string tempScale = Console.ReadLine().ToUpper();

					while (tempScale != "F" && tempScale != "C")
					{
						Console.WriteLine("Please select C or F!");
						tempScale = Console.ReadLine();
					}

					// Combines user inputs for google query
					string address = $"{city}, {state}";
					GoogleLocationService locationService = new GoogleLocationService();
					MapPoint point = locationService.GetLatLongFromAddress(address);

					float lat = (float)point.Latitude;
					float lng = (float)point.Longitude;

					// FOR TESTING GOOGLE LOCATION API RESPONSE
					//Console.WriteLine($"Chosen latitude: {lat}");
					//Console.WriteLine($"Chosen longitude: {lng}");

					// Plugs google response into openweathermap API
					WebRequest request = WebRequest.Create("http://api.openweathermap.org/data/2.5/weather?lat=" + lat + "&lon=" + lng + "&APPID=408e6e270720aed460c1c391408063f5");
					WebResponse response = request.GetResponse();
					Stream apiText = response.GetResponseStream();
					StreamReader reader = new StreamReader(apiText);
					serverData = reader.ReadToEnd();

					// FOR TESTING OPENWEATHERMAP API RESPONSE
					//Console.WriteLine(serverData);

					// Pulls current temperature in Kelvin
					Main tmp = JsonConvert.DeserializeObject<Main>((JObject.Parse(serverData)["main"]).ToString());
					Weather condition = JsonConvert.DeserializeObject<Weather>((JObject.Parse(serverData)["weather"][0]).ToString());

					Console.Write("Temperature: ");
					if (tempScale == "F")
					{
						Console.WriteLine(Math.Round((double)tmp.Temp * 1.8 - 459.67, 1) + "°F");
					}
					if (tempScale == "C")
					{
						Console.WriteLine(Math.Round((double)tmp.Temp - 273.15) + "°C");
					}

					Console.WriteLine("Conditions: " + condition.Description.ToUpper());

					reader.Close();
					response.Close();
				}
				catch (System.Net.WebException)
				{
					Console.WriteLine("You may be over the request limit! Try again soon.");
				}
			}

		}
	}

}

