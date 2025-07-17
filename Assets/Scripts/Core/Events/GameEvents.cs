using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// ScriptableObject-контейнер для всех игровых событий.
    /// Обеспечивает централизованный доступ к событиям из разных частей проекта.
    /// </summary>
    [CreateAssetMenu(fileName = "GameEvents", menuName = "Events/Game Events Container")]
    public class GameEvents : ScriptableObject
    {
        [Header("Инициализация")]
        public StatefulVoidEvent onYandexSDKInitialized;

        [Header("Игровой процесс")]
        public VoidEvent onRequestNewGame;
        public VoidEvent onNewGameStarted;

        [Header("События сетки (Grid)")]
        public CellDataAnimateEvent onCellAdded;
        public CellDataEvent onCellUpdated;
        public GuidEvent onCellRemoved;
        public VoidEvent onGridCleared;
        public VoidEvent onLinesRemoved;

        [Header("Действия пользователя")]
        public VoidEvent onUndoLastAction;
        public VoidEvent onAddExistingNumbers;
        public VoidEvent onRequestHint;

        [Header("Поиск пар")]
        public CellPairEvent onAttemptMatch;
        public CellPairEvent onMatchFound;
        public VoidEvent onInvalidMatch;

        [Header("Подсказки")]
        public CellPairEvent onHintFound;
        public VoidEvent onNoHintFound;

        [Header("Счетчики действий")]
        public VoidEvent onRequestRefillCounters;
        public VoidEvent onRequestDisableCounters;
        public VoidEvent onDisableCountersConfirmed;
        public CountersChangedEvent onCountersChanged;

        [Header("Реклама")]
        public VoidEvent onShowRewardedAdForRefill;

        [Header("Подсчет очков")]
        public PairScoreEvent onPairScoreAdded;
        public LineScoreEvent onLineScoreAdded;
        public PairScoreEvent onPairScoreUndone;
        public LineScoreEvent onLineScoreUndone;
        public StatisticsChangedEvent onStatisticsChanged;
        public VoidEvent onBoardCleared;

        [Header("UI")]
        public BoolEvent onToggleTopLine;
        public VoidEvent onShowMenu;
        public VoidEvent onHideMenu;
        public VoidEvent onShowStatistics;
        public VoidEvent onHideStatistics;
        public VoidEvent onShowRules;
        public VoidEvent onHideRules;
        public VoidEvent onShowOptions;
        public VoidEvent onHideOptions;
    }
}