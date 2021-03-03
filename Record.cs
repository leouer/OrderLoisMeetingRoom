using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderMeetingRoom
{
    public class Record
    {
        public int Id { get; set; }
        public DateTimeOffset StartDateTime { get; set; }
        public DateTimeOffset EndDateTime { get; set; }
        public string MeetingRoom { get; set; }
        public string Description { get; set; }
        public string UserName { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType()) return false;

            Record p = (Record)obj;
            return (this.Id == p.Id);
        }
    }
}
