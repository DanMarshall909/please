using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
	private static readonly HttpClient client = new HttpClient();

	static async Task Main(string[] args)
	{
		if (args.Length == 0)
		{
			Console.WriteLine("Usage: please.exe <task description>");
			Console.WriteLine("Example: please.exe \"list all files in the current directory\"");
			Environment.Exit(1);
		}

		// Join all arguments as the task description
		string taskDescription = string.Join(" ", args);

		// Default Ollama settings
		string ollamaURL = Environment.GetEnvironmentVariable("OLLAMA_URL") ?? "http://localhost:11434";
		string model = Environment.GetEnvironmentVariable("OLLAMA_MODEL") ?? "llama3.2";

		try
		{
			// Generate PowerShell script
			string script = await GeneratePowerShellScript(ollamaURL, model, taskDescription);
			Console.Write(script);
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"Error: {ex.Message}");
			Environment.Exit(1);
		}
	}

	static async Task<string> GeneratePowerShellScript(string baseURL, string model, string taskDescription)
	{
		string prompt = $@"You are a PowerShell expert. Generate a complete, working PowerShell script to accomplish the following task:

{taskDescription}

Requirements:
- Write clean, well-commented PowerShell code
- Include error handling where appropriate
- Use PowerShell best practices
- Only return the PowerShell script code, no explanations or markdown formatting
- The script should be ready to run as-is

PowerShell Script:";

		var request = new
		{
			model = model,
			prompt = prompt,
			stream = false,
			options = new
			{
				temperature = 0.3,
				top_p = 0.9
			}
		};

		// Serialize request to JSON
		string jsonRequest = JsonSerializer.Serialize(request);
		var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

		// Set timeout
		client.Timeout = TimeSpan.FromMinutes(2);

		try
		{
			// Make request to Ollama
			HttpResponseMessage response = await client.PostAsync($"{baseURL}/api/generate", content);

			if (!response.IsSuccessStatusCode)
			{
				string errorBody = await response.Content.ReadAsStringAsync();
				throw new Exception($"Ollama API returned status {response.StatusCode}: {errorBody}");
			}

			string responseBody = await response.Content.ReadAsStringAsync();

			// Parse response
			using (JsonDocument doc = JsonDocument.Parse(responseBody))
			{
				JsonElement root = doc.RootElement;
				if (root.TryGetProperty("response", out JsonElement responseElement))
				{
					return responseElement.GetString()?.Trim() ?? "";
				}
				else
				{
					throw new Exception("Invalid response format from Ollama");
				}
			}
		}
		catch (HttpRequestException ex)
		{
			throw new Exception($"Failed to connect to Ollama at {baseURL}: {ex.Message}\nMake sure Ollama is running");
		}
		catch (TaskCanceledException)
		{
			throw new Exception("Request timed out. The model might be taking too long to generate the script.");
		}
	}
}
