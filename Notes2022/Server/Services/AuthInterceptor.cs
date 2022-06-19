using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Identity;
using Notes2022.Server.Proto;
using Notes2022.Server.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Notes2022.Shared;

namespace Notes2022.Server.Services
{
    /// <summary>
    /// Class AuthInterceptor - Needed to replace [Authorize] for Json Transcoding
    /// Implements the <see cref="Interceptor" />
    /// </summary>
    /// <seealso cref="Interceptor" />
    public class AuthInterceptor : Interceptor
    {
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthInterceptor"/> class.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        public AuthInterceptor(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Server-side handler for intercepting an incoming unary call.
        /// This one does Authorization and as a side effect sets the ApplicationUser object
        /// </summary>
        /// <typeparam name="TRequest">Request message type for this method.</typeparam>
        /// <typeparam name="TResponse">Response message type for this method.</typeparam>
        /// <param name="request">The request value of the incoming invocation.</param>
        /// <param name="context">An instance of <see cref="T:Grpc.Core.ServerCallContext" /> representing
        /// the context of the invocation.</param>
        /// <param name="continuation">A delegate that asynchronously proceeds with the invocation, calling
        /// the next interceptor in the chain, or the service request handler,
        /// in case of the last interceptor and return the response value of
        /// the RPC. The interceptor can choose to call it zero or more times
        /// at its discretion.</param>
        /// <returns>A future representing the response value of the RPC. The interceptor
        /// can simply return the return value from the continuation intact,
        /// or an arbitrary response value as it sees fit.</returns>
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            string[] strings = context.Method.Split('/');
            string method = strings[^1];

            if (NeedsAuth(method))             // check method name to see if it needs authorized
            {
                context = await BindAuth(context, method);    // do the authorization
            }
            return await continuation(request, context);
        }

        /// <summary>
        /// Sets the User var and may add the authorization
        /// Used instead of [Authorize] attribute for Json transcoding
        /// </summary>
        /// <param name="context">The call context.</param>
        /// <returns>ServerCallContext with (added) authorization</returns>
        /// <exception cref="Notes2022.Server.Proto.AuthReply.Status">Call not authorized!</exception>
        private async Task<ServerCallContext> BindAuth(ServerCallContext context, string method)
        {   // get the headers
            ApplicationUser? user = new();
            Dictionary<string, string>? dict = context.RequestHeaders.ToDictionary(x => x.Key, x => x.Value);
            try
            {   // check for direct gRPC
                user = await _userManager.FindByIdAsync(new JwtSecurityTokenHandler().ReadJwtToken(dict["authorization"][7..]).Subject);
            }
            catch (Exception)                           // no auth header (gRPC) - try cookie (Json Transcoding)
            {
                string[] cookies = dict["cookie"].Split(',');
                foreach (string cookie in cookies)
                {
                    if (cookie.StartsWith(Globals.CookieName))
                    {
                        try
                        {
                            LoginReply? reply = JsonSerializer.Deserialize<LoginReply>(Globals.Base64Decode(cookie[(Globals.CookieName.Length + 1)..]));
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                            context.RequestHeaders.Add("authorization", $"Bearer {reply.Jwt}");
                            user = await _userManager.FindByIdAsync(reply.Info.Subject);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                            break;
                        }
                        catch (Exception)
                        {
                            throw new RpcException(new Status(StatusCode.Unauthenticated, "Call not authorized!"));
                        }
                    }
                }
            }
            if (user is null || string.IsNullOrEmpty(user.Email))
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Call not authorized!"));
            await CheckRoles(method, user);
            context.UserState.Add("User", user);    // this will be used by the gRPC service method
            return context;
        }

        /// <summary>
        /// Checks if method needs authorization.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if auth needed, <c>false</c> otherwise.</returns>
        private static bool NeedsAuth(string method)
        {
            return method switch      // check for methods requiring No Auththorization
            {
                "Register" or "Login" or "ConfirmEmail" or "ResendEmail" or "ResetPassword" or "ResetPassword2" or "NoOp" or "GetAbout" or "GetTextFile" or "GetHomePageMessage" => false,
                _ => true,
            };
        }

        /// <summary>
        /// Checks the roles.  In this case we have only one role with extra access: Admin
        /// </summary>
        /// <param name="context">The ServerCallContext.</param>
        /// <exception cref="Notes2022.Server.Proto.AuthReply.Status">Call not authorized!</exception>
        private async Task CheckRoles(string method, ApplicationUser user)
        {
            switch (method)      // check for methods requiring Admin access
            {
                case "GetUserList":         // list of methods to require Admin access for
                case "GetUserRoles":        // add others below...
                case "UpdateUserRoles":
                case "GetAdminPageModel":
                case "CreateNoteFile":
                case "UpdateNoteFile":
                case "DeleteNoteFile":

                    if (! await _userManager.IsInRoleAsync(user, UserRoles.Admin))
                        throw new RpcException(new Status(StatusCode.Unauthenticated, "Call not authorized!"));
                    break;

                // could add other Roles as needed
                default:
                    break;
            }
        }
    }
}
