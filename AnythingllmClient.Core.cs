using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jiuyong.AnythingLLM
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Net.Http;
	using System.Text.Json.Serialization;
	using System.Text.Json;
	using System.IO;

	public enum HttpMethod
	{
		Auto,
		Get,
		Post,
		Delete,
	}

	public partial class AnythingllmClient
	{
		private readonly string _apiKey;
		//private readonly string _secretKey;
		readonly string _llmBaseUri;

		public AnythingllmClient(string apiKey/*, string secretKey*/, string llmBaseUri = "http://localhost:3001/api")
		{
			_apiKey = apiKey;
			this._llmBaseUri = llmBaseUri;
			//_secretKey = secretKey;
		}

		private HttpClient InitClient()
		{
			HttpClient client = new();
			client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue($"Bearer", _apiKey);
			return client;
		}

		private string GetFullUri(string commandUri)
		{
			//return Path.Combine(_llmBaseUri, commandUri);
			//严谨方法全部失败。
			var r = _llmBaseUri + commandUri;
			return r;
		}

		HttpClient InitRequest(string commandUri, out string fullUri)
		{
			string url = GetFullUri(commandUri);
			fullUri = url;

			var client = InitClient();
			return client;
		}

		private static async Task<P> ParseResponse<P>(HttpResponseMessage response, P anserSample)
		{
			if (response.IsSuccessStatusCode)
			{
				var p_txt = await response.Content.ReadAsStringAsync();
				var p = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(p_txt, anserSample);
				return p ?? throw new Exception(p_txt);
			}
			else
			{
				throw new Exception($"请求失败，状态码: {response.StatusCode}，原因：“${response.ReasonPhrase}”。");
			}
		}

		public async Task<P> DoGetAsync<P>(string commandUri, P anserSample)
		{

			using var client = InitRequest(commandUri, out string url);

			var response = await client.GetAsync(url);

			return await ParseResponse(response, anserSample);
		}

		public async Task<P> DoRequestAsync<Q, P>(string commandUri, P anserSample, Q message = default!, HttpMethod method = HttpMethod.Auto)
		{
			string url = Path.Combine(_llmBaseUri, commandUri);
			using HttpClient client = new();
			client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue($"Bearer", _apiKey);
			//var hds = client.DefaultRequestHeaders;
			//hds.Clear();

			//var timestamp = (DateTimeOffset.Now.UtcTicks - TimeZero).ToString();
			//var timestamp = DateTime.Now.GetUtcTimeTicks().ToString();
			string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(message);
			//string signature = Helper.CalculateMd5(_secretKey + jsonData + timestamp);

			//Dictionary<string, string> headers = new()
			//{
			//	//{ "Content-Type", "application/json" },
			//	//{ "Authorization", $"Bearer {_apiKey}" },
			//	//{ "X-BC-Request-Id", Guid.NewGuid().ToString() },
			//	//{ "X-BC-Timestamp", timestamp },
			//	//{ "X-BC-Signature", signature },
			//	//{ "X-BC-Sign-Algo", "MD5" }
			//};

			StringContent content = new(jsonData, Encoding.UTF8, "application/json");
			//var hds = content.Headers;
			//foreach (var header in headers)
			//{
			//	hds.Add(header.Key, header.Value);
			//}

			HttpResponseMessage response;
			switch (method)
			{
				case HttpMethod.Get:
					response = await client.GetAsync(url);
					break;
				case HttpMethod.Post:
					response = await client.PostAsync(url, content);
					break;
				case HttpMethod.Delete:
					response = await client.DeleteAsync(url);
					break;
				case HttpMethod.Auto:
				//break;
				default:
					if (null == message)
					{
						response = await client.GetAsync(url);
					}
					else
					{
						response = await client.PostAsync(url, content);
					}
					break;
			}

			if (response.IsSuccessStatusCode)
			{
				var p_txt = await response.Content.ReadAsStringAsync();
				P? p;
				p = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(p_txt, anserSample);
				return p ?? throw new Exception(p_txt);
			}
			else
			{
				throw new Exception($"请求失败，状态码: {response.StatusCode}");
			}
		}

	}

}
