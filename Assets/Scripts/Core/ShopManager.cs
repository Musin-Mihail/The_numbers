using System.Collections;
using System.Linq;
using Core.Events;
using PlayablesStudio.Plugins.YandexGamesSDK.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using YandexGames.Types.IAP;

namespace Core
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

        private YandexGamesSDK _sdk;
        private YGProduct _productInfo;
        private bool _isPurchased;

        private void OnEnable()
        {
            if (gameEvents)
            {
                gameEvents.onCountersChanged.AddListener(HandleCountersChanged);
            }
        }

        private void OnDisable()
        {
            if (gameEvents)
            {
                gameEvents.onCountersChanged.RemoveListener(HandleCountersChanged);
            }
        }

        private IEnumerator Start()
        {
            purchaseButton.interactable = false;
            priceText.text = "Загрузка...";
            while (!YandexGamesSDK.IsInitialized)
            {
                yield return null;
            }

            _sdk = YandexGamesSDK.Instance;
            if (!gameEvents)
            {
                priceText.text = "Ошибка";
                yield break;
            }

            if (_sdk.Purchases != null)
            {
                CheckForPreviousPurchases();
            }
            else
            {
                priceText.text = "N/A";
            }
        }

        private void CheckForPreviousPurchases()
        {
            _sdk.Purchases.GetPurchasedProducts((success, purchasedProductsResponse, error) =>
            {
                if (success && purchasedProductsResponse?.purchasedProducts != null)
                {
                    if (purchasedProductsResponse.purchasedProducts.Any(p => p.productID == productIdToDisplay))
                    {
                        _isPurchased = true;
                        SetProductAsPurchased();
                        return;
                    }
                }

                if (!_isPurchased)
                {
                    LoadProductCatalog();
                }
            });
        }

        private void LoadProductCatalog()
        {
            _sdk.Purchases.GetProductCatalog((success, response, error) =>
            {
                if (!success || response?.products == null) return;
                foreach (var product in response.products)
                {
                    if (product.id != productIdToDisplay) continue;
                    _productInfo = product;
                    UpdateProductUI();
                    break;
                }
            });
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
            if (webRequest.result != UnityWebRequest.Result.Success) yield break;
            var texture = DownloadHandlerTexture.GetContent(webRequest);
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            targetImage.sprite = sprite;
            targetImage.color = Color.white;
        }
    }
}