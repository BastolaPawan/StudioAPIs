using Shared.DTOs;

namespace Shared.Helpers.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(UserDto user);
    }
}