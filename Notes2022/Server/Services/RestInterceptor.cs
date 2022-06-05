using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Notes2022.Server.Services
{
    public class RestInterceptor : Interceptor
    {

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            //if (!context.Method.StartsWith("/notes2022server"))
            //{

            //}

            return await continuation(request, context);  //  Notes2022Service.BindAuth(context));
        }

    }
}
