using System;
using Core.Shop;
using YG;

namespace DataProviders
{
    public class YandexShopDataProvider : IShopDataProvider
    {
        public void FetchShopData(string productId, Action<ShopDataResult> onComplete)
        {
            if (YG2.purchases == null)
            {
                var errorResult = new ShopDataResult
                {
                    IsError = true,
                    ErrorMessage = "Магазин не инициализирован"
                };
                onComplete?.Invoke(errorResult);
                return;
            }

            var productInfo = YG2.PurchaseByID(productId);

            if (productInfo == null)
            {
                var errorResult = new ShopDataResult
                {
                    IsError = true,
                    ErrorMessage = $"Товар с ID '{productId}' не найден"
                };
                onComplete?.Invoke(errorResult);
                return;
            }

            var successResult = new ShopDataResult
            {
                IsPurchased = false,
                ProductInfo = productInfo,
                IsError = false
            };
            onComplete?.Invoke(successResult);
        }
    }
}