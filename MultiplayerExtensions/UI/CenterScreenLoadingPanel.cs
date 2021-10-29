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
        public static CenterScreenLoadingPanel? self { get; private set; }

        [Inject]
        internal void Inject(IMultiplayerSessionManager sessionManager, ILobbyGameStateController gameStateController, CenterStageScreenController screenController)
        {
            self = this;
            this.sessionManager = sessionManager;
            this.gameStateController = gameStateController;
            this.screenController = screenController;

            BeatSaberMarkupLanguage.Tags.VerticalLayoutTag verticalTag = new BeatSaberMarkupLanguage.Tags.VerticalLayoutTag();
            GameObject vertical = verticalTag.CreateObject(transform);
            (vertical.transform as RectTransform).sizeDelta = new Vector2(60, 60);
            (vertical.transform as RectTransform).anchoredPosition = new Vector2(0.0f, -30.0f);
            var layout = vertical.AddComponent<LayoutElement>().minWidth = 60;

            GameObject existingLoadingControl = Resources.FindObjectsOfTypeAll<LoadingControl>().First().gameObject;
            GameObject loadingControlGameObject = GameObject.Instantiate(existingLoadingControl, vertical.transform);
            loadingControl = loadingControlGameObject.GetComponent<LoadingControl>();
            loadingControl.Hide();
        }

        public void Update()
        {
            if (isDownloading)
            {
                return;
            }
            else if (screenController.countdownShown && sessionManager.syncTime >= gameStateController.startTime && gameStateController.levelStartInitiated)
            {
                if (loadingControl != null)
                    loadingControl.ShowLoading("Loading...");
            }
            else
            {
                if (loadingControl != null)
                    loadingControl.Hide();
            }
        }

        public void OnDisable()
        {
            if (loadingControl != null)
                loadingControl.Hide();
        }

        public void Report(double value)
        {
            isDownloading = (value < 1.0);
            if (loadingControl != null)
            {
                loadingControl.ShowDownloadingProgress("Downloading...", (float)value);
            }
        }
    }
}
