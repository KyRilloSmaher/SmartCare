using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Handlers.ResponsesHandler
{
    public class ResponseHandler : IResponseHandler
    {
        #region Feilds



        #endregion

        #region Constructors

        public ResponseHandler()
        {

        }
        #endregion


        #region Methods
        public Response<T> Deleted<T>(T data, string message = null)
        {
            return new Response<T>()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Succeeded = true,
                Message = message ?? "DeletedSuccessfully"
            };
        }
        public Response<T> Success<T>(T entity, string message = null)
        {
            return new Response<T>()
            {
                Data = entity,
                StatusCode = System.Net.HttpStatusCode.OK,
                Succeeded = true,
                Message = message ?? "Success",
            };
        }
        public Response<T> Unauthorized<T>(string message = null)
        {
            return new Response<T>()
            {
                StatusCode = System.Net.HttpStatusCode.Unauthorized,
                Succeeded = true,
                Message = message ?? "Unauthorized"
            };
        }
        public Response<T> BadRequest<T>(string message = null)
        {
            return new Response<T>()
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Succeeded = false,
                Message = message ?? "NotFound"
            };
        }
        public Response<T> BadRequest<T>(T data, string message = null)
        {
            return new Response<T>()
            {
                Data = data,
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Succeeded = false,
                Message = message ?? "Not Found"
            };
        }
        public Response<T> UnprocessableEntity<T>(string message = null)
        {
            return new Response<T>()
            {
                StatusCode = System.Net.HttpStatusCode.UnprocessableEntity,
                Succeeded = false,
                Message = message ?? "InternalServerError"
            };
        }
        public Response<T> Failed<T>(string message = null)
        {
            return new Response<T>()
            {
                StatusCode = System.Net.HttpStatusCode.FailedDependency,
                Succeeded = false,
                Message = message ?? "InternalServerError"
            };
        }
        public Response<T> NotFound<T>(string message = null)
        {
            return new Response<T>()
            {
                StatusCode = System.Net.HttpStatusCode.NotFound,
                Succeeded = false,
                Message = message ?? "NotFound"
            };
        }

        public Response<T> Created<T>(T entity)
        {
            return new Response<T>()
            {
                Data = entity,
                StatusCode = System.Net.HttpStatusCode.Created,
                Succeeded = true,
                Message = "CreatedSuccessfully",
            };
        }
        public Response<T> Added<T>(string message = null)
        {
            return new Response<T>()
            {

                StatusCode = System.Net.HttpStatusCode.OK,
                Succeeded = true,
                Message = message ?? "AddedSuccessfully"

            };
        }
        #endregion
    }
}
