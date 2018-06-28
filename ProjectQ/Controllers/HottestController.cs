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
      
        // GET: Hottest
        public ActionResult Index()
        {
            return View();
        
        }

        public ActionResult FindHottestNearby()
        {
            return View(new HotNearby());
        }
    }
}