using Microsoft.AspNetCore.SignalR;
using Notes2022.Server.Data;
using Notes2022.Server.Proto;
using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;

namespace Notes2022.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly NotesDbContext _db;

        public ChatHub(NotesDbContext db)
        {
            _db = db;
        }
        public async Task SendMessage(string user, string id, string message)
        {
            TalkLog talkLog = new()
            {
                FromId = id,
                ToId = String.Empty,
                FromName = user,
                ToName = "--GeneralChat--",
                Message = message,
                MessageTime = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            _db.TalkLog.Add(talkLog);
            await _db.SaveChangesAsync();

            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
