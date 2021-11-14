using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MultiplayerExtensions.UI
{
    class CenterScreenLoadingPanel : MonoBehaviour, IProgress<double>
    {
        private IMultiplayerSessionManager sessionManager;
        private ILobbyGameStateController gameStateController;
        private CenterStageScreenController screenController;
        private LoadingControl? loadingControl;
        private bool isDownloading;
        public int playersReady;
        public static CenterScreenLoadingPanel? Instance { get; private set; }

        [Inject]
        internal void Inject(IMultiplayerSessionManager sessionManager, ILobbyGameStateController gameStateController, CenterStageScreenController screenController)
        {
            Instance = this;
            this.sessionManager = sessionManager;
            this.gameStateController = gameStateController;
            this.screenController = screenController;

            BeatSaberMarkupLanguage.Tags.VerticalLayoutTag verticalTag = new BeatSaberMarkupLanguage.Tags.VerticalLayoutTag();
            GameObject vertical = verticalTag.CreateObject(transform);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            (vertical.transform as RectTransform).sizeDelta = new Vector2(60, 60);
            (vertical.transform as RectTransform).anchoredPosition = new Vector2(0.0f, -30.0f);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            var layout = vertical.AddComponent<LayoutElement>().minWidth = 60;

            GameObject existingLoadingControl = Resources.FindObjectsOfTypeAll<LoadingControl>().First().gameObject;
            GameObject loadingControlGameObject = GameObject.Instantiate(existingLoadingControl, vertical.transform);
            loadingControl = loadingControlGameObject.GetComponent<LoadingControl>();
            loadingControl.Hide();
        }

        public void FixedUpdate()
        {
            if (isDownloading)
            {
                return;
            }
            else if (screenController.countdownShown && sessionManager.syncTime >= gameStateController.startTime && gameStateController.levelStartInitiated)
            {
                if (loadingControl != null)
                    loadingControl.ShowLoading($"{playersReady + 1} of {(sessionManager.connectedPlayerCount + 1)} players ready...");
            }
            else
            {
                if (loadingControl != null)
                    loadingControl.Hide();
                playersReady = 0;
            }
        }

        public void OnDisable()
        {
            if (loadingControl != null)
                loadingControl.Hide();
            playersReady = 0;
        }

        public void Report(double value)
        {
            isDownloading = (value < 1.0);
            if (loadingControl != null)
            {
                loadingControl.ShowDownloadingProgress($"Downloading ({value * 100:F2}%)...", (float)value);
            }
        }
    }
}
