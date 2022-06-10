using Microsoft.AspNetCore.SignalR;
using Notes2022.Server.Data;
using Notes2022.Server.Proto;
using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;
using UserDictEntry = System.Collections.Generic.KeyValuePair<string, Notes2022.Server.Proto.ActiveUsers>;
using UserDictList = System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, Notes2022.Server.Proto.ActiveUsers>>;

namespace Notes2022.Server.Hubs
{
    /// <summary>
    /// This hub handles userlist/count and talk
    /// </summary>
    public class MasterHub : Hub
    {
        /// <summary>
        /// holds list of users if so configured
        /// </summary>
        public static Dictionary<string, ActiveUsers> UserDict { get; private set; }
        /// <summary>
        /// database for logging and holding user list if not configured for UserDict
        /// </summary>
        private readonly NotesDbContext _db;

        public MasterHub(NotesDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Opens the session.  Keeps track of user list at user login/relogin and periodic heart beat.  Calls: SendUpdate()
        /// 
        /// Sends
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="userName">Name of the user.</param>
        public async Task OpenSession(string userId, string userName)
        {
            if (UserDict == null)
                UserDict = new Dictionary<string, ActiveUsers>();
            string clientId = Context.ConnectionId;
            await UserCleanUp();
            if (Globals.UserDict)
            {
                ActiveUsers? au;
                try { au = UserDict[clientId]; } catch { au = null; }
                if (au != null)
                {
                    au.CheckinTime = Timestamp.FromDateTimeOffset(DateTime.UtcNow);
                    UserDict[clientId] = au;
                }
                else
                {
                    ActiveUsers me = new()
                    {
                        DisplayName = userName,
                        Subject = userId,
                        CheckinTime = Timestamp.FromDateTimeOffset(DateTime.UtcNow),
                        ClientId = clientId
                    };
                    UserDict.Add(clientId, me);
                }
                await SendUpdate();
                return;
            }

            ActiveUsers? activeUsers = _db.ActiveUsers.SingleOrDefault(p => p.Subject == userId && p.ClientId == clientId);
            if (activeUsers is not null)
            {   // update the time to prevent cleanup.
                activeUsers.CheckinTime = Timestamp.FromDateTimeOffset(DateTime.UtcNow);
                _db.Update(activeUsers);
            }
            else
            {   // add entry
                ActiveUsers me = new()
                {
                    DisplayName = userName,
                    Subject = userId,
                    CheckinTime = Timestamp.FromDateTimeOffset(DateTime.UtcNow),
                    ClientId = clientId
                };
                _db.ActiveUsers.Add(me);
            }
            await _db.SaveChangesAsync();
            await SendUpdate();
        }

        /// <summary>
        /// Closes the session.  Called at logout/disconnect
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="userName">Name of the user.</param>
        public async Task CloseSession()
        {
            if (UserDict == null)
                UserDict = new Dictionary<string, ActiveUsers>();
            string clientId = Context.ConnectionId;
            await UserCleanUp();

            if (Globals.UserDict)
            {
                ActiveUsers? au;
                try { au = UserDict[clientId]; } catch { au = null; }
                if (au != null)
                    UserDict.Remove(clientId);
                await SendUpdate();
                return;
            }
            ActiveUsers? activeUsers = _db.ActiveUsers.SingleOrDefault(p => p.ClientId == clientId);
            if (activeUsers is not null)
            {
                _db.ActiveUsers.Remove(activeUsers);
                await _db.SaveChangesAsync();
            }
            await SendUpdate();
        }

        /// <summary>
        /// User clean up/de-ghosting.  Deletes tracking for users who have not had a heart beat in a while
        /// </summary>
        public async Task UserCleanUp()
        {
            Timestamp then = Timestamp.FromDateTimeOffset(DateTime.UtcNow.AddMinutes(-1).AddSeconds(-20));
            if (Globals.UserDict)
            {
                UserDictList? aul = UserDict.ToList();
                foreach (UserDictEntry au in aul)
                {
                    if (au.Value.CheckinTime <= then)
                    {
                        LoginLog log = new()
                        {
                            TheTime = Timestamp.FromDateTime(DateTime.UtcNow),
                            UserId = au.Value.Subject,
                            UserName = au.Value.DisplayName,
                            EventName = "DeGhost - Not active"
                        };

                        _db.LoginLog.Add(log);
                        await _db.SaveChangesAsync();

                        UserDict.Remove(au.Key);
                    }
                }
                return;
            }
            List<ActiveUsers> inactiveUsers = _db.ActiveUsers.Where(p => p.CheckinTime <= then).ToList();
            if (inactiveUsers != null && inactiveUsers.Count > 0)
            {
                _db.ActiveUsers.RemoveRange(inactiveUsers);
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Sends an update to active users: Count and list: await Clients.All.SendAsync("ReceiveActiveUsers", count, activeUsers);
        /// </summary>
        public async Task SendUpdate()
        {
            if (UserDict == null)
                UserDict = new Dictionary<string, ActiveUsers>();

            if (Globals.UserDict)
            {
                List<ActiveUsers> aul = UserDict.Values.ToList();
                await Clients.All.SendAsync("ReceiveActiveUsers", aul.Count, aul);
                return;
            }

            List<ActiveUsers> activeUsers = _db.ActiveUsers.ToList();
            int count = 0;
            if (activeUsers != null && activeUsers.Count > 0)
                count = activeUsers.Count;
            // targets should update their local values.
            await Clients.All.SendAsync("ReceiveActiveUsers", count, activeUsers);
        }

        /// <summary>
        /// Talk request. Logged. Sends: await Clients.Single(ToclientId).SendAsync("TalkRequest", ToclientId, FromclientId, userName, toName);
        /// </summary>
        /// <param name="ToclientId">The toclient identifier.</param>
        /// <param name="FromclientId">The fromclient identifier.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="toName">To name.</param>
        public async Task TalkRequest(string ToclientId, string FromclientId, string userName, string toName)
        {
            await LogMessage(ToclientId, FromclientId, userName, ">>>>> Talk Requested <<<<<");

            if (Context.ConnectionId == ToclientId)
            {
                await Clients.Single(Context.ConnectionId).SendAsync("TalkRejected", "You can not talk to yourself!");
                return;
            }
            // send the request to the target.  Target should ask user if they want to talk...
            await Clients.Single(ToclientId).SendAsync("TalkRequest", ToclientId, FromclientId, userName, toName);
        }

        /// <summary>
        /// Talk was the rejected. Logged.  Sends: await Clients.Single(FromclientId).SendAsync("TalkRejected", $"{userName} does not want to talk now.");
        /// </summary>
        /// <param name="FromclientId">The fromclient identifier.</param>
        /// <param name="userName">Name of the user.</param>
        public async Task TalkRejected(string ToclientId, string FromclientId, string userName)
        {
            await LogMessage(ToclientId, FromclientId, userName, ">>>>> Talk Rejected <<<<<");
            // Target should inform user of message.
            await Clients.Single(FromclientId).SendAsync("TalkRejected", $"{userName} does not want to talk now.");
        }

        /// <summary>
        /// Talk accepted.  Create a group. Sends: await Clients.Groups(groupid).SendAsync("TalkAccepted", ToclientId, FromclientId, userName, toName);
        /// </summary>
        /// <param name="ToclientId">The toclient identifier.</param>
        /// <param name="FromclientId">The fromclient identifier.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="toName">To name.</param>
        public async Task TalkAccepted(string ToclientId, string FromclientId, string userName, string toName)
        {
            await LogMessage(ToclientId, FromclientId, userName, ">>>>> Talk Started <<<<<");

            string groupid = ToclientId + ":" + FromclientId;
            await Groups.AddToGroupAsync(ToclientId, groupid);
            await Groups.AddToGroupAsync(FromclientId, groupid);
            // clients should open talk dialog/window/page
            await Clients.Groups(groupid).SendAsync("TalkAccepted", ToclientId, FromclientId, userName, toName);
            await Task.Delay(500);
            await Clients.Groups(groupid).SendAsync("PrivateMessage", "Notes 2022", "You are connected!");
        }

        /// <summary>
        /// Send Private message to sender and receiver. Sends: await Clients.Groups(groupid).SendAsync("PrivateMessage", userName, message);
        /// </summary>
        /// <param name="ToclientId">The toclient identifier.</param>
        /// <param name="FromclientId">The fromclient identifier.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="message">The message.</param>
        public async Task PrivateMessage(string ToclientId, string FromclientId, string userName, string message)
        {
            await LogMessage(ToclientId, FromclientId, userName, message);

            string groupid = ToclientId + ":" + FromclientId;
            // targets should add to local message list.
            await Clients.Groups(groupid).SendAsync("PrivateMessage", userName, message);
        }

        /// <summary>
        /// Ends the talk.  Tell both parties and remove both from group.  Sends: await Clients.Groups(groupid).SendAsync("EndTalk");
        /// </summary>
        /// <param name="ToclientId">The toclient identifier.</param>
        /// <param name="FromclientId">The fromclient identifier.</param>
        public async Task EndTalk(string ToclientId, string FromclientId, string userName)
        {
            await LogMessage(ToclientId, FromclientId, userName, ">>>>> Talk Ended <<<<<");

            string groupid = ToclientId + ":" + FromclientId;
            // targets should close talk dialog/window/page.
            await Clients.Groups(groupid).SendAsync("EndTalk");

            await Groups.RemoveFromGroupAsync(ToclientId, groupid);
            await Groups.RemoveFromGroupAsync(FromclientId, groupid);
        }

        private async Task LogMessage(string ToclientId, string FromclientId, string userName, string message)
        {
            // Get sender and receiver for logging
            ActiveUsers to = UserDict[ToclientId];
            ActiveUsers from;
            if (userName == to.DisplayName)
            {
                from = to;
                to = UserDict[FromclientId];
            }
            else
                from = UserDict[FromclientId];
            // log message
            TalkLog talkLog = new()
            {
                FromId = from.Subject,
                ToId = to.Subject,
                FromName = from.DisplayName,
                ToName = to.DisplayName,
                Message = message,
                MessageTime = Timestamp.FromDateTime(DateTime.UtcNow)
            };
            _db.TalkLog.Add(talkLog);
            await _db.SaveChangesAsync();
        }
    }
}
