using Arcade.UI.MenuStates;
using Challenges;
using MelonLoader;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace UNBEATABLE_Discord_RPC
{
    public class CustomEvents
    {
        public delegate void ArcadeMenuStateChange(ArcadeMenuState from, ArcadeMenuState to, bool instant);
        public static ArcadeMenuStateChange OnArcadeMenuStateChange;

        public delegate void ChallengeSelectedChange(int index, BaseChallengeDescriptor baseChallengeDescriptor);
        public static ChallengeSelectedChange OnChallengeSelectedChange;

        public delegate void SetChallengeBoard(ChallengeBoardDescriptor boardDescriptor);
        public static SetChallengeBoard OnSetChallengeBoard;

        /*public delegate void PausedStateChange(PausedState pauseState);
        public static PausedStateChange OnPausedStateChange;
        public static void PausedStateChanged()
        {
            previousPausedState = JeffBezosController.pausedState;
            OnPausedStateChange.Invoke(JeffBezosController.pausedState);
        }

        private static PausedState previousPausedState = PausedState.None;
        public static void PauseStateChangedDebounced()
        {
            if (JeffBezosController.pausedState != previousPausedState)
            {
                previousPausedState = JeffBezosController.pausedState;
                OnPausedStateChange.Invoke(JeffBezosController.pausedState);
            }
        }*/

        public delegate void RhythmResume();
        public static RhythmResume OnRhythmResume;

        public delegate void StartStory();
        public static StartStory OnStartStory;
    }
}
