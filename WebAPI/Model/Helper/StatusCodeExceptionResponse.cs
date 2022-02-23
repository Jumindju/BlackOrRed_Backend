namespace WebAPI.Model.Helper;

public record StatusCodeExceptionResponse(string Message, Exception? Inner);