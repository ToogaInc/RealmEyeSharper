// MIT License
// 
// Copyright (c) 2019 Matteo Contrini
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
// Taken from: https://github.com/matteocontrini/PlainHttp

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Threading;
using IHttpClientFactory = PlainHttp.IHttpClientFactory;

namespace RealmAspNet.RealmEye.Proxy
{
	/// <summary>
	///     Factory that creates and caches HttpClient.
	///     Supports both proxied and non-proxied clients.
	///     A similar concept is better explained here by @matteocontrini:
	///     https://stackoverflow.com/a/52708837/1633924
	/// </summary>
	public class ProxyHttpClientFactory : IHttpClientFactory
    {
	    /// <summary>
	    ///     Cache for the clients
	    /// </summary>
	    private readonly ConcurrentDictionary<string, HttpClient> _clients = new();


	    /// <summary>
	    ///     Gets a cached client for the host associated to the input URL
	    /// </summary>
	    /// <param name="uri"><see cref="Uri" /> used as the cache key</param>
	    /// <returns>A cached <see cref="HttpClient" /> instance for the host</returns>
	    public HttpClient GetClient(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            return PerHostClientFromCache(uri);
        }

	    /// <summary>
	    ///     Gets a random cached client with a proxy attached to it
	    /// </summary>
	    /// <param name="proxyUri"><see cref="Uri" /> of the proxy, used as the cache key</param>
	    /// <returns>A cached <see cref="HttpClient" /> instance with a random proxy. Returns null if no proxies are available</returns>
	    public HttpClient GetProxiedClient(Uri proxyUri)
        {
            return ProxiedClientFromCache(proxyUri);
        }

	    /// <summary>
	    ///     Deletes the proxied client.
	    /// </summary>
	    /// <param name="proxyUri">The proxy URI.</param>
	    /// <returns>True if the client was deleted + disposed, false otherwise.</returns>
	    public void DeleteProxiedClient(Uri proxyUri)
        {
            if (!_clients.TryRemove(proxyUri.ToString(), out var client))
                return;

            client.Dispose();
        }


        private HttpClient PerHostClientFromCache(Uri uri)
        {
            return _clients.AddOrUpdate(
                uri.Host,
                _ => CreateClient(),
                (_, client) => client
            );
        }


        private HttpClient ProxiedClientFromCache(Uri proxyUri)
        {
            return _clients.AddOrUpdate(
                proxyUri.ToString(),
                _ => CreateProxiedClient(proxyUri),
                (_, client) => client
            );
        }

        protected virtual HttpClient CreateProxiedClient(Uri proxyUrl)
        {
            var proxy = new WebProxy(proxyUrl);

            if (!string.IsNullOrEmpty(proxyUrl.UserInfo))
            {
                var parts = proxyUrl.UserInfo.Split(':', 2);
                proxy.Credentials = new NetworkCredential(parts[0], parts[1]);
            }

            var handler = new SocketsHttpHandler
            {
                Proxy = proxy,
                UseProxy = true,
                PooledConnectionLifetime = TimeSpan.FromMinutes(10),
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var client = new HttpClient(handler)
            {
                Timeout = Timeout.InfiniteTimeSpan
            };

            return client;
        }

        protected virtual HttpClient CreateClient()
        {
            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(30),
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(1)
            };

            return client;
        }
    }
}