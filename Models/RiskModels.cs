namespace AACS.Risk.Web.Models;

// ============================================================================
// CORE ENTITIES
// ============================================================================

public class DomainDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class ServiceDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class SiteDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int DomainId { get; set; }
    public string? DomainName { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string Email { get; set; } = "";
    public int? PortfolioId { get; set; }
    public string? PortfolioName { get; set; }
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
}

public class RoleDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class PortfolioDto
{
    public int PortfolioId { get; set; }
    public string? Name { get; set; }
}

// ============================================================================
// RISK CATEGORIES & CRITERIA
// ============================================================================

public class RiskCategoryDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int DomainId { get; set; }
    public string? DomainName { get; set; }
    public string? RiskType { get; set; }
    public string? RatingType { get; set; }
    public int ServiceId { get; set; }
    public string? ServiceName { get; set; }
}

public class RiskCriteriaDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? RiskCategoryId { get; set; }
    public string? RiskCategoryName { get; set; }
}

// ============================================================================
// TURNAROUND RISKS (TR)
// ============================================================================

public class TRRiskDto
{
    public int TRRiskId { get; set; }
    public string? Name { get; set; }
    public int SiteId { get; set; }
    public string? SiteName { get; set; }
    public int ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public int RiskCategoryId { get; set; }
    public string? RiskCategoryName { get; set; }
    public int OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public string? RAGRating { get; set; }
    public string? Status { get; set; }
    public DateTime? LastUpdated { get; set; }
    public int? PortfolioId { get; set; }
    public string? PortfolioName { get; set; }
    public int? UserId { get; set; }
    public string? RatingType { get; set; }

    // Computed/Latest Score
    public decimal? CurrentScore { get; set; }
    public decimal? PreviousScore { get; set; }
    public decimal? Variance { get; set; }
    public string? TrendDirection { get; set; }
}

public class TRRiskScoreDto
{
    public int Id { get; set; }
    public int TRRiskId { get; set; }
    public DateTime RatingDate { get; set; }
    public decimal Score { get; set; }
    public string? RAGRating { get; set; }
    public string? Notes { get; set; }
    public int? EnteredBy { get; set; }
    public string? EnteredByName { get; set; }
    public DateTime? EnteredAt { get; set; }
    public string? RatingType { get; set; }
}

public class TRRiskThresholdDto
{
    public int Id { get; set; }
    public int RiskCategoryId { get; set; }
    public string? RiskCategoryName { get; set; }
    public int SiteId { get; set; }
    public string? SiteName { get; set; }
    public int OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public decimal BenchmarkValue { get; set; }
    public int ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public decimal GreenMax { get; set; } = 3.9m;
    public decimal AmberMax { get; set; } = 6.9m;
    public bool IsActive { get; set; } = true;
    public DateTime? LastModified { get; set; }
}

// ============================================================================
// JOURNAL NOTES & REMINDERS
// ============================================================================

public class JournalNoteDto
{
    public int JournalNoteId { get; set; }
    public int? TRRiskId { get; set; }
    public string? TRRiskName { get; set; }
    public int? FocalRiskId { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
}

public class ReminderDto
{
    public int ReminderId { get; set; }
    public int? TRRiskId { get; set; }
    public string? TRRiskName { get; set; }
    public int? FocalRiskId { get; set; }
    public string? Message { get; set; }
    public DateTime RemindAt { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public bool IsDismissed { get; set; }
}

public class EmailAlertDto
{
    public int EmailAlertId { get; set; }
    public int? TRRiskId { get; set; }
    public string? TRRiskName { get; set; }
    public int? FocalRiskId { get; set; }
    public string? Subject { get; set; }
    public string? Message { get; set; }
    public string? SentTo { get; set; }
    public DateTime? SentAt { get; set; }
}

// ============================================================================
// DASHBOARD & SUMMARY DTOs
// ============================================================================

public class RiskDashboardSummaryDto
{
    public int TotalRisks { get; set; }
    public int HighRiskCount { get; set; }
    public int MediumRiskCount { get; set; }
    public int LowRiskCount { get; set; }
    public decimal AverageScore { get; set; }
    public decimal AggregateScore { get; set; }
    public string? TrendDirection { get; set; }
    public decimal? TrendValue { get; set; }
}

public class TopRiskDto
{
    public int TRRiskId { get; set; }
    public string? Name { get; set; }
    public string? SiteName { get; set; }
    public string? CategoryName { get; set; }
    public string? OwnerName { get; set; }
    public decimal Score { get; set; }
    public string? RAGRating { get; set; }
    public decimal? Variance { get; set; }
    public string? TrendDirection { get; set; }
}

public class WatchlistItemDto
{
    public int TRRiskId { get; set; }
    public string? Name { get; set; }
    public string? SiteName { get; set; }
    public decimal Score { get; set; }
    public string? RAGRating { get; set; }
    public string? Reason { get; set; }
}

public class RiskTrendDataDto
{
    public DateTime Month { get; set; }
    public decimal AverageScore { get; set; }
    public int HighRiskCount { get; set; }
    public int MediumRiskCount { get; set; }
    public int LowRiskCount { get; set; }
}

// ============================================================================
// SUMMARY REPORTS
// ============================================================================

public class RiskBySiteSummaryDto
{
    public int SiteId { get; set; }
    public string? SiteName { get; set; }
    public int TotalRisks { get; set; }
    public int HighRisk { get; set; }
    public int MediumRisk { get; set; }
    public int LowRisk { get; set; }
    public decimal AverageScore { get; set; }
}

public class RiskByCategorySummaryDto
{
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int TotalRisks { get; set; }
    public int HighRisk { get; set; }
    public int MediumRisk { get; set; }
    public int LowRisk { get; set; }
    public decimal AverageScore { get; set; }
}

public class RiskByOwnerSummaryDto
{
    public int OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public int TotalRisks { get; set; }
    public int HighRisk { get; set; }
    public int MediumRisk { get; set; }
    public int LowRisk { get; set; }
    public decimal AverageScore { get; set; }
}

// ============================================================================
// VARIANCE REPORT
// ============================================================================

public class VarianceReportItemDto
{
    public int TRRiskId { get; set; }
    public string? RiskName { get; set; }
    public string? SiteName { get; set; }
    public string? CategoryName { get; set; }
    public string? OwnerName { get; set; }
    public decimal CurrentScore { get; set; }
    public decimal PreviousScore { get; set; }
    public decimal Variance { get; set; }
    public decimal VariancePercent { get; set; }
    public string? CurrentRAG { get; set; }
    public string? PreviousRAG { get; set; }
    public string? TrendDirection { get; set; }
    public DateTime LastUpdated { get; set; }
}

// ============================================================================
// FOCAL RISKS (if needed)
// ============================================================================

public class FocalRiskDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int RiskCategoryId { get; set; }
    public string? RiskCategoryName { get; set; }
    public int SiteId { get; set; }
    public string? SiteName { get; set; }
    public string? Status { get; set; }
    public int OwnerId { get; set; }
    public string? OwnerName { get; set; }
}

// ============================================================================
// CHAT / AI MODELS
// ============================================================================

public class ChatSessionRequest
{
    public string Message { get; set; } = "";
    public Guid? SessionId { get; set; }
    public string? UserId { get; set; }
}

public class ChatSessionResponse
{
    public Guid SessionId { get; set; }
    public string? Response { get; set; }
    public long MessageId { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
    public List<DocumentSourceDto>? Sources { get; set; }
    public List<SuggestedQuestionDto>? SuggestedQuestions { get; set; }
}

public class DocumentSourceDto
{
    public int DocumentId { get; set; }
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public string? Excerpt { get; set; }
    public decimal RelevanceScore { get; set; }
}

public class SuggestedQuestionDto
{
    public string Text { get; set; } = "";
    public string? Icon { get; set; }
    public string? Category { get; set; }
}

public class ChatFeedbackRequest
{
    public long MessageId { get; set; }
    public Guid SessionId { get; set; }
    public string? UserId { get; set; }
    public int Rating { get; set; }
    public string? Category { get; set; }
    public string? FeedbackText { get; set; }
}

// ============================================================================
// CREATE/UPDATE REQUEST DTOs
// ============================================================================

public class CreateTRRiskRequest
{
    public string? Name { get; set; }
    public int SiteId { get; set; }
    public int ServiceId { get; set; }
    public int RiskCategoryId { get; set; }
    public int OwnerId { get; set; }
    public string? RAGRating { get; set; }
    public string? Status { get; set; }
    public int? PortfolioId { get; set; }
    public string? RatingType { get; set; }
}

public class UpdateTRRiskRequest
{
    public int TRRiskId { get; set; }
    public string? Name { get; set; }
    public int SiteId { get; set; }
    public int ServiceId { get; set; }
    public int RiskCategoryId { get; set; }
    public int OwnerId { get; set; }
    public string? RAGRating { get; set; }
    public string? Status { get; set; }
    public int? PortfolioId { get; set; }
    public string? RatingType { get; set; }
}

public class AddRiskScoreRequest
{
    public int TRRiskId { get; set; }
    public DateTime RatingDate { get; set; }
    public decimal Score { get; set; }
    public string? RAGRating { get; set; }
    public string? Notes { get; set; }
    public int EnteredBy { get; set; }
    public string? RatingType { get; set; }
}

public class CreateThresholdRequest
{
    public int RiskCategoryId { get; set; }
    public int SiteId { get; set; }
    public int OwnerId { get; set; }
    public decimal BenchmarkValue { get; set; }
    public int ServiceId { get; set; }
    public decimal GreenMax { get; set; } = 3.9m;
    public decimal AmberMax { get; set; } = 6.9m;
}

public class CreateJournalNoteRequest
{
    public int? TRRiskId { get; set; }
    public int? FocalRiskId { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public int UserId { get; set; }
}

public class CreateReminderRequest
{
    public int? TRRiskId { get; set; }
    public int? FocalRiskId { get; set; }
    public string? Message { get; set; }
    public DateTime RemindAt { get; set; }
    public int UserId { get; set; }
}

public class VarianceTrendItem
{
    public string Month { get; set; } = "";
    public int Improved { get; set; }
    public int Worsened { get; set; }
}

// ============================================================================
// HELP SECTION DTOs
// ============================================================================

public class HelpSectionDto
{
    public int Id { get; set; }
    public string PageKey { get; set; } = "";
    public string? SectionTitle { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<HelpItemDto> Items { get; set; } = new();
}

public class HelpItemDto
{
    public int Id { get; set; }
    public int HelpSectionId { get; set; }
    public string ItemType { get; set; } = "info_card"; // info_card, definition, threshold
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? IconName { get; set; }
    public string? ColorClass { get; set; } // red, orange, yellow, green, blue, purple, teal, gray
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public int DisplayOrder { get; set; }
    public List<HelpTagDto> Tags { get; set; } = new();
}

public class HelpTagDto
{
    public int Id { get; set; }
    public int HelpItemId { get; set; }
    public string TagText { get; set; } = "";
    public string? TagType { get; set; }
    public int DisplayOrder { get; set; }
}