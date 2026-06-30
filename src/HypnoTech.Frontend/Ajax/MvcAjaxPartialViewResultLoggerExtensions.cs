using System;
using Microsoft.Extensions.Logging;

namespace HypnoTech.Frontend.Ajax
{
    internal static class MvcAjaxPartialViewResultLoggerExtensions
    {
        private static readonly Action<ILogger, Exception> _jsonInputFormatterCrashed;

        private static readonly Action<ILogger, string, Exception> _jsonResultExecuting;

        static MvcAjaxPartialViewResultLoggerExtensions()
        {
            _jsonInputFormatterCrashed = LoggerMessage.Define(
                LogLevel.Debug,
                1,
                "AjaxPartialViewResult JSON input formatter threw an exception.");

            _jsonResultExecuting = LoggerMessage.Define<string>(
                LogLevel.Information,
                1,
                "Executing AjaxPartialViewResult, writing value of type '{Type}'.");
        }

        public static void AjaxPartialViewResultJsonInputException(this ILogger logger, Exception exception)
        {
            _jsonInputFormatterCrashed(logger, exception);
        }

        public static void AjaxPartialViewResultExecuting(this ILogger logger, object value)
        {
            var type = value == null ? "null" : value.GetType().FullName;
            _jsonResultExecuting(logger, type, null);
        }
    }
}
