using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Services
{
    /// <summary>
    /// Вспомогательный класс для асинхронной загрузки изображений по URL.
    /// </summary>
    public static class ImageDownloader
    {
        /// <summary>
        /// Загружает изображение и устанавливает его в компонент Image.
        /// </summary>
        /// <param name="targetImage">Компонент Image, в который будет загружено изображение.</param>
        /// <param name="url">URL изображения.</param>
        public static IEnumerator LoadImage(Image targetImage, string url)
        {
            if (!targetImage || string.IsNullOrEmpty(url))
            {
                yield break;
            }

            var request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                if (targetImage)
                {
                    targetImage.sprite = sprite;
                }
            }
            else
            {
                Debug.Log("Ошибка загрузки изображения: " + request.error + " с URL: " + url);
            }
        }
    }
}