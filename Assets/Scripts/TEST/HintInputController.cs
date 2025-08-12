using Core.Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TEST
{
    /// <summary>
    /// Отвечает за обработку ввода с клавиатуры для вызова подсказки.
    /// </summary>
    public class HintInputController : MonoBehaviour
    {
        [Header("Каналы событий")]
        [Tooltip("Ссылка на ScriptableObject с игровыми событиями")]
        [SerializeField] private GameEvents gameEvents;

        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.hKey.wasPressedThisFrame) return;
            if (gameEvents)
            {
                Debug.Log("Клавиша H нажата, вызываем onRequestHint.");
                gameEvents.onRequestHint.Raise();
            }
            else
            {
                Debug.LogWarning("GameEvents не назначен в HintInputController.");
            }
        }
    }
}
