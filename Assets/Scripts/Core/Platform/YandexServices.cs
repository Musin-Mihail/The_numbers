using System;
using System.Linq;
using Core.Events;
using Model;
using UnityEngine;
using View.Grid;
using YG;

namespace Core.Platform
{
    /// <summary>
    /// Реализация сервиса сохранения и загрузки для платформы Yandex Games.
    /// </summary>
    public class YandexSaveLoadService : ISaveLoadService
    {
        private readonly GridModel _gridModel;
        private readonly StatisticsModel _statisticsModel;
        private readonly ActionCountersModel _actionCountersModel;
        private readonly GameEvents _gameEvents;

        private bool _isLoading;
        private bool _isSaving;

        public YandexSaveLoadService(GridModel gridModel, StatisticsModel statisticsModel, ActionCountersModel actionCountersModel, GameEvents gameEvents)
        {
            _gridModel = gridModel;
            _statisticsModel = statisticsModel;
            _actionCountersModel = actionCountersModel;
            _gameEvents = gameEvents;
        }

        /// <summary>
        /// Запрашивает сохранение игры.
        /// </summary>
        public void RequestSave()
        {
            SaveGame();
        }

        private void SaveGame()
        {
            if (_isLoading || _isSaving) return;

            _isSaving = true;

            YG2.saves.gridCells = _gridModel.GetAllCellData().Select(c => new CellDataSerializable(c)).ToList();
            YG2.saves.statistics = new StatisticsModelSerializable(_statisticsModel);
            YG2.saves.actionCounters = new ActionCountersModelSerializable(_actionCountersModel);
            YG2.saves.isGameEverSaved = true;

            YG2.SaveProgress();

            _isSaving = false;
            Debug.Log("Запрос на сохранение игровых данных через YandexSaveLoadService.");
        }

        /// <summary>
        /// Загружает игру с серверов Yandex.
        /// </summary>
        /// <param name="onComplete">Callback, вызываемый по завершении. True при успехе.</param>
        public void LoadGame(Action<bool> onComplete)
        {
            _isLoading = true;

            if (YG2.saves.isGameEverSaved)
            {
                _gridModel.RestoreState(YG2.saves.gridCells);
                _statisticsModel.SetState(YG2.saves.statistics.score, YG2.saves.statistics.multiplier);
                _actionCountersModel.RestoreState(YG2.saves.actionCounters);

                var gridView = ServiceProvider.GetService<GridView>();
                gridView?.FullRedraw();

                if (_gameEvents != null)
                {
                    _gameEvents.onToggleTopLine?.Raise(YG2.saves.isTopLineVisible);
                    if (YG2.saves.actionCounters.areCountersDisabled)
                    {
                        _gameEvents.onCountersChanged?.Raise((-1, -1, -1));
                    }
                    else
                    {
                        _gameEvents.onCountersChanged?.Raise((YG2.saves.actionCounters.undoCount, YG2.saves.actionCounters.addNumbersCount, YG2.saves.actionCounters.hintCount));
                    }

                    _gameEvents.onStatisticsChanged?.Raise((YG2.saves.statistics.score, YG2.saves.statistics.multiplier));
                }

                Debug.Log("Игровые данные успешно загружены из сохранений Yandex.");
                _isLoading = false;
                onComplete?.Invoke(true);
            }
            else
            {
                Debug.LogWarning("Данные сохранения в Yandex не найдены. Будет начата новая игра.");
                _isLoading = false;
                onComplete?.Invoke(false);
            }
        }

        /// <summary>
        /// Устанавливает видимость верхней строки и сохраняет состояние.
        /// </summary>
        public void SetTopLineVisibility(bool isVisible)
        {
            if (YG2.saves.isTopLineVisible == isVisible) return;
            YG2.saves.isTopLineVisible = isVisible;
            RequestSave();
        }
    }

    /// <summary>
    /// Реализация платформенных сервисов (покупки, реклама) для Yandex Games.
    /// </summary>
    public class YandexPlatformService : IPlatformServices
    {
        public event Action<string> OnPurchaseSuccess;
        public event Action<string> OnPurchaseFailed;
        public event Action<string> OnRewardVideoSuccess;

        public YandexPlatformService()
        {
            YG2.onPurchaseSuccess += OnYgPurchaseSuccess;
            YG2.onPurchaseFailed += OnYgPurchaseFailed;
            YG2.onRewardAdv += OnYgRewardVideo;
        }

        ~YandexPlatformService()
        {
            YG2.onPurchaseSuccess -= OnYgPurchaseSuccess;
            YG2.onPurchaseFailed -= OnYgPurchaseFailed;
            YG2.onRewardAdv -= OnYgRewardVideo;
        }

        /// <summary>
        /// Инициирует покупку через Yandex SDK.
        /// </summary>
        public void Purchase(string productId)
        {
            YG2.BuyPayments(productId);
        }

        /// <summary>
        /// Показывает рекламу с вознаграждением через Yandex SDK.
        /// </summary>
        public void ShowRewardedAd(string rewardId)
        {
            YG2.RewardedAdvShow(rewardId);
        }

        private void OnYgPurchaseSuccess(string purchasedId)
        {
            OnPurchaseSuccess?.Invoke(purchasedId);
        }

        private void OnYgPurchaseFailed(string failedId)
        {
            OnPurchaseFailed?.Invoke(failedId);
        }

        private void OnYgRewardVideo(string rewardId)
        {
            OnRewardVideoSuccess?.Invoke(rewardId);
        }
    }

    /// <summary>
    /// Реализация сервиса таблицы лидеров для Yandex Games.
    /// </summary>
    public class YandexLeaderboardService : ILeaderboardService
    {
        private readonly string _leaderboardName;

        public YandexLeaderboardService(string leaderboardName)
        {
            _leaderboardName = leaderboardName;
        }

        /// <summary>
        /// Обновляет счет в таблице лидеров Yandex.
        /// </summary>
        public void UpdateLeaderboard(int score)
        {
            if (YG2.player.auth)
            {
                YG2.SetLeaderboard(_leaderboardName, score);
                Debug.Log($"Таблица лидеров '{_leaderboardName}' обновлена с результатом: {score}");
            }
            else
            {
                Debug.LogWarning("Игрок не авторизован. Результат не отправлен в таблицу лидеров.");
            }
        }
    }
}