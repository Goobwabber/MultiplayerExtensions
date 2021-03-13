using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MultiplayerExtensions.HarmonyPatches
{
    public static class HarmonyManager
    {
        public static readonly string HarmonyId = "com.github.Zingabopp.MultiplayerExtensions";
        private static Harmony? _harmony;
        internal static Harmony Harmony
        {
            get
            {
                return _harmony ??= new Harmony(HarmonyId);
            }
        }
        internal static readonly BindingFlags allBindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        internal static readonly BindingFlags allInstanceBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        internal static readonly BindingFlags allStaticBindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        internal static readonly HashSet<HarmonyPatchInfo> AppliedPatches = new HashSet<HarmonyPatchInfo>();
        internal static readonly HashSet<HarmonyPatchInfo> DefaultPatches = new HashSet<HarmonyPatchInfo>();

        static HarmonyManager()
        {
            AddDefaultPatch<EnableCustomLevelsPatch>();
            AddDefaultPatch<CustomLevelEntitlementPatch>();
            AddDefaultPatch<MultiplayerBigAvatarAnimator_Init>();
            AddDefaultPatch<CoreGameHUDController_Start>();
            AddDefaultPatch<LoggingPatch>();
            AddDefaultPatch<GetMasterServerEndPointPatch>();
            AddDefaultPatch<SetLobbyCodePatch>();
            AddDefaultPatch<LobbyEnvironmentLoadPatch>();
            AddDefaultPatch<StartGameLevelEntitlementPatch>();
            AddDefaultPatch<UpdateReliableFrequencyPatch>();
            AddDefaultPatch<UpdateUnreliableFrequencyPatch>();
            AddDefaultPatch<PlayerPlacementAnglePatch>();
            AddDefaultPatch<PlayerLayoutSpotsCountPatch>();
            AddDefaultPatch<IncreaseMaxPlayersClampPatch>();
            AddDefaultPatch<IncreaseMaxPlayersPatch>();
            AddDefaultPatch<MissingLevelStartPatch>();
            AddDefaultPatch<ConnectedPlayerInstallerPatch>();
            AddDefaultPatch<CenterStageGameDataPatch>();
            AddDefaultPatch<DisableSpeedModifiersPatch>();
            AddDefaultPatch<AprilFoolsPatch>();
            //AddDefaultPatch<RemoveByteLimitPatch>(); (doesn't support generics)
        }

        private static void AddDefaultPatch<T>() where T : class
        {
            HarmonyPatchInfo? patch = GetPatch<T>();
            if (patch != null)
                DefaultPatches.Add(patch);
            else
                Plugin.Log?.Warn($"Could not add default patch '{typeof(T).Name}'");
        }

        internal static bool ApplyPatch(HarmonyPatchInfo patchInfo)
        {
            bool applied = patchInfo.ApplyPatch(Harmony);
            if (applied)
                AppliedPatches.Add(patchInfo);
            return applied;
        }

        internal static bool RemovePatch(HarmonyPatchInfo patchInfo)
        {
            bool removed = patchInfo.RemovePatch(Harmony);
            if (removed)
                AppliedPatches.Remove(patchInfo);
            return removed;
        }

        internal static bool ApplyPatch(Harmony harmony, MethodInfo original, HarmonyMethod? prefix = null, HarmonyMethod? postfix = null, HarmonyMethod? transpiler = null)
        {
            try
            {
                string? patchTypeName = null;
                if (prefix != null)
                    patchTypeName = prefix.method.DeclaringType?.Name;
                else if (postfix != null)
                    patchTypeName = postfix.method.DeclaringType?.Name;
                else if (transpiler != null)
                    patchTypeName = transpiler.method.DeclaringType?.Name;
                //Plugin.Log?.Debug($"Harmony patching {original.Name} with {patchTypeName}");
                harmony.Patch(original, prefix, postfix, transpiler);
                return true;
            }
            catch (Exception e)
            {
                Plugin.Log?.Error($"Unable to patch method {original.Name}: {e.Message}");
                Plugin.Log?.Debug(e);
                return false;
            }
        }

        internal static void ApplyDefaultPatches()
        {
            HarmonyPatchInfo[] patches = DefaultPatches.ToArray();
            Plugin.Log?.Debug($"Applying {patches.Length} Harmony patches.");
            for (int i = 0; i < patches.Length; i++)
                ApplyPatch(patches[i]);
        }

        internal static void UnpatchAll()
        {
            foreach (HarmonyPatchInfo? patch in AppliedPatches.ToList())
            {
                patch.RemovePatch();
            }
            Harmony.UnpatchAll(HarmonyId);
        }



        public static HarmonyPatchInfo? GetPatch<T>() where T : class
        {
            return GetPatch(typeof(T));
        }

        /// <summary>
        /// Attempts to create a <see cref="HarmonyPatchInfo"/> from a patch class annotated with <see cref="HarmonyPatch"/>.
        /// Returns null after logging errors if it fails.
        /// </summary>
        /// <param name="patchClass"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="patchClass"/> is null.</exception>
        public static HarmonyPatchInfo? GetPatch(Type patchClass)
        {
            if (patchClass == null) throw new ArgumentNullException(nameof(patchClass), "patchClass cannot be null.");
            try
            {
                //Plugin.Log?.Debug($"Getting patch info for {patchClass.Name}");
                HarmonyPatch[] patches = patchClass.GetCustomAttributes<HarmonyPatch>().ToArray() ?? Array.Empty<HarmonyPatch>();
                if (patches.Length == 0)
                    throw new ArgumentException($"Type '{patchClass.Name}' has no 'HarmonyPatch' annotations.");
                //Plugin.Log?.Debug($"  Found {patches.Length} HarmonyPatch annotations.");
                Type? originalType = null;
                string? originalMemberName = null;
                MethodType methodType = MethodType.Normal;
                Type[] parameters = Array.Empty<Type>();
                for (int i = 0; i < patches.Length; i++)
                {
                    HarmonyMethod info = patches[i].info;
                    if (info.declaringType != null)
                    {
                        originalType = info.declaringType;
                    }
                    if (!string.IsNullOrEmpty(info.methodName))
                    {
                        originalMemberName = info.methodName;
                    }
                    if (info.methodType.HasValue)
                    {
                        methodType = info.methodType.Value;
                    }
                    if ((info.argumentTypes?.Length ?? 0) > 0)
                    {
                        parameters = info.argumentTypes!;
                    }
                }
                if (originalType == null)
                    throw new ArgumentException($"Original type could not be determined.");
                if (methodType == MethodType.Normal && (originalMemberName == null || originalMemberName.Length == 0))
                    methodType = MethodType.Constructor;
                //Plugin.Log?.Debug($"  Attempting to create patch for {GetPatchedMethodString(originalType.Name, originalMemberName, methodType, parameters)}");
                MethodBase originalMethod = methodType switch
                {
                    MethodType.Normal => GetMethod(originalType, originalMemberName!, parameters),
                    MethodType.Getter => originalType.GetProperty(originalMemberName, allBindingFlags).GetMethod,
                    MethodType.Setter => originalType.GetProperty(originalMemberName, allBindingFlags).SetMethod,
                    MethodType.Constructor => originalType.GetConstructor(allInstanceBindingFlags, null, parameters, Array.Empty<ParameterModifier>()),
                    MethodType.StaticConstructor => throw new NotImplementedException("Static constructor patches are not supported."),
                    //originalType.GetConstructor(allStaticBindingFlags, null, parameters, Array.Empty<ParameterModifier>()),
                    _ => throw new NotImplementedException($"MethodType '{methodType}' is unrecognized.")
                };
                if (originalMethod == null)
                    throw new ArgumentException($"Could not find original method '{originalType.Name}.{originalMemberName}'.");
                MethodInfo prefix = patchClass.GetMethod("Prefix", allBindingFlags);
                MethodInfo postfix = patchClass.GetMethod("Postfix", allBindingFlags);
                MethodInfo transpiler = patchClass.GetMethod("Transpiler", allBindingFlags);
                return new HarmonyPatchInfo(Harmony, originalMethod, prefix, postfix, transpiler);
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Error getting Harmony patch '{patchClass.Name}'");
                Plugin.Log.Debug(ex);
                return null;
            }
        }

        private static string GetPatchedMethodString(string originalTypeName, string? originalMemberName, MethodType methodType, Type[] parameters)
        {
            return methodType switch
            {
                MethodType.Normal => $"Method: '{originalTypeName}.{originalMemberName}({string.Join(", ", parameters.Select(p => p.Name))})'",
                MethodType.Getter => $"Property Getter: '{originalTypeName}.{originalMemberName}'",
                MethodType.Setter => $"Property Setter: '{originalTypeName}.{originalMemberName}'",
                MethodType.Constructor => $"Constructor: '{originalTypeName}({string.Join(", ", parameters.Select(p => p.Name))})'",
                MethodType.StaticConstructor => $"Static Constructor: '{originalTypeName}({string.Join(", ", parameters.Select(p => p.Name))})'",
                _ => $"Unknown MethodType: '{methodType}'",
            };
        }

        private static MethodInfo GetMethod(Type originalType, string name, Type[]? parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return originalType.GetMethod(name, allBindingFlags);
            else
                return originalType.GetMethod(name, allBindingFlags, null, parameters, Array.Empty<ParameterModifier>());
        }
    }
}
