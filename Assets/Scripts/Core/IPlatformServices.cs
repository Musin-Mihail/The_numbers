using System;

namespace Core
{
    public interface IPlatformServices
    {
        event Action<string> OnPurchaseSuccess;
        event Action<string> OnPurchaseFailed;
        event Action<string> OnRewardVideoSuccess;
        void Purchase(string productId);
        void ShowRewardedAd(string rewardId);
    }
    public interface ISaveLoadService
    {
        void RequestSave();
        void LoadGame(Action<bool> onComplete);
        void SetTopLineVisibility(bool isVisible);
    }
    public interface ILeaderboardService
    {
        void UpdateLeaderboard(int score);
    }
}