using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace ProjectQ.Models
{
    public class CityCoord
    {
        public string lon { get; set; }
        public string lat { get; set; }
    }

    public class CitySunRise
    {
        public string lat { get; set; }
        public string set { get; set; }
    }

    public class City
    {
        public string id { get; set; }
        public string name { get; set; }
        public CityCoord coord { get; set; }
        public string country { get; set; }
        [XmlAttribute("sun rise")]
        public CitySunRise sunRise { get; set; }
    }

    public class Temperature
    {
        public decimal value{ get; set; }  // we need this value so make sure it's the right type
        public string min { get; set; } 
        public string max { get; set; }
        public string unit { get; set; }
    }

    public class Humidity
    {
        public string value { get; set; }
        public string unit { get; set; }
    }

    public class Pressure
    {
        public string value { get; set; }
        public string unit { get; set; }
    }

    public class WindSpeed
    {
        public string value { get; set; }
        public string name { get; set; }
    }

    public class WindDirection
    {
        public string value { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    }

    public class WindGust
    {
        public string value { get; set; }
        public string name { get; set; }
    }

    public class Wind
    {
        public WindSpeed speed { get; set; }
        public WindDirection direction { get; set; }
        public WindGust gust { get; set; }
    }

    public class Clouds
    {
        public string value { get; set; }
        public string name { get; set; }
        public string low { get; set; }
        public string middle { get; set; }
        public string high { get; set; }
    }

    public class Precipitation
    {
        public string mode { get; set; }
    }

    public class Weather
    {
        public string number { get; set; }
        public string value { get; set; }
        public string icon { get; set; }
    }

    public class LastUpdate
    {
        public string value { get; set; }
        public string unix { get; set; }
    }

    public class CityListItem
    {
        public City city { get; set; }
        public Temperature temperature { get; set; }
        public Humidity humidity { get; set; }
        public Pressure pressure { get; set; }
        public Wind wind { get; set; }
        public Clouds clouds { get; set; }
        public Precipitation precipitation { get; set; }
        public Weather weather { get; set; }
        public LastUpdate lastupdate { get; set; }
    }
    
    [XmlRoot("cities")]
    public class CityList
    {
        [XmlArray("list")]
        [XmlArrayItem("item")]
        public List<CityListItem> CityListItems;
    }

}