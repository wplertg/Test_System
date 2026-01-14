using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public interface ICurrentUser
{
    Guid UserId { get; }
    string UserName { get; }
    string Role { get; }
}

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _httpContextAccessor = accessor;
    }

    public Guid UserId =>
        Guid.Parse(
            _httpContextAccessor.HttpContext!
                .User.FindFirstValue(JwtRegisteredClaimNames.Sub)!
        );

    public string UserName =>
        _httpContextAccessor.HttpContext!
            .User.FindFirstValue(JwtRegisteredClaimNames.UniqueName) ?? "";

    public string Role =>
        _httpContextAccessor.HttpContext!
            .User.FindFirstValue(ClaimTypes.Role) ?? "";
}
