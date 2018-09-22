using System;
using System.IO;
using System.Net;
using System.Web.Hosting;
using Newtonsoft.Json.Linq;

namespace ProjectQ.Models
{
    /// <summary>
    /// Gets route between two points using google maps API when instantiated
    /// </summary>
    public class Route
    {
        /// <summary>
        /// Distance in Miles
        /// </summary>
        public decimal Distance { get; set; }
        public long SecondsTravel { get; set; }
        public string TimeAsText { get; set; }
        public string Status { get; set; }

        public decimal FromLat { get; set; }
        public decimal FromLng { get; set; }
        public decimal ToLat { get; set; }
        public decimal ToLng { get; set; }

        public Route()
        {
            Status = "";
            SecondsTravel = long.MaxValue;  // we're usually looking for the quickest route so make blank routes the longest
        }

        public Route GetTime()
        {
            using (var webClient = new WebClient())
            {
                try
                {
                    // query google API
                    var DirectionsUrl = @"https://maps.googleapis.com/maps/api/directions/json?origin={0},{1}&destination={2},{3}&key={4}";
                    var json = webClient.DownloadString(string.Format(DirectionsUrl, FromLat, FromLng, ToLat, ToLng, 
                            File.ReadAllText(HostingEnvironment.MapPath(@"\App_Data\DirectionsApiKey.txt") ?? throw new InvalidOperationException())));
                    var directions = JObject.Parse(json);

                    // put results in object properties
                    SecondsTravel = (long)(directions.SelectToken("routes[0].legs[0].duration.value"));
                    TimeAsText = (string)directions.SelectToken("routes[0].legs[0].duration.text");
                    Distance = (decimal)directions.SelectToken("routes[0].legs[0].distance.value") / 1609.334m; //Convert distance from meters to miles
                    Status = "OK";
                }
                catch (Exception e)
                {
                    Status = e.Message;
                }
            }

            return this;
        }

    }
}