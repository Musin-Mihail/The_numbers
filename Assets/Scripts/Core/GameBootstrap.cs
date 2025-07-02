using System;
using Core.Events;
using DataProviders;
using Gameplay;
using Model;
using UnityEngine;
using UnityEngine.UI;
using View.Grid;
using View.UI;

namespace Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Scene Dependencies")]
        [SerializeField] private GridView view;
        [SerializeField] private HeaderNumberDisplay headerNumberDisplay;
        [SerializeField] private ConfirmationDialog confirmationDialog;
        [SerializeField] private Toggle topLineToggle;

        [Header("Event Channels")]
        [SerializeField] private VoidEvent onRequestNewGame;
        [SerializeField] private VoidEvent onNewGameStarted;
        [SerializeField] private VoidEvent onRequestRefillCounters;
        [SerializeField] private VoidEvent onRefillCountersConfirmed;
        [SerializeField] private VoidEvent onRequestDisableCounters;
        [SerializeField] private VoidEvent onDisableCountersConfirmed;
        [SerializeField] private BoolEvent onToggleTopLine;
        [SerializeField] private CellPairEvent onAttemptMatch;
        [SerializeField] private CellPairEvent onMatchFound;
        [SerializeField] private VoidEvent onInvalidMatch;
        [SerializeField] private VoidEvent onUndoLastAction;
        [SerializeField] private VoidEvent onAddExistingNumbers;
        [SerializeField] private VoidEvent onRequestHint;
        [SerializeField] private CellPairEvent onHintFound;
        [SerializeField] private VoidEvent onNoHintFound;
        [SerializeField] private CountersChangedEvent onCountersChanged;
        [SerializeField] private StatisticsChangedEvent onStatisticsChanged;
        [SerializeField] private PairScoreEvent onPairScoreAdded;
        [SerializeField] private LineScoreEvent onLineScoreAdded;
        [SerializeField] private PairScoreEvent onPairScoreUndone;
        [SerializeField] private LineScoreEvent onLineScoreUndone;

        private GameController _gameController;
        private GridModel _gridModel;
        private Action _requestNewGameAction;

        private void Awake()
        {
            _gridModel = new GridModel();
            var gridDataProvider = new GridDataProvider(_gridModel);
            var calculatingMatches = new MatchValidator(gridDataProvider);

            _gameController = new GameController(
                _gridModel,
                calculatingMatches,
                onAttemptMatch,
                onAddExistingNumbers,
                onUndoLastAction,
                onRequestHint,
                onRefillCountersConfirmed,
                onDisableCountersConfirmed,
                onCountersChanged,
                onRequestRefillCounters,
                onNoHintFound,
                onHintFound,
                onMatchFound,
                onInvalidMatch,
                onPairScoreAdded,
                onLineScoreAdded,
                onStatisticsChanged,
                onNewGameStarted,
                onPairScoreUndone,
                onLineScoreUndone
            );
            view.Initialize(
                _gridModel,
                headerNumberDisplay,
                onMatchFound,
                onInvalidMatch,
                onToggleTopLine,
                onNewGameStarted,
                onHintFound,
                onPairScoreAdded,
                onLineScoreAdded,
                onPairScoreUndone,
                onLineScoreUndone,
                onAttemptMatch
            );
        }

        private void Start()
        {
            if (topLineToggle)
            {
                onToggleTopLine.Raise(topLineToggle.isOn);
                topLineToggle.onValueChanged.AddListener(onToggleTopLine.Raise);
            }
            else
            {
                onToggleTopLine.Raise(true);
            }

            if (confirmationDialog)
            {
                _requestNewGameAction = () => confirmationDialog.Show("Начать новую игру?", StartNewGameInternal);
                onRequestNewGame.AddListener(_requestNewGameAction);
                onRequestRefillCounters.AddListener(HandleRequestRefillCounters);
                onRequestDisableCounters.AddListener(HandleRequestDisableCounters);
            }
            else
            {
                onRequestNewGame.AddListener(StartNewGameInternal);
            }
        }

        private void HandleRequestDisableCounters()
        {
            confirmationDialog.Show("Отключить счётчики?", () => { onDisableCountersConfirmed.Raise(); });
        }

        private void HandleRequestRefillCounters()
        {
            confirmationDialog.Show("Добавить количество к cчетчикам?", () => { onRefillCountersConfirmed.Raise(); });
        }

        private void OnDestroy()
        {
            if (topLineToggle)
            {
                topLineToggle.onValueChanged.RemoveListener(onToggleTopLine.Raise);
            }

            if (confirmationDialog)
            {
                if (_requestNewGameAction != null)
                {
                    onRequestNewGame.RemoveListener(_requestNewGameAction);
                }

                onRequestRefillCounters.RemoveListener(HandleRequestRefillCounters);
                onRequestDisableCounters.RemoveListener(HandleRequestDisableCounters);
            }
            else
            {
                onRequestNewGame.RemoveListener(StartNewGameInternal);
            }
        }

        private void StartNewGameInternal()
        {
            _gameController.StartNewGame();
        }
    }
}