using Core.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;
using YG.Utils.Pay;

namespace Core.Shop
{
    /// <summary>
    /// Управляет UI элементами магазина, отображая информацию о товарах и их статусе.
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        [Header("UI Элементы")]
        [SerializeField] private Button purchaseButton;
        [SerializeField] private TextMeshProUGUI priceText;

        [Header("Настройки товара")]
        [SerializeField] private string productIdToDisplay = "disable_counters_product_id";

        [Header("Event Channels")]
        [SerializeField] private GameEvents gameEvents;

        private bool _isShopInitialized;
        private Purchase _productInfo;

        private void OnEnable()
        {
            if (gameEvents)
            {
                gameEvents.onYandexSDKInitialized.AddListener(Initialize);
            }

            YG2.onPurchaseSuccess += HandlePurchaseSuccess;
        }

        private void OnDisable()
        {
            if (gameEvents)
            {
                gameEvents.onYandexSDKInitialized.RemoveListener(Initialize);
            }

            YG2.onPurchaseSuccess -= HandlePurchaseSuccess;
        }

        /// <summary>
        /// Инициализирует магазин после загрузки Yandex SDK.
        /// </summary>
        private void Initialize()
        {
            if (_isShopInitialized) return;
            _isShopInitialized = true;

            if (purchaseButton)
                purchaseButton.interactable = false;
            if (priceText)
                priceText.text = "Загрузка...";

            _productInfo = YG2.PurchaseByID(productIdToDisplay);

            if (_productInfo == null)
            {
                if (priceText)
                    priceText.text = "Товар не найден";
                Debug.LogError($"ShopManager Error: Товар с ID '{productIdToDisplay}' не найден. Проверьте настройки в InfoYG -> Payments.");
                return;
            }

            UpdateProductUI();
        }

        /// <summary>
        /// Обновляет UI товара, проверяя статус покупки.
        /// </summary>
        private void UpdateProductUI()
        {
            if (_productInfo == null) return;
            if (_productInfo.consumed)
            {
                SetProductAsPurchased();
            }
            else
            {
                SetProductAsAvailable();
            }
        }

        /// <summary>
        /// Обрабатывает событие успешной покупки.
        /// </summary>
        /// <param name="purchasedId">ID купленного товара.</param>
        private void HandlePurchaseSuccess(string purchasedId)
        {
            if (purchasedId == productIdToDisplay)
            {
                UpdateProductUI();
            }
        }

        /// <summary>
        /// Обновляет UI, чтобы показать товар как доступный для покупки.
        /// </summary>
        private void SetProductAsAvailable()
        {
            if (priceText)
                priceText.text = _productInfo.price;

            if (purchaseButton)
                purchaseButton.interactable = true;
        }

        /// <summary>
        /// Обновляет UI, чтобы показать товар как купленный.
        /// </summary>
        private void SetProductAsPurchased()
        {
            if (priceText)
                priceText.text = "Куплен";

            if (purchaseButton)
            {
                purchaseButton.interactable = false;
            }
        }
    }
}