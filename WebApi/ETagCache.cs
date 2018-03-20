using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace WebApi
{
    public class ETagCache
    {
        private readonly IDistributedCache cache;
        private readonly HttpContext httpContext;
        public ETagCache(IDistributedCache cache, IHttpContextAccessor httpContextAccessor)
        {
            this.cache = cache;
            httpContext = httpContextAccessor.HttpContext;
        }

        public T GetCachedObject<T>(string cacheKeyPrefix)
        {
            string requestETag = GetRequestedETag();

            if (!string.IsNullOrEmpty(requestETag))
            {
                // Construct the key for the cache 
                string cacheKey = $"{cacheKeyPrefix}-{requestETag}";

                // Get the cached item
                string cachedObjectJson = cache.GetString(cacheKey);

                // If there was a cached item then deserialise this 
                if (!string.IsNullOrEmpty(cachedObjectJson))
                {
                    T cachedObject = JsonConvert.DeserializeObject<T>(cachedObjectJson);
                    return cachedObject;
                }
            }

            return default;
        }

        public bool SetCachedObject(string cacheKeyPrefix, dynamic objectToCache)
        {
            if (!IsCacheable(objectToCache))
            {
                return true;
            }

            string requestETag = GetRequestedETag();
            string responseETag = Convert.ToBase64String(objectToCache.RowVersion);

            // Add the contact to the cache for 30 mins if not already in the cache
            if (responseETag != null)
            {
                string cacheKey = $"{cacheKeyPrefix}-{responseETag}";
                string serializedObjectToCache = JsonConvert.SerializeObject(objectToCache);
                cache.SetString(cacheKey, serializedObjectToCache, new DistributedCacheEntryOptions { AbsoluteExpiration = DateTime.Now.AddMinutes(30) });
            }

            // Add the current ETag to the HTTP header
            httpContext.Response.Headers.Add("ETag", responseETag);

            bool isModified = !(httpContext.Request.Headers.ContainsKey("If-None-Match") && responseETag == requestETag);
            return isModified;
        }

        private string GetRequestedETag()
        {
            if (httpContext.Request.Headers.ContainsKey("If-None-Match"))
            {
                return httpContext.Request.Headers["If-None-Match"].First();
            }
            return string.Empty;
        }

        private bool IsCacheable(dynamic objectToCache)
        {
            var type = objectToCache.GetType();
            return type.GetProperty("RowVersion") != null;
        }
    }
}