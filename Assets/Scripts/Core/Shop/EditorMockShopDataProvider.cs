using System;
using UnityEngine;

namespace Core.Shop
{
    public class EditorMockShopDataProvider : IShopDataProvider
    {
        public void FetchShopData(string productId, Action<ShopDataResult> onComplete)
        {
            Debug.LogWarning("ShopManager: Используется симуляция магазина (EditorMockShopDataProvider). Это ожидаемое поведение в редакторе.");
            var result = new ShopDataResult
            {
                IsError = true,
                ErrorMessage = "Тест в редакторе"
            };
            onComplete?.Invoke(result);
        }
    }
}