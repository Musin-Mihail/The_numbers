using System.Collections;
using Core.Events;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using YandexGames.Types.IAP;

namespace Core.Shop
{
    public class ShopManager : MonoBehaviour
    {
        [Header("UI Элементы")]
        [SerializeField] private Button purchaseButton;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Image currencyIcon;

        [Header("Настройки товара")]
        [SerializeField] private string productIdToDisplay = "disable_counters_product_id";

        [Header("Event Channels")]
        [SerializeField] private GameEvents gameEvents;

        private IShopDataProvider _dataProvider;
        private bool _isShopInitialized;
        private bool _isPurchased;
        private YGProduct _productInfo;

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

        private void Initialize()
        {
            if (_isShopInitialized) return;
            _isShopInitialized = true;

#if UNITY_WEBGL && !UNITY_EDITOR
            _dataProvider = new YandexShopDataProvider(this);
#else
            _dataProvider = new EditorMockShopDataProvider();
#endif

            purchaseButton.interactable = false;
            priceText.text = "Загрузка...";
            _dataProvider.FetchShopData(productIdToDisplay, OnShopDataFetched);
        }

        private void OnShopDataFetched(ShopDataResult result)
        {
            if (result.IsError)
            {
                priceText.text = result.ErrorMessage;
                Debug.LogError($"ShopManager Error: {result.ErrorMessage}");
                return;
            }

            _isPurchased = result.IsPurchased;
            _productInfo = result.ProductInfo;

            if (_isPurchased)
            {
                SetProductAsPurchased();
            }
            else
            {
                UpdateProductUI();
            }
        }

        private void UpdateProductUI()
        {
            if (_isPurchased || _productInfo == null) return;

            priceText.text = _productInfo.price;
            if (currencyIcon && !string.IsNullOrEmpty(_productInfo.priceCurrencyImage))
            {
                currencyIcon.gameObject.SetActive(true);
                StartCoroutine(LoadImageFromURL(_productInfo.priceCurrencyImage, currencyIcon));
            }

            purchaseButton.interactable = true;
        }

        private void HandleCountersChanged((int undo, int add, int hint) data)
        {
            if (data.undo != -1 || _isPurchased) return;
            _isPurchased = true;
            SetProductAsPurchased();
        }

        private void SetProductAsPurchased()
        {
            priceText.text = "Куплен";
            if (currencyIcon)
            {
                currencyIcon.gameObject.SetActive(false);
            }

            purchaseButton.interactable = false;
        }

        private IEnumerator LoadImageFromURL(string url, Image targetImage)
        {
            using var webRequest = UnityWebRequestTexture.GetTexture(url);
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Не удалось загрузить изображение валюты: {webRequest.error}");
                yield break;
            }

            var texture = DownloadHandlerTexture.GetContent(webRequest);
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            targetImage.sprite = sprite;
            targetImage.color = Color.white;
        }
    }
}