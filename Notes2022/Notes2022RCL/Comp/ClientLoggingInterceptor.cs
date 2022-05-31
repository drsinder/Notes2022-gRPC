using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notes2022RCL.Comp
{
    public class ClientLoggingInterceptor : Interceptor
    {
        public ClientLoggingInterceptor()
        {
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            try
            {
                Console.WriteLine($"Client Starting call. Type: {context.Method.Type}. " +
                    $"Method: {context.Method.Name}.");

                AsyncUnaryCall<TResponse>? call = continuation(request, context);

                AsyncUnaryCall<TResponse>? ret = new AsyncUnaryCall<TResponse>(
                        HandleResponse(call.ResponseAsync),
                        call.ResponseHeadersAsync,
                        call.GetStatus,
                        call.GetTrailers,
                        call.Dispose);

                Console.WriteLine($"Client Completed call. Type: {MethodType.Unary}. " +
                    $"Method: {context.Method.Name}.");

                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error thrown by {context.Method} - {ex.Message}.");
                throw;
            }
        }

        private async Task<TResponse> HandleResponse<TResponse>(Task<TResponse> inner)
        {
            try
            {
                Stopwatch stopWatch = new Stopwatch();
                long elapsed = 0;
                stopWatch.Start();

                TResponse? ret = await inner;

                stopWatch.Stop();
                elapsed = stopWatch.ElapsedMilliseconds;

                Console.WriteLine($"Client Completed call. " +
                        $" --------------------------------->>> {DateTime.Now.ToLongTimeString()}  Took: {elapsed} milli-seconds.");


                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error thrown  - {ex.Message}.");
                throw new InvalidOperationException("Custom error", ex);
            }
        }
    }
}
