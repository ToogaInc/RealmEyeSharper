using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using IHttpClientFactory = PlainHttp.IHttpClientFactory;

namespace RealmAspNet.RealmEye.Proxy
{
	public class HttpClientFactory : IHttpClientFactory
	{
		/// <summary>
		/// Cache for the clients
		/// </summary>
		private readonly ConcurrentDictionary<string, HttpClient> clients =
			new ConcurrentDictionary<string, HttpClient>();

		/// <summary>
		/// Gets a cached client for the host associated to the input URL
		/// </summary>
		/// <param name="uri"><see cref="Uri"/> used as the cache key</param>
		/// <returns>A cached <see cref="HttpClient"/> instance for the host</returns>
		public HttpClient GetClient(Uri uri)
		{
			if (uri == null)
			{
				throw new ArgumentNullException(nameof(uri));
			}

			return PerHostClientFromCache(uri);
		}

		public bool DeleteProxiedClient(Uri proxyUri)
		{
			if (clients.TryRemove(proxyUri.Host, out var client))
			{
				client.Dispose();
				return true; 
			}

			return false; 
		}

		public void RemoveAllProxies()
		{
			foreach (var (_, client) in clients)
			{
				client.Dispose();
			}
			
			clients.Clear();
		}

		/// <summary>
		/// Gets a random cached client with a proxy attached to it
		/// </summary>
		/// <param name="proxyUri"><see cref="Uri"/> of the proxy, used as the cache key</param>
		/// <returns>A cached <see cref="HttpClient"/> instance with a random proxy. Returns null if no proxies are available</returns>
		public HttpClient GetProxiedClient(Uri proxyUri)
		{
			return ProxiedClientFromCache(proxyUri);
		}

		private HttpClient PerHostClientFromCache(Uri uri)
		{
			return this.clients.AddOrUpdate(
				key: uri.Host,
				addValueFactory: u => { return CreateClient(); },
				updateValueFactory: (u, client) => { return client; }
			);
		}

		private HttpClient ProxiedClientFromCache(Uri proxyUri)
		{
			return this.clients.AddOrUpdate(
				key: proxyUri.Host,
				addValueFactory: u => { return CreateProxiedClient(proxyUri); },
				updateValueFactory: (u, client) => { return client; }
			);
		}

		private static HttpClient CreateProxiedClient(Uri proxyUrl)
		{
			HttpMessageHandler handler = new SocketsHttpHandler()
			{
				Proxy = new WebProxy(proxyUrl),
				UseProxy = true,
				PooledConnectionLifetime = TimeSpan.FromMinutes(10),
				UseCookies = false,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
			};

			HttpClient client = new HttpClient(handler)
			{
				Timeout = System.Threading.Timeout.InfiniteTimeSpan
			};

			return client;
		}

		private static HttpClient CreateClient()
		{
			HttpMessageHandler handler = new SocketsHttpHandler()
			{
				PooledConnectionLifetime = TimeSpan.FromMinutes(10),
				UseCookies = false,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
			};

			HttpClient client = new HttpClient(handler)
			{
				Timeout = System.Threading.Timeout.InfiniteTimeSpan
			};

			return client;
		}
	}
}