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
            // get current location
            // google maps api
            // if fail return View("Centre of the earth")

            // create open weathermap query string

            // request city list in load into xml doc

            var serializer = new XmlSerializer(typeof(CityList));
            using (var reader = XmlReader.Create("~\App_Data\forecast.xml"))
            {
                CityList cityList = (CityList)serializer.Deserialize(reader);
                List<CityListItem> Cities = cityList.CityListItems;
                
            }

            // find the higest temperature using xpath max query

            // iterate through the the city list and match each city with hightest temp value

            // calculate distance between current location and hot city

            // if city is closest set it as a match

            // if closest is here  - View (Hurrah - stay right where you are)

            // if closest city is elsewhere
            //  update "get moving model"
            //  View (GetMoving)

            return View();
        }
    }
}