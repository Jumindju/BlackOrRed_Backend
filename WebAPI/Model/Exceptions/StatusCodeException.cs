using System.Net;

namespace WebAPI.Model.Exceptions;

public class StatusCodeException : Exception
{
    public HttpStatusCode StatusCode { get; set; }

    public StatusCodeException(HttpStatusCode statusCode, string message)
    :base(message)
    {
        StatusCode = statusCode;
    }

    public StatusCodeException(HttpStatusCode statusCode, string message, Exception inner)
        : base(message, inner)
    {
        StatusCode = statusCode;
    }
}