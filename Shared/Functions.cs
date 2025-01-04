using GSF.Units;
using Microsoft.VisualBasic;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using WakalaPlus.Models;

namespace WakalaPlus.Shared
{
    public class Functions
    {
        private static readonly Random _random = new Random();
        private static readonly object _syncLock = new object();
        private static string _className = "Functions";


        internal static OnlineOfflineAgent GetAgentLiveLocation()
        {
            throw new NotImplementedException();
        }

        #region GetCurrentDateTime
        public static DateTime GetCurrentDateTime()
        {
            return DateTime.Now.ToLocalTime().AddHours(10);
        }
        #endregion
        #region GenerateTransId
        internal static string GenerateTransId()
        {
            return DateTime.Now.ToLocalTime().Ticks.ToString() + GenerateRandomNumber();
        }
        #endregion


        #region GenerateRandomNumber
        private static int GenerateRandomNumber()
        {
            int lowerBound = 1000000;
            int upperBound = 9999999;

            lock (_syncLock)
            {
                return _random.Next(lowerBound, upperBound);
            }
        }
        #endregion


        #region GetCurrentUserDetails
        public static SecUserAccount GetCurrentUserDetails(HttpContext context)
        {
            SecUserAccount loggedInUser = new SecUserAccount();
            loggedInUser.agentCode = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            loggedInUser.agentCode = context.User.FindFirst("agentCode")?.Value;
            loggedInUser.networkGroupCode = context.User.FindFirst("networkGroupCode")?.Value;
            loggedInUser.address = context.User.FindFirst("address")?.Value;
            loggedInUser.serviceGroupCode = context.User.FindFirst("serviceGroupCode")?.Value;

            // Convert latitude and longitude back to double
            double latitude;
            double longitude;

            if (double.TryParse(context.User.FindFirst("latitude")?.Value, out latitude))
            {
                loggedInUser.latitude = latitude;
            }

            if (double.TryParse(context.User.FindFirst("longitude")?.Value, out longitude))
            {
                loggedInUser.longitude = longitude;
            }

            return loggedInUser;
        }

        #endregion


    }
}
