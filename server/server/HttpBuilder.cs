using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    class HttpBuilder
    {
        public static HttpResponse InternalServerError()
        {
            

            return new HttpResponse()
            {
                ReasonPhrase = "InternalServerError",
                StatusCode = "500",
               
            };
        }

        public static HttpResponse NotFound()
        {
            

            return new HttpResponse()
            {
                ReasonPhrase = "NotFound",
                StatusCode = "404",
                
            };
        }
    }
}
