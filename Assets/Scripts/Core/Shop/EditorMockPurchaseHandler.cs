using System;
using UnityEngine;

namespace Core.Shop
{
    public class EditorMockPurchaseHandler : IPurchaseHandler
    {
        public void PurchaseProduct(string productId, Action<bool> onComplete)
        {
            Debug.LogWarning($"GameController: Симуляция покупки товара '{productId}' в редакторе.");
            onComplete?.Invoke(true);
        }
    }
}