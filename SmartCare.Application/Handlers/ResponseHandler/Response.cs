using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Handlers.ResponseHandler
{
    public class Response<T>
    {
        #region Feilds

        public HttpStatusCode StatusCode { get; set; }
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public Dictionary<string, List<string>> ErrorsBag { get; set; }
        public T Data { get; set; }
        #endregion

        #region Constructors
        public Response()
        {

        }
        public Response(T data, string message = null)
        {
            Succeeded = true;
            Message = message;
            Data = data;
        }
        public Response(string message)
        {
            Succeeded = false;
            Message = message;
        }
        public Response(string message, bool succeeded)
        {
            Succeeded = succeeded;
            Message = message;
        }

        #endregion
    }
}
