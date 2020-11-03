using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerExtensions.HarmonyPatches
{
    public class HarmonyPatchInfo
    {
        public Harmony HarmonyInstance { get; set; }
        public MethodInfo OriginalMethod { get; protected set; }
        public HarmonyMethod? PrefixMethod { get; protected set; }
        public HarmonyMethod? PostfixMethod { get; protected set; }
        public bool IsApplied { get; protected set; }

        public HarmonyPatchInfo(Harmony harmony, MethodInfo original, HarmonyMethod? prefix, HarmonyMethod? postfix)
        {
            HarmonyInstance = harmony;
            OriginalMethod = original ?? throw new ArgumentNullException(nameof(original), $"{nameof(original)} cannot be null when creating a HarmonyPatchInfo.");
            if (prefix == null && postfix == null)
                throw new ArgumentException("Prefix and postfix cannot both be null.");
            PrefixMethod = prefix;
            PostfixMethod = postfix;
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
                Plugin.Log?.Debug($"Harmony patching '{OriginalMethod.Name}' with '{patchTypeName}'");
                harmony.Patch(OriginalMethod, PrefixMethod, PostfixMethod);
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
            Plugin.Log?.Debug($"Removing Harmony patch '{patchTypeName}' from '{OriginalMethod.Name}'");
            if (PrefixMethod != null)
                harmony.Unpatch(OriginalMethod, PrefixMethod.method);
            if (PostfixMethod != null)
                harmony.Unpatch(OriginalMethod, PostfixMethod.method);
            IsApplied = false;
            HarmonyManager.AppliedPatches.Remove(this);
            return true;
        }
    }
}
