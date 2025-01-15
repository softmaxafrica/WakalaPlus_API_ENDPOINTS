using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MySqlX.XDevAPI;
using NuGet.Protocol.Plugins;
using System;
using System.Security.Principal;
using System.Threading.Tasks;
using WakalaPlus.Controllers;
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
        public AgentController _agentController = new AgentController(config);
        public CustomerController _customerController = new CustomerController(config);



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
            try
            {
                if (string.IsNullOrEmpty(ticket.agentCode))
                {
                    // If no specific agent to exclude, send to all other connected clients
                    await Clients.Others.SendAsync("ReceiveTicket", ticket);
                }
                else
                {
                    // Get the connection ID of the agent to exclude
                    string agentConnectionId = await GetConnectionIdByPhoneNumber(ticket.agentCode);

                    if (!string.IsNullOrEmpty(agentConnectionId))
                    {
                        // Exclude the specific agent's connection ID
                        await Clients.AllExcept(agentConnectionId).SendAsync("ReceiveTicket", ticket);
                    }
                    else
                    {
                        // Handle the case where the connection ID for the agent was not found
                        await Clients.Others.SendAsync("ReceiveTicket", ticket);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error in SendTicketToAgents: {ex.Message}");
            }
        }


        public async Task SendCancellRequest(PreparedCustomerTicket ticket, string senderType)
        {
            try
            {
                await Clients.All.SendAsync("ReceiveCancellRequest", ticket, senderType);
                string connectionId = senderType == "agent"
                    ? await GetConnectionIdByPhoneNumber(ticket.phoneNumber)
                    : await GetConnectionIdByPhoneNumber(ticket.agentCode);

                if (!string.IsNullOrEmpty(connectionId))
                {
                    await Clients.Client(connectionId).SendAsync("ReceiveCancellRequest", ticket, senderType);
                }
                else
                {
                    Console.WriteLine($"Connection ID not found for {senderType}: {ticket.phoneNumber ?? ticket.agentCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Sending Cancellation Request: {ex.Message}");
            }
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

                        _customerController.CreateCustomerTicket(ticket);
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


        private async Task<string> GetConnectionIdByPhoneNumber(string phoneNumber)
        {
            try
            {
                var deviceDetails = await _dbContext.DeviceDetails
                    .FirstOrDefaultAsync(d => d.deviceId == phoneNumber);

                if (deviceDetails != null)
                {
                    return deviceDetails.connectionId;
                }
                else
                {
                    Console.WriteLine("Device details not found for the given phone number.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving connection ID: {ex.Message}");
            }

            return null;
        }

        public async Task SyncLocationData(AgentLocationSync agentLocation)
        {
            try
            {
                      // Call the service to update the agent location
                var updatedAgent = await _agentController.UpdateAgentLocationAndSendBackToCustomers(agentLocation);
                // Check if the update was successful
                if (updatedAgent != null)
                {
                    // Notify all clients about the updated agent
                    await Clients.All.SendAsync("ReceiveLocationUpdate", new
                    {
                        agentCode = updatedAgent.agentCode,
                        latitude = updatedAgent.latitude,
                        longitude = updatedAgent.longitude,
                        status = updatedAgent.status
                    });
                }
                else
                {
                    Console.WriteLine("Failed to update agent or agent not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating agent location: {ex.Message}");
            }
        }


    }
}

