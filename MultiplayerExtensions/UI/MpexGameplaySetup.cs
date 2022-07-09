using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using IPA.Utilities;
using SiraUtil.Logging;
using System;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.UI
{
    public class MpexGameplaySetup : NotifiableBase, IInitializable, IDisposable
    {
        public const string ResourcePath = "MultiplayerExtensions.UI.MpexGameplaySetup.bsml";

        private FieldAccessor<GameplaySetupViewController, bool>.Accessor _showModifiers
            = FieldAccessor<GameplaySetupViewController, bool>.GetAccessor(nameof(_showModifiers));
        private FieldAccessor<GameplaySetupViewController, bool>.Accessor _showEnvironmentOverrideSettings
            = FieldAccessor<GameplaySetupViewController, bool>.GetAccessor(nameof(_showEnvironmentOverrideSettings));
        private FieldAccessor<GameplaySetupViewController, bool>.Accessor _showColorSchemesSettings
            = FieldAccessor<GameplaySetupViewController, bool>.GetAccessor(nameof(_showColorSchemesSettings));
        private FieldAccessor<GameplaySetupViewController, bool>.Accessor _showMultiplayer
            = FieldAccessor<GameplaySetupViewController, bool>.GetAccessor(nameof(_showMultiplayer));

        private GameplaySetupViewController _gameplaySetup;
        private MultiplayerSettingsPanelController _multiplayerSettingsPanel;
        private MainFlowCoordinator _mainFlowCoordinator;
        private MpexSetupFlowCoordinator _setupFlowCoordinator;
        private Config _config;
        private SiraLog _logger;

        internal MpexGameplaySetup(
            GameplaySetupViewController gameplaySetup,
            MainFlowCoordinator mainFlowCoordinator,
            MpexSetupFlowCoordinator setupFlowCoordinator,
            Config config,
            SiraLog logger)
        {
            _gameplaySetup = gameplaySetup;
            _multiplayerSettingsPanel = gameplaySetup.GetField<MultiplayerSettingsPanelController, GameplaySetupViewController>("_multiplayerSettingsPanelController");
            _mainFlowCoordinator = mainFlowCoordinator;
            _setupFlowCoordinator = setupFlowCoordinator;
            _config = config;
            _logger = logger;
        }

        public void Initialize()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), ResourcePath), _multiplayerSettingsPanel.gameObject, this);
            while (0 < _vert.transform.childCount)
                _vert.transform.GetChild(0).SetParent(_multiplayerSettingsPanel.transform);
        }

        public void Dispose()
        {
            
        }

        [UIAction("preferences-click")]
        private void PresentPreferences()
        {
            FlowCoordinator deepestChildFlowCoordinator = DeepestChildFlowCoordinator(_mainFlowCoordinator);
            _setupFlowCoordinator.parentFlowCoordinator = deepestChildFlowCoordinator;
            deepestChildFlowCoordinator.PresentFlowCoordinator(_setupFlowCoordinator);
        }

        private FlowCoordinator DeepestChildFlowCoordinator(FlowCoordinator root)
        {
            var flow = root.childFlowCoordinator;
            if (flow == null) return root;
            if (flow.childFlowCoordinator == null || flow.childFlowCoordinator == flow)
            {
                return flow;
            }
            return DeepestChildFlowCoordinator(flow);
        }

        [UIObject("vert")]
        private GameObject _vert = null!;

        [UIValue("solo-environment")]
        private bool _soloEnvironment
        {
            get => _config.SoloEnvironment;
            set 
            {
                _config.SoloEnvironment = value;
                _gameplaySetup.Setup(
                    _showModifiers(ref _gameplaySetup),
                    _showEnvironmentOverrideSettings(ref _gameplaySetup),
                    _showColorSchemesSettings(ref _gameplaySetup),
                    _showMultiplayer(ref _gameplaySetup),
                    PlayerSettingsPanelController.PlayerSettingsPanelLayout.Multiplayer
                );
                NotifyPropertyChanged();
            }
        }
    }
}
