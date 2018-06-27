using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace ProjectQ.Models
{
    // These classes can be used to deserialize the XML from OpenWeatherMap API "Current Wather Data - Cities is Cycle (sic)"
    // example call http://api.openweathermap.org/data/2.5/find?lat=54.0&lon=-1.54&cnt=50&mode=xml&APIKEY=3bd67cdea0def5d878ff62921fdb5f9c
    // API documentation at https://openweathermap.org/current
    // XML tags have only be add for elements required for this project. Add the tags and set the datatype if more are required.

    [XmlRoot("cities")]
    public class CityList
    {
        public int calctime { get; set; }
        public int count { get; set; }
        public string mode { get; set; }
        [XmlArray("list")]
        [XmlArrayItem("item")]
        public List<CityListItem> CityListItems;       
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

    public class City
    {
        [XmlAttribute("id")]
        public string id { get; set; }
        [XmlAttribute("name")]
        public string name { get; set; }

        public CityCoord coord { get; set; }
        public string country { get; set; }
        public CitySun sun { get; set; }
    }

    public class CityCoord
    {
        [XmlAttribute("lon")]
        public decimal Longtitude { get; set; }
        [XmlAttribute("lat")]
        public decimal Latitude { get; set; }
    }

    public class CitySun
    {
        [XmlAttribute("rise")]
        public DateTime Sunrise { get; set; }
        [XmlAttribute("set")]
        public DateTime SunSet { get; set; }
    }

    public class Temperature
    {
        [XmlAttribute("value")]
        public decimal Value { get; set; }
        [XmlAttribute("min")]
        public decimal Min { get; set; }
        [XmlAttribute("max")]
        public decimal Max { get; set; }
        [XmlAttribute("unit")]
        public string Unit { get; set; }

        // Report the temperature in slightly more sensible units
        [XmlIgnore]
        public decimal TempCelcius
        {
            get {
                return (Value-273.15m);
            }
        }
            


    }

    public class Humidity
    {
        // TODO Map & Type Attributes if required.
        public string value { get; set; }
        public string unit { get; set; }
    }

    public class Pressure
    {
        // TODO Map & Type Attributes if required.
        public string value { get; set; }
        public string unit { get; set; }
    }

    public class Wind
    {
        public WindSpeed speed { get; set; }
        public WindDirection direction { get; set; }
        public WindGust gust { get; set; }
    }

    public class WindSpeed
    {
        // TODO Map & Type Attributes if required.
        public string value { get; set; }
        public string name { get; set; }
    }

    public class WindDirection
    {
        // TODO Map & Type Attributes if required.
        public string value { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    }

    public class WindGust
    {
        // TODO Map & Type Attributes if required.
        public string value { get; set; }
        public string name { get; set; }
    }

    public class Clouds
    {
        // TODO Map & Type Attributes if required.
        public string value { get; set; }
        public string name { get; set; }
        public string low { get; set; }
        public string middle { get; set; }
        public string high { get; set; }
    }

    public class Precipitation
    {
        // TODO Map & Type Attributes if required.
        public string mode { get; set; }
    }

    public class Weather
    {
        // TODO Map & Type Attributes if required.
        public string number { get; set; }
        public string value { get; set; }
        public string icon { get; set; }
    }

    public class LastUpdate
    {
        // TODO Map & Type Attributes if required.
        public string value { get; set; }
        public string unix { get; set; }
    }

}