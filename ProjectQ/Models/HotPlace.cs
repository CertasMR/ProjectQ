using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace ProjectQ.Models
{

    public static class APICounter
    {
        private const int MaxCallsPerMin = 60;
        public static int CallCount { get; set; }
        public static DateTime LastReset { get; set; }

        static APICounter()
        {
            CallCount = 0;
            LastReset = DateTime.Now;
        }

        /// <summary>
        /// Returns true if call can be made and increments counter. False is call quota has been used up.
        /// </summary>
        /// <returns></returns>
        public static bool TryCall()
        {
            if (LastReset.AddMinutes(1) > DateTime.Now)
            {
                LastReset = DateTime.Now;
                CallCount = 1;
                return true;
            }
            else if (CallCount >= MaxCallsPerMin)
            {
                return false;
            }
            else
            {
                CallCount++;
                return true;
            }
        }
    }

    /// <summary>
    /// Gets route between two points using goole maps API when instanciated
    /// </summary>
    public class Route
    {
        // TODO - API key should be configured for IP whitelist or obfucated for production
        const string DistanceApiKey = "AIzaSyCrtsC3FqsuYt3taz0e-7-_2OScNWXO1Hg";
        const string DirectionsUrl = @"https://maps.googleapis.com/maps/api/directions/json?origin={0},{1}&destination={2},{3}&key={4}";

        /// <summary>
        /// Distanec in Miles
        /// </summary>
        public decimal Distance { get; set; }
        public long SecondsTravel { get; set; }
        public string TimeAsText { get; set; }
        public string Status { get; set; }

        public Route()
        {
            Status = "";
            SecondsTravel = long.MaxValue;  // we're usually looking for the quickest route so make blank routes the longest
        }

        public Route(decimal fromLat, decimal fromLng, decimal toLat, decimal toLng)
        {
            using (var webClient = new WebClient())
            {
                try
                {
                    var json = webClient.DownloadString(String.Format(DirectionsUrl, fromLat, fromLng, toLat, toLng, DistanceApiKey));
                    var directions = JObject.Parse(json);
                    SecondsTravel = ((long)(directions.SelectToken("routes[0].legs[0].duration.value")));
                    TimeAsText = (string)(directions.SelectToken("routes[0].legs[0].duration.text"));
                    Distance = ((Decimal)(directions.SelectToken("routes[0].legs[0].distance.value"))) / 1609.334m; //Convert distance from meters to miles
                    Status = "OK";
                }
                catch (Exception e)
                {
                    Status = e.Message;
                }
            }
        }
    }

    /// <summary>
    /// Finds the hottest place close to a point and requests the driving time to that location.
    /// </summary>
    public class HotPlace
    {
        // TODO - API key should be configured for IP whitelist or obfucated for production
        public const string WeatherApiKey = "3bd67cdea0def5d878ff62921fdb5f9c";

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
            var ForecastUrl = "http://api.openweathermap.org/data/2.5/find?lat={0}&lon={1}&cnt=50&mode=xml&APIKEY={2}";
            ForecastUrl = String.Format(ForecastUrl, BaseLat, BaseLng, WeatherApiKey);

            // get the forecasts and deserialise them
            // (yes, you could do also this with XmlDocuments.SelectNodes instead of deserializing)
            var LocalForecast = new XmlDocument();
            try
            {
                while (!APICounter.TryCall())
                {
                    // Wait until the quota resets
                    Thread.Sleep(1000);
                }

                LocalForecast.Load(ForecastUrl);
                var serializer = new XmlSerializer(typeof(CityList));
                CityList cityList = (CityList)serializer.Deserialize(new XmlNodeReader(LocalForecast));

                // find the highest temerature 
                HotTemp = cityList.CityListItems.Max(t => t.temperature.TempCelcius);

                // now find all of the places where it's that hot
                var HotPlaces = from city in cityList.CityListItems
                                where city.temperature.TempCelcius == HotTemp
                                select city;

                // Next let's find out which town in closest. The results are in distance order as the crow flies but I have a regular car, not a flying one :-(
                var HottestClosestPlace = new CityListItem();
                RouteToHottest = new Route();

                foreach (var place in HotPlaces)
                {
                    // get the driving directions from GoogleMaps and check if this is the closest hottest place.
                    var CheckRoute = new Route(BaseLat, BaseLng, place.city.coord.Latitude, place.city.coord.Longtitude);

                    if (CheckRoute.SecondsTravel < RouteToHottest.SecondsTravel)
                    {
                        // this hot place is closer, save the details
                        HotTown = place.city.name;
                        RouteToHottest = CheckRoute;
                        HotLat = place.city.coord.Latitude;
                        HotLng = place.city.coord.Longtitude;
                    }
                }

                BaseTown = cityList.CityListItems[0].city.name;
                BaseTemp = cityList.CityListItems[0].temperature.TempCelcius;
                Status += "OK";
            }
            catch (Exception e)
            {
                Status += "\r\n" + e.Message;
            }
        }
    }

    public abstract class _HotPlaceSearcher
    {

        public int MaxSteps { get; set; }
        public int Steps { get; set; }

        public HotPlace BasePlace { get; set; }
        public HotPlace HottestPlace { get; set; }
        public List<string> SearchList { get; set; }
        public Route RouteToHottest { get; set; }
        public decimal DegreesHotter
        {
            get
            {
                return HottestPlace.HotTemp - BasePlace.BaseTemp;
            }
        }
    }


    public class HotPlaceExplorer : _HotPlaceSearcher
    {
        // Explore a bit further
        // Find the hottest place nearby, then find the hottest place neat that.
        // Repeat up to max steps



        public HotPlaceExplorer(decimal latitude, decimal longtitude)
        {
            MaxSteps = 30;
            SearchList = new List<string>();
            Steps = 0;

            BasePlace = new HotPlace(latitude, longtitude);
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
            RouteToHottest = new Route(BasePlace.BaseLat, BasePlace.BaseLng, HottestPlace.HotLat, HottestPlace.HotLng);
        }
    }

    public class HotPlaceScatter : _HotPlaceSearcher
    {

        public HotPlaceScatter(decimal latitude, decimal longtitude)
        {
            MaxSteps = 10;

            var BasePlace = new HotPlace(latitude, latitude);

            const string GeocodeApiKey = "AIzaSyCrtsC3FqsuYt3taz0e-7-_2OScNWXO1Hg";
            var GeocodeURL = "https://maps.googleapis.com/maps/api/geocode/json?latlng={0},{1}&key={2}";
            var rnd = new Random();

            // box size for UK mainland
            var minLat = 49.3m;
            var maxLat = 58.9m;
            var minLng = -10.7m;
            var maxLng = 2.1m;

            var PlacesToLook = 10;
            decimal[,] Coords = new decimal[10, 2];

            for (int placeCount = 0; placeCount < PlacesToLook; placeCount++)
            {
                var PlaceValid = false;
                while (!PlaceValid)
                {
                    // get a random location in the box
                    decimal lat = minLat + ((decimal)(rnd.NextDouble()) * (maxLat - minLat));
                    decimal lng = minLng + ((decimal)(rnd.NextDouble()) * (maxLng - minLng));

                    using (var webClient = new WebClient())
                    {
                        // check if the value is found in the uk
                        try
                        {

                        
                        var json = webClient.DownloadString(String.Format(GeocodeURL, lat, lng, GeocodeApiKey));
                        var location = JObject.Parse(json);

                        var AddressComponents = from l in location["results"]["address_components"]["short_name"]
                                                where l.ToString() == "UK"
                                                select l;

                        //if ((string)location.SelectToken("results[0].address_components[4].short_name") == "GB")
                        if (AddressComponents.Count() > 0)
                        {
                            PlaceValid = true;
                            Coords[placeCount, 0] = lat;
                            Coords[placeCount, 1] = lng;
                        }
                        }
                        catch
                        {
                            // Couldn't geocode so ignore
                        }
                    }
                }
            }

            // explore from each of the random locations
            var Explorers = new List<HotPlaceExplorer>();
            for (var i = 0; i < PlacesToLook; i++)
            {
                Explorers.Add(new HotPlaceExplorer(Coords[i, 0], Coords[i, 1]));
            }

            // find the hottest
            var MaxTemp = Explorers.Max(p => p.HottestPlace.HotTemp);

            // Check if here is hottest
            if (MaxTemp == BasePlace.BaseTemp)
            {
                HottestPlace = BasePlace;
                return;
            }

            // Get all locations with hottest temp
            var HotPlaces = from e in Explorers
                            where (e.HottestPlace.HotTemp == MaxTemp)
                            select e.HottestPlace;

            // check which is closest. (we only know how far they are from the random place at the moment)
            var ShortestTime = int.MaxValue;
            foreach (var place in Explorers)
            {
                var CheckRoute = new Route(BasePlace.BaseLat, BasePlace.BaseLng, place.HottestPlace.HotLat, place.HottestPlace.HotLng);
                if (CheckRoute.SecondsTravel < ShortestTime)
                {
                    HottestPlace = place.HottestPlace;
                    return;
                }
            }
        }

    }
}
