using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderMeetingRoom
{
    public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType()) return false;

            User p = (User)obj;
            return (this.UserId == p.UserId);
        }
    }
}
