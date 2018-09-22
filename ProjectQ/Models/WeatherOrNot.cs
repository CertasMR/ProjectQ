using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Profile;

namespace ProjectQ.Models
{
    public class WeatherOrNot
    {
        public string Status{ get; set; }

        public WeatherOrNot()
        {
            Status = "OK";
        }
    }

    //
    public class Forecast
    {
        public int Id { get; set; }
        public string City { get; set; }
        public DateTime ForecastIssuedAt { get; set; }
        public DateTime ForecastIssuedFor { get; set; }
        public decimal Temperature { get; set; }
        public string Conditions { get; set; }
        
        public Forecast()
        {

        }
    }

    /// <summary>
    /// Required for Entity Framework
    /// </summary>
    public class Context : DbContext
    {

        public DbSet<Forecast> Forecasts { get; set; }

        public Context() : base("DefaultConnection")
        {
        }
    }


}