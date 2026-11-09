using Race.MVC.Models;

namespace Race.MVC.Services
{
    public interface IApiClient
    {
        Task<ApiResult<T>> GetAsync<T>(string url);
        Task<ApiResult> PostAsync(string url);
        Task<ApiResult> PutAsync(string url);
        Task<ApiResult> DeleteAsync(string url);

        Task<ApiResult<LoginResponseDto>> LoginAsync(string email, string password);
        Task LogoutAsync();
    }
}
