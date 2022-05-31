using Grpc.Core;
using Grpc.Core.Interceptors;
using System.Diagnostics;

namespace Notes2022.Server.Services
{
    public class ServerLoggingInterceptor : Interceptor
    {
        private readonly ILogger _logger;

        public ServerLoggingInterceptor(ILogger<ServerLoggingInterceptor> logger)
        {
            _logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            _logger.LogInformation($"Starting receiving call. Type: {MethodType.Unary}. " +
                $"Method: {context.Method}.");
            try
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                TResponse? ret = await continuation(request, context);
                
                stopWatch.Stop();
                long elapsed = stopWatch.ElapsedMilliseconds;

                if (elapsed > Globals.ErrorThreshold)
                {
                    _logger.LogError($"Completed receiving call. Type: {MethodType.Unary}. " +
                        $"Method: {context.Method}. >>> {DateTime.Now.ToLongTimeString()}  Took: {elapsed} milli-seconds. Call took too long.");
                }
                else if (elapsed > Globals.WarnThreshold)
                {
                    _logger.LogWarning($"Completed receiving call. Type: {MethodType.Unary}. " +
                        $"Method: {context.Method}. >>> {DateTime.Now.ToLongTimeString()}  Took: {elapsed} milli-seconds. Call took a long time.");
                }
                else
                {
                    _logger.LogInformation($"Completed receiving call. Type: {MethodType.Unary}. " +
                        $"Method: {context.Method}. >>> {DateTime.Now.ToLongTimeString()}  Took: {elapsed} milli-seconds.");
                }

                return ret;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error thrown by {context.Method} - {ex.Message}.");
                throw;
            }
        }

        //private static long nanoTime()
        //{
        //    bool x = Stopwatch.IsHighResolution;
        //    long nano = 10000L * Stopwatch.GetTimestamp();
        //    nano /= TimeSpan.TicksPerMillisecond;
        //    nano *= 100L;
        //    return nano;
        //}
    }
}
