using ProjectQ.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;

namespace ProjectQ.Controllers
{
    public class HottestController : Controller
    {
        // GET: Hottest
        public ActionResult Index()
        {
            var UserLatitude = 0m;
            var UserLontitude = 0m;

            // get current location
            // google maps api
            // if fail return View("Centre of the earth")

            UserLatitude = 54.0m;
            UserLontitude = -1.54m;

            // create open weathermap query string

            var API_URL = "http://api.openweathermap.org/data/2.5/find?lat={0}&lon={1}&cnt=50&mode=xml&APIKEY=3bd67cdea0def5d878ff62921fdb5f9c";
            API_URL = String.Format(API_URL, UserLatitude.ToString(), UserLontitude.ToString());

            // get the forecasts and deserialise them in to the model

            var LocalForecast = new XmlDocument();
            LocalForecast.Load(API_URL);
            var serializer = new XmlSerializer(typeof(CityList));
            CityList cityList = (CityList)serializer.Deserialize(new XmlNodeReader(LocalForecast));

            // find the higest temperature using xpath max query

            // iterate through the the city list and match each city with hightest temp value

            // calculate distance between current location and hot city

            // if city is closest set it as a match

            // if closest is here  - View (Hurrah - stay right where you are)

            // if closest city is elsewhere
            //  update "get moving model"
            //  View (GetMoving)

            return View(cityList);
        }
    }
}