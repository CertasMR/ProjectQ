using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;


namespace ProjectQ.Models
{
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

    public class HotPlaceExplorer
    {
        // Explore a bit further
        // Find the hottest place nearby, then find the hottest place neat that.
        // Repeat up to max steps

        const int MaxSteps = 30;
        public int Steps { get; set; }

        public HotPlace BasePlace { get; set; }
        public HotPlace HottestPlace { get; set; }
        public List<string> PassingThrough { get; set; }
        public Route RouteToHottest { get; set; }
        public decimal DegreesHotter
        {
            get
            {
                return HottestPlace.HotTemp - BasePlace.BaseTemp;
            }
        }

        public HotPlaceExplorer(decimal latitude, decimal longtitude)
        {
            PassingThrough = new List<string>();
            Steps = 0;

            BasePlace = new HotPlace(latitude, longtitude);
            HottestPlace = BasePlace;

            // Check if we need to explore
            if (BasePlace.BaseTown == BasePlace.HotTown) return;  // You're already in the hottest place. Stay where you are

            while ((HottestPlace.BaseTown != HottestPlace.HotTown) && (Steps < MaxSteps))
            {
                HottestPlace = new HotPlace(HottestPlace.HotLat, HottestPlace.HotLng);
                PassingThrough.Add(HottestPlace.BaseTown);
                Steps++;
            }

            // Find out how far the hottest place is...
            RouteToHottest = new Route(BasePlace.BaseLat, BasePlace.BaseLng, HottestPlace.HotLat, HottestPlace.HotLng);
        }
    }

    public class HotPlaceScatter
    {
        public HotPlaceScatter(decimal latitude, decimal longtitude)
        {
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
            decimal[,] Coords= new decimal[10,2];

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
                        var json = webClient.DownloadString(String.Format(GeocodeURL, lat, lng, GeocodeApiKey));
                        var location = JObject.Parse(json);
                        if ((string)location.SelectToken("results.address_components[6].shortname") == "UK")
                        {                          
                            PlaceValid = true;
                            Coords[placeCount, 0] = lat;
                            Coords[placeCount, 1] = lng;
                        }
                    }
                }
            }

            var Explorers = new List<HotPlaceExplorer>();

            for (var i = 0; i < PlacesToLook; i++)
            {
                Explorers.Add(new HotPlaceExplorer(Coords[i,0], Coords[i,1]));
            }

            var MaxTemp = -100m;
            HotPlaceExplorer LaraCroft;  //sorry
            foreach (var explorer in Explorers)
            {
                if 
            }

        }



        // For each place
        //   get circle forcast     (HotPlace)
        //   get max temp

        // with max temp town (Explore) from there 

    }
}
}
