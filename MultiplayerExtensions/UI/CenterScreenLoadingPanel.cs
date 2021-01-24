using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MultiplayerExtensions.UI
{
    class CenterScreenLoadingPanel : BSMLResourceViewController
    {
        public override string ResourceName => "MultiplayerExtensions.UI.CenterScreenLoading.bsml";
        private IMultiplayerSessionManager sessionManager;
        private ILobbyGameStateController gameStateController;
        private CenterStageScreenController screenController;

        [Inject]
        internal void Inject(IMultiplayerSessionManager sessionManager, ILobbyGameStateController gameStateController, CenterStageScreenController screenController)
        {
            this.sessionManager = sessionManager;
            this.gameStateController = gameStateController;
            this.screenController = screenController;
            base.DidActivate(true, false, true);
            loadIndicator.color = Color.white;
        }

        [UIComponent("LoadingDisplay")]
        public RectTransform loadingDisplay;

        [UIComponent("LoadIndicator")]
        public Image loadIndicator;

        public void Update()
        {
            if (screenController.countdownShown && sessionManager.syncTime >= gameStateController.startTime && gameStateController.levelStartInitiated)
            {
                if (loadingDisplay != null)
                    loadingDisplay.gameObject.SetActive(true);
            }
            else
            {
                if (loadingDisplay != null)
                    loadingDisplay.gameObject.SetActive(false);
            }
        }
    }
}
