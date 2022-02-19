﻿namespace WebAPI.Helper.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLoggingMiddleware(this IApplicationBuilder builder)
        => builder.UseMiddleware<RequestLoggingMiddleware>();
}