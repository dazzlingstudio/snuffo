using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Umbraco.Core.Composing;

namespace Snuffo.Web
{
    public class SnuffoStartUpHandlers : IUserComposer
    {
        public void Compose(Composition composition)
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);            
        }
    }
}