using System;
using System.Collections;
using System.Linq;
using PlayablesStudio.Plugins.YandexGamesSDK.Runtime;
using UnityEngine;
using YandexGames.Types.IAP;

namespace Core.Shop
{
    public class YandexShopDataProvider : IShopDataProvider
    {
        private readonly MonoBehaviour _coroutineRunner;
        private const int MaxRetries = 5;
        private const float RetryDelay = 0.5f;

        public YandexShopDataProvider(MonoBehaviour coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
        }

        public void FetchShopData(string productId, Action<ShopDataResult> onComplete)
        {
            _coroutineRunner.StartCoroutine(FetchShopDataCoroutine(productId, onComplete));
        }

        private IEnumerator FetchShopDataCoroutine(string productId, Action<ShopDataResult> onComplete)
        {
            var sdk = YandexGamesSDK.Instance;
            if (sdk.Purchases == null)
            {
                onComplete?.Invoke(new ShopDataResult { IsError = true, ErrorMessage = "Модуль покупок не доступен." });
                yield break;
            }

            // --- Шаг 1: Проверяем купленные товары с механизмом повторных попыток ---
            var isPurchased = false;
            var purchaseCheckSuccess = false;
            for (var i = 0; i < MaxRetries; i++)
            {
                var isCheckDone = false;
                var isFatalError = false;
                var fatalErrorMessage = "";

                sdk.Purchases.GetPurchasedProducts((success, purchasedProductsResponse, error) =>
                {
                    if (success)
                    {
                        purchaseCheckSuccess = true;
                        if (purchasedProductsResponse?.purchasedProducts != null &&
                            purchasedProductsResponse.purchasedProducts.Any(p => p.productID == productId))
                        {
                            isPurchased = true;
                        }
                    }
                    else
                    {
                        if (error != null && error.Contains("is not initialized"))
                        {
                            Debug.LogWarning($"Попытка {i + 1}/{MaxRetries}: API покупок еще не готово. Повторная попытка...");
                        }
                        else
                        {
                            isFatalError = true;
                            fatalErrorMessage = error;
                        }
                    }

                    isCheckDone = true;
                });

                yield return new WaitUntil(() => isCheckDone);

                if (purchaseCheckSuccess) break;
                if (isFatalError)
                {
                    onComplete?.Invoke(new ShopDataResult { IsError = true, ErrorMessage = $"Фатальная ошибка при проверке покупок: {fatalErrorMessage}" });
                    yield break;
                }

                if (i < MaxRetries - 1) yield return new WaitForSeconds(RetryDelay);
            }

            if (!purchaseCheckSuccess)
            {
                onComplete?.Invoke(new ShopDataResult { IsError = true, ErrorMessage = "Не удалось получить список покупок после нескольких попыток." });
                yield break;
            }

            if (isPurchased)
            {
                onComplete?.Invoke(new ShopDataResult { IsPurchased = true });
                yield break;
            }

            var catalogLoadDone = false;
            YGProduct productInfo = null;
            var productNotFound = false;
            var catalogError = "";

            sdk.Purchases.GetProductCatalog((success, response, error) =>
            {
                if (success && response?.products != null)
                {
                    productInfo = response.products.FirstOrDefault(p => p.id == productId);
                    if (productInfo == null) productNotFound = true;
                }
                else
                {
                    catalogError = error;
                }

                catalogLoadDone = true;
            });

            yield return new WaitUntil(() => catalogLoadDone);

            if (!string.IsNullOrEmpty(catalogError))
            {
                onComplete?.Invoke(new ShopDataResult { IsError = true, ErrorMessage = $"Ошибка при загрузке каталога: {catalogError}" });
                yield break;
            }

            if (productNotFound)
            {
                onComplete?.Invoke(new ShopDataResult { IsError = true, ErrorMessage = $"Товар с ID '{productId}' не найден в каталоге." });
                yield break;
            }

            onComplete?.Invoke(new ShopDataResult { IsPurchased = false, ProductInfo = productInfo });
        }
    }
}