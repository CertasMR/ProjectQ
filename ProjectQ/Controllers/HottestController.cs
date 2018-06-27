using ProjectQ.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ProjectQ.Controllers
{
    public class HottestController : Controller
    {
        string GoogleApiKey = "AIzaSyCrtsC3FqsuYt3taz0e-7-_2OScNWXO1Hg";
        string LocatorAPIKey = "AIzaSyAfqlBZdGetNxTer2aMJ-4eWpFDHLHfn0g";
        string WeatherApiKey = "3bd67cdea0def5d878ff62921fdb5f9c";

        // GET: Hottest
        public ActionResult Index()
        {

            // get current location
            // google maps api
            // if fail return View("Centre of the earth")

            var UserLatitude = 0m;
            var UserLontitude = 0m;
                       
            using (var webClient = new WebClient())
            {
                var LocatorURL = "https://www.googleapis.com/geolocation/v1/geolocate?key={0}";
                var LocatorParams = "{considerIp: \"true\"}";
                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                var json = webClient.UploadString(String.Format(LocatorURL, LocatorAPIKey),LocatorParams);
                var location = JObject.Parse(json);
                UserLatitude = ((Decimal)(location.SelectToken("location.lat")));
                UserLontitude = ((Decimal)(location.SelectToken("location.lng")));                
            }

            // create open weathermap query string

            var ForecastUrl = "http://api.openweathermap.org/data/2.5/find?lat={0}&lon={1}&cnt=50&mode=xml&APIKEY={2}";
            ForecastUrl = String.Format(ForecastUrl, UserLatitude.ToString(), UserLontitude.ToString(), WeatherApiKey);

            // get the forecasts and deserialise them in to the model
            // Yes, you could do also this with XmlDocuments.SelectNodes instead of deserializing

            var LocalForecast = new XmlDocument();
            LocalForecast.Load(ForecastUrl);
            var serializer = new XmlSerializer(typeof(CityList));
            CityList cityList = (CityList)serializer.Deserialize(new XmlNodeReader(LocalForecast));

            // find the highest temerature 
            var MaxTemp = cityList.CityListItems.Max(t => t.temperature.TempCelcius);

            // now find all of the places where it's that hot
            var HotPlaces = from city in cityList.CityListItems
                            where city.temperature.TempCelcius == MaxTemp
                            select city;


            // Next let's find out which town in closest. The results are in distance order as the crow flies but I have a regular car, not a flying one :-(
            var DirectionsUrl = @"https://maps.googleapis.com/maps/api/directions/json?origin={0},{1}&destination={2},{3}&key={4}";
            var HottestClosestPlace = new CityListItem();
            var ShortestDistance = 10000m;

            foreach (var place in HotPlaces)
            {
                // get the driving directions from GoogleMaps and check if this is the closest hottest place.
                var webClient = new WebClient();
                var json = webClient.DownloadString(String.Format(DirectionsUrl, UserLatitude, UserLontitude, place.city.coord.Latitude, place.city.coord.Latitude,GoogleApiKey));
                var directions = JObject.Parse(json);
                var thisDistance = ((Decimal)(directions.SelectToken("routes[0].legs[0].distance.value"))) / 1609.334m; //Convert distance from meters to miles
                if (thisDistance < ShortestDistance)
                {
                    HottestClosestPlace = place;
                    ShortestDistance = thisDistance;
                }
            }

            // Store data for the results view
            var hotNearby = new HotNearbyModel()
            {
                HotTemp = MaxTemp,
                HotTown = HottestClosestPlace.city.name,
                YourTown = cityList.CityListItems[0].city.name,
                YourTemp = cityList.CityListItems[0].temperature.TempCelcius 
            };
          

            // if closest city is elsewhere
            //  update "get moving model"
            //  View (GetMoving)

            //return View(cityList);
            return View(hotNearby);
        }

        //public ActionResult HotNearby()
        //{

        //}

    }
}