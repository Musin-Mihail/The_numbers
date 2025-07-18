using Core.Events;
using Model;
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

        private GameEvents _gameEvents;
        private Purchase _productInfo;
        private ActionCountersModel _actionCountersModel;

        /// <summary>
        /// Инициализация зависимостей, полученных из GameBootstrap.
        /// </summary>
        public void Initialize(GameEvents gameEvents, ActionCountersModel actionCountersModel)
        {
            _gameEvents = gameEvents;
            _actionCountersModel = actionCountersModel;
        }

        private void OnEnable()
        {
            YG2.onGetSDKData += InitializeShopProduct;
            YG2.onPurchaseSuccess += HandlePurchaseSuccess;
        }

        private void OnDisable()
        {
            YG2.onGetSDKData -= InitializeShopProduct;
            YG2.onPurchaseSuccess -= HandlePurchaseSuccess;
        }

        /// <summary>
        /// Инициализирует информацию о продукте после загрузки Yandex SDK.
        /// </summary>
        private void InitializeShopProduct()
        {
            if (_actionCountersModel == null)
            {
                Debug.LogError("ShopManager не смог получить ActionCountersModel. Убедитесь, что модель регистрируется до вызова onGetSDKData.");
                if (purchaseButton) purchaseButton.interactable = false;
                if (priceText) priceText.text = "Ошибка";
                return;
            }

            YG2.ConsumePurchaseByID(GameConstants.DisableCountersProductId);
            _productInfo = YG2.PurchaseByID(GameConstants.DisableCountersProductId);
            UpdateProductUI();
        }

        /// <summary>
        /// Обновляет UI товара, проверяя состояние игровой модели.
        /// </summary>
        private void UpdateProductUI()
        {
            if (_actionCountersModel == null) return;
            if (_actionCountersModel.AreCountersDisabled)
            {
                SetProductAsPurchased();
            }
            else
            {
                if (_productInfo != null)
                {
                    SetProductAsAvailable();
                }
                else
                {
                    if (priceText) priceText.text = "Товар не найден";
                    Debug.LogError($"Ошибка ShopManager: Товар с ID '{GameConstants.DisableCountersProductId}' не найден. Проверьте настройки в InfoYG -> Payments.");
                    if (purchaseButton) purchaseButton.interactable = false;
                }
            }
        }

        /// <summary>
        /// Обрабатывает событие успешной покупки (как новой, так и необработанной).
        /// </summary>
        /// <param name="purchasedId">ID купленного товара.</param>
        private void HandlePurchaseSuccess(string purchasedId)
        {
            if (purchasedId != GameConstants.DisableCountersProductId) return;
            Debug.Log($"Покупка '{purchasedId}' успешно обработана. Обновление UI.");
            UpdateProductUI();
        }

        /// <summary>
        /// Обновляет UI, чтобы показать товар как доступный для покупки.
        /// </summary>
        private void SetProductAsAvailable()
        {
            if (priceText)
                priceText.text = _productInfo.price;

            if (!purchaseButton) return;
            purchaseButton.interactable = true;
            purchaseButton.onClick.RemoveAllListeners();
            purchaseButton.onClick.AddListener(() => _gameEvents.onRequestDisableCounters.Raise());
        }

        /// <summary>
        /// Обновляет UI, чтобы показать товар как купленный.
        /// </summary>
        private void SetProductAsPurchased()
        {
            if (priceText)
                priceText.text = "Куплен";

            if (!purchaseButton) return;
            purchaseButton.interactable = false;
            purchaseButton.onClick.RemoveAllListeners();
        }
    }
}