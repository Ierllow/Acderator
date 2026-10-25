using MagicOnion;

namespace Acderator.Shared
{
    public interface IAuthService : IService<IAuthService>
    {
        UnaryResult<LoginResponse> LoginAsync(LoginRequest request);
        UnaryResult<RegisterResponse> RegisterAsync(RegisterRequest request);
        UnaryResult<LogoutResponse> LogoutAsync(LogoutRequest request);
    }

    public interface IUserService : IService<IUserService>
    {
        UnaryResult<UserDataResponse> GetDataAsync(UserDataRequest request);
    }

    public interface IScoreService : IService<IScoreService>
    {
        UnaryResult<ScoreBeginResponse> BeginAsync(ScoreBeginRequest request);
        UnaryResult<ScoreSubmitResponse> SubmitAsync(ScoreSubmitRequest request);
    }

    public interface IChartService : IService<IChartService>
    {
        UnaryResult<ChartResponse> GetAsync(ChartRequest request);
    }
}
