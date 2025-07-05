using System;
using PlayablesStudio.Plugins.YandexGamesSDK.Runtime;
using UnityEngine;

namespace Core.Shop
{
    public class YandexPurchaseHandler : IPurchaseHandler
    {
        public void PurchaseProduct(string productId, Action<bool> onComplete)
        {
            var sdk = YandexGamesSDK.Instance;
            if (!sdk || sdk.Purchases == null)
            {
                Debug.LogError("SDK или модуль покупок не инициализирован.");
                onComplete?.Invoke(false);
                return;
            }

            sdk.Purchases.PurchaseProduct(productId, (success, purchaseData, error) =>
            {
                if (success && purchaseData != null)
                {
                    onComplete?.Invoke(true);
                }
                else
                {
                    Debug.LogError($"Ошибка покупки: {error}");
                    onComplete?.Invoke(false);
                }
            });
        }
    }
}