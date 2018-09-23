using ProjectQ.Models;
using System;
using System.Web.Mvc;

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


        [Route("Nearby/{lat}/{lng}")]
        public ActionResult Nearby(string lat, string lng)
        {
            lat = lat.Replace('_', '.');
            lng = lng.Replace('_', '.');
            return View(new HotPlace(Decimal.Parse(lat), decimal.Parse(lng)));
        }


        [Route("Explore/{lat}/{lng}")]
        public ActionResult Explore(string lat, string lng)
        {
            //lat = lat.Replace('_', '.');
            //lng = lng.Replace('_', '.');
            return View(new Coordinates { Lat = lat, Lng=lng});
        }

        [Route("ExploreResults/{lat}/{lng}")]
        public ActionResult ExploreResults(string lat, string lng)
        {
            lat = lat.Replace('_', '.');
            lng = lng.Replace('_', '.');
            return PartialView(new HotPlaceExplorer(decimal.Parse(lat), decimal.Parse(lng)));
        }

        
        [Route("ScatterGunSearch/{lat}/{lng}")]
        public ActionResult ScatterGunSearch(string lat, string lng)
        {
            //lat = lat.Replace('_', '.');
            //lng = lng.Replace('_', '.');
            return View(new Coordinates { Lat = lat, Lng = lng });
        }

        [Route("ScatterGunSearchResults/{lat}/{lng}")]
        public ActionResult ScatterGunSearchResults(string lat, string lng)
        {
            lat = lat.Replace('_', '.');
            lng = lng.Replace('_', '.');
            return PartialView(new HotPlaceScatter(decimal.Parse(lat), decimal.Parse(lng)));
        }


    }
}