﻿using HarmonyLib;
using IPA.Utilities;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace MultiplayerExtensions.Patchers
{
    public class EnvironmentPatcher : IAffinity
    {
        private readonly GameplaySetupViewController _gameplaySetup;
        private readonly GameScenesManager _scenesManager;
        private readonly SiraLog _logger;

        internal EnvironmentPatcher(
            GameplaySetupViewController gameplaySetup,
            GameScenesManager scenesManager,
            SiraLog logger)
        {
            _gameplaySetup = gameplaySetup;
            _scenesManager = scenesManager;
            _logger = logger;
        }

        private static readonly MethodInfo _setupGameplaySetup = typeof(GameplaySetupViewController).GetMethod(nameof(GameplaySetupViewController.Setup));
        private static readonly MethodInfo _setupGameplaySetupAttacher = SymbolExtensions.GetMethodInfo(() => SetupGameplaySetupAttacher(null!, false, false, false, false, 0));

        [AffinityTranspiler]
        [AffinityPatch(typeof(GameServerLobbyFlowCoordinator), "DidActivate")]
        private IEnumerable<CodeInstruction> EnableEnvironmentTab(IEnumerable<CodeInstruction> instructions) =>
            new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _setupGameplaySetup))
                .Set(OpCodes.Callvirt, _setupGameplaySetupAttacher)
                .InstructionEnumeration();

        private static void SetupGameplaySetupAttacher(GameplaySetupViewController gameplaySetup, bool showModifiers, bool showEnvironmentOverrideSettings, bool showColorSchemesSettings, bool showMultiplayer, PlayerSettingsPanelController.PlayerSettingsPanelLayout playerSettingsPanelLayout) =>
            gameplaySetup.Setup(showModifiers, true, showColorSchemesSettings, showMultiplayer, playerSettingsPanelLayout);

        private EnvironmentInfoSO _originalEnvironmentInfo = null!;

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerLevelScenesTransitionSetupDataSO), "Init")]
        private void SetEnvironmentScene(IDifficultyBeatmap difficultyBeatmap, ref EnvironmentInfoSO ____multiplayerEnvironmentInfo)
        {
            if (_gameplaySetup.environmentOverrideSettings.overrideEnvironments)
            {
                _originalEnvironmentInfo = ____multiplayerEnvironmentInfo;
                ____multiplayerEnvironmentInfo = _gameplaySetup.environmentOverrideSettings.GetOverrideEnvironmentInfoForType(difficultyBeatmap.GetEnvironmentInfo().environmentType);
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(MultiplayerLevelScenesTransitionSetupDataSO), "Init")]
        private void ResetEnvironmentScene(IDifficultyBeatmap difficultyBeatmap, ref EnvironmentInfoSO ____multiplayerEnvironmentInfo)
        {
            if (_gameplaySetup.environmentOverrideSettings.overrideEnvironments)
            {
                ____multiplayerEnvironmentInfo = _originalEnvironmentInfo;
            }
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(ScenesTransitionSetupDataSO), "Init")]
        private void AddEnvironmentOverrides(ref SceneInfo[] scenes)
        {
            if (_gameplaySetup.environmentOverrideSettings.overrideEnvironments && scenes.Any(scene => scene.name.Contains("Multiplayer")))
            {
                scenes = scenes.AddItem(_originalEnvironmentInfo.sceneInfo).ToArray();
            }
        }

        private List<MonoBehaviour> _behavioursToInject = new();

        [AffinityPostfix]
        [AffinityPatch(typeof(SceneDecoratorContext), "GetInjectableMonoBehaviours")]
        private void PreventEnvironmentInjection(SceneDecoratorContext __instance, List<MonoBehaviour> monoBehaviours, DiContainer ____container)
        {
            var scene = __instance.gameObject.scene;
            if (_scenesManager.IsSceneInStack("MultiplayerEnvironment") && _gameplaySetup.environmentOverrideSettings.overrideEnvironments)
            {
                _logger.Info($"Fixing bind conflicts on scene '{scene.name}'.");
                List<MonoBehaviour> removedBehaviours = new();

                //if (scene.name == "MultiplayerEnvironment")
                //    removedBehaviours = monoBehaviours.FindAll(behaviour => behaviour is ZenjectBinding binding && binding.Components.Any(c => c is LightWithIdManager));
                if (scene.name.Contains("Environment") && !scene.name.Contains("Multiplayer"))
                    removedBehaviours = monoBehaviours.FindAll(behaviour => (behaviour is ZenjectBinding binding && 
                    binding.Components.Any(c => c is LightWithIdManager)));

                if (removedBehaviours.Any())
                {
                    _logger.Info($"Removing behaviours '{string.Join(", ", removedBehaviours.Select(behaviour => behaviour.GetType()))}' from scene '{scene.name}'.");
                    monoBehaviours.RemoveAll(monoBehaviour => removedBehaviours.Contains(monoBehaviour));
                }

                if (scene.name.Contains("Environment") && !scene.name.Contains("Multiplayer"))
                {
                    _logger.Info($"Preventing environment injection.");
                    _behavioursToInject = new(monoBehaviours);
                    monoBehaviours.Clear();
                }
            }
        }

        private List<GameObject> objectsToEnable = new();

        [AffinityPrefix]
        [AffinityPatch(typeof(GameScenesManager), "ActivatePresentedSceneRootObjects")]
        private void PreventEnvironmentActivation(List<string> scenesToPresent)
        {
            string defaultScene = scenesToPresent.FirstOrDefault(scene => scene.Contains("Environment") && !scene.Contains("Multiplayer"));
            if (defaultScene != null)
            {
                if (scenesToPresent.Contains("MultiplayerEnvironment"))
                {
                    _logger.Info($"Preventing environment activation. ({defaultScene})");
                    objectsToEnable = SceneManager.GetSceneByName(defaultScene).GetRootGameObjects().ToList();
                    scenesToPresent.Remove(defaultScene);
                } 
                else
                {
                    // Make sure hud is enabled in solo
                    var sceneObjects = SceneManager.GetSceneByName(defaultScene).GetRootGameObjects().ToList();
                    foreach (GameObject gameObject in sceneObjects)
                    {
                        var hud = gameObject.transform.GetComponentInChildren<CoreGameHUDController>();
                        if (hud != null)
                            hud.gameObject.SetActive(true);
                    }
                }
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(GameObjectContext), "GetInjectableMonoBehaviours")]
        private void InjectEnvironment(GameObjectContext __instance, List<MonoBehaviour> monoBehaviours)
        {
            if (__instance.transform.name.Contains("LocalActivePlayer") && _gameplaySetup.environmentOverrideSettings.overrideEnvironments)
            {
                _logger.Info($"Injecting environment.");
                monoBehaviours.AddRange(_behavioursToInject);
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(GameObjectContext), "RunInternal")]
        private void ActivateEnvironment(GameObjectContext __instance, DiContainer ____container)
        {
            if (__instance.transform.name.Contains("LocalActivePlayer"))
            {
                if (_gameplaySetup.environmentOverrideSettings.overrideEnvironments)
                {
                    _logger.Info($"Activating environment.");
                    ____container.Inject(____container.Resolve<EnvironmentColorManager>());

                    foreach (GameObject gameObject in objectsToEnable)
                    {
                        gameObject.SetActive(true);
                        var hud = gameObject.transform.GetComponentInChildren<CoreGameHUDController>();
                        if (hud != null)
                            hud.gameObject.SetActive(false);
                    }

                    var activeObjects = __instance.transform.Find("IsActiveObjects");
                    activeObjects.Find("Lasers").gameObject.SetActive(false);
                    activeObjects.Find("Construction").gameObject.SetActive(false);
                }
            }
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(EnvironmentSceneSetup), nameof(EnvironmentSceneSetup.InstallBindings))]
        private bool RemoveDuplicateInstalls(EnvironmentSceneSetup __instance)
        {
            DiContainer container = __instance.GetProperty<DiContainer, MonoInstallerBase>("Container");
            return !container.HasBinding<EnvironmentBrandingManager.InitData>();
        }
    }
}