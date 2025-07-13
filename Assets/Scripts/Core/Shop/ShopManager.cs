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
        private bool _isPurchased;
        private Purchase _productInfo;

        private void OnEnable()
        {
            if (!gameEvents) return;
            gameEvents.onCountersChanged.AddListener(HandleCountersChanged);
            gameEvents.onYandexSDKInitialized.AddListener(Initialize);
        }

        private void OnDisable()
        {
            if (!gameEvents) return;
            gameEvents.onCountersChanged.RemoveListener(HandleCountersChanged);
            gameEvents.onYandexSDKInitialized.RemoveListener(Initialize);
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
        /// Обновляет UI товара (цена, доступность кнопки).
        /// </summary>
        private void UpdateProductUI()
        {
            if (_isPurchased || _productInfo == null) return;

            if (priceText)
                priceText.text = _productInfo.price;

            if (purchaseButton)
                purchaseButton.interactable = true;
        }

        /// <summary>
        /// Обрабатывает изменение счетчиков, чтобы определить, был ли куплен товар.
        /// </summary>
        private void HandleCountersChanged((int undo, int add, int hint) data)
        {
            if (data.undo != -1 || _isPurchased) return;
            _isPurchased = true;
            SetProductAsPurchased();
        }

        /// <summary>
        /// Обновляет UI, чтобы показать товар как купленный.
        /// </summary>
        private void SetProductAsPurchased()
        {
            if (priceText)
                priceText.text = "Куплен";

            if (purchaseButton)
                purchaseButton.interactable = false;
        }
    }
}