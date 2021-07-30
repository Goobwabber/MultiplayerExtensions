using HMUI;
using IPA.Utilities;
using UnityEngine;

namespace MultiplayerExtensions.Environments
{
	class MultiplayerGameplayHud : MonoBehaviour
	{
		protected IConnectedPlayer _connectedPlayer = null!;
		protected MultiplayerGameplayAnimator _gameplayAnimator = null!;

		protected CoreGameHUDController _coreGameHUDController = null!;
		protected GameObject _songProgressPanelGO = null!;
		protected GameObject _energyPanelGO = null!;

		internal void Construct(IConnectedPlayer connectedPlayer, MultiplayerGameplayAnimator gameplayAnimator)
		{
			_connectedPlayer = connectedPlayer;
			_gameplayAnimator = gameplayAnimator;

			if (gameplayAnimator is MultiplayerLocalActivePlayerGameplayAnimator localGameplayAnimator)
			{
				_coreGameHUDController = localGameplayAnimator.GetField<CoreGameHUDController, MultiplayerLocalActivePlayerGameplayAnimator>("_coreGameHUDController");

				_songProgressPanelGO = _coreGameHUDController.GetField<GameObject, CoreGameHUDController>("_songProgressPanelGO");
				_energyPanelGO = _coreGameHUDController.GetField<GameObject, CoreGameHUDController>("_energyPanelGO");
			}
		}

		internal void Start()
		{
            if (Plugin.Config.SingleplayerHUD && _coreGameHUDController != null)
            {
                Plugin.Log?.Debug("Setting up multiplayer HUD");

                _coreGameHUDController.transform.position = new Vector3(0f, 0f, 10f);
                _coreGameHUDController.transform.eulerAngles = new Vector3(270f, 0f, 0f);

                _energyPanelGO.transform.localPosition = new Vector3(0f, 4f, 0f);
                _energyPanelGO.transform.localEulerAngles = new Vector3(90f, 0f, 0f);

                if (!_coreGameHUDController.transform.Find("LeftPanel"))
                {
                    Transform comboPanel = _coreGameHUDController.transform.Find("ComboPanel");
                    Transform scoreCanvas = _coreGameHUDController.transform.Find("ScoreCanvas");
                    Transform multiplierCanvas = _coreGameHUDController.transform.Find("MultiplierCanvas");

                    GameObject leftPanel = new GameObject();
                    GameObject rightPanel = new GameObject();
                    leftPanel.name = "LeftPanel";
                    rightPanel.name = "RightPanel";
                    leftPanel.transform.parent = _coreGameHUDController.transform;
                    rightPanel.transform.parent = _coreGameHUDController.transform;
                    leftPanel.transform.localPosition = new Vector3(-2.5f, 0f, 1f);
                    rightPanel.transform.localPosition = new Vector3(2.5f, 0f, 1f);

                    _songProgressPanelGO.transform.SetParent(rightPanel.transform, true);
                    _songProgressPanelGO.transform.localPosition = new Vector3(0f, -1.1f, 0f);
                    _songProgressPanelGO.transform.SetParent(_coreGameHUDController.transform, true);

                    multiplierCanvas.transform.SetParent(rightPanel.transform, true);
                    multiplierCanvas.transform.localPosition = new Vector3(0f, 0f, 0f);
                    multiplierCanvas.transform.SetParent(_coreGameHUDController.transform, true);

                    comboPanel.transform.SetParent(leftPanel.transform, true);
                    comboPanel.transform.localPosition = new Vector3(0f, 0f, 0f);
                    comboPanel.transform.SetParent(_coreGameHUDController.transform, true);

                    scoreCanvas.transform.SetParent(leftPanel.transform, true);
                    scoreCanvas.transform.localPosition = new Vector3(0f, -1.1f, 0f);
                    scoreCanvas.transform.SetParent(_coreGameHUDController.transform, true);

                    foreach (CurvedTextMeshPro panel in scoreCanvas.GetComponentsInChildren<CurvedTextMeshPro>())
                    {
                        panel.enabled = true;
                    }
                }
            }
        }
	}
}
