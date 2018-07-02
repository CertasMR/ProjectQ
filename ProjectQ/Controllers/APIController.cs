using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ProjectQ.Models;

namespace ProjectQ.Controllers
{
    [RoutePrefix("API")]
    public class APIController : ApiController
    {
        [HttpGet]
        [Route("Nearby/{lat}/{lng}")]
        public HotPlace Nearby(string lat, string lng)
        {
            lat = lat.Replace('_', '.');
            lng = lng.Replace('_', '.');
            return new HotPlace(Decimal.Parse(lat), Decimal.Parse(lng));
        }

        [HttpGet]
        [Route("Explore/{lat}/{lng}")]
        public HotPlaceExplorer Explore(string lat, string lng)
        {
            lat = lat.Replace('_', '.');
            lng = lng.Replace('_', '.');
            return new HotPlaceExplorer(Decimal.Parse(lat), Decimal.Parse(lng));
        }

    }
}
