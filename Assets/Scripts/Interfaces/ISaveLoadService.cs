using System;

namespace Interfaces
{
    /// <summary>
    /// Интерфейс для сервиса сохранения и загрузки игры.
    /// </summary>
    public interface ISaveLoadService
    {
        /// <summary>
        /// Запрашивает сохранение текущего состояния игры.
        /// </summary>
        void RequestSave();

        /// <summary>
        /// Загружает сохраненное состояние игры.
        /// </summary>
        /// <param name="onComplete">Callback, вызываемый по завершении загрузки. Параметр bool указывает на успех.</param>
        void LoadGame(Action<bool> onComplete);

        /// <summary>
        /// Устанавливает и сохраняет состояние видимости верхней строки.
        /// </summary>
        /// <param name="isVisible">True, если строка должна быть видимой.</param>
        void SetTopLineVisibility(bool isVisible);
    }
}