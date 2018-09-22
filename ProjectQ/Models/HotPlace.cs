using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Web.Hosting;

namespace ProjectQ.Models
{

    /// <summary>
    /// Finds the hottest place close to a point and requests the driving time to that location.
    /// </summary>
    public class HotPlace
    {

        public readonly string WeatherApiKey = File.ReadAllText(HostingEnvironment.MapPath(@"~\App_Data\WeatherApiKey.txt"));

        public string BaseTown { get; set; }
        public decimal BaseTemp { get; set; }
        public decimal BaseLat { get; set; }
        public decimal BaseLng { get; set; }

        public string HotTown { get; set; }
        public decimal HotTemp { get; set; }
        public decimal HotLat { get; set; }
        public decimal HotLng { get; set; }

        public string Status { get; set; }

        public Route RouteToHottest { get; set; }

        public decimal DegreesHotter
        {
            get
            {
                return HotTemp - BaseTemp;
            }
        }

        /// <summary>
        /// Find the hottest, closest place out of the 50 nearest the the co-ordinates
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longtitude"></param>      
        public HotPlace(decimal latitude, decimal longtitude)
        {
            BaseLat = latitude;
            BaseLng = longtitude;
            GetHottestNearby();
        }

        /// <summary>
        /// Find the hottest place nearby and store the values in the properties
        /// </summary>
        private void GetHottestNearby()
        {


            // build request for forecasts
            var forecastUrl = "http://api.openweathermap.org/data/2.5/find?lat={0}&lon={1}&cnt=50&mode=xml&APIKEY={2}";
            forecastUrl = string.Format(forecastUrl, BaseLat, BaseLng, WeatherApiKey);

            // get the forecasts and deserialise them
            // (yes, you could do also this with XmlDocuments.SelectNodes instead of deserializing)
            var localForecast = new XmlDocument();
            try
            {
                while (!ApiCounter.TryCall())
                {
                    // Wait until the quota resets
                    Thread.Sleep(1000);
                }

                // get the forecast and load into an object
                localForecast.Load(forecastUrl);
                var serializer = new XmlSerializer(typeof(CityList));
                var cityList = (CityList)serializer.Deserialize(new XmlNodeReader(localForecast));

                BaseTown = cityList.CityListItems[0].city.name;
                BaseTemp = cityList.CityListItems[0].temperature.TempCelcius;

                // get place(s) with the hottest temperature



                //var hotPlaces = cityList.CityListItems.Where(c =>
                //    c.temperature.TempCelcius ==
                //    cityList.CityListItems.Max(m => m.temperature.TempCelcius)).Select(c => c.city);


                // find the highest temperature 
                HotTemp = cityList.CityListItems.Max(t => t.temperature.TempCelcius);

                // now find all of the places where it's that hot
                var hotPlaces = from city in cityList.CityListItems
                                where city.temperature.TempCelcius == HotTemp
                                select city;

                // Next let's find out which town in closest. The results are in distance order as the crow flies but I have a regular car, not a flying one :-(
                // var HottestClosestPlace = new CityListItem();
                RouteToHottest = new Route();

                foreach (var place in hotPlaces)
                {
                    // get the driving directions from GoogleMaps and check if this is the closest hottest place.
                    var checkRoute = new Route { FromLat = BaseLat, FromLng = BaseLng, ToLat = place.city.coord.Latitude, ToLng = place.city.coord.Longtitude }.GetTime();

                    if (checkRoute.SecondsTravel < RouteToHottest.SecondsTravel)
                    {
                        // this hot place is closer, save the details
                        HotTown = place.city.name;
                        RouteToHottest = checkRoute;
                        HotLat = place.city.coord.Latitude;
                        HotLng = place.city.coord.Longtitude;
                    }
                }

                Status += "OK";
            }
            catch (Exception e)
            {
                Status += "\r\n" + e.Message;
            }
        }
    }

    public abstract class HotPlaceSearcher
    {

        public int MaxSteps { get; set; }
        public int Steps { get; set; }

        public HotPlace BasePlace { get; set; }
        public HotPlace HottestPlace { get; set; }
        public List<string> SearchList { get; set; }
        public Route RouteToHottest { get; set; }
        public decimal DegreesHotter => HottestPlace.HotTemp - BasePlace.BaseTemp;

    }

    public class HotPlaceExplorer : HotPlaceSearcher
    {
        // Explore a bit further
        // Find the hottest place nearby, then find the hottest place neat that.
        // Repeat up to max steps


        public HotPlaceExplorer(decimal latitude, decimal longitude)
        {
            MaxSteps = 30;
            SearchList = new List<string>();
            Steps = 0;

            BasePlace = new HotPlace(latitude, longitude);
            HottestPlace = BasePlace;

            // Check if we need to explore
            if (BasePlace.BaseTown == BasePlace.HotTown) return;  // You're already in the hottest place. Stay where you are

            while ((HottestPlace.BaseTown != HottestPlace.HotTown) && (Steps < MaxSteps))
            {
                HottestPlace = new HotPlace(HottestPlace.HotLat, HottestPlace.HotLng);
                SearchList.Add(HottestPlace.BaseTown);
                Steps++;
            }

            // Find out how far the hottest place is...
            RouteToHottest = new Route
            {
                FromLat = BasePlace.BaseLat,
                FromLng = BasePlace.BaseLng,
                ToLat = HottestPlace.HotLat,
                ToLng = HottestPlace.HotLng
            }.GetTime();
        }
    }

    public class HotPlaceScatter : HotPlaceSearcher
    {

        public HotPlaceScatter(decimal latitude, decimal longitude)
        {
            MaxSteps = 10;
            BasePlace = new HotPlace(latitude, longitude);
            const int placesToLook = 10;

            var rnd = new Random();

            // box size for UK mainland
            const decimal minLat = 49.3m;
            const decimal maxLat = 58.9m;
            const decimal minLng = -10.7m;
            const decimal maxLng = 2.1m;

            var searchCentreCoords = new decimal[10, 2];

            // find a random selection of places to look that fall in the UK boundary
            for (var placeCount = 0; placeCount < placesToLook; placeCount++)
            {
                var placeValid = false;
                while (!placeValid)
                {
                    // get a random location in the box
                    var lat = minLat + (decimal)rnd.NextDouble() * (maxLat - minLat);
                    var lng = minLng + (decimal)rnd.NextDouble() * (maxLng - minLng);

                    using (var webClient = new WebClient())
                    {
                        // check if the value is found in the uk
                        try
                        {
                            const string geocodeUrl = "https://maps.googleapis.com/maps/api/geocode/json?latlng={0},{1}&key={2}";
                            var json = webClient.DownloadString(String.Format(geocodeUrl, lat, lng, File.ReadAllText(HostingEnvironment.MapPath(@"~\App_Data\GeocodeApiKey.txt"))));
                            var location = JObject.Parse(json);

                            //var addressComponents = from l in location["results"][0]["formatted_address"]
                            //                        where l.ToString().EndsWith("UK")
                            //                        select l;

                            if (location["results"][0]["formatted_address"].ToString().EndsWith("UK"))
                            {
                                placeValid = true;
                                searchCentreCoords[placeCount, 0] = lat;
                                searchCentreCoords[placeCount, 1] = lng;
                            }
                        }
                        catch (Exception e)
                        {
                            // Couldn't geocode so ignore
                        }
                    }
                }
            }

            // explore from each of the random locations
            var explorers = new List<HotPlaceExplorer>();            
            for (var i = 0; i < placesToLook; i++)
            {
                explorers.Add(new HotPlaceExplorer(searchCentreCoords[i, 0], searchCentreCoords[i, 1]));
            }
            SearchList = explorers.Select(e => e.BasePlace.BaseTown).ToList();

            // find the hottest
            var maxTemp = explorers.Max(p => p.HottestPlace.HotTemp);

            // Check if here is hottest
            if (maxTemp <= BasePlace.BaseTemp)
            {
                HottestPlace = BasePlace;
                return;
            }

            // Get all locations with hottest temp
            var hotPlaces = from e in explorers
                            where (e.HottestPlace.HotTemp == maxTemp)
                            select e.HottestPlace;

            // check which is closest. (we only know how far they are from the random place at the moment)
            var shortestTime = long.MaxValue;
            foreach (var place in hotPlaces)
            {
                var checkRoute = new Route
                {
                    FromLat = BasePlace.BaseLat,
                    FromLng = BasePlace.BaseLng,
                    ToLat = place.HotLat,
                    ToLng = place.HotLng
                }.GetTime();

                if (checkRoute.SecondsTravel >= shortestTime) continue;

                shortestTime = checkRoute.SecondsTravel;
                RouteToHottest = checkRoute;
                HottestPlace = place;
            }
        }
    }
}
