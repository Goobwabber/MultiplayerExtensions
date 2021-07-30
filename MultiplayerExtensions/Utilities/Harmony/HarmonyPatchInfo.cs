using HarmonyLib;
using System;
using System.Reflection;

namespace MultiplayerExtensions.Utilities
{
    public class HarmonyPatchInfo
    {
        public Harmony HarmonyInstance { get; set; }
        public MethodBase OriginalMethod { get; protected set; }
        public HarmonyMethod? PrefixMethod { get; protected set; }
        public HarmonyMethod? PostfixMethod { get; protected set; }
        public HarmonyMethod? TranspilerMethod { get; protected set; }
        public bool IsApplied { get; protected set; }

        public HarmonyPatchInfo(Harmony harmony, MethodBase original, HarmonyMethod? prefix, HarmonyMethod? postfix, HarmonyMethod transpiler)
        {
            HarmonyInstance = harmony;
            OriginalMethod = original ?? throw new ArgumentNullException(nameof(original), $"{nameof(original)} cannot be null when creating a HarmonyPatchInfo.");
            if (prefix == null && postfix == null && transpiler == null)
                throw new ArgumentException("Prefix, Postfix and Transpiler cannot all be null.");
            PrefixMethod = prefix;
            PostfixMethod = postfix;
            TranspilerMethod = transpiler;
        }
        public HarmonyPatchInfo(Harmony harmony, MethodBase original, MethodInfo? prefix, MethodInfo? postfix, MethodInfo transpiler)
        {
            HarmonyInstance = harmony;
            OriginalMethod = original ?? throw new ArgumentNullException(nameof(original), $"{nameof(original)} cannot be null when creating a HarmonyPatchInfo.");
            if (prefix == null && postfix == null && transpiler == null)
                throw new ArgumentException("Prefix, Postfix and Transpiler cannot all be null.");
            if (prefix != null)
                PrefixMethod = GetHarmonyMethod(prefix);
            if (postfix != null)
                PostfixMethod = GetHarmonyMethod(postfix);
            if (transpiler != null)
                TranspilerMethod = GetHarmonyMethod(transpiler);
        }

        private HarmonyMethod GetHarmonyMethod(MethodInfo methodInfo)
        {
            HarmonyMethod method = new HarmonyMethod(methodInfo);
            HarmonyPriority? priority = methodInfo.GetCustomAttribute<HarmonyPriority>();
            HarmonyBefore? beforePatches = methodInfo.GetCustomAttribute<HarmonyBefore>();
            HarmonyAfter? afterPatches = methodInfo.GetCustomAttribute<HarmonyAfter>();
            if (priority != null)
                method.priority = priority.info.priority;
            if (beforePatches != null)
                method.before = beforePatches.info.before;
            if (afterPatches != null)
                method.after = afterPatches.info.after;

            return method;
        }

        public bool ApplyPatch(Harmony? harmony = null)
        {
            if (harmony == null)
                harmony = HarmonyInstance ?? throw new ArgumentNullException(nameof(harmony), $"Must have a non-null HarmonyInstance for ApplyPatch()");
            if (IsApplied) return false;
            try
            {
                string? patchTypeName = null;
                if (PrefixMethod != null)
                    patchTypeName = PrefixMethod.method.DeclaringType?.Name;
                else if (PostfixMethod != null)
                    patchTypeName = PostfixMethod.method.DeclaringType?.Name;
                else if (TranspilerMethod != null)
                    patchTypeName = TranspilerMethod.method.DeclaringType?.Name;
                //Plugin.Log?.Debug($"Harmony patching '{OriginalMethod.Name}' with '{patchTypeName}'");
                harmony.Patch(OriginalMethod, PrefixMethod, PostfixMethod, TranspilerMethod);
                IsApplied = true;
                HarmonyManager.AppliedPatches.Add(this);
                return true;
            }
            catch (Exception e)
            {
                Plugin.Log?.Error($"Unable to patch method {OriginalMethod.Name}: {e.Message}");
                Plugin.Log?.Debug(e);
                return false;
            }
        }

        public bool RemovePatch(Harmony? harmony = null)
        {
            if (harmony == null)
                harmony = HarmonyInstance ?? throw new ArgumentNullException(nameof(harmony), $"Must have a non-null HarmonyInstance for ApplyPatch()");
            string? patchTypeName = null;
            if (PrefixMethod != null)
                patchTypeName = PrefixMethod.method.DeclaringType?.Name;
            else if (PostfixMethod != null)
                patchTypeName = PostfixMethod.method.DeclaringType?.Name;
            else if (TranspilerMethod != null)
                patchTypeName = TranspilerMethod.method.DeclaringType?.Name;
            Plugin.Log?.Debug($"Removing Harmony patch '{patchTypeName}' from '{OriginalMethod.Name}'");
            if (PrefixMethod != null)
                harmony.Unpatch(OriginalMethod, PrefixMethod.method);
            if (PostfixMethod != null)
                harmony.Unpatch(OriginalMethod, PostfixMethod.method);
            if (TranspilerMethod != null)
                harmony.Unpatch(OriginalMethod, TranspilerMethod.method);
            IsApplied = false;
            HarmonyManager.AppliedPatches.Remove(this);
            return true;
        }
    }
}
