using System;
using Core.Shop;
using YG;

namespace Core
{
    public class YandexPurchaseHandler : IPurchaseHandler
    {
        public void PurchaseProduct(string productId, Action<bool> onComplete)
        {
            YG2.BuyPayments(productId);
        }
    }
}