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
		private Queue<Uri> _proxies;

		public ProxyManager()
		{
			_proxies = new Queue<Uri>();
		}

		public async Task<Uri> GetNextProxy()
		{
			if (_proxies.Count == 0) await GetProxies();
			var p = _proxies.Dequeue();
#if DEBUG
			Console.WriteLine($"[ProxyManager] Returning Proxy: {p}");
#endif
			return p;
		}

		public void AddProxy(Uri proxy)
		{
#if DEBUG
			Console.WriteLine($"[ProxyManager] Adding Proxy: {proxy}");
#endif
			_proxies.Enqueue(proxy);
		}


		public async Task<int> GetProxies()
		{
#if DEBUG
			Console.WriteLine("[ProxyManager] Getting Proxies.");
#endif
			using var reqRes = await Constants.ProxyClient.GetAsync(
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

		public async Task ReplaceProxy(Uri proxy)
		{
			using var reqRes = await Constants.ProxyClient.PostAsync(
				"https://proxy.webshare.io/api/proxy/replacement/",
				new StringContent("{\"ip_address\": \"" + proxy.Host + "/32\"}", Encoding.UTF8, "application/json")
			);

			Console.WriteLine(
				$"[ProxyManager] Replacing Proxy: {proxy} (Host: {proxy.Host}) (Status: {(int) reqRes.StatusCode})");
		}
	}
}