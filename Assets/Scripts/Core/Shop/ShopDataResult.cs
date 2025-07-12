using System;
using YG.Utils.Pay;

namespace Core.Shop
{
    public class ShopDataResult
    {
        public bool IsPurchased;
        public Purchase ProductInfo;
        public bool IsError;
        public string ErrorMessage;
    }

    public interface IShopDataProvider
    {
        void FetchShopData(string productId, Action<ShopDataResult> onComplete);
    }
}