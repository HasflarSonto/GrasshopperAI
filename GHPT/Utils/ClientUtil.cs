using GHPT.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System.Linq;
using System.Text;

namespace GHPT.Utils
{
	public static class ClientUtil
	{
		public static async Task<ResponsePayload> Ask(GPTConfig config, string prompt, double temperature = 0.7)
		{
			var url = "https://api.openai.com/v1/chat/completions";

			AskPayload payload = new(
				config.Model,
				prompt,
				temperature);

			return await Ask(config, payload);
		}

		public static async Task<ResponsePayload> Ask(GPTConfig config, AskPayload payload)
		{
			var url = "https://api.openai.com/v1/chat/completions";

			var client = new HttpClient();
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.Token}");

			var jsonPayload = JsonConvert.SerializeObject(payload);
			CreateDebugPanel($"Sending request to OpenAI API:\nModel: {config.Model}\nTemperature: {payload.Temperature}\nPayload: {jsonPayload}", "API Request");

			var response = await client.PostAsync(url, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

			int statusCode = (int)response.StatusCode;
			if (statusCode < 200 || statusCode >= 300)
			{
				var errorContent = await response.Content.ReadAsStringAsync();
				CreateDebugPanel($"API Error: {response.StatusCode} {response.ReasonPhrase}\n{errorContent}", "API Error");
				throw new System.Exception($"Error: {response.StatusCode} {response.ReasonPhrase} {errorContent}");
			}

			var result = await response.Content.ReadAsStringAsync();
			CreateDebugPanel($"Received API response:\n{result}", "API Response");

			return JsonConvert.DeserializeObject<ResponsePayload>(result);
		}

		private static void CreateDebugPanel(string content, string name)
		{
			try
			{
				var doc = Grasshopper.Instances.ActiveCanvas.Document;
				if (doc == null) return;

				var panel = new GH_Panel();
				panel.NickName = name;
				panel.UserText = content;
				panel.CreateAttributes();
				
				// Find the GHPT component using a different approach
				IGH_DocumentObject ghptComponent = null;
				foreach (var obj in doc.Objects)
				{
					if (obj.GetType().Name == "GHPT")
					{
						ghptComponent = obj;
						break;
					}
				}

				if (ghptComponent != null)
				{
					var pivot = ghptComponent.Attributes.Pivot;
					panel.Attributes.Pivot = new System.Drawing.PointF(pivot.X + 300, pivot.Y);
				}

				doc.AddObject(panel, false);
				doc.NewSolution(true, GH_SolutionMode.Silent);
			}
			catch (Exception ex)
			{
				// If we can't create the panel, at least log the error
				Console.WriteLine($"Error creating debug panel: {ex.Message}");
			}
		}
	}
}
