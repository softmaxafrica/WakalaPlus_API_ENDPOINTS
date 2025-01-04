﻿using Microsoft.AspNetCore.Http;
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
using NuGet.Protocol;
using Microsoft.AspNetCore.Identity;
 using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;
using MySqlX.XDevAPI.Common;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.AspNetCore.SignalR;

using WakalaPlus.Hubs;
using MySqlX.XDevAPI;
using Mysqlx;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Mysqlx.Expr;


namespace WakalaPlus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : Controller
    {

        private readonly IHubContext<SignalHub> _hubContext;
        private static string className = "CustomerController";

        private static IConfiguration _config;
        private static string _transactionId;

        #region constructor
        public CustomerController(IHubContext<SignalHub> hubContext, IConfiguration config)
        {
            _config = config;
            _hubContext = hubContext;
        }
        #endregion
        #region GetAllAgents
        [HttpGet]

        [Route("GetAllAgents")]
        public ActionResult<List<Agent>> GetAllAgents()
        {
            using (var db = new AppDbContext(_config))

                try
                {
                    var Agents = db.Agents.ToList();
                    if (Agents.Any())
                    {
                        return Ok(Agents); //Http 200 Ok
                    }
                    else
                    {
                        return NotFound("No Agent Found"); //HttpClient 404 NOT FOUND
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Internal Server Error");
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
        #region InsertCustomerTicket
        [HttpPost]
        [Route("CreateCustomerTicket")]
        public IActionResult CreateCustomerTicket([FromBody] PreparedCustomerTicket ticket)
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
                ticketData.ticketStatus = "OPEN";
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
            _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
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


    }
}
