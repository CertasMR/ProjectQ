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
        [Route("HotPlaceNear/{lat}/{lng}")]
        public HotPlace HotPlaceNear(decimal lat, decimal lng)
        {
            return new HotPlace(lat, lng);
        }      
    }
}
