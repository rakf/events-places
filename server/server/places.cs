using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    class places
    {
        public string status { get; set; }
        public List<Dictionary<string, string>> response { get; set; }
       /* public places(string stat, List<Dictionary<string, string>> data )
        {
            status = stat;
            response = data;
        }*/
    }
}
