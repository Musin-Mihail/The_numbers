using System.Collections.Generic;
using Core;

namespace YG
{
    /// <summary>
    /// Частичный класс, определяющий структуру сохраняемых данных для Yandex Games.
    /// </summary>
    public partial class SavesYG
    {
        /// <summary>
        /// Определяет, видна ли верхняя строка с дублирующимися номерами.
        /// </summary>
        public bool isTopLineVisible = true;

        /// <summary>
        /// Сериализованные данные всех ячеек на игровой сетке.
        /// </summary>
        public List<CellDataSerializable> gridCells = new();

        /// <summary>
        /// Сериализованные данные статистики игрока (счет, множитель).
        /// </summary>
        public StatisticsModelSerializable statistics = new();

        /// <summary>
        /// Сериализованные данные счетчиков действий (отмена, добавление чисел, подсказки).
        /// </summary>
        public ActionCountersModelSerializable actionCounters = new();

        /// <summary>
        /// Хранит список версий обновлений, которые игрок уже видел.
        /// Это позволяет показывать анимацию для каждого нового обновления.
        /// </summary>
        public List<int> seenUpdateVersions = new();

        /// <summary>
        /// Сохраненный язык игры. По умолчанию - пустая строка.
        /// </summary>
        public string language = "";
    }
}