using HarmonyLib;
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
        private readonly GameScenesManager _scenesManager;
        private readonly Config _config;
        private readonly SiraLog _logger;

        internal EnvironmentPatcher(
            GameScenesManager scenesManager,
            Config config,
            SiraLog logger)
        {
            _scenesManager = scenesManager;
            _config = config;
            _logger = logger;
        }

        private List<MonoBehaviour> _behavioursToInject = new();

        [AffinityPostfix]
        [AffinityPatch(typeof(SceneDecoratorContext), "GetInjectableMonoBehaviours")]
        private void PreventEnvironmentInjection(SceneDecoratorContext __instance, List<MonoBehaviour> monoBehaviours, DiContainer ____container)
        {
            var scene = __instance.gameObject.scene;
            if (_scenesManager.IsSceneInStack("MultiplayerEnvironment") && _config.SoloEnvironment)
            {
                _logger.Info($"Fixing bind conflicts on scene '{scene.name}'.");
                List<MonoBehaviour> removedBehaviours = new();

                //if (scene.name == "MultiplayerEnvironment")
                //    removedBehaviours = monoBehaviours.FindAll(behaviour => behaviour is ZenjectBinding binding && binding.Components.Any(c => c is LightWithIdManager));
                if (scene.name.Contains("Environment") && !scene.name.Contains("Multiplayer"))
                    removedBehaviours = monoBehaviours.FindAll(behaviour => (behaviour is ZenjectBinding binding && binding.Components.Any(c => c is LightWithIdManager)));

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

        private List<InstallerBase> _normalInstallers = new();
        private List<Type> _normalInstallerTypes = new();
        private List<ScriptableObjectInstaller> _scriptableObjectInstallers = new();
        private List<MonoInstaller> _monoInstallers = new();
        private List<MonoInstaller> _installerPrefabs = new();

        [AffinityPrefix]
        [AffinityPatch(typeof(SceneDecoratorContext), "InstallDecoratorInstallers")]
        private void PreventEnvironmentInstall(SceneDecoratorContext __instance, List<InstallerBase> ____normalInstallers, List<Type> ____normalInstallerTypes, List<ScriptableObjectInstaller> ____scriptableObjectInstallers, List<MonoInstaller> ____monoInstallers, List<MonoInstaller> ____installerPrefabs)
        {
            var scene = __instance.gameObject.scene;
            if (_scenesManager.IsSceneInStack("MultiplayerEnvironment") && _config.SoloEnvironment && scene.name.Contains("Environment") && !scene.name.Contains("Multiplayer"))
            {
                _logger.Info($"Preventing environment installation.");

                _normalInstallers = new(____normalInstallers);
                _normalInstallerTypes = new(____normalInstallerTypes);
                _scriptableObjectInstallers = new(____scriptableObjectInstallers);
                _monoInstallers = new(____monoInstallers);
                _installerPrefabs = new(____installerPrefabs);

                ____normalInstallers.Clear();
                ____normalInstallerTypes.Clear();
                ____scriptableObjectInstallers.Clear();
                ____monoInstallers.Clear();
                ____installerPrefabs.Clear();
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

                    // fix ring lighting dogshit
                    _allowRingCreation = true;
                    var trackLaneRingManagers = objectsToEnable[0].transform.GetComponentsInChildren<TrackLaneRingsManager>();
                    foreach (var trackLaneRingManager in trackLaneRingManagers)
                        trackLaneRingManager.Awake();
                    _allowRingCreation = false;
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
            if (__instance.transform.name.Contains("LocalActivePlayer") && _config.SoloEnvironment)
            {
                _logger.Info($"Injecting environment.");
                monoBehaviours.AddRange(_behavioursToInject);
            }
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(GameObjectContext), "InstallInstallers")]
        private void InstallEnvironment(GameObjectContext __instance, List<InstallerBase> ____normalInstallers, List<Type> ____normalInstallerTypes, List<ScriptableObjectInstaller> ____scriptableObjectInstallers, List<MonoInstaller> ____monoInstallers, List<MonoInstaller> ____installerPrefabs)
        {
            if (__instance.transform.name.Contains("LocalActivePlayer") && _config.SoloEnvironment)
            {
                _logger.Info($"Installing environment.");
                ____normalInstallers.AddRange(_normalInstallers);
                ____normalInstallerTypes.AddRange(_normalInstallerTypes);
                ____scriptableObjectInstallers.AddRange(_scriptableObjectInstallers);
                ____monoInstallers.AddRange(_monoInstallers);
                ____installerPrefabs.AddRange(_installerPrefabs);
            }
        }

        private Dictionary<Type, MethodInfo?> _methodInfoCache = new();

        [AffinityPostfix]
        [AffinityPatch(typeof(GameObjectContext), "InstallSceneBindings")]
        private void ActivateEnvironment(GameObjectContext __instance)
        {
            if (__instance.transform.name.Contains("LocalActivePlayer") && _config.SoloEnvironment)
            {
                _logger.Info($"Activating environment.");
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
                activeObjects.Find("BigSmokePS").gameObject.SetActive(false);
                activeObjects.Find("DustPS").gameObject.SetActive(false);
            }
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(EnvironmentSceneSetup), nameof(EnvironmentSceneSetup.InstallBindings))]
        private bool RemoveDuplicateInstalls(EnvironmentSceneSetup __instance)
        {
            DiContainer container = __instance.GetProperty<DiContainer, MonoInstallerBase>("Container");
            return !container.HasBinding<EnvironmentBrandingManager.InitData>();
        }

        private bool _allowRingCreation = false;

        [AffinityPostfix]
        [AffinityPatch(typeof(GameplayCoreInstaller), nameof(GameplayCoreInstaller.InstallBindings))]
        private void SetEnvironmentColors(GameplayCoreInstaller __instance)
        {
            if (!_scenesManager.IsSceneInStack("MultiplayerEnvironment"))
                return;

            DiContainer container = __instance.GetProperty<DiContainer, MonoInstallerBase>("Container");
            var colorManager = container.Resolve<EnvironmentColorManager>();
            container.Inject(colorManager);
            colorManager.Awake();
            colorManager.Start();

            foreach (var gameObject in objectsToEnable)
            {
                var lightSwitchEventEffects = gameObject.transform.GetComponentsInChildren<LightSwitchEventEffect>();
                foreach (var component in lightSwitchEventEffects)
                    component.Awake();
            }
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(TrackLaneRingsManager), "Awake")]
        private bool PreventRingCreation()
        {
            if (!_scenesManager.IsSceneInStack("MultiplayerEnvironment"))
                return false;

            _logger.Info($"Allow ring creation: {_allowRingCreation}");
            return _allowRingCreation;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerLevelScenesTransitionSetupDataSO), "Init")]
        private void CustomSongColorsPatch(ref IDifficultyBeatmap difficultyBeatmap, ref ColorScheme? overrideColorScheme)
        {
            if (!_config.SoloEnvironment)
                return;
            var songData = SongCore.Collections.RetrieveDifficultyData(difficultyBeatmap);
            if (songData == null)
                return;
            if (songData._colorLeft == null && songData._colorRight == null && songData._envColorLeft == null && songData._envColorRight == null && songData._obstacleColor == null && songData._envColorLeftBoost == null && songData._envColorRightBoost == null)
                return;

            var environmentInfoSO = difficultyBeatmap.GetEnvironmentInfo();
            var fallbackScheme = overrideColorScheme ?? new ColorScheme(environmentInfoSO.colorScheme);

            _logger.Info("Custom Song Colors On");
            var saberLeft = songData._colorLeft == null ? fallbackScheme.saberAColor : ColorFromMapColor(songData._colorLeft);
            var saberRight = songData._colorRight == null ? fallbackScheme.saberBColor : ColorFromMapColor(songData._colorRight);
            var envLeft = songData._envColorLeft == null
                ? songData._colorLeft == null ? fallbackScheme.environmentColor0 : ColorFromMapColor(songData._colorLeft)
                : ColorFromMapColor(songData._envColorLeft);
            var envRight = songData._envColorRight == null
                ? songData._colorRight == null ? fallbackScheme.environmentColor1 : ColorFromMapColor(songData._colorRight)
                : ColorFromMapColor(songData._envColorRight);
            var envLeftBoost = songData._envColorLeftBoost == null ? envLeft : ColorFromMapColor(songData._envColorLeftBoost);
            var envRightBoost = songData._envColorRightBoost == null ? envRight : ColorFromMapColor(songData._envColorRightBoost);
            var obstacle = songData._obstacleColor == null ? fallbackScheme.obstaclesColor : ColorFromMapColor(songData._obstacleColor);
            overrideColorScheme = new ColorScheme("SongCoreMapColorScheme", "SongCore Map Color Scheme", true, "SongCore Map Color Scheme", false, saberLeft, saberRight, envLeft,
                envRight, true, envLeftBoost, envRightBoost, obstacle);
        }

        private Color ColorFromMapColor(SongCore.Data.ExtraSongData.MapColor mapColor) =>
            new Color(mapColor.r, mapColor.g, mapColor.b);
    }
}
