using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiuyongTests.AnythingLLM
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Net.Http;
	using System.Text.Json.Serialization;
	using System.Text.Json;
	using System.IO;

	public partial class AnythingllmClient
	{

		// /v1/workspace/{slug}/thread/{threadSlug}/chat
		public async Task<string> DoChatAsync(string slug, string message, string threadSlug = null!)
		{
			var p = new
			{
				id = "chat-uuid",
				type = "abort | textResponse",
				textResponse = "Response to your query",
				sources = new[]
				{
					new
					{
						title = "anythingllm.txt",
						chunk = "This is a context chunk used in the answer of the prompt by the LLM,"
					}
				},
				close = true,
				error = "null | text string of the failure mode."
			};

			var uri = String.IsNullOrWhiteSpace(threadSlug) ? $"v1/workspace/{slug}/chat" : $"v1/workspace/{slug}/thread/{threadSlug}/chat";
			var msg = new
			{
				message,
				mode = "chat",
				//userId = 1
			};
			var rp = await DoRequestAsync(uri, p, msg);
			var ro = rp.textResponse;
			return ro;
		}

	}

}
