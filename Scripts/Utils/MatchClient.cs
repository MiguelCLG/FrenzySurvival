using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class Match
{
    public int GameId { get; set; }
    public DateTime MatchDate { get; set; }
    public string Notes { get; set; }
    public bool IsFinished { get; set; }
}

public class MatchResponse
{
    public int MatchId { get; set; }
}

public class MatchDataPoint
{
    public string PlayerName { get; set; }
    public int GamePoints { get; set; }
    public string PointsDescription { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class MatchClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://gamescoringapi.azurewebsites.net";

    public MatchClient()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // Method to create a new match and return its ID
    public async Task<int?> PostMatchAsync(Match matchData)
    {
        var jsonContent = JsonSerializer.Serialize(matchData);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync($"{BaseUrl}/match", content);

        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();

            // Deserialize the response to get MatchId
            var matchResponse = JsonSerializer.Deserialize<MatchResponse>(responseContent);
            Console.WriteLine($"Success! Match created with ID: {matchResponse?.MatchId}");
            return matchResponse?.MatchId;
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Content: {responseContent}");
            return null;
        }
    }

    // Method to create a new match data point for a specified match
    public async Task PostMatchDataPointAsync(int matchId, MatchDataPoint dataPoint)
    {
        var jsonContent = JsonSerializer.Serialize(dataPoint);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync($"{BaseUrl}/match-data-point/{matchId}", content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Success! Match data point created at: {response.Headers.Location}");
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Created Match Data Point: {responseContent}");
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Content: {responseContent}");
        }
    }
}