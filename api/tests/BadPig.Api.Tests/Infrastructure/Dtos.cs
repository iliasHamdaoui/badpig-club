namespace BadPig.Api.Tests.Infrastructure;

// What the tests read from the answers of the API. They are written from the public contract (the JSON), never from the
// API's own types, so a test fails when the contract changes, not when the code is reorganised.

public sealed record IdDto(string Id);

public sealed record PageDto<T>(List<T> Items, int Page, int PageSize, int TotalCount, int TotalPages);

public sealed record ProfileDto(string Id, string Name, string Email, string[] Groups, string Membership, bool MembershipUpToDate, string? Title,
    string? Licence, DateTimeOffset? MemberSince);

public sealed record SeasonDto(string Id, string Name, string StartsOn, string EndsOn, int GraceDays, decimal InternalFee, decimal ExternalFee, bool IsCurrent);

public sealed record HolidayDto(string Id, string Name, string StartsOn, string EndsOn);

public sealed record ClubSessionDayDto(string Day, string StartTime, string EndTime);

public sealed record ClubSessionToComeDto(string Date, string StartTime, string EndTime, bool IsToday);

public sealed record ClubClosureDto(string Name, string EndsOn);

public sealed record ClubSessionsDto(string Today, List<ClubSessionDayDto> Weekly, bool IsOpenToday, ClubClosureDto? Closure, List<ClubSessionToComeDto> Next);

public sealed record MailboxDto(bool IsAvailable, int UnreadCount, string? WebmailUrl, DateTimeOffset? CheckedAt);

public sealed record SeasonStatisticsDto(string SeasonId, string Name, int InternalCount, int ExternalCount, int TotalCount, decimal InternalRevenue, decimal ExternalRevenue, decimal TotalRevenue);

/// Read from /club/board.
public sealed record BoardMemberDto(string AccountId, string Name, string Email, string? Title);

public sealed record UserDto(
    string Id,
    string Name,
    string Email,
    bool IsActive,
    string[] Groups,
    bool CanManage,
    string Status,
    string? BoardAction,
    string Membership,
    string? MembershipSeason,
    string MembershipKind,
    decimal? MembershipAmount,
    string? MembershipValidatedByName,
    string? Licence,
    string? TitleId,
    string? TitleLabel);

public sealed record TitleDto(string Id, string Label, string? HolderId, string? HolderName);

public sealed record AuditEntryDto(string Actor, string Action, string? TargetId);

public sealed record AnnouncementItemDto(
    string Id,
    string Title,
    string Summary,
    string Kind,
    bool IsPublished,
    bool IsPinned,
    DateTimeOffset? PublishedAt,
    string? PosterId,
    int AttachmentCount,
    int? LikeCount,
    bool LikedByMe,
    string? AuthorName,
    DateOnly ExpiresOn,
    bool IsExpired,
    string Form);

public sealed record AnnouncementFileDto(string Id, string FileName, string ContentType, long SizeBytes, bool IsPoster);

public sealed record AnnouncementDto(
    string Id,
    string Title,
    string BodyHtml,
    string Kind,
    string Form,
    bool IsPublished,
    bool IsPinned,
    DateTimeOffset? PublishedAt,
    List<AnnouncementFileDto> Files,
    int? LikeCount,
    bool LikedByMe,
    string? AuthorName,
    DateOnly ExpiresOn,
    bool IsExpired,
    bool CommentsEnabled,
    PollDto? Poll);

public sealed record PollOptionDto(string Id, string Label, int? VoteCount, List<string>? Voters);

public sealed record PollDto(
    bool AllowMultiple,
    bool IsAnonymous,
    DateOnly ClosesOn,
    bool IsClosed,
    bool ResultsVisible,
    bool CanVote,
    int? ParticipantCount,
    List<string> MyOptionIds,
    List<PollOptionDto> Options);

public sealed record QuoteDto(string Id, string? AuthorName, string Excerpt, bool IsDeleted);

public sealed record CommentDto(string Id, string AuthorName, string Text, DateTimeOffset CreatedAt, bool IsMine, bool CanDelete, QuoteDto? ReplyTo);

public sealed record ProductVariantDto(string Id, string Label, int PriceCents, int Quantity, int Reserved, int Available, int LowStockThreshold, bool IsLowStock, bool IsActive);

public sealed record ProductItemDto(string Id, string Name, string Description, bool IsActive, bool HasLowStock, DateTimeOffset CreatedAt, long? ImageVersion, List<ProductVariantDto> Variants);

public sealed record StockMovementDto(string Id, int Change, string Reason, string Note, string AuthorName, string? OrderId, DateTimeOffset CreatedAt);

public sealed record CatalogVariantDto(string Id, string Label, int PriceCents, int Available, bool InStock);

public sealed record CatalogItemDto(string Id, string Name, string Description, long? ImageVersion, List<CatalogVariantDto> Variants);

public sealed record OrderLineDto(string ProductName, string VariantLabel, int UnitPriceCents, int Quantity);

public sealed record OrderDto(string Id, string Status, int TotalCents, DateTimeOffset CreatedAt, DateTimeOffset ExpiresAt, DateTimeOffset? PaidAt, DateTimeOffset? DeliveredAt, string? CheckoutUrl, List<OrderLineDto> Lines);

public sealed record MyOrderItemDto(string Id, string Status, int TotalCents, DateTimeOffset CreatedAt, int ItemCount);

public sealed record BoardOrderDto(string Id, string BuyerName, string Status, int TotalCents, DateTimeOffset CreatedAt, DateTimeOffset? PaidAt, DateTimeOffset? DeliveredAt, string? DeliveredByName, List<OrderLineDto> Lines);

public sealed record ShopAuditEntryDto(long Id, DateTimeOffset OccurredAt, string Actor, string Action, string? TargetId, string? Subject, string? Details);

public sealed record TeamItemDto(string Id, string Name, string SeasonId, string Season, string? CaptainId, string? CaptainName, bool IsMyTeam, int MemberCount, bool IsPlayer);

public sealed record InterclubSeasonDto(string Id, string Name, bool IsCurrent, int TeamCount);

public sealed record FixtureItemDto(string Id, string TeamId, string TeamName, string Date, string? StartTime, string Opponent, string Venue, string? Place,
    int? ClubScore, int? OpponentScore, int DecidedMatches, int ClubMatches, int OpponentMatches, bool IsLive);

public sealed record FixtureDto(string Id, string TeamId, string TeamName, string Season, string? CaptainId, string? CaptainName, string Date, string? StartTime,
    string Opponent, string Venue, string? Place, int? ClubScore, int? OpponentScore, List<MatchDto> Matches, bool CanManage);

public sealed record ProposalAnswerDto(string AccountId, string Name, string Answer);

public sealed record ProposalDateDto(string Id, string Date, string? StartTime, int Yes, int Maybe, int No, string? MyAnswer, List<ProposalAnswerDto>? Answers);

public sealed record ProposalDto(string Id, string TeamId, string TeamName, string Opponent, string Venue, string? Place, bool IsOpen, string? FixtureId, bool CanAnswer, bool CanManage,
    List<ProposalDateDto> Dates);

public sealed record TeamMemberDto(string AccountId, string Name, bool IsCaptain);

public sealed record TeamCandidateDto(string AccountId, string Name);

public sealed record PlayerDto(string AccountId, string Name);

public sealed record SetDto(int Club, int Opponent);

public sealed record MatchDto(string Slot, List<PlayerDto> Players, List<SetDto> Sets, string? Ending, string? Withdrawn, string? Outcome);
