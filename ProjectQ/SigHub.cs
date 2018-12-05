using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace ProjectQ
{
    public class SigHub : Hub
    {
        public void Send(string name, string message)
        {            
            Clients.All.addNewMessageToPage(name, message);
        }

        public void ScatterGun(decimal lat, decimal lng)
        {
            //return PartialView(new HotPlaceScatter(decimal.Parse(lat), decimal.Parse(lng)));
            try
            {
                Clients.Caller.showPlace("yay");
                Thread.Sleep(5000);
                Clients.Caller.showPlace("woo");
                Thread.Sleep(5000);
                Clients.Caller.showPlace("ok");
                Thread.Sleep(5000);
                Clients.Caller.showPlace("now");
Thread.Sleep(100);
            }
            catch (Exception e)
            {

            }
        }

        public void ShowPlace(string place)
        {
            return;
            // try
            //    {
            //        Clients.Caller.showPlace(place);
            //    }
            //    catch (Exception e)
            //    {

            //    }

        }
    }
}