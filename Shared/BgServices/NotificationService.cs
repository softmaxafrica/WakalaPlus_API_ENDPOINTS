using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlX.XDevAPI;
using WakalaPlus.Hubs;
using WakalaPlus.Models;
using WakalaPlus.Shared;
using static Org.BouncyCastle.Math.EC.ECCurve;
using WakalaPlus.Hubs;

namespace WakalaPlus.Shared.BgServices
{
    public class NotificationService : BackgroundService
    {
        private readonly IServiceProvider _services;
        NotificationMessages MessageObj;
        private static IConfiguration _config;
        private readonly IHubContext<SignalHub> _hubContext;
 
        public NotificationService(IHubContext<SignalHub> hubContext,IServiceProvider services, IConfiguration config)
        {
            _services = services;
            MessageObj = new NotificationMessages();
            _config = config;
            _hubContext = hubContext;  
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var db = new AppDbContext(_config))
                {
                    // Retrieve pending messages from the database
                    var pendingMessages = db.NotificationMessages
                        .Where(x => x.status == "PENDING").ToList(); 

                    foreach (var messageDt in pendingMessages)
                    {
                        MessageObj.senderIdentity = messageDt.senderIdentity;
                        MessageObj.receiverIdentity = messageDt.receiverIdentity;
                        MessageObj.message = messageDt.message;

                        var deviceDetails = db.DeviceDetails
                            .FirstOrDefault(x => x.Identity == messageDt.receiverIdentity);

                        if (deviceDetails != null)
                        {
                            MessageObj.connectionId = deviceDetails.connectionId;
                              await CallForAgent(MessageObj);
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
            }
        }

        async  Task CallForAgent(NotificationMessages MessageObj)
            {
                  await _hubContext.Clients.All.SendAsync("AgentCallMessage", MessageObj);
            //  await _hubContext.Clients.All.SendAsync("AgentCallMessage", MessageObj);
            // await Clients.Client(connectionId).SendAsync("CallForAgent", incommingAgentCallmessage);
            //await _hubContext.Clients.Client(MessageObj.receiverConnectionId).SendAsync("AgentCallMessage", MessageObj);

        }

    }
}
