using System;
using YandexGames.Types.IAP;

namespace Core.Shop
{
    public class ShopDataResult
    {
        public bool IsPurchased;
        public YGProduct ProductInfo;
        public bool IsError;
        public string ErrorMessage;
    }

    public interface IShopDataProvider
    {
        void FetchShopData(string productId, Action<ShopDataResult> onComplete);
    }
}