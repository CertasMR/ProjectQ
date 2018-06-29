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
    public class HotPlace
    {
        // API keys, these should be configured for IP whitelist of obfucated for production
        const string DistanceApiKey = "AIzaSyCrtsC3FqsuYt3taz0e-7-_2OScNWXO1Hg";        
        const string WeatherApiKey = "3bd67cdea0def5d878ff62921fdb5f9c";

        public string BaseTown { get; set; }
        public decimal BaseTemp { get; set; }
        public string HotTown { get; set; }
        public decimal HotTemp { get; set; }
        public decimal Distance { get; set; }
        public long SecsToHottest { get; set; }
        public string TimeToHottestAsText { get; set; }
        public string Status { get; set; }
        public decimal DegreesHotter
        {
            get
            {
                return HotTemp  - BaseTemp;
            }
        }

        private decimal Lat = 0m;
        private decimal Lng = 0m;
       
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
            Lat = latitude;
            Lng = longtitude;
            GetHottestNearby();
        }

        /// <summary>
        /// Find the hottest place nearby and store the values in the properties
        /// </summary>
        private void GetHottestNearby()
        {

            // build request for forecasts
            var ForecastUrl = "http://api.openweathermap.org/data/2.5/find?lat={0}&lon={1}&cnt=50&mode=xml&APIKEY={2}";
            ForecastUrl = String.Format(ForecastUrl, Lat, Lng, WeatherApiKey);

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
                var DirectionsUrl = @"https://maps.googleapis.com/maps/api/directions/json?origin={0},{1}&destination={2},{3}&key={4}";
                var HottestClosestPlace = new CityListItem();
                SecsToHottest = int.MaxValue; // Set shortest trip to arbitrarily high value. 

                foreach (var place in HotPlaces)
                {
                    // get the driving directions from GoogleMaps and check if this is the closest hottest place.
                    var webClient = new WebClient();
                    var json = webClient.DownloadString(String.Format(DirectionsUrl, Lat, Lng, place.city.coord.Latitude, place.city.coord.Longtitude , DistanceApiKey));
                    var directions = JObject.Parse(json);
                    var thisTripTime = ((int)(directions.SelectToken("routes[0].legs[0].duration.value")));
                    if (thisTripTime < SecsToHottest )
                    {
                        HotTown = place.city.name;
                        SecsToHottest = thisTripTime;
                        TimeToHottestAsText = (string)(directions.SelectToken("routes[0].legs[0].duration.text"));
                        Distance  = ((Decimal)(directions.SelectToken("routes[0].legs[0].distance.value"))) / 1609.334m; //Convert distance from meters to miles
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

}
