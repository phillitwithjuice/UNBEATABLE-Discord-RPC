using Arcade.UI.MenuStates;
using Challenges;
using HarmonyLib;
using MelonLoader;
using Minigames.Moped;
using Rhythm;
using Sirenix.Utilities;
using System.Reflection;
using System.Reflection.Emit;

namespace UNBEATABLE_Discord_RPC
{
    [HarmonyPatch(typeof(ArcadeMenuStateMachine), "SetCurrentState")]
    class ArcadeMenuStateMachinePatch1
    {
        static void Postfix(ref ArcadeMenuStateMachine __instance, ref bool instant)
        {
            CustomEvents.OnArcadeMenuStateChange.Invoke(__instance.PreviousState, __instance.CurrentState, instant);
        }
    }

    [HarmonyPatch(typeof(ChallengesView), nameof(ChallengesView.OnChallengeSelectedChange))]
    class ChallengesViewPatch1
    {
        static void Postfix(ref ChallengeButton challengeButton)
        {
            CustomEvents.OnChallengeSelectedChange.Invoke(challengeButton.Index, challengeButton.ChallengeDescriptor);
        }
    }

    [HarmonyPatch(typeof(ChallengesView), nameof(ChallengesView.SetSelectedChallenge))]
    class ChallengesViewPatch2
    {
        static void Postfix(ref ChallengeBoardDescriptor boardDescriptor)
        {
            CustomEvents.OnSetChallengeBoard.Invoke(boardDescriptor);
        }
    }

    /*[HarmonyPatch]
    class JeffBezosControllerPatch1
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            return [
                AccessTools.Method(typeof(JeffBezosController), "OdinReset"),
                AccessTools.Method(typeof(JeffBezosController), "OnLevelFinishedLoading"),
                AccessTools.Method(typeof(JeffBezosController), "Update"),
                AccessTools.Method(typeof(LevelManager), "OnFinishedLoading"),
                AccessTools.Method(typeof(MopedGameManager), nameof(MopedGameManager.NuclearCleanup)),
                AccessTools.Method(typeof(QuestJournalController), "PlayNotification"),
                AccessTools.Method(typeof(QuestJournalController), "DismissNotification"),
                AccessTools.Method(typeof(LoopTracker), nameof(LoopTracker.PleaseJustReset)),
                AccessTools.Method(typeof(RhythmController), "Update"),
            ];
        }

        static FieldInfo f_pausedState = AccessTools.Field(typeof(JeffBezosController), nameof(JeffBezosController.pausedState));
        static MethodInfo m_OnPauseStateChange = SymbolExtensions.GetMethodInfo(() => CustomEvents.PausedStateChanged());
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            var found = false;
            foreach (var instruction in instructions)
            {
                yield return instruction;
                if (instruction.StoresField(f_pausedState))
                {
                    yield return new CodeInstruction(OpCodes.Call, m_OnPauseStateChange);
                    found = true;
                }
            }
            if (found is false)
                Melon<Core>.Logger.Error($"Cannot find {f_pausedState} in {original}");
        }
    }

    [HarmonyPatch]
    class JeffBezosControllerPatch2
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            return [
                AccessTools.Method(typeof(LevelManager), "TransitionalLoad"),
                AccessTools.Method(typeof(PauseMenu), "OptionRender"),
            ];
        }

        static FieldInfo f_pausedState = AccessTools.Field(typeof(JeffBezosController), nameof(JeffBezosController.pausedState));
        static MethodInfo m_OnPauseStateChange = SymbolExtensions.GetMethodInfo(() => CustomEvents.PauseStateChangedDebounced());
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            var found = false;
            foreach (var instruction in instructions)
            {
                yield return instruction;
                if (instruction.StoresField(f_pausedState))
                {
                    yield return new CodeInstruction(OpCodes.Call, m_OnPauseStateChange);
                    found = true;
                }
            }
            if (found is false)
                Melon<Core>.Logger.Error($"Cannot find {f_pausedState} in {original}");
        }
    }*/


    [HarmonyPatch(typeof(RhythmTracker), nameof(RhythmTracker.Resume))]
    class RhythmTrackerPatch1
    {
        static void Postfix()
        {
            CustomEvents.OnRhythmResume.Invoke();
        }
    }


    [HarmonyPatch(typeof(StorySlotUI), "StartSlot")]
    class StorySlotUIPatch1
    {
        static void Postfix()
        {
            CustomEvents.OnStartStory.Invoke();
        }
    }
}
