using GHPT.IO;
using GHPT.Prompts;
using System.Text.Json;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System.Linq;

namespace GHPT.Utils
{

	public static class PromptUtils
	{

		private const string SPLITTER = "// JSON: ";

		public static string GetChatGPTJson(string chatGPTResponse)
		{
			try
			{
				// Split by "// JSON:" and take the last part
				string[] parts = chatGPTResponse.Split(new string[] { "// JSON:" }, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length < 2)
				{
					CreateDebugPanel("No JSON part found in response", "JSON Extraction Error");
					return string.Empty;
				}

				string jsonPart = parts[1].Trim();
				CreateDebugPanel($"Extracted JSON part:\n{jsonPart}", "JSON Extraction");
				return jsonPart;
			}
			catch (Exception ex)
			{
				CreateDebugPanel($"Error extracting JSON: {ex.Message}", "JSON Extraction Error");
				return string.Empty;
			}
		}

		public static PromptData GetPromptDataFromResponse(string chatGPTJson)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(chatGPTJson))
				{
					CreateDebugPanel("Empty JSON response received", "JSON Processing Error");
					return new PromptData()
					{
						Additions = new List<Addition>(),
						Connections = new List<ConnectionPairing>(),
						Advice = "No valid response received from the AI. Please try again."
					};
				}

				JsonSerializerOptions options = new()
				{
					AllowTrailingCommas = true,
					PropertyNameCaseInsensitive = true,
					IgnoreReadOnlyFields = true,
					IgnoreReadOnlyProperties = true,
					ReadCommentHandling = JsonCommentHandling.Skip,
					WriteIndented = true,
					IncludeFields = true,
					NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
					Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
					Converters = { new NumberToStringConverter() }
				};

				try
				{
					PromptData result = JsonSerializer.Deserialize<PromptData>(chatGPTJson, options);
					result.ComputeTiers();
					
					// Fix the null-conditional operator issue
					int additionsCount = result.Additions != null ? result.Additions.Count() : 0;
					int connectionsCount = result.Connections != null ? result.Connections.Count() : 0;
					
					CreateDebugPanel($"Successfully deserialized JSON with {additionsCount} additions and {connectionsCount} connections", "JSON Deserialization");
					return result;
				}
				catch (JsonException ex)
				{
					CreateDebugPanel($"Failed to deserialize JSON response: {ex.Message}", "JSON Deserialization Error");
					return new PromptData()
					{
						Additions = new List<Addition>(),
						Connections = new List<ConnectionPairing>(),
						Advice = "Failed to process AI response. Please try again."
					};
				}
			}
			catch (Exception ex)
			{
				CreateDebugPanel($"Error in GetPromptDataFromResponse: {ex.Message}\n{ex.StackTrace}", "JSON Processing Error");
				return new PromptData()
				{
					Additions = new List<Addition>(),
					Connections = new List<ConnectionPairing>(),
					Advice = $"Error processing response: {ex.Message}"
				};
			}
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

		public static async Task<PromptData> AskQuestion(GPTConfig config, string question)
		{
			try
			{
				CreateDebugPanel($"Starting AskQuestion with config: {config.Name}, Model: {config.Model}", "API Request");
				string prompt = Prompt.GetPrompt(question);
				CreateDebugPanel($"Generated prompt:\n{prompt}", "Prompt Generation");
				
				var payload = await ClientUtil.Ask(config, prompt);
				Console.WriteLine($"API Response received: {payload != null}");
				
				if (payload == null || payload.Choices == null || !payload.Choices.Any())
				{
					Console.WriteLine("No valid response from API");
					return new PromptData()
					{
						Additions = new List<Addition>(),
						Connections = new List<ConnectionPairing>(),
						Advice = "No response received from the AI service. Please check your API key and try again."
					};
				}

				string payloadJson = payload.Choices.FirstOrDefault().Message.Content;
				Console.WriteLine($"Raw response content: {payloadJson}");
				
				// Check for exact TOO_COMPLEX match
				if (payloadJson.Trim() == Prompt.TOO_COMPLEX)
				{
					Console.WriteLine("Exact TOO_COMPLEX match found");
					return new PromptData()
					{
						Additions = new List<Addition>(),
						Connections = new List<ConnectionPairing>(),
						Advice = Prompt.TOO_COMPLEX
					};
				}

				string chatGPTJson = GetChatGPTJson(payloadJson);
				Console.WriteLine($"Extracted JSON: {chatGPTJson}");
				
				return GetPromptDataFromResponse(chatGPTJson);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in AskQuestion: {ex.Message}\n{ex.StackTrace}");
				return new PromptData()
				{
					Additions = new List<Addition>(),
					Connections = new List<ConnectionPairing>(),
					Advice = $"Error: {ex.Message}"
				};
			}
		}

	}

	public class NumberToStringConverter : System.Text.Json.Serialization.JsonConverter<string>
	{
		public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Number)
			{
				return reader.GetDouble().ToString();
			}
			return reader.GetString() ?? string.Empty;
		}

		public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value);
		}
	}

}
