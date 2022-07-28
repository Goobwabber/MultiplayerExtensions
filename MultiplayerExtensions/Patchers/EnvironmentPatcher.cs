using HarmonyLib;
using IPA.Utilities;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace MultiplayerExtensions.Patchers
{
    [HarmonyPatch]
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
            else
            {
                _behavioursToInject.Clear();
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
            else if (!_scenesManager.IsSceneInStack("MultiplayerEnvironment"))
            {
                _normalInstallers.Clear();
                _normalInstallerTypes.Clear();
                _scriptableObjectInstallers.Clear();
                _monoInstallers.Clear();
                _installerPrefabs.Clear();
            }
        }

        private List<GameObject> _objectsToEnable = new();

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
                    _objectsToEnable = SceneManager.GetSceneByName(defaultScene).GetRootGameObjects().ToList();
                    scenesToPresent.Remove(defaultScene);

                    // fix ring lighting dogshit
                    var trackLaneRingManagers = _objectsToEnable[0].transform.GetComponentsInChildren<TrackLaneRingsManager>();
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
        [AffinityPatch(typeof(Context), "InstallInstallers", AffinityMethodType.Normal, null, typeof(List<InstallerBase>), typeof(List<Type>), typeof(List<ScriptableObjectInstaller>), typeof(List<MonoInstaller>), typeof(List<MonoInstaller>))]
        private void InstallEnvironment(Context __instance, List<InstallerBase> normalInstallers, List<Type> normalInstallerTypes, List<ScriptableObjectInstaller> scriptableObjectInstallers, List<MonoInstaller> installers, List<MonoInstaller> installerPrefabs)
        {
            if (__instance is GameObjectContext instance && __instance.transform.name.Contains("LocalActivePlayer") && _config.SoloEnvironment)
            {
                _logger.Info($"Installing environment.");
                normalInstallers.AddRange(_normalInstallers);
                normalInstallerTypes.AddRange(_normalInstallerTypes);
                scriptableObjectInstallers.AddRange(_scriptableObjectInstallers);
                installers.AddRange(_monoInstallers);
                installerPrefabs.AddRange(_installerPrefabs);
            }
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(GameObjectContext), "InstallInstallers")]
        private void LoveYouCountersPlus(GameObjectContext __instance)
        {
            if (__instance.transform.name.Contains("LocalActivePlayer") && _config.SoloEnvironment)
            {
                DiContainer container = __instance.GetProperty<DiContainer, GameObjectContext>("Container");
                var hud = (CoreGameHUDController)_behavioursToInject.Find(x => x is CoreGameHUDController);
                container.Bind<CoreGameHUDController>().FromInstance(hud).AsSingle();
                var multihud = __instance.transform.GetComponentInChildren<CoreGameHUDController>();
                multihud.gameObject.SetActive(false);
                var multiPositionHud = __instance.transform.GetComponentInChildren<MultiplayerPositionHUDController>();
                multiPositionHud.transform.position += new Vector3(0, 0.01f, 0);
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(GameObjectContext), "InstallSceneBindings")]
        private void ActivateEnvironment(GameObjectContext __instance)
        {
            if (__instance.transform.name.Contains("LocalActivePlayer") && _config.SoloEnvironment)
            {
                _logger.Info($"Activating environment.");
                foreach (GameObject gameObject in _objectsToEnable)
                    gameObject.SetActive(true);

                var activeObjects = __instance.transform.Find("IsActiveObjects");
                activeObjects.Find("Lasers").gameObject.SetActive(false);
                activeObjects.Find("Construction").gameObject.SetActive(false);
                activeObjects.Find("BigSmokePS").gameObject.SetActive(false);
                activeObjects.Find("DustPS").gameObject.SetActive(false);
                activeObjects.Find("DirectionalLights").gameObject.SetActive(false);

                var localActivePlayer = __instance.transform.GetComponent<MultiplayerLocalActivePlayerFacade>();
                var activeOnlyGameObjects = localActivePlayer.GetField<GameObject[], MultiplayerLocalActivePlayerFacade>("_activeOnlyGameObjects");
                var newActiveOnlyGameObjects = activeOnlyGameObjects.Concat(_objectsToEnable);
                localActivePlayer.SetField("_activeOnlyGameObjects", newActiveOnlyGameObjects.ToArray());
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Context), "InstallSceneBindings")]
        private static void HideOtherPlayerPlatforms(Context __instance)
        {
            if (__instance.transform.name.Contains("ConnectedPlayer"))
            {
                if (Plugin.Config.DisableMultiplayerPlatforms)
                    __instance.transform.Find("Construction").gameObject.SetActive(false);
                if (Plugin.Config.DisableMultiplayerLights)
                    __instance.transform.Find("Lasers").gameObject.SetActive(false);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnvironmentSceneSetup), nameof(EnvironmentSceneSetup.InstallBindings))]
        private static bool RemoveDuplicateInstalls(EnvironmentSceneSetup __instance)
        {
            DiContainer container = __instance.GetProperty<DiContainer, MonoInstallerBase>("Container");
            return !container.HasBinding<EnvironmentBrandingManager.InitData>();
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(GameplayCoreInstaller), nameof(GameplayCoreInstaller.InstallBindings))]
        private void SetEnvironmentColors(GameplayCoreInstaller __instance)
        {
            if (!_config.SoloEnvironment || !_scenesManager.IsSceneInStack("MultiplayerEnvironment"))
                return;

            DiContainer container = __instance.GetProperty<DiContainer, MonoInstallerBase>("Container");
            var colorManager = container.Resolve<EnvironmentColorManager>();
            container.Inject(colorManager);
            colorManager.Awake();
            colorManager.Start();

            foreach (var gameObject in _objectsToEnable)
            {
                var lightSwitchEventEffects = gameObject.transform.GetComponentsInChildren<LightSwitchEventEffect>();
                foreach (var component in lightSwitchEventEffects)
                    component.Awake();
            }
        }
    }
}
