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
    [RoutePrefix("Hottest")]
    public class HottestController : Controller
    {
      
        // GET: Hottest
        public ActionResult Index()
        {
            return View();       
        }


        [Route("FindNear/{lat}/{lng}")]
        public ActionResult FindNear(string lat, string lng)
        {
            lat = lat.Replace('_', '.');
            lng = lng.Replace('_', '.');
            return View(new HotPlace(Decimal.Parse(lat), Decimal.Parse(lng)));
        }


        [Route("Explore/{lat}/{lng}")]
        public ActionResult Explore(decimal lat, decimal lng)
        {
            return View(new HotPlace(lat, lng));
        }

        [Route("ScatterGunSearch/{lat}/{lng}")]
        public ActionResult ScatterGunSearch(decimal lat, decimal lng)
        {
            return View(new HotPlace(lat, lng));
        }

    }
}