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

    public class Route
    {
        const string DistanceApiKey = "AIzaSyCrtsC3FqsuYt3taz0e-7-_2OScNWXO1Hg";
        const string DirectionsUrl = @"https://maps.googleapis.com/maps/api/directions/json?origin={0},{1}&destination={2},{3}&key={4}";

        public decimal Distance { get; set; }
        public long SecondsTravel { get; set; }
        public string TimeAsText { get; set; }
        public string Status { get; set; }

        public Route()
        {
            Status = "";
            SecondsTravel = long.MaxValue;
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

    public class HotPlace
    {
        // API keys, these should be configured for IP whitelist of obfucated for production

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
        /// <remarks>
        /// This be used for exploring from a know hot place or using a scattergun search stragegy.
        /// Also allows API access it we want to add it later.
        /// </remarks>
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

            // get the forecasts and deserialise them in to the model
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
            // for 1 to N
            //   while point is not valid
            //      generate a point UK lat/lng range       (49.3, -10.7  to   58.9,2.1)
            //      get town name                           (google maps geocode?)
            //      if town is in UK then valid             

            // For each place
            //   get circle forcast     (HotPlace)
            //   get max temp

            // with max temp town (Explore) from there 
            
        }
    }
}
