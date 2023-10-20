﻿using Newtonsoft.Json;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using RobloxNET.Exceptions;

namespace RobloxNET.Utilities.Groups
{
    public static class MemberManagement
    {
        private static string roblosecurity = null;
        public static async Task SetUserGroupRole(RobloxSession session, int groupId, int userId, int role)
        {
            roblosecurity = session.GetRoblosecurity();

            if (roblosecurity == null)
            {
                throw new Exceptions.InvalidLoginException("The current RobloxSession is not logged into any account! Did you forget to run LoginAsync()?");
            }

            HttpClient client = CreateHttpClient();

            //Get roleID from the inserted role number
            var rolesResponse = await client.GetAsync("/v1/groups/" + groupId + "/roles");
            var rolesString = await rolesResponse.Content.ReadAsStringAsync();

            string replace = "\"groupId\":" + Convert.ToString(groupId) + ",";
            rolesString = rolesString.Replace(replace, "");
            replace = "{\"roles\":";
            rolesString = rolesString.Replace(replace, "");
            rolesString = rolesString.Remove(rolesString.Length - 1, 1);
            var roleList = JsonConvert.DeserializeObject<List<RoleList>>(rolesString);

            bool roleFound = false;
            int roleId = 0;
            foreach (RoleList currentRole in roleList)
            {
                if (currentRole.rank == role)
                {
                    roleFound = true;
                    roleId = currentRole.id;
                    break;
                }
            }
            if (!roleFound)
            {
                throw new Exceptions.InvalidRoleException("The inserted role number could not be found! Ensure you enter a valid role number that's between 0-255!");
            }

            string patchContentJSON = "{ \"roleId\": " + roleId + " }"; 
            var payload = new StringContent(patchContentJSON, Encoding.UTF8, "application/json");
            var firstResponse = await client.PatchAsync("/v1/groups/" + groupId + "/users/" + userId, null);
            client.DefaultRequestHeaders.Add("x-csrf-token", firstResponse.Headers.GetValues("x-csrf-token").First());
            var secondResponse = await client.PatchAsync("/v1/groups/" + groupId + "/users/" + userId, payload);

            string response1String = await secondResponse.Content.ReadAsStringAsync();
        }
        private static HttpClient CreateHttpClient()
        {
            var baseAddress = new Uri("https://groups.roblox.com");
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler();
            handler.CookieContainer = cookieContainer;
            cookieContainer.Add(baseAddress, new Cookie(".ROBLOSECURITY", roblosecurity));
            HttpClient client = new HttpClient(handler);
            client.BaseAddress = baseAddress;

            return client;
        }
    }
}