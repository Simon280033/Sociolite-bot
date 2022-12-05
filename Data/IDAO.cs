using REST.Model.ExchangeClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTeamsApp2.Data
{
    internal interface IDAO
    {
        public HttpResponseMessage PostTeam(string channelId);

        public Task<ActivityRequestObject> TeamAndActivityByChannelId(string channelId);
    }
}
