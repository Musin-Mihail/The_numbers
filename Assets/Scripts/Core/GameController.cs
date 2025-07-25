﻿using System;
using System.Collections;
using Core.Events;
using Core.Handlers;
using Core.UndoSystem;
using Gameplay;
using Interfaces;
using Model;
using UnityEngine;
using View.Grid;

namespace Core
{
    /// <summary>
    /// Управляет основной игровой логикой, координируя взаимодействие между моделями,
    /// представлениями и обработчиками игровых событий.
    /// </summary>
    public class GameController : IDisposable
    {
        private readonly GridModel _gridModel;
        private readonly ActionHistory _actionHistory;
        private readonly ActionCountersModel _actionCountersModel;
        private readonly StatisticsModel _statisticsModel;
        private readonly GameEvents _gameEvents;
        private readonly GameManager _gameManager;
        private readonly GridView _gridView;

        private readonly MatchHandler _matchHandler;
        private readonly PlayerActionHandler _playerActionHandler;
        private readonly HintHandler _hintHandler;
        private readonly PlatformBridge _platformBridge;

        /// <summary>
        /// Инициализирует GameController и все его дочерние обработчики.
        /// </summary>
        public GameController(
            GridModel gridModel,
            MatchValidator matchValidator,
            GameEvents gameEvents,
            ActionHistory actionHistory,
            ActionCountersModel actionCountersModel,
            StatisticsModel statisticsModel,
            GameManager gameManager,
            GridView gridView,
            IPlatformServices platformServices)
        {
            _gridModel = gridModel;
            _gameEvents = gameEvents;
            _actionHistory = actionHistory;
            _actionCountersModel = actionCountersModel;
            _statisticsModel = statisticsModel;
            _gameManager = gameManager;
            _gridView = gridView;

            _matchHandler = new MatchHandler(gridModel, matchValidator, statisticsModel, actionHistory, gameEvents, gameManager);
            _playerActionHandler = new PlayerActionHandler(actionCountersModel, actionHistory, gridModel, statisticsModel, gameEvents, gameManager);
            _hintHandler = new HintHandler(gridModel, matchValidator, actionCountersModel, gameEvents, gridView, gameManager);
            _platformBridge = new PlatformBridge(platformServices, gameEvents, actionCountersModel, gameManager);

            _gameEvents.onBoardCleared.AddListener(HandleBoardCleared);
        }

        /// <summary>
        /// Освобождает ресурсы, отписываясь от событий в дочерних обработчиках.
        /// </summary>
        public void Dispose()
        {
            _gameEvents.onBoardCleared.RemoveListener(HandleBoardCleared);

            _matchHandler?.Dispose();
            _playerActionHandler?.Dispose();
            _hintHandler?.Dispose();
            _platformBridge?.Dispose();
        }

        /// <summary>
        /// Начинает новую игру, очищая поле, историю и опционально сбрасывая статистику.
        /// </summary>
        /// <param name="resetStatisticsAndCounters">Если true, сбрасывает статистику и счетчики действий.</param>
        public void StartNewGame(bool resetStatisticsAndCounters)
        {
            _gridView.ResetSelectionAndHints();
            _gridModel.ClearField();
            _actionHistory.Clear();

            if (resetStatisticsAndCounters)
            {
                _actionCountersModel.ResetCounters();
                _statisticsModel.Reset();
            }

            _gameEvents.onStatisticsChanged.Raise((_statisticsModel.Score, _statisticsModel.Multiplier));

            for (var i = 0; i < GameConstants.InitialLinesOnStart; i++)
            {
                _gridModel.CreateLine(i);
            }

            _gameEvents.onNewGameStarted.Raise();
            _gameManager?.RequestSave();
        }

        /// <summary>
        /// Обрабатывает событие полной очистки доски.
        /// </summary>
        private void HandleBoardCleared()
        {
            _gameManager.StartCoroutine(BoardClearedRoutine());
        }

        /// <summary>
        /// Корутина, которая запускает новую игру после небольшой задержки после очистки доски.
        /// </summary>
        private IEnumerator BoardClearedRoutine()
        {
            yield return new WaitForSeconds(2.0f);
            StartNewGame(false);
        }
    }
}