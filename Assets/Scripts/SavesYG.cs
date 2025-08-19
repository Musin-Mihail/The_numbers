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
        /// Рекордный счет игрока.
        /// </summary>
        public long record = 0;

        /// <summary>
        /// УСТАРЕВШЕЕ ПОЛЕ. Используется только для обратной совместимости со старыми сохранениями.
        /// Новые сохранения используют `gridState`.
        /// </summary>
        [System.Obsolete("This field is for backward compatibility with old saves. Use gridState instead.")]
        public List<CellDataSerializable> gridCells = new();

        /// <summary>
        /// Компактное представление сетки в виде строки для экономии места.
        /// </summary>
        public string gridState = "";

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
        /// Хранит идентификаторы выполненных миграций данных, чтобы избежать их повторного запуска.
        /// </summary>
        public List<string> seenMigrationIds = new();
    }
}