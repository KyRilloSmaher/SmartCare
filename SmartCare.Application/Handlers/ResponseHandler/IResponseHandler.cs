using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Handlers.ResponseHandler
{
    public interface IResponseHandler
    {
        Response<T> Deleted<T>(T data, string message = null);
        Response<T> Success<T>(T entity, string message = null);
        Response<T> Unauthorized<T>(string message = null);
        Response<T> BadRequest<T>(string message = null);
        Response<T> BadRequest<T>(T data, string message = null);
        Response<T> UnprocessableEntity<T>(string message = null);
        Response<T> Failed<T>(string message = null);
        Response<T> NotFound<T>(string message = null);
        Response<T> Created<T>(T entity);
        Response<T> Added<T>(string message = null);
    }
}
