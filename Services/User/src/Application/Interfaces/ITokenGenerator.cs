namespace User.Application.Interfaces;

public interface ITokenGenerator
{
    public string GenerateAccessToken(Domain.Entities.User user);
    public string GenerateRefreshToken();
}