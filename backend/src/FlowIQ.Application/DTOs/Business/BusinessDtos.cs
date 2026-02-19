namespace FlowIQ.Application.DTOs.Business;

public record CreateBusinessRequest(string Name, string? Description, string? Category, string? Address);

public record UpdateBusinessRequest(string Name, string? Description, string? Category, string? Address);

public record BusinessResponse(
    Guid Id,
    string Name,
    string? Description,
    string? Category,
    string? Address,
    Guid UserId,
    DateTime CreatedAt);
