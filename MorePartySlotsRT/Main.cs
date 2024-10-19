using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.VM.GroupChanger;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.EntitySystem.Entities;
using static UnityModManagerNet.UnityModManager;
using UnityEngine;
using MorePartySlots;
using System.Reflection.Emit;

namespace MorePartySlotsRT;

public static class Main {
    internal static Harmony HarmonyInstance;
    internal static ModEntry.ModLogger log;
    internal static string text;
    internal static Settings settings;
    internal static ModEntry modEntry;

    public static bool Load(ModEntry modEntry) {
        Main.modEntry = modEntry;
        log = modEntry.Logger;
        modEntry.OnGUI = OnGUI;
        modEntry.OnSaveGUI = OnSaveGUI;
        HarmonyInstance = new Harmony(modEntry.Info.Id);
        settings = Settings.Load<Settings>(modEntry);
        text = settings.Slots.ToString();
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        return true;
    }
    static void OnSaveGUI(ModEntry modEntry) {
        settings.Save(modEntry);
    }

    public static void OnGUI(ModEntry modEntry) {
        GUILayout.Label($"Current Slots: {settings.Slots}", GUILayout.ExpandWidth(false));
        text = GUILayout.TextField(text, GUILayout.Width(100));
        if (GUILayout.Button("Apply Change", GUILayout.ExpandWidth(false))) {
            if (int.TryParse(text, out var tmp)) {
                settings.Slots = tmp;
                OnSaveGUI(modEntry);
                HarmonyInstance.UnpatchAll(modEntry.Info.Id);
                HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            }
        }
    }
    [HarmonyPatch]
    public static class Const_Patches {
        [HarmonyTargetMethods]
        static IEnumerable<MethodBase> GetMethods() {
            yield return AccessTools.Method(typeof(Recruit), nameof(Recruit.ShowPartyInterface));
            yield return AccessTools.Constructor(typeof(GroupChangerCommonVM), [typeof(Action), typeof(Action), typeof(List<UnitReference>), typeof(List<BlueprintUnit>), typeof(bool), typeof(bool), typeof(bool)]);
            yield return AccessTools.Method(typeof(GroupChangerVM), nameof(GroupChangerVM.CanMoveCharacterFromRemoteToParty));
            yield return AccessTools.Method(typeof(PartyMembersDetach), nameof(PartyMembersDetach.RunAction));
            yield return AccessTools.Method(typeof(PartyMembersDetachEvaluated), nameof(PartyMembersDetachEvaluated.RunAction));
            //yield return AccessTools.Constructor(typeof(PartyVM), []);
            //yield return AccessTools.PropertySetter(typeof(PartyVM), nameof(PartyVM.StartIndex));
            MethodBase i = null;
            try {
                i = AccessTools.Method(AccessTools.Inner(ModEntries.First(m => m.Info.Id == "PlayableX").Assembly.GetType("PlayableX.PlayableNavigator.NavigatorPatches"), "GroupChangerVM_CanMoveCharacterFromRemoteToParty_Patch"), "CanMoveCharacterFromRemoteToParty");
            } catch { }
            if (i != null) yield return i;
        }
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> TranspilerPatch(IEnumerable<CodeInstruction> instructions) {
            return ConvertConstants(instructions, settings.Slots);
        }

        private static OpCode[] LdConstants = {
            OpCodes.Ldc_I4_0,
            OpCodes.Ldc_I4_1,
            OpCodes.Ldc_I4_2,
            OpCodes.Ldc_I4_3,
            OpCodes.Ldc_I4_4,
            OpCodes.Ldc_I4_5,
            OpCodes.Ldc_I4_6,
            OpCodes.Ldc_I4_7,
            OpCodes.Ldc_I4_8,
        };

        public static IEnumerable<CodeInstruction> ConvertConstants(IEnumerable<CodeInstruction> instructions, int to) {
            Func<CodeInstruction> makeReplacement;
            if (to <= 8)
                makeReplacement = () => new CodeInstruction(LdConstants[to]);
            else
                makeReplacement = () => new CodeInstruction(OpCodes.Ldc_I4_S, to);

            foreach (var ins in instructions) {
                if (ins.opcode == OpCodes.Ldc_I4_6)
                    yield return makeReplacement().WithLabels(ins.labels);
                else
                    yield return ins;
            }
        }
    }
    [HarmonyPatch]
    public static class Const_Patches2 {
        [HarmonyTargetMethods]
        static IEnumerable<MethodBase> GetMethods() {
            yield return AccessTools.Method(typeof(Recruit), nameof(Recruit.ShowPartyInterface));
        }
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> TranspilerPatch(IEnumerable<CodeInstruction> instructions) {
            return ConvertConstants(instructions, settings.Slots - 1);
        }

        private static OpCode[] LdConstants = {
            OpCodes.Ldc_I4_0,
            OpCodes.Ldc_I4_1,
            OpCodes.Ldc_I4_2,
            OpCodes.Ldc_I4_3,
            OpCodes.Ldc_I4_4,
            OpCodes.Ldc_I4_5,
            OpCodes.Ldc_I4_6,
            OpCodes.Ldc_I4_7,
            OpCodes.Ldc_I4_8,
        };

        public static IEnumerable<CodeInstruction> ConvertConstants(IEnumerable<CodeInstruction> instructions, int to) {
            Func<CodeInstruction> makeReplacement;
            if (to <= 8)
                makeReplacement = () => new CodeInstruction(LdConstants[to]);
            else
                makeReplacement = () => new CodeInstruction(OpCodes.Ldc_I4_S, to);

            foreach (var ins in instructions) {
                if (ins.opcode == OpCodes.Ldc_I4_5)
                    yield return makeReplacement().WithLabels(ins.labels);
                else
                    yield return ins;
            }
        }
    }
}
