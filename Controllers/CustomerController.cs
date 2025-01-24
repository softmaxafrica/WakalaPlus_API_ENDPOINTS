using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using WakalaPlus.Models;
using WakalaPlus.Shared;
using static Org.BouncyCastle.Math.EC.ECCurve;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Data;
 
using Microsoft.AspNetCore.SignalR;

using WakalaPlus.Hubs;
using MySqlX.XDevAPI;
using Mysqlx;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Mysqlx.Expr;
using YourApiNamespace.Controllers;


namespace WakalaPlus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : Controller
    {

        private readonly IHubContext<SignalHub> _hubContext;
        private static string className = "CustomerController";
        private readonly AppDbContext _context =new AppDbContext(_config);
        private static IConfiguration _config;
        private static string _transactionId;

        #region constructor
        public CustomerController( IConfiguration config)
        {
            _config = config;
             //_hubContext = hubContext;
        }
        #endregion
      


        #region Retrieve All Agents

        [HttpGet]

        [Route("GetAllAgents")]
        public IActionResult GetAllAgents()
        {
            var executionResult = new ExecutionResult();

            try
            {
                var AllAgents = _context.Agents.ToList();
                executionResult.SetDataList(AllAgents);
                executionResult.SetTotalCount(AllAgents.Count);
                executionResult.SetGeneralInfo(nameof(CustomerController), nameof(GetAllAgents), "Data retrieved successfully");

                return Ok(executionResult.GetServerResponse());
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(nameof(CustomerController), nameof(GetAllAgents), ex);
                return StatusCode((int)HttpStatusCode.InternalServerError, executionResult.GetServerResponse());
            }
        }
 


        #endregion

        #region retrieveOnline Agents
        [AllowAnonymous]
        [HttpGet]
        [Route("GetOnlineAgents/{network}/{serviceRequested}")]
        public IActionResult GetOnlineAgents(string network, string serviceRequested)
        {
            string functionName = "GetOnlineAgents";
            var executionResult = new ExecutionResult();

            try
            {
                using (var db = new AppDbContext(_config))
                {
                    executionResult = DoGetOnlineAgents(db, network, serviceRequested);
                    if (executionResult.GetSuccess() == false)
                    {
                        return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
                    }
                }

                return Ok(executionResult.GetServerResponse());
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
            }
        }


        #endregion

        #region doRetrieve Online Agents
        internal static ExecutionResult DoGetOnlineAgents(AppDbContext db, string network, string serviceRequested)
        {
            string functionName = "GetOnlineAgents";
            var executionResult = new ExecutionResult();

            try
            {
                var agentsList = db.OnlineOfflineAgent
     .Where(a => a.status == "ONLINE" &&
                 a.serviceGroupCode.Contains(serviceRequested) &&
                 a.networksOperating.Contains(network))
     .ToList();
                executionResult.SetDataList(agentsList);
                return executionResult;
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return executionResult;
            }
        }

        #endregion


        #region CheckAgentsToSendTicket
        [HttpGet]

        [Route("CheckForAvailableAgents")]
        public ExecutionResult CheckForAvailableAgents(string serviceRequested, string network)
        {
            string functionName = "CheckForAvailableAgents";
            var executionResult = new ExecutionResult();

            try
            {
                // Fetch only online agents matching the requested service and network
                List<OnlineOfflineAgent> onlineAgents = _context.OnlineOfflineAgent
                    .Where(a => a.status == "ONLINE" &&
                                a.serviceGroupCode.Contains(serviceRequested) &&
                                a.networksOperating.Contains(network))
                    .ToList();

                // If no online agents are found, return a meaningful message
                if (onlineAgents == null || onlineAgents.Count == 0)
                {
                    executionResult.SetGeneralInfo(nameof(CustomerController), functionName,
                        "No online agents available for the requested service and network.");
                    return executionResult;
                }

                // Successfully found agents
                executionResult.SetSuccess($"Found {onlineAgents.Count} online agents available.");
                return executionResult;
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(nameof(CustomerController), functionName, ex);
                return executionResult;
            }
        }


        #endregion
        #region InsertCustomerTicket
        [HttpPost]
        [Route("CreateCustomerTicket")]
        public IActionResult CreateCustomerTicket(PreparedCustomerTicket ticket)
        {

            string functionName = "CreateCustomerTicket";
            var executionResult = new ExecutionResult();
            try
            {
                using (var db = new AppDbContext(_config))
                using (var trans = db.Database.BeginTransaction())
                {
                    executionResult = DoCreateCustomerTicket(db, ticket);
                    if (executionResult.GetSuccess() == false)
                    {
                        return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
                    }
                    trans.Commit();
                }


                executionResult.SetGeneralInfo(className, functionName, "SUCCESS");

                return Ok(executionResult.GetServerResponse());
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
            }
        }
        #endregion

        #region DoCreateCustomerTicket
        internal static ExecutionResult DoCreateCustomerTicket(AppDbContext db, PreparedCustomerTicket ticket)
        {
            string functionName = "DoCreateCustomerTicket";
            var executionResult = new ExecutionResult();
 
            try
            {
                var existingTicket = db.CustomerTickets.FirstOrDefault(t => t.transactionId == ticket.transactionId);
                if (existingTicket != null)
                {
                    // Delete the existing ticket
                    db.CustomerTickets.Remove(existingTicket);
                }
                CustomerTickets ticketData = new CustomerTickets();
                _transactionId = ticket.transactionId;
                ticketData.transactionId = _transactionId;
                ticketData.network = ticket.network;
                ticketData.customerLatitude = ticket.custLatitude;
                ticketData.customerLongitude = ticket.custLongitude;
                ticketData.serviceRequested = ticket.serviceRequested;
                ticketData.agentCode = ticket.agentCode;
                ticketData.agentLatitude = ticket.agentLatitude;
                ticketData.agentLongitude = ticket.agentLongitude;
                ticketData.ticketStatus = "ASSIGNED";
                ticketData.ticketCreationDateTime = (DateTime)ticket.createdDate;
                ticketData.ticketLastResponseDateTime = (DateTime)ticket.LastResponseDateTime;
                ticketData.phoneNumber = ticket.phoneNumber;
                ticketData.description = ticket.description;

                db.CustomerTickets.Add(ticketData);
                db.SaveChanges();

                executionResult.SetData(ticketData);

                return executionResult;
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);

                db.CustomerTickets.Reverse();
                return executionResult;
            }

        }
        #endregion

        #region PublishMessage
        [HttpGet]
        [Route("PushMessage/{message}")]
        public IActionResult PushMessage(string message)
        {
            //_hubContext.Clients.All.SendAsync("ReceiveMessage", message);
            return Ok("Done");
        }

 
        [HttpPost]
        [Route("CallForAgent")]
        public IActionResult CallForAgent([FromBody] NotificationMessages MessageData)
        {

         
            
            using (var db = new AppDbContext(_config))
            {
                try
                {
                    MessageData.status = "PENDING";
                    var Receiver = db.DeviceDetails.Where(x => x.Identity == MessageData.receiverIdentity).FirstOrDefault();
                    if (Receiver != null)
                    {
                        MessageData.receiverConnectionId= Receiver.connectionId;
                    }
                    else
                    {
                        return BadRequest("Reciever's Address Not Found. Please Contact Your Technical Team");
                    }

                    db.NotificationMessages.Add(MessageData);

                    _hubContext.Clients.All.SendAsync("AgentCallMessage", MessageData);
 
                    db.SaveChanges();
                    return Ok(MessageData);
                }
                catch
                {
                    return BadRequest("Internal Server Error");
                }
            }
        
          


            

        }

        #endregion

        [HttpGet("SendMessageToClient")]
        public IActionResult SendMessageToClient([FromQuery] string identifier)
        {
            using (var db = new AppDbContext(_config))
            {
                var device = db.DeviceDetails.FirstOrDefault(x => x.Identity == identifier);

                if (device != null)
                {
                    return Ok(device.connectionId);
                }
                else
                {
                    return NotFound("Agent not found");
                }
            }
        }

        #region Retrieve Ticket History
        [AllowAnonymous]
        [HttpGet]
        [Route("GetTransactionsHistory/{customerId}")]
        public IActionResult GetTransactionsHistory(string customerId)
        {
            string functionName = "GetTransactionsHistory";
            var executionResult = new ExecutionResult();

            try
            {
                using (var db = new AppDbContext(_config))
                {

                    executionResult = DoGetTransactionsHistory(db, customerId);
                    if (executionResult.GetSuccess() == false)
                    {
                        return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
                    }
                }

                return Ok(executionResult.GetServerResponse());
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
            }
        }
        #endregion

        #region doRetrieve TransactionsHistory
        internal static ExecutionResult DoGetTransactionsHistory(AppDbContext db, string customerId)
        {
            string functionName = "GetTransactionsHistory";
            var executionResult = new ExecutionResult();

            try
            {
                var TransactionsHistory = db.CustomerTickets
           .Where(a => a.phoneNumber == customerId)
           .OrderByDescending(a => a.transactionId)
           .ToList();

                executionResult.SetDataList(TransactionsHistory);
                return executionResult;
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return executionResult;
            }
        }

        #endregion

        #region registerCustomer
        [HttpPost]
        [Route("RegisterCustomer")]
        public IActionResult RegisterCustomer([FromBody] Customer data)
        {
            string functionName = "RegisterCustomer";
            var executionResult = new ExecutionResult();
            try
            {
                using (var db = new AppDbContext(_config))
                using (var trans = db.Database.BeginTransaction())
                {

                    executionResult = DoInsertCustomer(db, data);
                    if (!executionResult.GetSuccess())
                    {
                        return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
                    }

                    trans.Commit();
                }

                // response
                executionResult.SetGeneralInfo(className, functionName, "SUCCESS");
                return Ok(executionResult.GetServerResponse());
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred: {ex}");
                executionResult.SetInternalServerError(className, functionName, ex);
                return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
            }
        }

        internal static ExecutionResult DoInsertCustomer(AppDbContext db, Customer data)
        {
            string functionName = "DoInsertCustomer";
            var executionResult = new ExecutionResult();

            try
            {
                Customer CustomerRegRequest = new Customer();

                CustomerRegRequest.customerId = data.customerId;
                CustomerRegRequest.FullName= data.FullName;
                CustomerRegRequest.RegDate = System.DateTime.Now.ToLocalTime();
                CustomerRegRequest.Nida= data.Nida;
                CustomerRegRequest.latitude = data.latitude;
                CustomerRegRequest.longitude = data.longitude;



                db.Customers.Add(CustomerRegRequest);
                db.SaveChanges();

                // set payload data
                executionResult.SetData(CustomerRegRequest);
                return executionResult;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred: {ex}");
                executionResult.SetInternalServerError(className, functionName, ex);
                return executionResult;
            }
        }
        #endregion

        #region CustomerLogin
        [HttpPost("Login")]
        public IActionResult Login([FromBody] Customer customerLogged)
        {
            string functionName = "Login";
            ExecutionResult executionResult = AuthenticateUser(_config, customerLogged);

            try
            {
                if (!executionResult.GetSuccess())
                {
                    // If the authentication failed, return appropriate status code
                    return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
                }

                // If authentication is successful, return a success response
                return Ok(executionResult.GetServerResponse());
            }
            catch (Exception ex)
            {
                // Log the exception and return internal server error
                executionResult.SetInternalServerError(className, functionName, ex);
                return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
            }
        }

        private static ExecutionResult AuthenticateUser(IConfiguration config, Customer customerLogged)
        {
            string functionName = "AuthenticateUser";
            var executionResult = new ExecutionResult();

            try
            {
                using (var db = new AppDbContext(config)) // Use the passed configuration parameter instead of the static _config
                {
                    // Validate user credentials
                    var cust = db.Customers.FirstOrDefault(a => a.customerId.ToLower() == customerLogged.customerId.ToLower());

                    if (cust != null)
                    {
                        executionResult.SetData(cust);
                    }
                    else
                    {
                        // If customer not found, set a bad request error
                        executionResult.SetBadRequestError("Customer not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception and set internal server error
                executionResult.SetInternalServerError(className, functionName, ex);
            }

            return executionResult;
        }

        #endregion

        #region TrackAgentLocation
         [AllowAnonymous]
        [HttpGet]
        [Route("TrackAgentLocation")]
        public IActionResult TrackAgentLocation()
        {
            string functionName = "TrackAgentLocation";
            var executionResult = new ExecutionResult();

            try
            {
                using (var db = new AppDbContext(_config))
                {

                    executionResult = DoTrackAgentLocation(db);
                    if (executionResult.GetSuccess() == false)
                    {
                        return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
                    }
                }

                return Ok(executionResult.GetServerResponse());
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
            }
        }


        #endregion
        #region Do TrackAgentLocation
        internal static ExecutionResult DoTrackAgentLocation(AppDbContext db)
        {
            string functionName = "TrackAgentLocation";
            var executionResult = new ExecutionResult();
            try
            {
                var agentsList = db.OnlineOfflineAgent
     .Where(a => a.status == "ONLINE")
     .ToList();
                executionResult.SetDataList(agentsList);
                return executionResult;
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return executionResult;
            }
        }

        #endregion


        #region Retrieve AssignedTicket
        [AllowAnonymous]
        [HttpGet("GetAssignedTicket/{transID}/{customerId}")]
        public IActionResult GetAssignedTicket(string transID, string customerId)
        {
            string functionName = "GetAssignedTicket";
            var executionResult = new ExecutionResult();

            try
            {
                using (var db = new AppDbContext(_config))
                {
                    executionResult = DoGetAssignedTicket(db, transID, customerId);
                    if (executionResult.GetSuccess())
                    {
                        return Ok(executionResult.GetServerResponse());
                    }
                    else
                    {
                        return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception details
                executionResult.SetInternalServerError(className, functionName, ex);
                return StatusCode(500, "An internal server error occurred.");
            }
        }
        #endregion

        #region doRetrieve AssignedTicket
        internal static ExecutionResult DoGetAssignedTicket(AppDbContext db, string transID, string customerId)
        {
            string functionName = "DoGetAssignedTicket";
            var executionResult = new ExecutionResult();
            try
            {
                var assignedTicket = db.CustomerTickets
                    .Where(a => a.phoneNumber == customerId &&
                                a.ticketStatus == "ASSIGNED" &&
                                a.transactionId == transID)
                    .ToList();
                executionResult.SetDataList(assignedTicket);
                return executionResult;
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return executionResult;
            }
        }
        #endregion

        #region Delete All Customers

        [HttpDelete("DeleteAllCustomers")]
        public async Task<IActionResult> DeleteAllCustomers()
        {
            var executionResult = new ExecutionResult();
            try
            {
                // Get all the device connections from the database
                var customers = _context.Customers.ToList();

                if (customers.Any())
                {
                    // Remove all the devices
                    _context.Customers.RemoveRange(customers);
                    await _context.SaveChangesAsync();

                    executionResult.SetGeneralInfo(nameof(CustomerController), nameof(DeleteAllCustomers), "All Customers deleted successfully");
                }
                else
                {
                    executionResult.SetGeneralInfo(nameof(CustomerController), nameof(DeleteAllCustomers), "No device connections found to delete");
                }

                return Ok(executionResult.GetServerResponse());
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(nameof(CustomerController), nameof(DeleteAllCustomers), ex);
                return StatusCode((int)HttpStatusCode.InternalServerError, executionResult.GetServerResponse());
            }
        }

        #endregion
    }
}
