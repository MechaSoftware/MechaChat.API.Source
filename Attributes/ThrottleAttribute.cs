using Microsoft.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace MechaChat.API.Attributes
{
    public class ThrottleAttribute : ActionFilterAttribute
    {
        private int _API_RATEQUOTA = 20;

        // per x minute value
        private int _API_TIMELIMIT = 1;

        private int _API_BLOCKDURATION = 15;

        private readonly object syncLock = new object();

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // Extract access_token or id or ip address to uniquely identify an API call
            var access_token = actionContext.Request.GetOwinContext().Request.RemoteIpAddress;

            if (access_token != null)
            {

                string throttleBaseKey = GetThrottleBaseKey(access_token);
                string throttleCounterKey = GetThrottleCounterKey(access_token);

                lock (syncLock)
                {
                    //add a listner for new api request count
                    if (HttpRuntime.Cache[throttleBaseKey] == null)
                    {
                        // add api unique key.. this cache will get expire after _API_TIMELIMIT
                        HttpRuntime.Cache.Add(throttleBaseKey, DateTime.UtcNow, null, DateTime.Now.AddMinutes(_API_TIMELIMIT), Cache.NoSlidingExpiration, CacheItemPriority.High, null);

                        // add count as value for that api.. this cache will get expire after _API_TIMELIMIT
                        HttpRuntime.Cache.Add(throttleCounterKey, 1, null, DateTime.Now.AddMinutes(_API_TIMELIMIT), Cache.NoSlidingExpiration, CacheItemPriority.High, null);
                    }
                    else
                    {
                        //listener exists for api request count
                        var current_requests = (int)HttpRuntime.Cache[throttleCounterKey];

                        if (current_requests < _API_RATEQUOTA)
                        {
                            // increase api count
                            HttpRuntime.Cache.Insert(throttleCounterKey, current_requests + 1, null, DateTime.Now.AddMinutes(_API_TIMELIMIT), Cache.NoSlidingExpiration, CacheItemPriority.High, null);
                        }

                        //hit rate limit, wait for another 5 minutes (_API_BLOCKDURATION)
                        else
                        {
                            HttpRuntime.Cache.Insert(throttleBaseKey, DateTime.UtcNow, null, DateTime.Now.AddMinutes(_API_BLOCKDURATION), Cache.NoSlidingExpiration, CacheItemPriority.High, null);

                            HttpRuntime.Cache.Insert(throttleCounterKey, current_requests + 1, null, DateTime.Now.AddMinutes(_API_BLOCKDURATION), Cache.NoSlidingExpiration, CacheItemPriority.High, null);

                            Forbidden(actionContext);
                        }
                    }
                }
            }
            else
            {
                BadRequest(actionContext);
            }

            base.OnActionExecuting(actionContext);
        }

        private string GetThrottleBaseKey(string app_id)
        {
            return Identifier.THROTTLE_BASE_IDENTIFIER + app_id;
        }

        private string GetThrottleCounterKey(string app_id)
        {
            return Identifier.THROTTLE_COUNTER_IDENTIFIER + app_id;
        }

        private void BadRequest(HttpActionContext actionContext)
        {
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        private void Forbidden(HttpActionContext actionContext)
        {
            var access_token = actionContext.Request.GetOwinContext().Request.RemoteIpAddress;

            string throttleBaseKey = GetThrottleBaseKey(access_token);
            string throttleCounterKey = GetThrottleCounterKey(access_token);

            DateTime utcExpiresValue = GetCacheUtcExpiryDateTime(throttleBaseKey);

            var counter = HttpRuntime.Cache.Get(throttleCounterKey);

            var message = new
            {
                error = "Rate Limit",
                message = "You are being rate limited!",
                limit = $"Maximum of 50 requests per minute. You created {counter} requests.",
                retry_after = $"You can retry after {utcExpiresValue.Minute} minutes and {utcExpiresValue.Second} seconds."
            };

            actionContext.Response = actionContext.Request.CreateResponse((HttpStatusCode)429, message);
        }

        private DateTime GetCacheUtcExpiryDateTime(string cacheKey)
        {
            var aspnetcachestoreprovider = System.Web.HttpRuntime.Cache.GetType().GetProperty("InternalCache", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(System.Web.HttpRuntime.Cache, null);
            var intenralcachestore = aspnetcachestoreprovider.GetType().GetField("_cacheInternal", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(aspnetcachestoreprovider);
            Type TEnumCacheGetOptions = System.Web.HttpRuntime.Cache.GetType().Assembly.GetTypes().Where(d => d.Name == "CacheGetOptions").FirstOrDefault();
            object cacheEntry = intenralcachestore.GetType().GetMethod("DoGet", BindingFlags.NonPublic | BindingFlags.Instance, null, CallingConventions.Any, new[] { typeof(bool), typeof(string), TEnumCacheGetOptions }, null).Invoke(intenralcachestore, new Object[] { true, cacheKey, 1 }); ;
            PropertyInfo utcExpiresProperty = cacheEntry.GetType().GetProperty("UtcExpires", BindingFlags.NonPublic | BindingFlags.Instance);
            DateTime utcExpiresValue = (DateTime)utcExpiresProperty.GetValue(cacheEntry, null);

            return utcExpiresValue;
        }
    }

    public static class Identifier
    {
        public static readonly string THROTTLE_BASE_IDENTIFIER = "LA_THROTTLE_BASE_";
        public static readonly string THROTTLE_COUNTER_IDENTIFIER = "LA_THROTTLE_COUNT_";
    }
}
