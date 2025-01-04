using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI;
using NuGet.Protocol.Plugins;
using System;
using System.Security.Principal;
using System.Threading.Tasks;
using WakalaPlus.Models;
using WakalaPlus.Shared;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace WakalaPlus.Hubs
{
    public class SignalHub : Hub
    {
        public static readonly Dictionary<string, string> DeviceConnections = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _connectionIdMap = new Dictionary<string, string>();
        
        public string _connectionId;
        private static IConfiguration config;
        private readonly AppDbContext _dbContext;
         
        private static string className = "SignalHub";

        public SignalHub(IConfiguration _config, AppDbContext dbContext)
        {
            config = _config;
            _dbContext = dbContext;
        }
       
        
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext == null) return;

            // Extract the device details from the query string or headers
            var deviceId = httpContext.Request.Query["deviceId"];
            var identity = httpContext.Request.Query["identity"]; // e.g., "agent" or "customer"
            var connectionId = Context.ConnectionId;

            if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(identity))
            {
                await Clients.Caller.SendAsync("ErrorMessage", "Device ID and Identity are required.");
                return;
            }

            // Prepare device connection details
            var deviceDetail = new DeviceDetails
            {
                deviceId = deviceId,
                Identity = identity,
                connectionId = connectionId,
                LastAction = "Insert",
                createdDate = DateTime.UtcNow
            };

            using (var context = new AppDbContext(config))
            {
                // Check if the device already exists
                var existingDevice = context.DeviceDetails.FirstOrDefault(d => d.deviceId == deviceId);

                if (existingDevice != null)
                {
                    // Update existing device details
                    existingDevice.connectionId = connectionId;
                    existingDevice.LastAction = "Update";
                    existingDevice.createdDate = DateTime.UtcNow;
                }
                else
                {
                    // Insert new device details
                    context.DeviceDetails.Add(deviceDetail);
                }

                await context.SaveChangesAsync();
            }

            // Notify all clients (optional)
            await Clients.All.SendAsync("ReceiveMessage", $"Device {deviceId} connected with identity {identity}.");
        }


        public async Task SendMessage(string message)
        {
            // Broadcasting the message to all connected clients
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        
        public async Task SendTicketToAgents(PreparedCustomerTicket ticket)
        {
            Console.WriteLine("Sending ticket to agents..."); // Add logging for debugging
            await Clients.All.SendAsync("ReceiveTicket", ticket);
        }


        public async Task AgentsendUpdatedTicketToRequestor(PreparedCustomerTicket ticket)
        {
            try
            {
                // Retrieve the connectionId of the customer using phoneNumber from the ticket
                var deviceDetails = await _dbContext.DeviceDetails
                    .FirstOrDefaultAsync(d => d.deviceId == ticket.phoneNumber);

                if (deviceDetails != null)
                {
                    string connectionId = deviceDetails.connectionId;

                    // Check if connectionId is valid
                    if (!string.IsNullOrEmpty(connectionId))
                    {
                        // Send the updated ticket to the customer using the connectionId
                        await Clients.Client(connectionId).SendAsync("ReceiveUpdatedTicket", ticket);
                        Console.WriteLine("Updated ticket sent to the customer successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Customer connection ID not found.");
                    }
                }
                else
                {
                    Console.WriteLine("Device details not found for the given phone number.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending updated ticket to customer: {ex.Message}");
            }
        }
    }

    // Override OnConnectedAsync to notify all clients about a new connection
    //public override async Task OnConnectedAsync()
    //{
    //    await Clients.All.SendAsync("ReceiveMessage", "Another connection has been added");
    //}
}

