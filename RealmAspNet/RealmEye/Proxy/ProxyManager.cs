using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RealmAspNet.RealmEye.Proxy
{
	public class ProxyManager
	{
		private static HttpClient _proxyClient;

		static ProxyManager()
		{
			_proxyClient = new HttpClient(new HttpClientHandler
			{
				AllowAutoRedirect = true
			});
			_proxyClient.DefaultRequestHeaders.Add("Authorization", $"Token {Constants.Configuration["proxy_key"]}");
		}


		private readonly Queue<Uri> _proxies;
		private readonly string _apiKey;

		/// <summary>
		/// The `ProxyManager` constructor.
		/// </summary>
		/// <param name="apiKey">The API key.</param>
		public ProxyManager(string apiKey)
		{
			_proxies = new Queue<Uri>();
			_apiKey = apiKey;
		}

		/// <summary>
		/// Gets the next proxy available. If no proxies are available, this will retrieve a list of new proxies and
		/// then return the next one.
		/// </summary>
		/// <returns>The next available proxy to use.</returns>
		/// <exception cref="Exception">If no API key is set.</exception>
		public async Task<Uri> GetNextProxy()
		{
			if (_apiKey == string.Empty) 
				throw new Exception("No API Key set.");
			
			if (_proxies.Count == 0) 
				await GetProxies();
			var p = _proxies.Dequeue();
			return p;
		}

		/// <summary>
		/// Adds a proxy to the queue of proxies to use. Use this method to "return" a proxy that you got from the
		/// `GetNextProxy` method.
		/// </summary>
		/// <param name="proxy">The proxy to add to the queue.</param>
		/// <exception cref="Exception">If no API key is set.</exception>
		public void AddProxy(Uri proxy)
		{
			if (_apiKey == string.Empty) 
				throw new Exception("No API Key set.");
			
			_proxies.Enqueue(proxy);
		}

		/// <summary>
		/// Gets the proxies from the Webshare.io API.
		/// </summary>
		/// <returns>The number of proxies retrieved. If no API key is set, this will return 0.</returns>
		public async Task<int> GetProxies()
		{
			if (_apiKey == string.Empty)
				return 0;
			
			using var reqRes = await _proxyClient.GetAsync(
				"https://proxy.webshare.io/api/proxy/list/"
			);

			var json = JsonConvert.DeserializeObject<WebshareResponse>(await reqRes.Content.ReadAsStringAsync());
			if (json is null || json.Count == 0) return 0;

			foreach (var uri in json.Results.Select(x =>
				new Uri($"http://{x.Username}:{x.Password}@{x.ProxyAddress}:{x.Ports.Http}")))
			{
				_proxies.Enqueue(uri);
			}

			return (int) json.Count;
		}

		/// <summary>
		/// Removes the proxy from Webshare.io.
		/// </summary>
		/// <param name="proxy">The proxy to remove.</param>
		/// <exception cref="Exception">If no API key is set.</exception>
		public async Task RemoveProxy(Uri proxy)
		{
			if (_apiKey == string.Empty) 
				throw new Exception("No API Key set.");
			using var reqRes = await _proxyClient.PostAsync(
				"https://proxy.webshare.io/api/proxy/replacement/",
				new StringContent("{\"ip_address\": \"" + proxy.Host + "/32\"}", Encoding.UTF8, "application/json")
			);
		}
	}
}