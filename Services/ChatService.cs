using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using AACS.Risk.Web.Models;

namespace AACS.Risk.Web.Services;

public class ChatService : IChatService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ChatService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IRiskService _riskService;
    private readonly string _apiKey;
    private readonly string _model;

    // Store conversation history per session
    private static readonly Dictionary<Guid, List<ChatMessageItem>> _sessionHistory = new();
    private static readonly Dictionary<Guid, SessionMetadata> _sessionMetadata = new();

    public ChatService(HttpClient httpClient, ILogger<ChatService> logger, IConfiguration configuration, IRiskService riskService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        _riskService = riskService;
        _apiKey = configuration["OpenAI:ApiKey"] ?? "";
        _model = configuration["OpenAI:Model"] ?? "gpt-5.2-2025-12-11"; 

        // Configure HttpClient for OpenAI
        _httpClient.BaseAddress = new Uri("https://api.openai.com/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<ChatSessionResponse> SendSessionMessageAsync(ChatSessionRequest request)
    {
        try
        {
            // Create or get session
            var sessionId = request.SessionId ?? Guid.NewGuid();

            if (!_sessionHistory.ContainsKey(sessionId))
            {
                _sessionHistory[sessionId] = new List<ChatMessageItem>();
                _sessionMetadata[sessionId] = new SessionMetadata
                {
                    CreatedAt = DateTime.UtcNow,
                    UserId = request.UserId ?? "anonymous"
                };
            }

            // Update session activity
            if (_sessionMetadata.TryGetValue(sessionId, out var metadata))
            {
                metadata.LastActivityAt = DateTime.UtcNow;
                metadata.MessageCount++;
            }

            _logger.LogInformation("Processing chat message for session {SessionId}: {Message}", sessionId, request.Message);

            // Build context from database
            var contextData = await BuildDatabaseContextAsync(request.Message);

            // Build the system prompt using the modular helper
            var systemPrompt = BuildSystemPrompt(contextData);

            // Add user message to history
            _sessionHistory[sessionId].Add(new ChatMessageItem { Role = "user", Content = request.Message });

            // Build messages array for OpenAI
            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            // Add conversation history (limit to last 10 exchanges to manage tokens)
            var historyToSend = _sessionHistory[sessionId].TakeLast(10).ToList();
            foreach (var msg in historyToSend)
            {
                messages.Add(new { role = msg.Role, content = msg.Content });
            }

            // Call OpenAI API
            var openAiRequest = new
            {
                model = _model,
                messages = messages,
                max_completion_tokens = 8192,
                temperature = 0.5  // Slightly lower for more consistent responses
            };

            var json = JsonSerializer.Serialize(openAiRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("v1/chat/completions", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var openAiResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseJson);

                var assistantMessage = openAiResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "I couldn't generate a response.";

                // Add assistant response to history
                _sessionHistory[sessionId].Add(new ChatMessageItem { Role = "assistant", Content = assistantMessage });

                // Parse suggestions from the response
                var (cleanedResponse, suggestions) = ParseSuggestionsFromResponse(assistantMessage);

                // Generate a message ID
                var messageId = DateTime.UtcNow.Ticks;

                return new ChatSessionResponse
                {
                    SessionId = sessionId,
                    Response = cleanedResponse,
                    MessageId = messageId,
                    Success = true,
                    SuggestedQuestions = suggestions.Any() ? suggestions : GetDefaultSuggestedQuestions(request.Message)
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("OpenAI API error: {StatusCode} - {Error}", response.StatusCode, errorContent);

                return new ChatSessionResponse
                {
                    SessionId = sessionId,
                    Success = false,
                    Error = "AI service error: " + response.StatusCode
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            return new ChatSessionResponse
            {
                SessionId = request.SessionId ?? Guid.NewGuid(),
                Success = false,
                Error = "Failed to process your request. Please try again."
            };
        }
    }

    private (string CleanedResponse, List<SuggestedQuestionDto> Suggestions) ParseSuggestionsFromResponse(string response)
    {
        var suggestions = new List<SuggestedQuestionDto>();
        var cleanedResponse = response;

        // Parse ---SUGGESTIONS--- block
        var suggestionsRegex = new Regex(@"---SUGGESTIONS---\s*([\s\S]*?)\s*---END_SUGGESTIONS---", RegexOptions.IgnoreCase);
        var match = suggestionsRegex.Match(response);

        if (match.Success)
        {
            var suggestionsBlock = match.Groups[1].Value;
            cleanedResponse = suggestionsRegex.Replace(response, "").Trim();

            // Parse each suggestion line
            var lines = suggestionsBlock.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine)) continue;

                // Parse format: [icon:name] Question text
                var iconMatch = Regex.Match(trimmedLine, @"\[icon:(\w+)\]\s*(.+)");
                if (iconMatch.Success)
                {
                    suggestions.Add(new SuggestedQuestionDto
                    {
                        Icon = MapIconName(iconMatch.Groups[1].Value),
                        Text = iconMatch.Groups[2].Value.Trim(),
                        Category = iconMatch.Groups[1].Value
                    });
                }
                else
                {
                    // Fallback for lines without icon
                    suggestions.Add(new SuggestedQuestionDto
                    {
                        Icon = "help_outline",
                        Text = trimmedLine.TrimStart('-', '*', ' '),
                        Category = "general"
                    });
                }
            }
        }

        return (cleanedResponse, suggestions);
    }

    private string MapIconName(string iconKey)
    {
        return iconKey.ToLower() switch
        {
            "warning" => "warning",
            "chart" => "trending_up",
            "building" => "location_city",
            "person" => "person",
            "category" => "category",
            "search" => "search",
            "calendar" => "event",
            "alert" => "notification_important",
            "users" => "groups",
            "dollar" => "attach_money",
            "clipboard" => "assignment",
            _ => "help_outline"
        };
    }

    private List<SuggestedQuestionDto> GetDefaultSuggestedQuestions(string userMessage)
    {
        var messageLower = userMessage.ToLower();

        // Context-aware default suggestions
        if (messageLower.Contains("site") || messageLower.Contains("location"))
        {
            return new List<SuggestedQuestionDto>
            {
                new() { Text = "What are the highest risks at this site?", Icon = "warning", Category = "Priority" },
                new() { Text = "Who owns the most risks here?", Icon = "person", Category = "Ownership" },
                new() { Text = "Compare this site to others", Icon = "compare_arrows", Category = "Analysis" }
            };
        }

        if (messageLower.Contains("trend") || messageLower.Contains("history") || messageLower.Contains("change"))
        {
            return new List<SuggestedQuestionDto>
            {
                new() { Text = "Which risks have worsened the most?", Icon = "trending_down", Category = "Trends" },
                new() { Text = "Show me the risk breakdown by category", Icon = "category", Category = "Analysis" },
                new() { Text = "What needs immediate attention?", Icon = "warning", Category = "Priority" }
            };
        }

        if (messageLower.Contains("owner") || messageLower.Contains("who"))
        {
            return new List<SuggestedQuestionDto>
            {
                new() { Text = "Show all risks for this owner", Icon = "person", Category = "Ownership" },
                new() { Text = "Which owners have the most high risks?", Icon = "warning", Category = "Priority" },
                new() { Text = "Break down by site", Icon = "location_city", Category = "Analysis" }
            };
        }

        // Default suggestions
        return new List<SuggestedQuestionDto>
        {
            new() { Text = "What risks need immediate attention?", Icon = "warning", Category = "Priority" },
            new() { Text = "Show me the risk breakdown by site", Icon = "location_city", Category = "Analysis" },
            new() { Text = "What's the trend over the last 6 months?", Icon = "trending_up", Category = "Trends" },
            new() { Text = "Who owns the most high-risk items?", Icon = "person", Category = "Ownership" }
        };
    }

    private async Task<DatabaseContext> BuildDatabaseContextAsync(string userMessage)
    {
        var context = new DatabaseContext();

        try
        {
            // Always get summary data
            context.Summary = await _riskService.GetDashboardSummaryAsync();

            // Get top risks (limited to 10)
            context.TopRisks = await _riskService.GetTopRisksAsync(10);

            // Get site summary
            context.SiteSummary = await _riskService.GetRiskBySiteSummaryAsync();

            // Get category summary
            context.CategorySummary = await _riskService.GetRiskByCategorySummaryAsync();

            // Get owner summary
            context.OwnerSummary = await _riskService.GetRiskByOwnerSummaryAsync();

            // NOTE: Removed AllRisks to reduce token usage - summaries provide enough context
            // context.AllRisks = await _riskService.GetAllTRRisksAsync();

            // Get sites, services, categories for reference
            context.Sites = await _riskService.GetAllSitesAsync();
            context.Services = await _riskService.GetServicesAsync();
            context.Categories = await _riskService.GetCategoriesAsync();
            context.Users = await _riskService.GetUsersAsync();

            // Get trend data
            context.TrendData = await _riskService.GetRiskTrendChartDataAsync(6);

            // Get watchlist
            context.Watchlist = await _riskService.GetWatchlistAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building database context");
        }

        return context;
    }

    private string BuildSystemPrompt(DatabaseContext context)
    {
        var sb = new StringBuilder();

        // Add the base system prompt from the helper
        sb.AppendLine(ChatServicePromptHelper.BuildSystemPrompt());

        sb.AppendLine();
        sb.AppendLine("=== CURRENT RISK DATA ===");
        sb.AppendLine();

        // Summary
        if (context.Summary != null)
        {
            sb.AppendLine("## Overall Summary:");
            sb.AppendLine($"- Total Risks: {context.Summary.TotalRisks}");
            sb.AppendLine($"- High Risk (Red): {context.Summary.HighRiskCount}");
            sb.AppendLine($"- Medium Risk (Amber): {context.Summary.MediumRiskCount}");
            sb.AppendLine($"- Low Risk (Green): {context.Summary.LowRiskCount}");
            sb.AppendLine($"- Average Score: {context.Summary.AverageScore:F1}");
            sb.AppendLine($"- Aggregate Score: {context.Summary.AggregateScore:F1}");
            sb.AppendLine();
        }

        // Top Risks
        if (context.TopRisks?.Any() == true)
        {
            sb.AppendLine("## Top 10 Highest Risks:");
            foreach (var risk in context.TopRisks.Take(10))
            {
                sb.AppendLine($"- {risk.Name} (Site: {risk.SiteName}, Category: {risk.CategoryName}, Owner: {risk.OwnerName}, Score: {risk.Score:F1}, RAG: {risk.RAGRating})");
            }
            sb.AppendLine();
        }

        // Site Summary
        if (context.SiteSummary?.Any() == true)
        {
            sb.AppendLine("## Risk by Site:");
            foreach (var site in context.SiteSummary)
            {
                sb.AppendLine($"- {site.SiteName}: {site.TotalRisks} risks (High: {site.HighRisk}, Medium: {site.MediumRisk}, Low: {site.LowRisk}, Avg: {site.AverageScore:F1})");
            }
            sb.AppendLine();
        }

        // Category Summary
        if (context.CategorySummary?.Any() == true)
        {
            sb.AppendLine("## Risk by Category:");
            foreach (var cat in context.CategorySummary)
            {
                sb.AppendLine($"- {cat.CategoryName}: {cat.TotalRisks} risks (High: {cat.HighRisk}, Medium: {cat.MediumRisk}, Low: {cat.LowRisk}, Avg: {cat.AverageScore:F1})");
            }
            sb.AppendLine();
        }

        // Owner Summary
        if (context.OwnerSummary?.Any() == true)
        {
            sb.AppendLine("## Risk by Owner:");
            foreach (var owner in context.OwnerSummary)
            {
                sb.AppendLine($"- {owner.OwnerName}: {owner.TotalRisks} risks (High: {owner.HighRisk}, Medium: {owner.MediumRisk}, Low: {owner.LowRisk}, Avg: {owner.AverageScore:F1})");
            }
            sb.AppendLine();
        }

        // Watchlist
        if (context.Watchlist?.Any() == true)
        {
            sb.AppendLine("## Watchlist Items:");
            foreach (var item in context.Watchlist)
            {
                sb.AppendLine($"- {item.Name} (Site: {item.SiteName}, Score: {item.Score:F1}, RAG: {item.RAGRating})");
            }
            sb.AppendLine();
        }

        // Available Sites
        if (context.Sites?.Any() == true)
        {
            sb.AppendLine($"## Available Sites: {string.Join(", ", context.Sites.Select(s => s.Name))}");
            sb.AppendLine();
        }

        // Available Services
        if (context.Services?.Any() == true)
        {
            sb.AppendLine($"## Available Services: {string.Join(", ", context.Services.Select(s => s.Name))}");
            sb.AppendLine();
        }

        // Available Categories
        if (context.Categories?.Any() == true)
        {
            sb.AppendLine($"## Risk Categories: {string.Join(", ", context.Categories.Select(c => c.Name))}");
            sb.AppendLine();
        }

        // Trend Data
        if (context.TrendData?.Any() == true)
        {
            sb.AppendLine("## Risk Trend (Last 6 Months):");
            foreach (var trend in context.TrendData.OrderBy(t => t.Month))
            {
                var total = trend.HighRiskCount + trend.MediumRiskCount + trend.LowRiskCount;
                sb.AppendLine($"- {trend.Month:MMM yyyy}: High={trend.HighRiskCount}, Medium={trend.MediumRiskCount}, Low={trend.LowRiskCount}, Total={total}, Avg={trend.AverageScore:F1}");
            }
            sb.AppendLine();
        }

        // NOTE: Removed All Risks Detail section to reduce token usage
        // The summaries, top risks, and trend data provide sufficient context for most queries

        sb.AppendLine("=== END OF DATA ===");

        return sb.ToString();
    }

    public Task SubmitFeedbackAsync(ChatFeedbackRequest request)
    {
        // Log feedback for now - could be stored in database later
        _logger.LogInformation("Feedback received for message {MessageId}: Rating={Rating}, Category={Category}, Text={Text}",
            request.MessageId, request.Rating, request.Category, request.FeedbackText);
        return Task.CompletedTask;
    }

    // Helper classes
    private class ChatMessageItem
    {
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";
    }

    private class SessionMetadata
    {
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivityAt { get; set; }
        public string UserId { get; set; } = "";
        public int MessageCount { get; set; }
    }

    private class DatabaseContext
    {
        public RiskDashboardSummaryDto? Summary { get; set; }
        public List<TopRiskDto>? TopRisks { get; set; }
        public List<RiskBySiteSummaryDto>? SiteSummary { get; set; }
        public List<RiskByCategorySummaryDto>? CategorySummary { get; set; }
        public List<RiskByOwnerSummaryDto>? OwnerSummary { get; set; }
        public List<TRRiskDto>? AllRisks { get; set; }
        public List<SiteDto>? Sites { get; set; }
        public List<ServiceDto>? Services { get; set; }
        public List<RiskCategoryDto>? Categories { get; set; }
        public List<UserDto>? Users { get; set; }
        public List<RiskTrendDataDto>? TrendData { get; set; }
        public List<WatchlistItemDto>? Watchlist { get; set; }
    }
}

// OpenAI Response Models
public class OpenAIResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("choices")]
    public List<OpenAIChoice>? Choices { get; set; }

    [JsonPropertyName("usage")]
    public OpenAIUsage? Usage { get; set; }
}

public class OpenAIChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public OpenAIMessage? Message { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

public class OpenAIMessage
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

public class OpenAIUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}