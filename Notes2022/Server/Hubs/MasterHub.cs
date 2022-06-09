using Microsoft.AspNetCore.SignalR;
using Notes2022.Server.Data;
using Notes2022.Server.Proto;
using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;

namespace Notes2022.Server.Hubs
{
    public class MasterHub : Hub
    {
        public static Dictionary<string, ActiveUsers> UserDict;
        private readonly NotesDbContext _db;

        public MasterHub(NotesDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Opens the session.  Keeps track of user list at user login/relogin
        /// and periodic heart beat
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
                ActiveUsers me = new() { DisplayName = userName, Subject = userId,
                    CheckinTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(DateTime.UtcNow), ClientId = clientId };
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
        /// User clean up.  Deletes track for users who hae not had a heart beat in a while
        /// </summary>
        private async Task UserCleanUp()
        {
           Timestamp then = Timestamp.FromDateTimeOffset(DateTime.UtcNow.AddMinutes(-1).AddSeconds(-20));
            if (Globals.UserDict)
            {
                List<KeyValuePair<string, ActiveUsers>>? aul = UserDict.ToList();
                foreach(var au in aul)
                {
                    if (au.Value.CheckinTime <= then)
                        UserDict.Remove(au.Key);
                }
                return;
            }
            List<ActiveUsers> inactiveUsers = _db.ActiveUsers.Where(p => p.CheckinTime <= then ).ToList();
            if (inactiveUsers != null && inactiveUsers.Count > 0)
            {
                _db.ActiveUsers.RemoveRange(inactiveUsers);
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Sends an update to active users.  Count and list.
        /// </summary>
        private async Task SendUpdate()
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

            await Clients.All.SendAsync("ReceiveActiveUsers", count, activeUsers);
        }

        /// <summary>
        /// Talk request.
        /// </summary>
        /// <param name="ToclientId">The toclient identifier.</param>
        /// <param name="FromclientId">The fromclient identifier.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="toName">To name.</param>
        public async Task TalkRequest(string ToclientId, string FromclientId, string userName, string toName)
        {
            if (Context.ConnectionId == ToclientId)
            {
                await Clients.Single(Context.ConnectionId).SendAsync("TalkRejected", "You can not talk to yourself!");
                return;
            }
            // send the request to the target
            await Clients.Single(ToclientId).SendAsync("TalkRequest", ToclientId, FromclientId, userName, toName);
        }

        /// <summary>
        /// Talk was the rejected.  Tell caller.
        /// </summary>
        /// <param name="FromclientId">The fromclient identifier.</param>
        /// <param name="userName">Name of the user.</param>
        public async Task TalkRejected(string FromclientId, string userName)
        {
            await Clients.Single(FromclientId).SendAsync("TalkRejected", $"{userName} does not want to talk now.");
        }

        /// <summary>
        /// Talk accepted.  Create a group.  Tell both parties.
        /// </summary>
        /// <param name="ToclientId">The toclient identifier.</param>
        /// <param name="FromclientId">The fromclient identifier.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="toName">To name.</param>
        public async Task TalkAccepted(string ToclientId, string FromclientId, string userName, string toName)
        {
            string groupid = ToclientId + ":" + FromclientId;
            await Groups.AddToGroupAsync(ToclientId, groupid);
            await Groups.AddToGroupAsync(FromclientId, groupid);

            await Clients.Groups(groupid).SendAsync("TalkAccepted", ToclientId, FromclientId, userName, toName);
            Thread.Sleep(500);
            await Clients.Groups(groupid).SendAsync("PrivateMessage", "Notes 2022", "You are connected!");
        }

        /// <summary>
        /// Send Private message to sender and receiver
        /// </summary>
        /// <param name="ToclientId">The toclient identifier.</param>
        /// <param name="FromclientId">The fromclient identifier.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="message">The message.</param>
        public async Task PrivateMessage(string ToclientId, string FromclientId, string userName, string message)
        {
            ActiveUsers to = UserDict[ToclientId];
            ActiveUsers from;
            if (userName == to.DisplayName)
            {
                from = to;
                to = UserDict[FromclientId];
            }
            else
                from = UserDict[FromclientId];

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

            string groupid = ToclientId + ":" + FromclientId;
            await Clients.Groups(groupid).SendAsync("PrivateMessage", userName, message);
        }

        /// <summary>
        /// Ends the talk.  Tell both parties and remove both from group.
        /// </summary>
        /// <param name="ToclientId">The toclient identifier.</param>
        /// <param name="FromclientId">The fromclient identifier.</param>
        public async Task EndTalk(string ToclientId, string FromclientId)
        {
            string groupid = ToclientId + ":" + FromclientId;

            await Clients.Groups(groupid).SendAsync("EndTalk");

            await Groups.RemoveFromGroupAsync(ToclientId, groupid);
            await Groups.RemoveFromGroupAsync(FromclientId, groupid);
        }
    }
}
