using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ProjectQ.Models;

namespace ProjectQ.Controllers
{
    [RoutePrefix("api")]
    public class GetApiController : ApiController
    {
        [HttpGet]
        [Route("Nearby/{lat}/{lng}/")]
        public HotPlace Nearby(decimal lat, decimal lng)
        {
            return new HotPlace(lat, lng);
        }

        [HttpGet]
        [Route("Explore/{lat}/{lng}/")]
        public HotPlaceExplorer Explore(decimal lat, decimal lng)
        {
            return new HotPlaceExplorer(lat, lng);
        }

        [HttpGet]
        [Route("WeatherOrNot")]
        public WeatherOrNot WeatherOrNot()
        {
            return new WeatherOrNot();
        }


    }
}
