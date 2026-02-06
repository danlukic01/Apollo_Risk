using AACS.Risk.Web.Models;

namespace AACS.Risk.Web.Services;

public interface IRiskService
{
    // ============================================================================
    // REFERENCE DATA
    // ============================================================================
    Task<List<DomainDto>> GetAllDomainsAsync();
    Task<List<ServiceDto>> GetServicesAsync();
    Task<List<SiteDto>> GetAllSitesAsync();
    Task<List<SiteDto>> GetSitesAsync();
    Task<List<UserDto>> GetOwnersAsync();
    Task<List<UserDto>> GetUsersAsync();
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<List<RoleDto>> GetRolesAsync();
    Task<List<PortfolioDto>> GetPortfoliosAsync();
    Task<List<RiskCategoryDto>> GetCategoriesAsync(int? serviceId = null);
    Task<List<string>> GetRiskTypesAsync();

    // ============================================================================
    // CRUD - DOMAINS
    // ============================================================================
    Task<DomainDto?> CreateDomainAsync(DomainDto domain);
    Task<bool> UpdateDomainAsync(DomainDto domain);
    Task<bool> DeleteDomainAsync(int id);

    // ============================================================================
    // CRUD - SERVICES
    // ============================================================================
    Task<ServiceDto?> CreateServiceAsync(ServiceDto service);
    Task<bool> UpdateServiceAsync(ServiceDto service);
    Task<bool> DeleteServiceAsync(int id);

    // ============================================================================
    // CRUD - SITES
    // ============================================================================
    Task<SiteDto?> CreateSiteAsync(SiteDto site);
    Task<bool> UpdateSiteAsync(SiteDto site);
    Task<bool> DeleteSiteAsync(int id);

    // ============================================================================
    // CRUD - USERS
    // ============================================================================
    Task<UserDto?> CreateUserAsync(UserDto user);
    Task<bool> UpdateUserAsync(UserDto user);
    Task<bool> DeleteUserAsync(int id);

    // ============================================================================
    // CRUD - RISK CATEGORIES
    // ============================================================================
    Task<RiskCategoryDto?> CreateRiskCategoryAsync(RiskCategoryDto category);
    Task<bool> UpdateRiskCategoryAsync(RiskCategoryDto category);
    Task<bool> DeleteRiskCategoryAsync(int id);

    // ============================================================================
    // TR RISKS
    // ============================================================================
    Task<List<TRRiskDto>> GetAllTRRisksAsync(int? siteId = null, int? serviceId = null, int? categoryId = null, int? ownerId = null);
    Task<TRRiskDto?> GetTRRiskByIdAsync(int id);
    Task<TRRiskDto?> CreateTRRiskAsync(CreateTRRiskRequest request);
    Task<bool> UpdateTRRiskAsync(UpdateTRRiskRequest request);
    Task<bool> DeleteTRRiskAsync(int id);

    // ============================================================================
    // TR RISK SCORES
    // ============================================================================
    Task<List<TRRiskScoreDto>> GetTRRiskScoresAsync(int trRiskId);
    Task<TRRiskScoreDto?> AddTRRiskScoreAsync(AddRiskScoreRequest request);

    // ============================================================================
    // TR RISK THRESHOLDS
    // ============================================================================
    Task<List<TRRiskThresholdDto>> GetTRRiskThresholdsAsync(int? categoryId = null, int? siteId = null, int? ownerId = null, int? serviceId = null);
    Task<TRRiskThresholdDto?> CreateTRRiskThresholdAsync(CreateThresholdRequest request);
    Task<bool> UpdateTRRiskThresholdAsync(TRRiskThresholdDto threshold);
    Task<bool> DeleteTRRiskThresholdAsync(int id);

    // ============================================================================
    // DASHBOARD DATA
    // ============================================================================
    Task<RiskDashboardSummaryDto> GetDashboardSummaryAsync(int? siteId = null, int? serviceId = null);
    Task<List<TopRiskDto>> GetTopRisksAsync(int count = 10, int? siteId = null, int? serviceId = null);
    Task<List<WatchlistItemDto>> GetWatchlistAsync(int? siteId = null, int? serviceId = null);
    Task<decimal> GetAggregateRiskScoreAsync(int? siteId = null, int? serviceId = null);
    Task<decimal> GetAverageRiskScoreAsync(int? siteId = null, int? serviceId = null);
    Task<string> GetTrendDirectionAsync(int? siteId = null, int? serviceId = null);
    Task<decimal?> GetTrendValueAsync(int? siteId = null, int? serviceId = null);
    Task<List<RiskTrendDataDto>> GetRiskTrendChartDataAsync(int months = 12, int? siteId = null, int? serviceId = null);

    // ============================================================================
    // SUMMARY REPORTS
    // ============================================================================
    Task<List<RiskBySiteSummaryDto>> GetRiskBySiteSummaryAsync(int? serviceId = null);
    Task<List<RiskByCategorySummaryDto>> GetRiskByCategorySummaryAsync(int? siteId = null, int? serviceId = null);
    Task<List<RiskByOwnerSummaryDto>> GetRiskByOwnerSummaryAsync(int? siteId = null, int? serviceId = null);

    // ============================================================================
    // VARIANCE REPORT
    // ============================================================================
    Task<List<VarianceReportItemDto>> GetVarianceReportAsync(int? siteId = null, int? serviceId = null, int? categoryId = null, DateTime? startDate = null, DateTime? endDate = null);

    // ============================================================================
    // JOURNAL NOTES
    // ============================================================================
    Task<List<JournalNoteDto>> GetRecentTRRiskJournalNotesAsync(int count = 10, int? trRiskId = null);
    Task<JournalNoteDto?> CreateJournalNoteAsync(CreateJournalNoteRequest request);
    Task<bool> DeleteJournalNoteAsync(int id);

    // ============================================================================
    // REMINDERS
    // ============================================================================
    Task<List<ReminderDto>> GetRemindersAsync(int? userId = null, bool includeExpired = false);
    Task<ReminderDto?> CreateReminderAsync(CreateReminderRequest request);
    Task<bool> DismissReminderAsync(int id);
    Task<bool> DeleteReminderAsync(int id);

    // ============================================================================
    // EMAIL ALERTS
    // ============================================================================
    Task<List<EmailAlertDto>> GetAlertsAsync(int count = 20, int? trRiskId = null);

    // ============================================================================
    // HELP SECTIONS
    // ============================================================================
    Task<HelpSectionDto?> GetHelpSectionByPageKeyAsync(string pageKey);
}