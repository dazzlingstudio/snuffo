using Dazzling.Studio.Utils;
using Uvendia.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.Models;

namespace Snuffo.Web.Models
{
    public class EventDetailModel : ContentModel
    {
        public EventDetailModel() : this(Current.UmbracoContext.PublishedRequest.PublishedContent)
        {

        }
        public EventDetailModel(IPublishedContent content) : base(content) {
            this.Event = new Event();
        }

        public Event @Event { get; set; }

        private Dictionary<long, int> _buffer = new Dictionary<long, int>();

        /// <summary>
        /// Gets the maximum tickets for sale.
        /// </summary>
        /// <param name="ts">The ts.</param>
        /// <returns></returns>
        /// <remarks>
        /// This method is to avoid multiple roundtrips to the database.
        /// </remarks>
        public int GetMaxTicketsForSale(TicketSale ts)
        {
            int max = 0;
            if (!_buffer.ContainsKey(ts.Id))
            {
                max = ts.GetMaxTicketsForSale(10, 20);
                _buffer.Add(ts.Id, max);
            }
            else
            {
                max = _buffer[ts.Id];
            }
            return max;
        }
    }
}