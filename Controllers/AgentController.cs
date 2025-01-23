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
using NuGet.Protocol;
using Microsoft.AspNetCore.Identity;
 using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using YourApiNamespace.Controllers;

namespace WakalaPlus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentController : Controller
    {
        private static string className = "AgentController";

        private static IConfiguration _config; 


        #region constructor
        public AgentController( IConfiguration config)
        {
            _config = config;
            //_tokenService = tokenService;

            // _loggedInUser = Functions.GetCurrentUserDetails(_context);
        }
        #endregion

        #region GetAllRequests
        [HttpGet]
        [Route("GetAllRequests")]
        public IActionResult GetAllRequests()
        {
            string functionName = "GetAllRequests";
            var executionResult = new ExecutionResult();
            var sysDate = Functions.GetCurrentDateTime();

            try
            {
                using (var db = new AppDbContext(_config))
                {
                    // Fetch all customer tickets
                    var tickets = db.CustomerTickets.ToList();

                    if (tickets == null || !tickets.Any())
                    {
                        executionResult.SetBadRequestError("No Tickets Found");
                        return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
                    }

                    // Return success response
                    executionResult.SetDataList(tickets);
                    executionResult.SetGeneralInfo(className, functionName, "SUCCESS");
                    return Ok(executionResult.GetServerResponse());
                }
            }
            catch (Exception ex)
            {
                // Handle and log the exception
                executionResult.SetInternalServerError(className, functionName, ex);
                return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
            }
        }
        #endregion

        #region GetAllGeneralTranslations
        [HttpGet]
        [Route("GetAllGeneralTranslations")]
        public IActionResult GetAllGeneralTranslations()
        {
            string functionName = "GetAllGeneralTranslations";
            var executionResult = new ExecutionResult();
            var sysDate = Functions.GetCurrentDateTime();

            try
            {
                using (var db = new AppDbContext(_config))
                {
                    // Fetch all translations
                    var translations = db.GeneralTranslations.ToList();

                    if (translations == null || !translations.Any())
                    {
                        executionResult.SetBadRequestError("No Translations Found");
                        return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
                    }

                    // Return success response
                    executionResult.SetDataList(translations);
                    executionResult.SetGeneralInfo(className, functionName, "SUCCESS");
                    return Ok(executionResult.GetServerResponse());
                }
            }
            catch (Exception ex)
            {
                // Handle and log the exception
                executionResult.SetInternalServerError(className, functionName, ex);
                return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
            }
        }
        #endregion

        #region GetOPenTicket  
        [AllowAnonymous]
        [HttpGet]
        [Route("GetOPenTicket/{agentCode}")]
        public IActionResult GetOnlineAgents(string agentCode)
        {
            string functionName = "GetOPenTicket";
            var executionResult = new ExecutionResult();

            try
            {
                using (var db = new AppDbContext(_config))
                {
 
                    executionResult = DoGetOPenTicket(db, agentCode);
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


        #region doRetrieve OpenTicket
        internal static ExecutionResult DoGetOPenTicket(AppDbContext db, string agentCode)
        {
            string functionName = "DoGetOPenTicket";
            var executionResult = new ExecutionResult();

            try
            {
                var OpenTicketList = db.CustomerTickets
                .Where(a => a.agentCode == agentCode && (a.ticketStatus == "OP" || a.ticketStatus == "OPEN"))
                .OrderByDescending(d => d.transactionId)
                .ToList();
                executionResult.SetDataList(OpenTicketList);
                return executionResult;
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return executionResult;
            }
        }

        #endregion
        
        #region checkAgentStatus
         [AllowAnonymous]
        [HttpGet]
        [Route("checkAgentStatus/{agentCode}")]
        public IActionResult checkAgentStatus(string agentCode)
        {
            string functionName = "checkAgentStatus";
            var executionResult = new ExecutionResult();

            try
            {
                using (var db = new AppDbContext(_config))
                {

                    executionResult = DocheckAgentStatus(db, agentCode);
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


         #region Do Check AgentSTATUS
        internal static ExecutionResult DocheckAgentStatus(AppDbContext db, string agentCode)
        {
            string functionName = "checkAgentStatus";
            var executionResult = new ExecutionResult();
            try { 
            string agentStatus = db.OnlineOfflineAgent.Where(a => a.agentCode == agentCode).Select(a => a.status).FirstOrDefault();
        
            executionResult.SetData(agentStatus);
            return executionResult;
        }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return executionResult;
            }
        }

        #endregion

        #endregion
        
        #region AgentRegistration
        [HttpPost]
        [Route("registeragent")]
        public IActionResult registeragent([FromBody] Agent data)
        {
            string functionName = "registeragent";
            var executionResult = new ExecutionResult();
            try
            {
                using (var db = new AppDbContext(_config))
                using (var trans = db.Database.BeginTransaction())
                {
                    executionResult = DoInsertAgent(db, data);
                    if (executionResult.GetSuccess() == false)
                    {
                        return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
                    }

                    // commit
                    trans.Commit();
                }

                // response
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

        #region DoAgentRegistration
        //internal static ExecutionResult DoInsertAgent(AppDbContext db, Agent data)
        //{
        //    string functionName = "DoInsert";
        //    var executionResult = new ExecutionResult();

        //    try
        //    {
        //        long count = 0;
        //        #region Check If there is any Pending Request for same  Agent
        //        count = db.Set<Agent>().Count(x => x.agentCode == data.agentCode);


        //        if (count > 0) // Pending Request for the same AGENTCode
        //        {
        //            var existingAgentRequest = db.Set<Agent>()
        //                                              .SingleOrDefault(x => x.agentCode == data.agentCode);

        //            if (existingAgentRequest != null)
        //            {

        //            }
        //        }
        //        #endregion


        //        Agent agentRegRequest = new Agent();
        //        OnlineOfflineAgent onlineOfflineAgent = new OnlineOfflineAgent();



        //        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(data.password);

        //        agentRegRequest.password = hashedPassword;


        //        agentRegRequest.agentCode = data.agentCode;

        //        agentRegRequest.agentPhone = data.agentPhone;
        //        agentRegRequest.agentFullName = data.agentFullName;
        //        agentRegRequest.address = data.address;
        //        agentRegRequest.longitude = data.longitude;
        //        agentRegRequest.latitude = data.latitude;
        //        agentRegRequest.networksOperating = data.networksOperating;
        //        agentRegRequest.nida = data.nida;
        //        agentRegRequest.serviceGroupCode = data.serviceGroupCode;
        //        agentRegRequest.regstrationStatus = "AP";

        //        var serviceName = db.GeneralTranslations.Where(x => x.code == agentRegRequest.serviceGroupCode)
        //                            .Select(x => x.value).FirstOrDefault();
        //        var networkName =  db.GeneralTranslations.Where(y=>y.code == agentRegRequest.networksOperating).Select(x => x.value).FirstOrDefault();

        //        agentRegRequest.serviceGroupCode = serviceName;
        //        agentRegRequest.networksOperating = networkName;



        //        //db.SaveChanges();

        //        onlineOfflineAgent.transactionId = Functions.GenerateTransId();
        //        onlineOfflineAgent.agentCode = agentRegRequest.agentCode;
        //        onlineOfflineAgent.agentFullName = agentRegRequest.agentFullName;
        //        onlineOfflineAgent.agentPhone = agentRegRequest.agentPhone;
        //        onlineOfflineAgent.address = agentRegRequest.address;
        //        onlineOfflineAgent.longitude = agentRegRequest.longitude;
        //        onlineOfflineAgent.latitude = agentRegRequest.latitude;
        //        onlineOfflineAgent.serviceGroupCode = agentRegRequest.serviceGroupCode;
        //        onlineOfflineAgent.networksOperating = agentRegRequest.networksOperating;
        //        onlineOfflineAgent.status = "ONLINE";

        //        db.Agents.Add(agentRegRequest);
        //        db.OnlineOfflineAgent.Add(onlineOfflineAgent); 


        //        db.SaveChanges();
        //        // set payload data
        //        executionResult.SetData(agentRegRequest);

        //        return executionResult;
        //    }
        //    catch (Exception ex)
        //    {
        //        executionResult.SetInternalServerError(className, functionName, ex);
        //        db.Agents.Reverse();
        //        return executionResult;
        //    }

        // }

        internal static ExecutionResult DoInsertAgent(AppDbContext db, Agent data)
        {
            string functionName = "DoInsert";
            var executionResult = new ExecutionResult();

            try
            {
                // Check if there is any pending request for the same agent
                long count = db.Set<Agent>().Count(x => x.agentCode == data.agentCode);

                if (count > 0) // Pending request for the same agent code
                {
                    var existingAgentRequest = db.Set<Agent>().SingleOrDefault(x => x.agentCode == data.agentCode);

                    if (existingAgentRequest != null)
                    {
                        // Handle pending request if needed
                    }
                }

                // Create a new Agent object and set its properties
                Agent agentRegRequest = new Agent
                {
                    password = BCrypt.Net.BCrypt.HashPassword(data.password),
                    agentCode = data.agentCode,
                    agentPhone = data.agentPhone,
                    agentFullName = data.agentFullName,
                    address = data.address,
                    longitude = data.longitude,
                    latitude = data.latitude,
                    networksOperating = data.networksOperating,
                    nida = data.nida,
                    serviceGroupCode = data.serviceGroupCode,
                    regstrationStatus = "AP"
                };

                // Retrieve service group name and network name
                //var serviceName = db.GeneralTranslations
                //                    .Where(x => x.code == agentRegRequest.serviceGroupCode)
                //                    .Select(x => x.value)
                //                    .FirstOrDefault();
                //var networkName = db.GeneralTranslations
                //                    .Where(y => y.code == agentRegRequest.networksOperating)
                //                    .Select(x => x.value)
                //                    .FirstOrDefault();

                //agentRegRequest.serviceGroupCode = serviceName;
                //agentRegRequest.networksOperating = networkName;

                // Add the agent to the Agents table
                db.Agents.Add(agentRegRequest);

                // Create a new OnlineOfflineAgent object and set its properties
                OnlineOfflineAgent onlineOfflineAgent = new OnlineOfflineAgent
                {
                    transactionId = Functions.GenerateTransId(),
                    agentCode = agentRegRequest.agentCode,
                    agentFullName = agentRegRequest.agentFullName,
                    agentPhone = agentRegRequest.agentPhone,
                    address = agentRegRequest.address,
                    longitude = agentRegRequest.longitude,
                    latitude = agentRegRequest.latitude,
                    serviceGroupCode = agentRegRequest.serviceGroupCode,
                    networksOperating = agentRegRequest.networksOperating,
                    status = "ONLINE"
                };

                // Add the onlineOfflineAgent to the OnlineOfflineAgent table
                db.OnlineOfflineAgent.Add(onlineOfflineAgent);

                // Save changes to the database
                db.SaveChanges();

                // Set payload data
                executionResult.SetData(agentRegRequest);

                return executionResult;
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return executionResult;
            }
        }

        #endregion

        #region login
        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] UserLoginModel userLoginModel)
        {
            string functionName = "Login";
            ExecutionResult executionResult = AuthenticateUser(_config, userLoginModel);
            IActionResult response = Unauthorized();

            try
            {
                if (executionResult.GetSuccess() == false)
                {
                    return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
                }
                SecUserAccount secUser = (SecUserAccount)executionResult.GetData();

                string token = GenerateJwtToken(secUser);


                //log login
                response = Ok(new
                {
                    token


                });

                return response;
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
            }
        }

        private static ExecutionResult AuthenticateUser(IConfiguration config, UserLoginModel userLoginModel)
        {
            string functionName = "AuthenticateUser";
            var executionResult = new ExecutionResult();

            try
            {
                //validate user details
                var sysDate = Functions.GetCurrentDateTime();
                Agent? secUser = null;

                string timeZoneName = "";

                using (var db = new AppDbContext(_config))
                {
                    userLoginModel.agentCode = userLoginModel.agentCode.ToLower();

                    // validate user credential
                    secUser = db.Agents.Where(a => a.agentCode.ToLower() == userLoginModel.agentCode).FirstOrDefault();
 
                    if (secUser != null)
                    {
                        #region verify password
                        bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(userLoginModel.password, secUser.password);

                        if (isPasswordCorrect)
                        {
                            var loggedInUser = new SecUserAccount()
                            {
                                agentCode = secUser.agentCode,
                                agentFullName = secUser.agentFullName,
                                networkGroupCode = secUser.networksOperating,
                                serviceGroupCode = secUser.serviceGroupCode,
                                regstrationStatus = secUser.regstrationStatus,
                                address = secUser.address,
                                longitude = secUser.longitude,
                                latitude = secUser.latitude,
                            };

                            if (loggedInUser.regstrationStatus == "AP")
                            {
                                //executionResult.SetData(loggedInUser);
                                executionResult.SetData(loggedInUser);
                                
                                return executionResult;
                            }
                            else
                            {
                                executionResult.SetBadRequestError("AGENT ACCOUNT IS NOT VERIFIED ");
                                return executionResult;
                            }
                        }
                        else
                        {
                            executionResult.SetBadRequestError("Neno La Siri Sio Sahihi");
                            return executionResult;
                        }
                    }

                    else
                    {

                        executionResult.SetBadRequestError("Kodi ya Wakala Sio Sahihi");
                        return executionResult;

                    }

                    #endregion
                }

            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return executionResult;
            }
        }
        #endregion

        #region generate JWt Token For Logged In Agent
        private string GenerateJwtToken(SecUserAccount user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("www.softmaxtz.com/S0ftM@x@frica-W@kalaPlux.Key");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim("agentCode", user.agentCode),
            new Claim("networkGroupCode", user.networkGroupCode),
            new Claim("address", user.address),
            new Claim("longitude",user.longitude.ToString()),
            new Claim("latitude",user.latitude.ToString()),
            new Claim("serviceGroupCode", user.serviceGroupCode),

                }),
                Expires = DateTime.UtcNow.AddHours(1), // Token expiration time
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        #endregion

        #region GoOnlineOfline

        [HttpPut]
        [Route("ChangeStatus")]
        public IActionResult ChangeStatus([FromBody] AgentStatusModel data)
        {
            string functionName = "ChangeStatus";
            var executionResult = new ExecutionResult();
            try
            {
                using (var db = new AppDbContext(_config))
                using (var trans = db.Database.BeginTransaction())
                {
                    executionResult = DoUpdateStatus(db, data);
                    if (executionResult.GetSuccess() == false)
                    {
                        return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
                    }
                    OnlineOfflineAgent statusData = (OnlineOfflineAgent)executionResult.GetData();
                    trans.Commit();
                    executionResult.SetData(db.OnlineOfflineAgent.Where(x => x.agentCode == statusData.agentCode).FirstOrDefault());
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

      
        internal static ExecutionResult DoUpdateStatus(AppDbContext db, AgentStatusModel data)
        {
            string functionName = "DoUpdateStatus";
            var executionResult = new ExecutionResult();

            try
            {
                var agentExists = db.OnlineOfflineAgent.Any(x => x.agentCode == data.agentCode);

                if (!agentExists)
                {
                    executionResult.SetBadRequestError("Agent Not Found. Please Complete Registration To Continue");
                    return executionResult;
                }

                OnlineOfflineAgent agentStatus = db.OnlineOfflineAgent.FirstOrDefault(x => x.agentCode == data.agentCode);
 
                var sysDate = Functions.GetCurrentDateTime();



                agentStatus.status = data.status;
                agentStatus.lastUpdate = sysDate;

                db.Attach(agentStatus);

                db.SaveChanges();

                executionResult.SetData(agentStatus);

                return executionResult;
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return executionResult;
            }
        }

        #endregion


        #region DeliveryReport

        [HttpPut]
        [Route("DeliveryReport")]
        public IActionResult DeliveryReport([FromBody] NotificationMessages messageData)
        {
            string functionName = "DeliveryReport";
            var executionResult = new ExecutionResult();
            try
            {
                using (var db = new AppDbContext(_config))
                using (var transaction = db.Database.BeginTransaction())
                {
                    executionResult = DoUpdateDeliveryReport(db, messageData);
                    if (!executionResult.GetSuccess())
                    {
                        return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
                    }

                    transaction.Commit();

                    executionResult.SetGeneralInfo(className, functionName, "SUCCESS");
                    return Ok(executionResult.GetServerResponse());
                }
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
            }
        }

        private ExecutionResult DoUpdateDeliveryReport(AppDbContext db, NotificationMessages messageData)
        {
            string functionName = "DoUpdateDeliveryReport";
            var executionResult = new ExecutionResult();
            try
            {
                var messagesToUpdate = db.NotificationMessages
                    .Where(x => x.status == "PENDING" && x.receiverIdentity == messageData.receiverIdentity)
                    .ToList();

                foreach (var msg in messagesToUpdate)
                {
                    msg.status = "DELIVERED";
                }

                db.SaveChanges();

                executionResult.SetData(messagesToUpdate);
                return executionResult;
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return executionResult;
            }
        }

        #endregion

        #region SyncLiveAgentLocation
        [HttpPut]
        [Route("SyncLiveAgentLocation")]
        public IActionResult SyncLiveAgentLocation([FromBody] AgentLocationSync currentLocation)
        {
            string functionName = "SyncLiveAgentLocation";
            var executionResult = new ExecutionResult();
            try
            {
                using (var db = new AppDbContext(_config))
                using (var transaction = db.Database.BeginTransaction())
                {
                    executionResult = DoSyncLocation(db, currentLocation);
                    if (!executionResult.GetSuccess())
                    {
                        return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
                    }

                    transaction.Commit();

                    executionResult.SetGeneralInfo(className, functionName, "SUCCESS");
                    return Ok(executionResult.GetServerResponse());
                }
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
            }
        }
        private ExecutionResult DoSyncLocation(AppDbContext db, AgentLocationSync currentLocation)
        {
            string functionName = "DoSyncLocation";
            var executionResult = new ExecutionResult();
            try
            {
                OnlineOfflineAgent SyncAgent = new OnlineOfflineAgent();
                var agentUpdating = db.OnlineOfflineAgent
                    .Where(x => x.agentCode == currentLocation.agentCode);

                foreach (var agent in agentUpdating)
                {
                    agent.latitude = currentLocation.latitude;
                    agent.longitude = currentLocation.longitude;
                    agent.lastUpdate = Functions.GetCurrentDateTime();
                    agent.address=agent.address;
                    agent.agentFullName = agent.agentFullName;
                    agent.agentPhone=agent.agentPhone;
                    agent.networksOperating=agent.networksOperating;
                    agent.serviceGroupCode=agent.serviceGroupCode;
                    agent.status = "ONLINE";
                    agent.transactionId=agent.transactionId;
                    agent.agentCode=agent.agentCode;
                }
                
                db.SaveChanges();

                executionResult.SetData(SyncAgent);
                return executionResult;
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return executionResult;
            }
        }
        #endregion

        #region UpdateAgentLocationAndSendBackToCustomers
        [HttpPost]
        [Route("UpdateAgentLocationAndSendBackToCustomers")]
        public async Task<OnlineOfflineAgent?> UpdateAgentLocationAndSendBackToCustomers(AgentLocationSync location)
        {
            try
            {
                using (var db = new AppDbContext(_config))
                {
                    // Retrieve the agent record based on the agentCode
                    var agent = await db.OnlineOfflineAgent
                    .FirstOrDefaultAsync(a => a.agentCode == location.agentCode);
                    if (agent != null)
                    {
                        // Update agent's location details
                        agent.latitude = location.latitude;
                        agent.longitude = location.longitude;
                        agent.status = location.status ?? "ONLINE";  // Default to "ONLINE"
                        agent.lastUpdate = DateTime.Now.ToLocalTime();
                        // Save changes to the database
                        await db.SaveChangesAsync();

                        // Return updated agent details
                        return agent;
                    }
                    else
                    {
                        Console.WriteLine("Agent not found.");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SyncAgentLocationAsync: {ex.Message}");
                return null;
            }
        }

        #endregion
        #region   UpdateTicketStatusToAttended Tickets
        [HttpPut]
        [Route("UpdateTicketStatusToAttended")]
        public IActionResult UpdateTicketStatusToAttended(PreparedCustomerTicket ticketData)
        {
            string functionName = "UpdateTicketStatusToAttended";
            var executionResult = new ExecutionResult();
            try
            {
                using (var db = new AppDbContext(_config))
                using (var trans = db.Database.BeginTransaction())
                {
                    executionResult = DoUpdateTicketStatusToAttended(db, ticketData);
                    if(executionResult.GetSuccess() == false)
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

        #region UpdateTicketStatusToAttended Tickets
        internal static ExecutionResult DoUpdateTicketStatusToAttended(AppDbContext db,PreparedCustomerTicket ticketData)
        {
            string functionName = "DoUpdateTicketStatusToAttended";
            var executionResult = new ExecutionResult();
            var sysDate = Functions.GetCurrentDateTime();
            try
            {

          

                var response = db.CustomerTickets.FirstOrDefault(x => x.agentCode == ticketData.agentCode && x.phoneNumber == ticketData.phoneNumber && x.serviceRequested==ticketData.serviceRequested && x.network ==ticketData.network && x.transactionId== ticketData.transactionId);
                if (response.Equals(null))
                {
                    executionResult.SetBadRequestError("Tiketi Hii Haijapatikana");
                    return executionResult;
                }
                else
                {
                    response.ticketLastResponseDateTime = ticketData.LastResponseDateTime;
                    response.ticketStatus = "ATTENDED";
                    response.agentCode = ticketData.agentCode;
                    response.agentLatitude = ticketData.agentLatitude;
                    response.agentLongitude=ticketData.agentLongitude;

                    db.Attach(response);

                    db.SaveChanges();
                    executionResult.SetData(response);
                    return executionResult;
                }
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return executionResult;
            }

        }
        #endregion

        #region CancelRequest
        [HttpPut]
        [Route("CancellRequest")]
        public IActionResult CancellRequest([FromBody] string ticketId)
        {
            string functionName = "CancellRequest";
            var executionResult = new ExecutionResult();
            var sysDate = Functions.GetCurrentDateTime();

            try
            {
                using (var db = new AppDbContext(_config))
                using (var trans = db.Database.BeginTransaction())
                {
                    // Fetch the ticket by transaction ID
                    var ticket = db.CustomerTickets.FirstOrDefault(x => x.transactionId == ticketId);
                    if (ticket == null)
                    {
                        executionResult.SetBadRequestError("No Ticket Found");
                        return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
                    }

                    // Update ticket status and last response date
                    ticket.ticketLastResponseDateTime = sysDate;
                    ticket.ticketStatus = "CANCELLED";

                    db.Attach(ticket);
                    db.SaveChanges();
                    trans.Commit();

                    // Return success response
                    executionResult.SetData(ticket);
                    executionResult.SetGeneralInfo(className, functionName, "SUCCESS");
                    return Ok(executionResult.GetServerResponse());
                }
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
            }
        }
        #endregion




        #region Retrieve Ticket History

        [AllowAnonymous]
       [HttpGet]
       [Route("GetTicketHistory/{agentCode}")]
        public IActionResult GetTicketHistory(string agentCode)
        {
            string functionName = "GetTicketHistory";
            var executionResult = new ExecutionResult();

            try
            {
                using (var db = new AppDbContext(_config))
                {

                    executionResult = DoGetTicketHistory(db, agentCode);
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

        #region doRetrieve Closed Ticket
        internal static ExecutionResult DoGetTicketHistory(AppDbContext db, string agentCode)
        {
            string functionName = "GetTicketHistory";
            var executionResult = new ExecutionResult();

            try
            {
                var TicketHistory = db.CustomerTickets
                    .Where(a => a.agentCode == agentCode &&
                                (a.ticketStatus == "AT" || a.ticketStatus == "ATTENDED" || a.ticketStatus == "ASSIGNED" || a.ticketStatus == "CANCELLED"))
                    .OrderByDescending(a => a.transactionId)
                    .ToList();
                
                executionResult.SetDataList(TicketHistory);
                return executionResult;
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return executionResult;
            }
        }

        #endregion


        #region AcceptRequest

        [HttpPut]
        [Route("AcceptTicketRequest")]
        public IActionResult AcceptTicketRequest([FromBody] PreparedCustomerTicket AcceptedRequest)
        {
            string functionName = "AcceptTicketRequest";
            var executionResult = new ExecutionResult();
            try
            {
                using (var db = new AppDbContext(_config))
                using (var transaction = db.Database.BeginTransaction())
                {
                    executionResult = DoUpdateAcceptedRequest(db, AcceptedRequest);
                    if (!executionResult.GetSuccess())
                    {
                        return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
                    }

                    transaction.Commit();

                    executionResult.SetGeneralInfo(className, functionName, "SUCCESS");
                    return Ok(executionResult.GetServerResponse());
                }
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return StatusCode(executionResult.GetStatusCode(), executionResult.GetServerResponse().Message);
            }
        }

        private ExecutionResult DoUpdateAcceptedRequest(AppDbContext db, PreparedCustomerTicket AcceptedRequest)
        {
            string functionName = "DoUpdateDeliveryReport";
            var executionResult = new ExecutionResult();
            try
            {
                var Req = db.CustomerTickets
                    .Where(x => x.ticketCreationDateTime == AcceptedRequest.createdDate)
                    .ToList();

                foreach (var NewAgent in Req)
                {
                    NewAgent.agentCode=AcceptedRequest.agentCode;
                    NewAgent.phoneNumber=AcceptedRequest.phoneNumber;
                    NewAgent.agentLatitude=AcceptedRequest.agentLatitude;
                    NewAgent.agentLongitude=AcceptedRequest.agentLongitude;
                    NewAgent.ticketStatus = "ASSIGNED";
                    NewAgent.ticketLastResponseDateTime=AcceptedRequest.LastResponseDateTime;
                }

                db.SaveChanges();

                executionResult.SetData(Req);
                return executionResult;
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(className, functionName, ex);
                return executionResult;
            }
        }

        #endregion
        #region Delete All Transactions

        [HttpDelete("DeleteAllTransactions")]
        public async Task<IActionResult> DeleteAllTransactions(AppDbContext db)
        {
            var executionResult = new ExecutionResult();
            try
            {
                // Get all the device connections from the database
                var tickets = db.CustomerTickets.ToList();

                if (tickets.Any())
                {
                    // Remove all the tickets
                    db.CustomerTickets.RemoveRange(tickets);
                    await db.SaveChangesAsync();

                    executionResult.SetGeneralInfo(nameof(AgentController), nameof(DeleteAllTransactions), "All Transactions deleted successfully");
                }
                else
                {
                    executionResult.SetGeneralInfo(nameof(AgentController), nameof(DeleteAllTransactions), "No Transaction found to delete");
                }

                return Ok(executionResult.GetServerResponse());
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(nameof(AgentController), nameof(DeleteAllTransactions), ex);
                return StatusCode((int)HttpStatusCode.InternalServerError, executionResult.GetServerResponse());
            }
        }

        #endregion


    }

}
