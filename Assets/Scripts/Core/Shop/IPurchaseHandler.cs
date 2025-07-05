using System;

namespace Core.Shop
{
    public interface IPurchaseHandler
    {
        void PurchaseProduct(string productId, Action<bool> onComplete);
    }
}