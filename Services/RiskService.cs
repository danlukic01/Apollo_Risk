using System.Data;
using Dapper;
using AACS.Risk.Web.Models;

namespace AACS.Risk.Web.Services;

public class RiskService : IRiskService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<RiskService> _logger;

    public RiskService(ISqlConnectionFactory connectionFactory, ILogger<RiskService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    private IDbConnection CreateConnection() => _connectionFactory.CreateConnection();

    // ============================================================================
    // REFERENCE DATA
    // ============================================================================

    public async Task<List<DomainDto>> GetAllDomainsAsync()
    {
        try
        {
            using var db = CreateConnection();
            const string sql = "SELECT Id, Name FROM Domains ORDER BY Name";
            var result = await db.QueryAsync<DomainDto>(sql);
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting domains");
            return new();
        }
    }

    public async Task<List<ServiceDto>> GetServicesAsync()
    {
        try
        {
            using var db = CreateConnection();
            const string sql = "SELECT Id, Name FROM Services ORDER BY Name";
            var result = await db.QueryAsync<ServiceDto>(sql);
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting services");
            return new();
        }
    }

    public async Task<List<SiteDto>> GetAllSitesAsync()
    {
        try
        {
            using var db = CreateConnection();
            const string sql = @"
                SELECT s.Id, s.Name, s.DomainId, d.Name AS DomainName
                FROM Sites s
                LEFT JOIN Domains d ON s.DomainId = d.Id
                ORDER BY s.Name";
            var result = await db.QueryAsync<SiteDto>(sql);
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sites");
            return new();
        }
    }

    public async Task<List<SiteDto>> GetSitesAsync() => await GetAllSitesAsync();

    public async Task<List<UserDto>> GetOwnersAsync()
    {
        try
        {
            using var db = CreateConnection();
            const string sql = @"
                SELECT Id, Name, Email, RoleId, PortfolioId
                FROM Users 
                ORDER BY Name";
            var result = await db.QueryAsync<UserDto>(sql);
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting owners");
            return new();
        }
    }

    public async Task<List<UserDto>> GetUsersAsync()
    {
        try
        {
            using var db = CreateConnection();
            const string sql = @"
                SELECT u.Id, u.Name, u.Email, u.RoleId, r.Name AS RoleName, u.PortfolioId
                FROM Users u
                LEFT JOIN Roles r ON u.RoleId = r.Id
                ORDER BY u.Name";
            var result = await db.QueryAsync<UserDto>(sql);
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return new();
        }
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = @"
                SELECT u.Id, u.Name, u.Email, u.RoleId, r.Name AS RoleName, u.PortfolioId
                FROM Users u
                LEFT JOIN Roles r ON u.RoleId = r.Id
                WHERE u.Email = @Email";
            return await db.QueryFirstOrDefaultAsync<UserDto>(sql, new { Email = email });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            return null;
        }
    }

    public async Task<List<RoleDto>> GetRolesAsync()
    {
        try
        {
            using var db = CreateConnection();
            const string sql = "SELECT Id, Name, Description FROM Roles ORDER BY Name";
            var result = await db.QueryAsync<RoleDto>(sql);
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles");
            return new();
        }
    }

    public async Task<List<PortfolioDto>> GetPortfoliosAsync()
    {
        try
        {
            using var db = CreateConnection();
            const string sql = "SELECT PortfolioId AS Id, Name FROM Portfolios ORDER BY Name";
            var result = await db.QueryAsync<PortfolioDto>(sql);
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting portfolios");
            return new();
        }
    }

    public async Task<List<RiskCategoryDto>> GetCategoriesAsync(int? serviceId = null)
    {
        try
        {
            using var db = CreateConnection();
            var sql = @"
                SELECT rc.Id, rc.Name, rc.Description,
                       rc.DomainId, d.Name AS DomainName, rc.RiskType, rc.RatingType,
                       rc.ServiceId, s.Name AS ServiceName
                FROM RiskCategories rc
                LEFT JOIN Domains d ON rc.DomainId = d.Id
                LEFT JOIN Services s ON rc.ServiceId = s.Id
                WHERE 1=1";

            if (serviceId.HasValue)
                sql += " AND rc.ServiceId = @ServiceId";

            sql += " ORDER BY rc.Name";

            var result = await db.QueryAsync<RiskCategoryDto>(sql, new { ServiceId = serviceId });
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting risk categories");
            return new();
        }
    }

    public async Task<List<string>> GetRiskTypesAsync()
    {
        try
        {
            using var db = CreateConnection();
            const string sql = "SELECT DISTINCT RiskType FROM RiskCategories WHERE RiskType IS NOT NULL ORDER BY RiskType";
            var result = await db.QueryAsync<string>(sql);
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting risk types");
            return new();
        }
    }

    // ============================================================================
    // CRUD - DOMAINS
    // ============================================================================

    public async Task<DomainDto?> CreateDomainAsync(DomainDto domain)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = @"
                INSERT INTO Domains (Name) 
                OUTPUT INSERTED.Id, INSERTED.Name
                VALUES (@Name)";
            return await db.QueryFirstOrDefaultAsync<DomainDto>(sql, domain);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating domain");
            return null;
        }
    }

    public async Task<bool> UpdateDomainAsync(DomainDto domain)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = "UPDATE Domains SET Name = @Name WHERE Id = @Id";
            var affected = await db.ExecuteAsync(sql, domain);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating domain");
            return false;
        }
    }

    public async Task<bool> DeleteDomainAsync(int id)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = "DELETE FROM Domains WHERE Id = @Id";
            var affected = await db.ExecuteAsync(sql, new { Id = id });
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting domain");
            return false;
        }
    }

    // ============================================================================
    // CRUD - SERVICES
    // ============================================================================

    public async Task<ServiceDto?> CreateServiceAsync(ServiceDto service)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = @"
                INSERT INTO Services (Name) 
                OUTPUT INSERTED.Id, INSERTED.Name
                VALUES (@Name)";
            return await db.QueryFirstOrDefaultAsync<ServiceDto>(sql, service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service");
            return null;
        }
    }

    public async Task<bool> UpdateServiceAsync(ServiceDto service)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = "UPDATE Services SET Name = @Name WHERE Id = @Id";
            var affected = await db.ExecuteAsync(sql, service);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service");
            return false;
        }
    }

    public async Task<bool> DeleteServiceAsync(int id)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = "DELETE FROM Services WHERE Id = @Id";
            var affected = await db.ExecuteAsync(sql, new { Id = id });
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting service");
            return false;
        }
    }

    // ============================================================================
    // CRUD - SITES
    // ============================================================================

    public async Task<SiteDto?> CreateSiteAsync(SiteDto site)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = @"
                INSERT INTO Sites (Name, DomainId) 
                OUTPUT INSERTED.Id, INSERTED.Name, INSERTED.DomainId
                VALUES (@Name, @DomainId)";
            return await db.QueryFirstOrDefaultAsync<SiteDto>(sql, site);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating site");
            return null;
        }
    }

    public async Task<bool> UpdateSiteAsync(SiteDto site)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = "UPDATE Sites SET Name = @Name, DomainId = @DomainId WHERE Id = @Id";
            var affected = await db.ExecuteAsync(sql, site);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating site");
            return false;
        }
    }

    public async Task<bool> DeleteSiteAsync(int id)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = "DELETE FROM Sites WHERE Id = @Id";
            var affected = await db.ExecuteAsync(sql, new { Id = id });
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting site");
            return false;
        }
    }

    // ============================================================================
    // CRUD - USERS
    // ============================================================================

    public async Task<UserDto?> CreateUserAsync(UserDto user)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = @"
                INSERT INTO Users (Name, Email, RoleId, PortfolioId) 
                OUTPUT INSERTED.Id, INSERTED.Name, INSERTED.Email, INSERTED.RoleId, INSERTED.PortfolioId
                VALUES (@Name, @Email, @RoleId, @PortfolioId)";
            return await db.QueryFirstOrDefaultAsync<UserDto>(sql, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return null;
        }
    }

    public async Task<bool> UpdateUserAsync(UserDto user)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = "UPDATE Users SET Name = @Name, Email = @Email, RoleId = @RoleId, PortfolioId = @PortfolioId WHERE Id = @Id";
            var affected = await db.ExecuteAsync(sql, user);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user");
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = "DELETE FROM Users WHERE Id = @Id";
            var affected = await db.ExecuteAsync(sql, new { Id = id });
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user");
            return false;
        }
    }

    // ============================================================================
    // CRUD - RISK CATEGORIES
    // ============================================================================

    public async Task<RiskCategoryDto?> CreateRiskCategoryAsync(RiskCategoryDto category)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = @"
                INSERT INTO RiskCategories (Name, Description, DomainId, RiskType, RatingType, ServiceId) 
                OUTPUT INSERTED.Id, INSERTED.Name, INSERTED.Description, 
                       INSERTED.DomainId, INSERTED.RiskType, INSERTED.RatingType, INSERTED.ServiceId
                VALUES (@Name, @Description, @DomainId, @RiskType, @RatingType, @ServiceId)";
            return await db.QueryFirstOrDefaultAsync<RiskCategoryDto>(sql, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating risk category");
            return null;
        }
    }

    public async Task<bool> UpdateRiskCategoryAsync(RiskCategoryDto category)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = @"
                UPDATE RiskCategories 
                SET Name = @Name, Description = @Description, DomainId = @DomainId, 
                    RiskType = @RiskType, RatingType = @RatingType, ServiceId = @ServiceId 
                WHERE Id = @Id";
            var affected = await db.ExecuteAsync(sql, category);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating risk category");
            return false;
        }
    }

    public async Task<bool> DeleteRiskCategoryAsync(int id)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = "DELETE FROM RiskCategories WHERE Id = @Id";
            var affected = await db.ExecuteAsync(sql, new { Id = id });
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting risk category");
            return false;
        }
    }

    // ============================================================================
    // TR RISKS
    // ============================================================================

    public async Task<List<TRRiskDto>> GetAllTRRisksAsync(int? siteId = null, int? serviceId = null, int? categoryId = null, int? ownerId = null)
    {
        try
        {
            using var db = CreateConnection();
            var sql = @"
                SELECT r.TRRiskId, r.Name, r.RiskCategoryId, 
                       rc.Name AS RiskCategoryName, r.SiteId, s.Name AS SiteName,
                       r.OwnerId, u.Name AS OwnerName, r.ServiceId, svc.Name AS ServiceName,
                       r.RAGRating, r.Status, r.LastUpdated, r.PortfolioId,
                       r.RatingType,
                       (SELECT TOP 1 NumericScore FROM TRRiskScores WHERE TRRiskId = r.TRRiskId ORDER BY RatingDate DESC) AS CurrentScore
                FROM TRRisks r
                LEFT JOIN RiskCategories rc ON r.RiskCategoryId = rc.Id
                LEFT JOIN Sites s ON r.SiteId = s.Id
                LEFT JOIN Users u ON r.OwnerId = u.Id
                LEFT JOIN Services svc ON r.ServiceId = svc.Id
                WHERE 1=1";

            if (siteId.HasValue) sql += " AND r.SiteId = @SiteId";
            if (serviceId.HasValue) sql += " AND r.ServiceId = @ServiceId";
            if (categoryId.HasValue) sql += " AND r.RiskCategoryId = @CategoryId";
            if (ownerId.HasValue) sql += " AND r.OwnerId = @OwnerId";

            sql += " ORDER BY r.Name";

            var result = await db.QueryAsync<TRRiskDto>(sql, new { SiteId = siteId, ServiceId = serviceId, CategoryId = categoryId, OwnerId = ownerId });
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting TR risks");
            return new();
        }
    }

    public async Task<TRRiskDto?> GetTRRiskByIdAsync(int id)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = @"
                SELECT r.TRRiskId, r.Name, r.RiskCategoryId, 
                       rc.Name AS RiskCategoryName, r.SiteId, s.Name AS SiteName,
                       r.OwnerId, u.Name AS OwnerName, r.ServiceId, svc.Name AS ServiceName,
                       r.RAGRating, r.Status, r.LastUpdated, r.PortfolioId,
                       r.RatingType,
                       (SELECT TOP 1 NumericScore FROM TRRiskScores WHERE TRRiskId = r.TRRiskId ORDER BY RatingDate DESC) AS CurrentScore
                FROM TRRisks r
                LEFT JOIN RiskCategories rc ON r.RiskCategoryId = rc.Id
                LEFT JOIN Sites s ON r.SiteId = s.Id
                LEFT JOIN Users u ON r.OwnerId = u.Id
                LEFT JOIN Services svc ON r.ServiceId = svc.Id
                WHERE r.TRRiskId = @Id";
            return await db.QueryFirstOrDefaultAsync<TRRiskDto>(sql, new { Id = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting TR risk by ID: {Id}", id);
            return null;
        }
    }

    public async Task<TRRiskDto?> CreateTRRiskAsync(CreateTRRiskRequest request)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = @"
                INSERT INTO TRRisks (Name, RiskCategoryId, SiteId, OwnerId, ServiceId, RAGRating, Status, LastUpdated, PortfolioId, RatingType)
                OUTPUT INSERTED.TRRiskId
                VALUES (@Name, @RiskCategoryId, @SiteId, @OwnerId, @ServiceId, @RAGRating, @Status, GETDATE(), @PortfolioId, @RatingType)";

            var newId = await db.QueryFirstOrDefaultAsync<int>(sql, request);
            if (newId > 0)
                return await GetTRRiskByIdAsync(newId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating TR risk");
            return null;
        }
    }

    public async Task<bool> UpdateTRRiskAsync(UpdateTRRiskRequest request)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = @"
                UPDATE TRRisks 
                SET Name = @Name, RiskCategoryId = @RiskCategoryId,
                    SiteId = @SiteId, OwnerId = @OwnerId, ServiceId = @ServiceId,
                    RAGRating = @RAGRating, Status = @Status, LastUpdated = GETDATE()
                WHERE TRRiskId = @TRRiskId";
            var affected = await db.ExecuteAsync(sql, request);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating TR risk");
            return false;
        }
    }

    public async Task<bool> DeleteTRRiskAsync(int id)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = "DELETE FROM TRRisks WHERE TRRiskId = @Id";
            var affected = await db.ExecuteAsync(sql, new { Id = id });
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting TR risk");
            return false;
        }
    }

    // ============================================================================
    // TR RISK SCORES
    // ============================================================================

    public async Task<List<TRRiskScoreDto>> GetTRRiskScoresAsync(int trRiskId)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = @"
                SELECT Id, TRRiskId, RatingDate, RatingValue AS RAGRating, 
                       NumericScore AS Score, Notes, EnteredBy, 
                       EnteredAt, RatingType
                FROM TRRiskScores
                WHERE TRRiskId = @TRRiskId
                ORDER BY RatingDate DESC";
            var result = await db.QueryAsync<TRRiskScoreDto>(sql, new { TRRiskId = trRiskId });
            var scores = result.ToList();

            // Log the first few scores for debugging
            if (scores.Any())
            {
                _logger.LogDebug("GetTRRiskScoresAsync for TRRiskId={TRRiskId}: Found {Count} scores. Latest: Id={Id}, Date={Date}, RAG={RAG}",
                    trRiskId, scores.Count, scores[0].Id, scores[0].RatingDate, scores[0].RAGRating);
            }

            return scores;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting TR risk scores for TRRiskId: {TRRiskId}", trRiskId);
            return new();
        }
    }

    public async Task<TRRiskScoreDto?> AddTRRiskScoreAsync(AddRiskScoreRequest request)
    {
        _logger.LogInformation("AddTRRiskScoreAsync called - TRRiskId: {TRRiskId}, RAGRating: {RAGRating}, RatingDate: {RatingDate}",
            request.TRRiskId, request.RAGRating, request.RatingDate);

        try
        {
            using var db = CreateConnection();

            // Ensure we have a valid EnteredBy - use provided value or get first user as fallback
            var enteredBy = request.EnteredBy;
            if (enteredBy <= 0)
            {
                _logger.LogWarning("EnteredBy is invalid ({EnteredBy}), fetching fallback user", enteredBy);
                var fallbackUser = await db.QueryFirstOrDefaultAsync<int>("SELECT TOP 1 Id FROM Users ORDER BY Id");
                enteredBy = fallbackUser > 0 ? fallbackUser : 1;
                _logger.LogInformation("Using fallback EnteredBy: {EnteredBy}", enteredBy);
            }

            // Insert the score
            const string insertSql = @"
                INSERT INTO TRRiskScores (TRRiskId, RatingDate, RatingValue, NumericScore, Notes, EnteredBy, EnteredAt, RatingType)
                OUTPUT INSERTED.Id, INSERTED.TRRiskId, INSERTED.RatingDate, INSERTED.RatingValue AS RAGRating, 
                       INSERTED.NumericScore AS Score, INSERTED.Notes, INSERTED.EnteredBy, 
                       INSERTED.EnteredAt, INSERTED.RatingType
                VALUES (@TRRiskId, @RatingDate, @RAGRating, @Score, @Notes, @EnteredBy, GETDATE(), @RatingType)";

            _logger.LogDebug("Executing INSERT with params: TRRiskId={TRRiskId}, RatingDate={RatingDate}, RAGRating={RAGRating}, Score={Score}, EnteredBy={EnteredBy}",
                request.TRRiskId, request.RatingDate, request.RAGRating, request.Score, enteredBy);

            var result = await db.QueryFirstOrDefaultAsync<TRRiskScoreDto>(insertSql, new
            {
                request.TRRiskId,
                request.RatingDate,
                request.RAGRating,
                request.Score,
                request.Notes,
                EnteredBy = enteredBy,
                request.RatingType
            });

            if (result != null)
            {
                _logger.LogInformation("Score inserted successfully with Id: {Id}", result.Id);

                // Then update the TRRisks table with the latest RAG rating
                const string updateSql = @"
                    UPDATE TRRisks SET RAGRating = @RAGRating, LastUpdated = GETDATE() 
                    WHERE TRRiskId = @TRRiskId";
                var rowsAffected = await db.ExecuteAsync(updateSql, new { request.RAGRating, request.TRRiskId });
                _logger.LogInformation("TRRisks updated, rows affected: {RowsAffected}", rowsAffected);
            }
            else
            {
                _logger.LogWarning("INSERT returned null result for TRRiskId: {TRRiskId}", request.TRRiskId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding TR risk score for TRRiskId: {TRRiskId}. Message: {Message}",
                request.TRRiskId, ex.Message);
            return null;
        }
    }

    // ============================================================================
    // TR RISK THRESHOLDS
    // ============================================================================

    public async Task<List<TRRiskThresholdDto>> GetTRRiskThresholdsAsync(int? categoryId = null, int? siteId = null, int? ownerId = null, int? serviceId = null)
    {
        try
        {
            using var db = CreateConnection();
            var sql = @"
                SELECT t.Id, t.RiskCategoryId, rc.Name AS RiskCategoryName,
                       t.SiteId, s.Name AS SiteName, t.OwnerId, u.Name AS OwnerName,
                       t.BenchmarkValue, t.ServiceId, svc.Name AS ServiceName,
                       t.LowMax AS GreenMax, t.ModerateMax AS AmberMax,
                       t.RatingType, t.UpdatedAt AS LastModified
                FROM TRRiskThresholds t
                LEFT JOIN RiskCategories rc ON t.RiskCategoryId = rc.Id
                LEFT JOIN Sites s ON t.SiteId = s.Id
                LEFT JOIN Users u ON t.OwnerId = u.Id
                LEFT JOIN Services svc ON t.ServiceId = svc.Id
                WHERE 1=1";

            if (categoryId.HasValue) sql += " AND t.RiskCategoryId = @CategoryId";
            if (siteId.HasValue) sql += " AND t.SiteId = @SiteId";
            if (ownerId.HasValue) sql += " AND t.OwnerId = @OwnerId";
            if (serviceId.HasValue) sql += " AND t.ServiceId = @ServiceId";

            sql += " ORDER BY s.Name, rc.Name";

            var result = await db.QueryAsync<TRRiskThresholdDto>(sql, new { CategoryId = categoryId, SiteId = siteId, OwnerId = ownerId, ServiceId = serviceId });
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting TR risk thresholds");
            return new();
        }
    }

    public async Task<TRRiskThresholdDto?> CreateTRRiskThresholdAsync(CreateThresholdRequest request)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = @"
                INSERT INTO TRRiskThresholds (RiskCategoryId, SiteId, OwnerId, BenchmarkValue, ServiceId, LowMax, ModerateMax, UpdatedAt)
                OUTPUT INSERTED.Id, INSERTED.RiskCategoryId, INSERTED.SiteId, 
                       INSERTED.OwnerId, INSERTED.BenchmarkValue, INSERTED.ServiceId, 
                       INSERTED.LowMax AS GreenMax, INSERTED.ModerateMax AS AmberMax
                VALUES (@RiskCategoryId, @SiteId, @OwnerId, @BenchmarkValue, @ServiceId, @GreenMax, @AmberMax, GETDATE())";
            return await db.QueryFirstOrDefaultAsync<TRRiskThresholdDto>(sql, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating TR risk threshold");
            return null;
        }
    }

    public async Task<bool> UpdateTRRiskThresholdAsync(TRRiskThresholdDto threshold)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = @"
                UPDATE TRRiskThresholds 
                SET RiskCategoryId = @RiskCategoryId, SiteId = @SiteId, OwnerId = @OwnerId,
                    BenchmarkValue = @BenchmarkValue, ServiceId = @ServiceId, 
                    LowMax = @GreenMax, ModerateMax = @AmberMax, UpdatedAt = GETDATE()
                WHERE Id = @Id";
            var affected = await db.ExecuteAsync(sql, threshold);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating TR risk threshold");
            return false;
        }
    }

    public async Task<bool> DeleteTRRiskThresholdAsync(int id)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = "DELETE FROM TRRiskThresholds WHERE Id = @Id";
            var affected = await db.ExecuteAsync(sql, new { Id = id });
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting TR risk threshold");
            return false;
        }
    }

    // ============================================================================
    // DASHBOARD DATA
    // ============================================================================

    public async Task<RiskDashboardSummaryDto> GetDashboardSummaryAsync(int? siteId = null, int? serviceId = null)
    {
        try
        {
            using var db = CreateConnection();
            var whereClause = "WHERE 1=1";
            if (siteId.HasValue) whereClause += " AND r.SiteId = @SiteId";
            if (serviceId.HasValue) whereClause += " AND r.ServiceId = @ServiceId";

            var sql = $@"
                WITH LatestScores AS (
                    SELECT TRRiskId, NumericScore,
                           ROW_NUMBER() OVER (PARTITION BY TRRiskId ORDER BY RatingDate DESC) AS rn
                    FROM TRRiskScores
                )
                SELECT 
                    COUNT(*) AS TotalRisks,
                    SUM(CASE WHEN r.RAGRating IN ('Red', 'High', 'Extreme') THEN 1 ELSE 0 END) AS HighRiskCount,
                    SUM(CASE WHEN r.RAGRating IN ('Amber', 'Moderate', 'Medium') THEN 1 ELSE 0 END) AS MediumRiskCount,
                    SUM(CASE WHEN r.RAGRating IN ('Green', 'Low') THEN 1 ELSE 0 END) AS LowRiskCount,
                    ISNULL(AVG(ls.NumericScore), 0) AS AverageScore,
                    ISNULL(SUM(ls.NumericScore), 0) AS AggregateScore
                FROM TRRisks r
                LEFT JOIN LatestScores ls ON ls.TRRiskId = r.TRRiskId AND ls.rn = 1
                {whereClause}";

            var summary = await db.QueryFirstOrDefaultAsync<RiskDashboardSummaryDto>(sql, new { SiteId = siteId, ServiceId = serviceId });
            return summary ?? new RiskDashboardSummaryDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard summary");
            return new RiskDashboardSummaryDto();
        }
    }

    public async Task<List<TopRiskDto>> GetTopRisksAsync(int count = 10, int? siteId = null, int? serviceId = null)
    {
        try
        {
            using var db = CreateConnection();
            var whereClause = "WHERE 1=1";
            if (siteId.HasValue) whereClause += " AND r.SiteId = @SiteId";
            if (serviceId.HasValue) whereClause += " AND r.ServiceId = @ServiceId";

            var sql = $@"
                ;WITH LatestScores AS (
                    SELECT TRRiskId, NumericScore, RatingDate,
                           ROW_NUMBER() OVER (PARTITION BY TRRiskId ORDER BY RatingDate DESC) AS rn
                    FROM TRRiskScores
                ),
                PreviousScores AS (
                    SELECT TRRiskId, NumericScore,
                           ROW_NUMBER() OVER (PARTITION BY TRRiskId ORDER BY RatingDate DESC) AS rn
                    FROM TRRiskScores
                )
                SELECT TOP (@Count) 
                       r.TRRiskId,
                       r.Name,
                       s.Name AS SiteName, 
                       rc.Name AS CategoryName, 
                       u.Name AS OwnerName,
                       ISNULL(ls.NumericScore, 0) AS Score,
                       r.RAGRating,
                       CASE 
                           WHEN ps.NumericScore IS NOT NULL AND ls.NumericScore IS NOT NULL 
                           THEN ROUND(ls.NumericScore - ps.NumericScore, 1)
                           ELSE NULL 
                       END AS Variance,
                       CASE 
                           WHEN ps.NumericScore IS NULL OR ls.NumericScore IS NULL THEN 'stable'
                           WHEN ls.NumericScore > ps.NumericScore THEN 'up'
                           WHEN ls.NumericScore < ps.NumericScore THEN 'down'
                           ELSE 'stable'
                       END AS TrendDirection
                FROM TRRisks r
                LEFT JOIN Sites s ON r.SiteId = s.Id
                LEFT JOIN RiskCategories rc ON r.RiskCategoryId = rc.Id
                LEFT JOIN Users u ON r.OwnerId = u.Id
                LEFT JOIN LatestScores ls ON ls.TRRiskId = r.TRRiskId AND ls.rn = 1
                LEFT JOIN PreviousScores ps ON ps.TRRiskId = r.TRRiskId AND ps.rn = 2
                {whereClause}
                ORDER BY ls.NumericScore DESC";

            var result = await db.QueryAsync<TopRiskDto>(sql, new { Count = count, SiteId = siteId, ServiceId = serviceId });
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top risks");
            return new();
        }
    }

    public async Task<List<WatchlistItemDto>> GetWatchlistAsync(int? siteId = null, int? serviceId = null)
    {
        try
        {
            using var db = CreateConnection();
            var whereClause = "WHERE 1=1";
            if (siteId.HasValue) whereClause += " AND r.SiteId = @SiteId";
            if (serviceId.HasValue) whereClause += " AND r.ServiceId = @ServiceId";

            var sql = $@"
                ;WITH LatestScores AS (
                    SELECT TRRiskId, NumericScore,
                           ROW_NUMBER() OVER (PARTITION BY TRRiskId ORDER BY RatingDate DESC) AS rn
                    FROM TRRiskScores
                )
                SELECT w.TRRiskId, 
                       r.Name, 
                       ISNULL(ls.NumericScore, 0) AS Score,
                       r.RAGRating, 
                       s.Name AS SiteName
                FROM WatchlistItems w
                INNER JOIN TRRisks r ON w.TRRiskId = r.TRRiskId
                LEFT JOIN Sites s ON r.SiteId = s.Id
                LEFT JOIN LatestScores ls ON ls.TRRiskId = r.TRRiskId AND ls.rn = 1
                {whereClause}
                ORDER BY ls.NumericScore DESC";

            var result = await db.QueryAsync<WatchlistItemDto>(sql, new { SiteId = siteId, ServiceId = serviceId });
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting watchlist");
            return new();
        }
    }

    public async Task<decimal> GetAggregateRiskScoreAsync(int? siteId = null, int? serviceId = null)
    {
        try
        {
            using var db = CreateConnection();
            var whereClause = "WHERE 1=1";
            if (siteId.HasValue) whereClause += " AND SiteId = @SiteId";
            if (serviceId.HasValue) whereClause += " AND ServiceId = @ServiceId";

            var sql = $@"
                SELECT ISNULL(SUM(ISNULL((SELECT TOP 1 NumericScore FROM TRRiskScores WHERE TRRiskId = r.TRRiskId ORDER BY RatingDate DESC), 0)), 0) 
                FROM TRRisks r {whereClause}";
            return await db.QueryFirstOrDefaultAsync<decimal>(sql, new { SiteId = siteId, ServiceId = serviceId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting aggregate risk score");
            return 0;
        }
    }

    public async Task<decimal> GetAverageRiskScoreAsync(int? siteId = null, int? serviceId = null)
    {
        try
        {
            using var db = CreateConnection();
            var whereClause = "WHERE 1=1";
            if (siteId.HasValue) whereClause += " AND r.SiteId = @SiteId";
            if (serviceId.HasValue) whereClause += " AND r.ServiceId = @ServiceId";

            var sql = $@"
                WITH LatestScores AS (
                    SELECT TRRiskId, NumericScore,
                           ROW_NUMBER() OVER (PARTITION BY TRRiskId ORDER BY RatingDate DESC) AS rn
                    FROM TRRiskScores
                )
                SELECT ISNULL(AVG(ls.NumericScore), 0)
                FROM TRRisks r
                LEFT JOIN LatestScores ls ON ls.TRRiskId = r.TRRiskId AND ls.rn = 1
                {whereClause}";
            return await db.QueryFirstOrDefaultAsync<decimal>(sql, new { SiteId = siteId, ServiceId = serviceId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting average risk score");
            return 0;
        }
    }

    public async Task<string> GetTrendDirectionAsync(int? siteId = null, int? serviceId = null)
    {
        try
        {
            using var db = CreateConnection();
            var whereClause = "WHERE 1=1";
            if (siteId.HasValue) whereClause += " AND r.SiteId = @SiteId";
            if (serviceId.HasValue) whereClause += " AND r.ServiceId = @ServiceId";

            var sql = $@"
                WITH RecentScores AS (
                    SELECT AVG(s.NumericScore) AS AvgScore, MONTH(s.RatingDate) AS Month
                    FROM TRRiskScores s
                    INNER JOIN TRRisks r ON s.TRRiskId = r.TRRiskId
                    {whereClause}
                    AND s.RatingDate >= DATEADD(MONTH, -2, GETDATE())
                    GROUP BY MONTH(s.RatingDate)
                )
                SELECT CASE 
                    WHEN COUNT(*) < 2 THEN 'Stable'
                    WHEN MAX(AvgScore) = MIN(AvgScore) THEN 'Stable'
                    WHEN (SELECT TOP 1 AvgScore FROM RecentScores ORDER BY Month DESC) > 
                         (SELECT TOP 1 AvgScore FROM RecentScores ORDER BY Month ASC) THEN 'Up'
                    ELSE 'Down'
                END
                FROM RecentScores";

            return await db.QueryFirstOrDefaultAsync<string>(sql, new { SiteId = siteId, ServiceId = serviceId }) ?? "Stable";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trend direction");
            return "Stable";
        }
    }

    public async Task<decimal?> GetTrendValueAsync(int? siteId = null, int? serviceId = null)
    {
        try
        {
            using var db = CreateConnection();
            var whereClause = "WHERE 1=1";
            if (siteId.HasValue) whereClause += " AND r.SiteId = @SiteId";
            if (serviceId.HasValue) whereClause += " AND r.ServiceId = @ServiceId";

            var sql = $@"
                SELECT 
                    (SELECT ISNULL(AVG(s.NumericScore), 0) FROM TRRiskScores s 
                     INNER JOIN TRRisks r ON s.TRRiskId = r.TRRiskId
                     {whereClause} AND s.RatingDate >= DATEADD(MONTH, -1, GETDATE()))
                    -
                    (SELECT ISNULL(AVG(s.NumericScore), 0) FROM TRRiskScores s 
                     INNER JOIN TRRisks r ON s.TRRiskId = r.TRRiskId
                     {whereClause} AND s.RatingDate >= DATEADD(MONTH, -2, GETDATE()) 
                     AND s.RatingDate < DATEADD(MONTH, -1, GETDATE()))";

            return await db.QueryFirstOrDefaultAsync<decimal?>(sql, new { SiteId = siteId, ServiceId = serviceId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trend value");
            return null;
        }
    }

    public async Task<List<RiskTrendDataDto>> GetRiskTrendChartDataAsync(int months = 12, int? siteId = null, int? serviceId = null)
    {
        try
        {
            using var db = CreateConnection();
            var whereClause = "WHERE 1=1";
            if (siteId.HasValue) whereClause += " AND r.SiteId = @SiteId";
            if (serviceId.HasValue) whereClause += " AND r.ServiceId = @ServiceId";

            var sql = $@"
                SELECT 
                    DATEFROMPARTS(YEAR(s.RatingDate), MONTH(s.RatingDate), 1) AS Month,
                    AVG(s.NumericScore) AS AverageScore,
                    SUM(CASE WHEN s.RatingValue IN ('Red', 'High', 'Extreme') THEN 1 ELSE 0 END) AS HighRiskCount,
                    SUM(CASE WHEN s.RatingValue IN ('Amber', 'Moderate', 'Medium') THEN 1 ELSE 0 END) AS MediumRiskCount,
                    SUM(CASE WHEN s.RatingValue IN ('Green', 'Low') THEN 1 ELSE 0 END) AS LowRiskCount
                FROM TRRiskScores s
                INNER JOIN TRRisks r ON s.TRRiskId = r.TRRiskId
                {whereClause}
                AND s.RatingDate >= DATEADD(MONTH, -@Months, GETDATE())
                GROUP BY YEAR(s.RatingDate), MONTH(s.RatingDate)
                ORDER BY YEAR(s.RatingDate), MONTH(s.RatingDate)";

            var result = await db.QueryAsync<RiskTrendDataDto>(sql, new { Months = months, SiteId = siteId, ServiceId = serviceId });
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting risk trend chart data");
            return new();
        }
    }

    // ============================================================================
    // SUMMARY REPORTS
    // ============================================================================

    public async Task<List<RiskBySiteSummaryDto>> GetRiskBySiteSummaryAsync(int? serviceId = null)
    {
        try
        {
            using var db = CreateConnection();
            var whereClause = "WHERE 1=1";
            if (serviceId.HasValue) whereClause += " AND r.ServiceId = @ServiceId";

            var sql = $@"
                WITH LatestScores AS (
                    SELECT TRRiskId, NumericScore,
                           ROW_NUMBER() OVER (PARTITION BY TRRiskId ORDER BY RatingDate DESC) AS rn
                    FROM TRRiskScores
                )
                SELECT s.Id AS SiteId, s.Name AS SiteName,
                       COUNT(*) AS TotalRisks,
                       SUM(CASE WHEN r.RAGRating IN ('Red', 'High', 'Extreme') THEN 1 ELSE 0 END) AS HighRisk,
                       SUM(CASE WHEN r.RAGRating IN ('Amber', 'Moderate', 'Medium') THEN 1 ELSE 0 END) AS MediumRisk,
                       SUM(CASE WHEN r.RAGRating IN ('Green', 'Low') THEN 1 ELSE 0 END) AS LowRisk,
                       ISNULL(AVG(ls.NumericScore), 0) AS AverageScore
                FROM TRRisks r
                INNER JOIN Sites s ON r.SiteId = s.Id
                LEFT JOIN LatestScores ls ON ls.TRRiskId = r.TRRiskId AND ls.rn = 1
                {whereClause}
                GROUP BY s.Id, s.Name
                ORDER BY ISNULL(AVG(ls.NumericScore), 0) DESC";

            var result = await db.QueryAsync<RiskBySiteSummaryDto>(sql, new { ServiceId = serviceId });
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting risk by site summary");
            return new();
        }
    }

    public async Task<List<RiskByCategorySummaryDto>> GetRiskByCategorySummaryAsync(int? siteId = null, int? serviceId = null)
    {
        try
        {
            using var db = CreateConnection();
            var whereClause = "WHERE 1=1";
            if (siteId.HasValue) whereClause += " AND r.SiteId = @SiteId";
            if (serviceId.HasValue) whereClause += " AND r.ServiceId = @ServiceId";

            var sql = $@"
                WITH LatestScores AS (
                    SELECT TRRiskId, NumericScore,
                           ROW_NUMBER() OVER (PARTITION BY TRRiskId ORDER BY RatingDate DESC) AS rn
                    FROM TRRiskScores
                )
                SELECT rc.Id AS CategoryId, rc.Name AS CategoryName,
                       COUNT(*) AS TotalRisks,
                       SUM(CASE WHEN r.RAGRating IN ('Red', 'High', 'Extreme') THEN 1 ELSE 0 END) AS HighRisk,
                       SUM(CASE WHEN r.RAGRating IN ('Amber', 'Moderate', 'Medium') THEN 1 ELSE 0 END) AS MediumRisk,
                       SUM(CASE WHEN r.RAGRating IN ('Green', 'Low') THEN 1 ELSE 0 END) AS LowRisk,
                       ISNULL(AVG(ls.NumericScore), 0) AS AverageScore
                FROM TRRisks r
                INNER JOIN RiskCategories rc ON r.RiskCategoryId = rc.Id
                LEFT JOIN LatestScores ls ON ls.TRRiskId = r.TRRiskId AND ls.rn = 1
                {whereClause}
                GROUP BY rc.Id, rc.Name
                ORDER BY ISNULL(AVG(ls.NumericScore), 0) DESC";

            var result = await db.QueryAsync<RiskByCategorySummaryDto>(sql, new { SiteId = siteId, ServiceId = serviceId });
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting risk by category summary");
            return new();
        }
    }

    public async Task<List<RiskByOwnerSummaryDto>> GetRiskByOwnerSummaryAsync(int? siteId = null, int? serviceId = null)
    {
        try
        {
            using var db = CreateConnection();
            var whereClause = "WHERE 1=1";
            if (siteId.HasValue) whereClause += " AND r.SiteId = @SiteId";
            if (serviceId.HasValue) whereClause += " AND r.ServiceId = @ServiceId";

            var sql = $@"
                WITH LatestScores AS (
                    SELECT TRRiskId, NumericScore,
                           ROW_NUMBER() OVER (PARTITION BY TRRiskId ORDER BY RatingDate DESC) AS rn
                    FROM TRRiskScores
                )
                SELECT u.Id AS OwnerId, u.Name AS OwnerName,
                       COUNT(*) AS TotalRisks,
                       SUM(CASE WHEN r.RAGRating IN ('Red', 'High', 'Extreme') THEN 1 ELSE 0 END) AS HighRisk,
                       SUM(CASE WHEN r.RAGRating IN ('Amber', 'Moderate', 'Medium') THEN 1 ELSE 0 END) AS MediumRisk,
                       SUM(CASE WHEN r.RAGRating IN ('Green', 'Low') THEN 1 ELSE 0 END) AS LowRisk,
                       ISNULL(AVG(ls.NumericScore), 0) AS AverageScore
                FROM TRRisks r
                INNER JOIN Users u ON r.OwnerId = u.Id
                LEFT JOIN LatestScores ls ON ls.TRRiskId = r.TRRiskId AND ls.rn = 1
                {whereClause}
                GROUP BY u.Id, u.Name
                ORDER BY ISNULL(AVG(ls.NumericScore), 0) DESC";

            var result = await db.QueryAsync<RiskByOwnerSummaryDto>(sql, new { SiteId = siteId, ServiceId = serviceId });
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting risk by owner summary");
            return new();
        }
    }

    // ============================================================================
    // VARIANCE REPORT
    // ============================================================================

    public async Task<List<VarianceReportItemDto>> GetVarianceReportAsync(int? siteId = null, int? serviceId = null, int? categoryId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            using var db = CreateConnection();
            var start = startDate ?? DateTime.Now.AddMonths(-6);
            var end = endDate ?? DateTime.Now;

            var whereClause = "WHERE 1=1";
            if (siteId.HasValue) whereClause += " AND r.SiteId = @SiteId";
            if (serviceId.HasValue) whereClause += " AND r.ServiceId = @ServiceId";
            if (categoryId.HasValue) whereClause += " AND r.RiskCategoryId = @CategoryId";

            var sql = $@"
                WITH StartScores AS (
                    SELECT TRRiskId, NumericScore AS Score, RatingValue AS RAG, ROW_NUMBER() OVER (PARTITION BY TRRiskId ORDER BY RatingDate ASC) AS rn
                    FROM TRRiskScores WHERE RatingDate >= @StartDate AND RatingDate <= @EndDate
                ),
                EndScores AS (
                    SELECT TRRiskId, NumericScore AS Score, RatingValue AS RAG, ROW_NUMBER() OVER (PARTITION BY TRRiskId ORDER BY RatingDate DESC) AS rn
                    FROM TRRiskScores WHERE RatingDate >= @StartDate AND RatingDate <= @EndDate
                )
                SELECT r.TRRiskId AS RiskId, r.Name AS RiskName, s.Name AS SiteName, rc.Name AS CategoryName,
                       u.Name AS OwnerName,
                       ISNULL(ss.Score, (SELECT TOP 1 NumericScore FROM TRRiskScores WHERE TRRiskId = r.TRRiskId ORDER BY RatingDate DESC)) AS PreviousScore,
                       ISNULL(es.Score, (SELECT TOP 1 NumericScore FROM TRRiskScores WHERE TRRiskId = r.TRRiskId ORDER BY RatingDate DESC)) AS CurrentScore,
                       ISNULL(es.Score, 0) - ISNULL(ss.Score, 0) AS Variance,
                       CASE WHEN ISNULL(ss.Score, 0) = 0 THEN 0 
                            ELSE ((ISNULL(es.Score, 0) - ISNULL(ss.Score, 0)) / NULLIF(ss.Score, 0)) * 100 
                       END AS VariancePercent,
                       ISNULL(es.RAG, r.RAGRating) AS CurrentRAG,
                       r.LastUpdated AS LastUpdated
                FROM TRRisks r
                LEFT JOIN Sites s ON r.SiteId = s.Id
                LEFT JOIN RiskCategories rc ON r.RiskCategoryId = rc.Id
                LEFT JOIN Users u ON r.OwnerId = u.Id
                LEFT JOIN StartScores ss ON r.TRRiskId = ss.TRRiskId AND ss.rn = 1
                LEFT JOIN EndScores es ON r.TRRiskId = es.TRRiskId AND es.rn = 1
                {whereClause}
                ORDER BY ABS(ISNULL(es.Score, 0) - ISNULL(ss.Score, 0)) DESC";

            var result = await db.QueryAsync<VarianceReportItemDto>(sql, new
            {
                SiteId = siteId,
                ServiceId = serviceId,
                CategoryId = categoryId,
                StartDate = start,
                EndDate = end
            });
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting variance report");
            return new();
        }
    }

    // ============================================================================
    // JOURNAL NOTES
    // ============================================================================

    public async Task<List<JournalNoteDto>> GetRecentTRRiskJournalNotesAsync(int count = 10, int? trRiskId = null)
    {
        try
        {
            using var db = CreateConnection();
            var whereClause = "WHERE 1=1";
            if (trRiskId.HasValue) whereClause += " AND jn.TRRiskId = @TRRiskId";

            var sql = $@"
                SELECT TOP (@Count) jn.JournalNoteId, jn.TRRiskId, r.Name AS TRRiskName,
                       jn.FocalRiskId, jn.Note AS Title, jn.Note AS Body, jn.UserId, u.Name AS UserName,
                       jn.CreatedAt AS CreatedDate
                FROM JournalNotes jn
                LEFT JOIN TRRisks r ON jn.TRRiskId = r.TRRiskId
                LEFT JOIN Users u ON jn.UserId = u.Id
                {whereClause}
                ORDER BY jn.CreatedAt DESC";

            var result = await db.QueryAsync<JournalNoteDto>(sql, new { Count = count, TRRiskId = trRiskId });
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting journal notes");
            return new();
        }
    }

    public async Task<JournalNoteDto?> CreateJournalNoteAsync(CreateJournalNoteRequest request)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = @"
                INSERT INTO JournalNotes (TRRiskId, FocalRiskId, Note, UserId, CreatedAt)
                OUTPUT INSERTED.JournalNoteId, INSERTED.TRRiskId, INSERTED.FocalRiskId, 
                       INSERTED.Note AS Title, INSERTED.Note AS Body, INSERTED.UserId, INSERTED.CreatedAt AS CreatedDate
                VALUES (@TRRiskId, @FocalRiskId, @Body, @UserId, GETDATE())";
            return await db.QueryFirstOrDefaultAsync<JournalNoteDto>(sql, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating journal note");
            return null;
        }
    }

    public async Task<bool> DeleteJournalNoteAsync(int id)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = "DELETE FROM JournalNotes WHERE JournalNoteId = @Id";
            var affected = await db.ExecuteAsync(sql, new { Id = id });
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting journal note");
            return false;
        }
    }

    // ============================================================================
    // REMINDERS
    // ============================================================================

    public async Task<List<ReminderDto>> GetRemindersAsync(int? userId = null, bool includeExpired = false)
    {
        try
        {
            using var db = CreateConnection();
            var whereClause = "WHERE 1=1";
            if (userId.HasValue) whereClause += " AND rm.UserId = @UserId";
            if (!includeExpired) whereClause += " AND rm.DueDate >= GETDATE()";

            var sql = $@"
                SELECT rm.ReminderId AS Id, rm.TRRiskId, r.Name AS TRRiskName,
                       rm.FocalRiskId, rm.DueDate,
                       rm.UserId, u.Name AS UserName, rm.IsSent AS IsDismissed
                FROM Reminders rm
                LEFT JOIN TRRisks r ON rm.TRRiskId = r.TRRiskId
                LEFT JOIN Users u ON rm.UserId = u.Id
                {whereClause}
                ORDER BY rm.DueDate ASC";

            var result = await db.QueryAsync<ReminderDto>(sql, new { UserId = userId });
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reminders");
            return new();
        }
    }

    public async Task<ReminderDto?> CreateReminderAsync(CreateReminderRequest request)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = @"
                INSERT INTO Reminders (TRRiskId, FocalRiskId, DueDate, UserId, IsSent)
                OUTPUT INSERTED.ReminderId AS Id, INSERTED.TRRiskId, INSERTED.FocalRiskId, 
                       INSERTED.DueDate, INSERTED.UserId, INSERTED.IsSent AS IsDismissed
                VALUES (@TRRiskId, @FocalRiskId, @DueDate, @UserId, 0)";
            return await db.QueryFirstOrDefaultAsync<ReminderDto>(sql, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reminder");
            return null;
        }
    }

    public async Task<bool> DismissReminderAsync(int id)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = "UPDATE Reminders SET IsSent = 1, SentAt = GETDATE() WHERE ReminderId = @Id";
            var affected = await db.ExecuteAsync(sql, new { Id = id });
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dismissing reminder");
            return false;
        }
    }

    public async Task<bool> DeleteReminderAsync(int id)
    {
        try
        {
            using var db = CreateConnection();
            const string sql = "DELETE FROM Reminders WHERE ReminderId = @Id";
            var affected = await db.ExecuteAsync(sql, new { Id = id });
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting reminder");
            return false;
        }
    }

    // ============================================================================
    // EMAIL ALERTS
    // ============================================================================

    public async Task<List<EmailAlertDto>> GetAlertsAsync(int count = 20, int? trRiskId = null)
    {
        try
        {
            using var db = CreateConnection();
            var whereClause = "WHERE 1=1";
            if (trRiskId.HasValue) whereClause += " AND ea.TRRiskId = @TRRiskId";

            var sql = $@"
                SELECT TOP (@Count) ea.EmailAlertId AS Id, ea.TRRiskId, r.Name AS TRRiskName,
                       ea.Subject, ea.Message AS Body, ea.SentTo, ea.SentAt AS SentDate
                FROM EmailAlerts ea
                LEFT JOIN TRRisks r ON ea.TRRiskId = r.TRRiskId
                {whereClause}
                ORDER BY ea.SentAt DESC";

            var result = await db.QueryAsync<EmailAlertDto>(sql, new { Count = count, TRRiskId = trRiskId });
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting alerts");
            return new();
        }
    }

    // ============================================================================
    // HELP SECTIONS
    // ============================================================================

    public async Task<HelpSectionDto?> GetHelpSectionByPageKeyAsync(string pageKey)
    {
        try
        {
            using var db = CreateConnection();

            // Get the help section
            const string sectionSql = @"
                SELECT Id, PageKey, SectionTitle, Description, IsActive
                FROM HelpSections
                WHERE PageKey = @PageKey AND IsActive = 1";

            var section = await db.QueryFirstOrDefaultAsync<HelpSectionDto>(sectionSql, new { PageKey = pageKey });

            if (section == null) return null;

            // Get the help items
            const string itemsSql = @"
                SELECT Id, HelpSectionId, ItemType, Title, Description, 
                       IconName, ColorClass, MinValue, MaxValue, DisplayOrder
                FROM HelpItems
                WHERE HelpSectionId = @SectionId
                ORDER BY DisplayOrder";

            var items = await db.QueryAsync<HelpItemDto>(itemsSql, new { SectionId = section.Id });
            section.Items = items.ToList();

            // Get tags for each item
            if (section.Items.Any())
            {
                var itemIds = section.Items.Select(i => i.Id).ToList();
                const string tagsSql = @"
                    SELECT Id, HelpItemId, TagText, TagType, DisplayOrder
                    FROM HelpTags
                    WHERE HelpItemId IN @ItemIds
                    ORDER BY DisplayOrder";

                var tags = await db.QueryAsync<HelpTagDto>(tagsSql, new { ItemIds = itemIds });
                var tagsByItem = tags.GroupBy(t => t.HelpItemId).ToDictionary(g => g.Key, g => g.ToList());

                foreach (var item in section.Items)
                {
                    item.Tags = tagsByItem.TryGetValue(item.Id, out var itemTags) ? itemTags : new List<HelpTagDto>();
                }
            }

            return section;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting help section for page: {PageKey}", pageKey);
            return null;
        }
    }
}
