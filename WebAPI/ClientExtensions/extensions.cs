using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xrm.Tools.WebAPI.Requests;
using Xrm.Tools.WebAPI.Results;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Dynamic;


namespace Xrm.Tools.WebAPI
{



    public partial class CRMWebAPI
    {
       
        public static CRMWebAPI CreateOnline(string clientId, string userName, string password, string authorityUrl, string apiUrl)
        {
            var ret = new CRMWebAPI(apiUrl, "", default(Guid),
                    delegate (string au)
                    {
                        return Task.Run(() =>
                        {
                            return GetAccessToken(clientId, userName, password, authorityUrl, apiUrl);
                        });
                    }
                );
            return ret;
        }        

        public static CRMWebAPI CreateOnpremise(string userName, string password, string apiUrl)
        {
            NetworkCredential cred = new NetworkCredential(userName, password);
            return new CRMWebAPI(apiUrl, cred);
        }

        public static string GetAccessToken(string clientId, string userName, string password, string authorityUrl, string apiUrl)
        {
            string token = "";
            Task.Run(async () =>
            {
                AuthenticationResult _authResult;
                AuthenticationContext authContext = new AuthenticationContext(authorityUrl, false);
                //Prompt for credentials 
                //_authResult = authContext.AcquireToken( 
                //    _serviceUrl, _clientId, new Uri(_redirectUrl)); 

                //No prompt for credentials 
                var credentials = new UserPasswordCredential(userName, password);
                var u = new Uri(apiUrl);
                var orgUrl = u.Scheme + "://" + u.Host;
                _authResult = await authContext.AcquireTokenAsync(orgUrl, clientId, credentials);

                token = _authResult.AccessToken;
                //return token;
            }).Wait();
           return token;
        }

        /*
        public static IDictionary<string, Object> CreateCrmObject()
        {
            return new ExpandoObject() as IDictionary<string, Object>;
        }
        */

        /// <summary>
        /// get current user
        /// </summary>
        /// <returns></returns>
        public async Task<ExpandoObject> WhoAmI()
        {
            return await ExecuteFunction("WhoAmI", null);
        }

        /// <summary>
        /// get multiple records matching the URI
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="QueryOptions"></param>
        /// <returns></returns>
        public async Task<ExpandoObject> GetMultiple(string entityCollection, CRMGetListOptions QueryOptions = null)
        {
            return await Get<ExpandoObject>(entityCollection, Guid.Empty, QueryOptions);
        }
        /// <summary>
        /// get a multiple records with the specified return type
        /// </summary>
        /// <typeparam name="ResultType"></typeparam>
        /// <param name="uri"></param>
        /// <param name="QueryOptions"></param>
        /// <returns></returns>
        public async Task<ResultType> GetMultiple<ResultType>(string entityCollection, CRMGetListOptions QueryOptions = null)
        {
            return await Get<ResultType>(entityCollection, Guid.Empty, QueryOptions);
        }

        /// <summary>
        /// get a single record matching the URI
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="QueryOptions"></param>
        /// <returns></returns>
        public async Task<ExpandoObject> GetByKey(string entityCollection, string entityKeyValue, CRMGetListOptions QueryOptions = null)
        {
            return await GetByKey<ExpandoObject>(entityCollection, entityKeyValue, QueryOptions);
        }
        /// <summary>
        /// get a single record with the specified return type
        /// </summary>
        /// <typeparam name="ResultType"></typeparam>
        /// <param name="uri"></param>
        /// <param name="QueryOptions"></param>
        /// <returns></returns>
        public async Task<ResultType> GetByKey<ResultType>(string entityCollection, string entityKeyValue, CRMGetListOptions QueryOptions = null)
        {
            await CheckAuthToken();
            string fullUrl = string.Empty;
            fullUrl = BuildGetUrl(entityCollection + "(" + entityKeyValue + ")", QueryOptions);
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), fullUrl);

            if ((QueryOptions != null) && (QueryOptions.FormattedValues))
                request.Headers.Add("Prefer", "odata.include-annotations=\"OData.Community.Display.V1.FormattedValue\"");

            var results = await _httpClient.SendAsync(request);

            results.EnsureSuccessStatusCode();
            var data = await results.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ResultType>(data);
        }

        public async Task<List<Option>> GetGlobalOptionset(string optionsetName)
        {
            // Label.UserLocalizedLabel.Label
            List<Option> ret = new List<Option>();
            await CheckAuthToken();
            string fullUrl = string.Empty;
            fullUrl = BuildGetUrl("GlobalOptionSetDefinitions(Name='"+ optionsetName + "')", null);
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), fullUrl);
            var results = await _httpClient.SendAsync(request);

            results.EnsureSuccessStatusCode();
            var data = await results.Content.ReadAsStringAsync();
            dynamic json = JsonConvert.DeserializeObject(data);
            foreach (var option in json.Options)
            {
                ret.Add(new Option { Value = (int)option.Value, Label = (string)option.Label.UserLocalizedLabel.Label });
            }
            return ret;
        }

        public async Task<List<Option>> GetOptionset(string entityName, string optionsetName)
        {
            List<Option> ret = new List<Option>();
            await CheckAuthToken();
            string fullUrl = string.Empty;

            fullUrl = BuildGetUrl("EntityDefinitions(LogicalName='"+entityName+"')/Attributes(LogicalName='"+optionsetName+"')/Microsoft.Dynamics.CRM.PicklistAttributeMetadata?$select=LogicalName&$expand=OptionSet($select=Options),GlobalOptionSet($select=Options)", null);
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), fullUrl);
            var results = await _httpClient.SendAsync(request);
            results.EnsureSuccessStatusCode();
            var data = await results.Content.ReadAsStringAsync();
            dynamic json = JsonConvert.DeserializeObject(data);
            if (json.OptionSet != null)
            {
                foreach (var option in json.OptionSet.Options)
                    ret.Add(new Option { Value = (int)option.Value, Label = (string)option.Label.UserLocalizedLabel.Label });
            }
            else if (json.GlobalOptionSet != null)
            {
                foreach (var option in json.GlobalOptionSet.Options)
                    ret.Add(new Option { Value = (int)option.Value, Label = (string)option.Label.UserLocalizedLabel.Label });
            }
            return ret;
        }

        public async Task<ExpandoObject> AddListmember(Guid listid, Guid entityid)
        {
            //return await ExecuteAction("lists(" + listid + ")/Microsoft.Dynamics.CRM.AddMemberList", "{\"EntityId\":\"" + entityid + "\"}");
            string data = "{\"EntityId\":\"" + entityid.ToString() + "\"}";
            return await ExecuteAction("Microsoft.Dynamics.CRM.AddMemberList", "lists", listid, data);
        }

        public async Task<ExpandoObject> AddListmembers(Guid listid, string entityType, Guid[] entityIds)
        {
            var list = new ExpandoObject() as IDictionary<string, Object>;
            list.Add("@odata.type", "Microsoft.Dynamics.CRM.list");
            list.Add("listid", listid);

            var members = new List<IDictionary<string, Object>>();
            entityIds.ToList().ForEach(delegate (Guid g)
            {
                var member = new ExpandoObject() as IDictionary<string, Object>;
                member.Add("@odata.type", "Microsoft.Dynamics.CRM." + entityType);
                member.Add(entityType + "id", g);
                members.Add(member);
            });

            var o = new ExpandoObject() as IDictionary<string, Object>;
            o.Add("List", list);
            o.Add("Members", members);
            //string data = JsonConvert.SerializeObject(o);

            return await ExecuteAction("AddListMembersList", o);
        }



        // niet meer nodig vw expandoobject as IDictionary
        public static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }

    }

    public class Option
    {
        public int Value { get; set; }
        public string Label { get; set; }
    }

}
