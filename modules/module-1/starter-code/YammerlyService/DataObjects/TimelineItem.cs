using Microsoft.Azure.Mobile.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YammerlyService.DataObjects
{
    public class TimelineItem : EntityData
    {
        public virtual Employee Author { get; set; }
        public string Text { get; set; }
        public string PhotoUrl { get; set; }
    }
}
