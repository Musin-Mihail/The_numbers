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
        /// Флаг, указывающий, была ли игра когда-либо сохранена.
        /// </summary>
        public bool isGameEverSaved;
    }
}