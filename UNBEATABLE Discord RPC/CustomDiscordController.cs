using Arcade.UI.MenuStates;
using Arcade.UI.SongSelect;
using Challenges;
using DiscordRPC;
using MelonLoader;
using Rhythm;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UNBEATABLE_Discord_RPC
{
    public class CustomDiscordController : MonoBehaviour
    {
        public static readonly Dictionary<string, string> DifficultyMap = new Dictionary<string, string>()
        {
            { "Beginner", "BEGINNER" },
            { "Easy", "NORMAL" },
            { "Normal", "HARD" },
            { "Hard", "Expert" },
            { "UNBEATABLE", "UNBEATABLE" },
        };

        public CustomDiscordComponent discordComponent;

        private string loadedSceneName = "";
        private Timestamps activityTimestamps;

        private RhythmGameContainer rhythmGameContainer;

        private ArcadeMenuStateMachine arcadeMenuStateMachine;

        private ArcadeSongList arcadeSongList;

        private HighScoreScreen highScoreScreen;

        private void Awake()
        {
            Melon<Core>.Logger.Msg("Custom Discord Controller injected");
            SceneManager.sceneLoaded += OnSceneLoad;
            CustomEvents.OnArcadeMenuStateChange += OnArcadeMenuStateChange;
            CustomEvents.OnChallengeSelectedChange += OnChallengeSelectedChange;
            CustomEvents.OnSetChallengeBoard += OnSetChallengeBoard;
            CustomEvents.OnRhythmPause += OnRythmPause;
            CustomEvents.OnRhythmResume += OnRhythmResume;
            CustomEvents.OnStartStory += OnStartStory;
            HighScoreScreen.ScoreScreenChallengeEvent += OnScoreEvent;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
            CustomEvents.OnArcadeMenuStateChange -= OnArcadeMenuStateChange;
            CustomEvents.OnChallengeSelectedChange -= OnChallengeSelectedChange;
            CustomEvents.OnSetChallengeBoard -= OnSetChallengeBoard;
            CustomEvents.OnRhythmPause -= OnRythmPause;
            CustomEvents.OnRhythmResume -= OnRhythmResume;
            CustomEvents.OnStartStory -= OnStartStory;
            HighScoreScreen.ScoreScreenChallengeEvent -= OnScoreEvent;
        }

        private void Start()
        {
            discordComponent.Presence = new DiscordRPC.RichPresence()
            {
                Details = "",
                State = "Getting Ready",
            };
        }

        private void OnSceneLoad(Scene LoadedScene, LoadSceneMode SceneMode)
        {
            UnityEngine.SceneManagement.Scene scene = (UnityEngine.SceneManagement.Scene)Convert.ChangeType(LoadedScene, typeof(UnityEngine.SceneManagement.Scene));
            loadedSceneName = scene.name;
            SetSceneState(afterLoad: true);
        }

        private void SetSceneState(bool afterLoad = false)
        {
            switch (JeffBezosController.rhythmGameType, loadedSceneName)
            {
                case (RhythmGameType.Story, "LogoScreen"):
                    discordComponent.Presence = new DiscordRPC.RichPresence()
                    {
                        Details = "",
                        State = "Getting Ready",
                    };
                    break;
                case (RhythmGameType.Story, "BootUp"):
                    break;
                case (RhythmGameType.Story, string sceneName) when sceneName == JeffBezosController.mainMenuScene:
                    SetMainMenuState(afterLoad: afterLoad);
                    break;
                case (RhythmGameType.Story, _):
                    SetStoryState(afterLoad: afterLoad);
                    break;
                case (RhythmGameType.ArcadeMode, _):
                    SetArcadeState(afterLoad: afterLoad);
                    break;
                case (RhythmGameType.WhiteLabel, _):
                    SetWhiteLabelState(afterLoad: afterLoad);
                    break;
            }
        }

        private void SetMainMenuState(bool afterLoad = false)
        {
            discordComponent.EnsureUNBEATABLEAppId();
            if (afterLoad)
            {
                activityTimestamps = Timestamps.Now;
            }
            discordComponent.Presence = new RichPresence()
            {
                Details = "",
                State = "Main Menu",
                Timestamps = activityTimestamps,
            };
        }

        private void SetStoryState(bool afterLoad = false)
        {
            discordComponent.EnsureUNBEATABLEAppId();
            discordComponent.Presence = new RichPresence()
            {
                Details = $"Slot {FileStorage.StorySaves.SelectedSlot + 1} Episode {FileStorage.variables.GetCurrentChapter() + 1}",
                State = $"Story Mode On {DifficultyMap[FileStorage.variables.difficulty]}",
                Timestamps = activityTimestamps,
            };
        }

        private void SetArcadeState(bool afterLoad = false)
        {
            discordComponent.EnsureUNBEATABLEAppId();
            if (afterLoad)
            {
                rhythmGameContainer = FindAnyObjectByType<RhythmGameContainer>();
            }
            if (loadedSceneName == JeffBezosController.arcadeMenuScene)
            {
                // let OnArcadeMenuStateChange and OnSelectedSongChanged handle updating the Discord status
                if (afterLoad)
                {
                    activityTimestamps = Timestamps.Now;
                    ArcadeSongList.Instance.OnSelectedSongChanged += OnSelectedSongChanged;
                    arcadeMenuStateMachine = FindFirstObjectByType<ArcadeMenuStateMachine>();
                    arcadeSongList = FindFirstObjectByType<ArcadeSongList>();
                }
            }
            else if (rhythmGameContainer != null && rhythmGameContainer.isActiveAndEnabled)
            {
                string details = GetCurrentlyPlayingSong(rhythmGameContainer.RhythmController.beatmap.metadata);
                Timestamps timestamps = GetCurrentPlaybackTimestamps(rhythmGameContainer.RhythmController.songTracker, rhythmGameContainer.RhythmController.beatmap.metadata);
                discordComponent.Presence = new RichPresence()
                {
                    Type = ActivityType.Listening,
                    Details = details,
                    State = "Jamming The Keys",
                    Timestamps = timestamps,
                };
            } else if (loadedSceneName == "ScoreScreenArcadeMode")
            {
                // let OnScoreEvent handle updating the Discord status
                if (afterLoad)
                {
                    activityTimestamps = Timestamps.Now;
                    highScoreScreen = FindFirstObjectByType<HighScoreScreen>();
                }
            }
        }

        private void SetWhiteLabelState(bool afterLoad = false)
        {
            discordComponent.EnsureWhiteLabelAppId();
        }

        private void OnStartStory()
        {
            activityTimestamps = Timestamps.Now;
        }

        private void OnArcadeMenuStateChange(ArcadeMenuState from, ArcadeMenuState to, bool Instant)
        {
            ShowArcadeMenuActivity(null, to);
        }

        private void OnSelectedSongChanged(ArcadeSongDatabase.BeatmapItem beatmapItem)
        {
            ShowArcadeMenuActivity(beatmapItem, null);
        }

        private void ShowArcadeMenuActivity(ArcadeSongDatabase.BeatmapItem beatmapItem, ArcadeMenuState state)
        {
            state ??= arcadeMenuStateMachine?.CurrentState;
            var songInfo = (beatmapItem?.Beatmap?.metadata) ?? (arcadeSongList?.GetSelectedSong()?.Beatmap?.metadata);
            var beatmapInfo = (beatmapItem?.BeatmapInfo) ?? (arcadeSongList?.GetSelectedSong()?.BeatmapInfo);
            string details;
            switch (state?.StateName)
            {
                case EArcadeMenuStates.None:
                    break;
                case EArcadeMenuStates.TitleScreen:
                    discordComponent.Presence = new RichPresence()
                    {
                        State = "Entering The Arcade",
                        Timestamps = activityTimestamps,
                    };
                    break;
                case EArcadeMenuStates.MainMenu:
                    discordComponent.Presence = new RichPresence()
                    {
                        State = "Arcade Menu",
                        Timestamps = activityTimestamps,
                    };
                    break;
                case EArcadeMenuStates.SongSelect:
                    details = GetCurrentlyPlayingSong(songInfo, beatmapInfo);
                    discordComponent.Presence = new RichPresence()
                    {
                        Details = details,
                        State = "Choosing A Song",
                        Timestamps = activityTimestamps,
                    };
                    break;
                case EArcadeMenuStates.DifficultySelect:
                    details = GetCurrentlyPlayingSong(songInfo, beatmapInfo);
                    discordComponent.Presence = new RichPresence()
                    {
                        Details = details,
                        State = "Choosing A Difficulty",
                        Timestamps = activityTimestamps,
                    };
                    break;
                case EArcadeMenuStates.FilterSelect:
                    break;
                case EArcadeMenuStates.Leaderboard:
                    details = GetCurrentlyPlayingSong(songInfo, beatmapInfo);
                    discordComponent.Presence = new RichPresence()
                    {
                        Details = details,
                        State = "🌠Stargazing🌠",
                        Timestamps = activityTimestamps,
                    };
                    break;
                case EArcadeMenuStates.Modifiers:
                    break;
                case EArcadeMenuStates.FolioView:
                    discordComponent.Presence = new RichPresence()
                    {
                        State = "Browsing Challenges",
                        Timestamps = activityTimestamps,
                    };
                    break;
                case EArcadeMenuStates.ChallengeView:
                    // let OnSetChallengeBoard and OnChallengeSelectedChange handle updating the Discord status
                    break;
                case EArcadeMenuStates.CharacterSelect:
                    break;
                case EArcadeMenuStates.PlayerLeaderboard:
                    discordComponent.Presence = new RichPresence()
                    {
                        State = "🌠Stargazing🌠",
                        Timestamps = activityTimestamps,
                    };
                    break;
                case EArcadeMenuStates.PlayerStats:
                    discordComponent.Presence = new RichPresence()
                    {
                        State = "🎵Seeing Someone Else🎵",
                        Timestamps = activityTimestamps,
                    };
                    break;
                default:
                    break;
            }
        }

        private void OnSetChallengeBoard(ChallengeBoardDescriptor boardDescriptor)
        {
            discordComponent.Presence = new RichPresence()
            {
                State = $"Browsing {boardDescriptor.boardName} [{boardDescriptor.CompletedChallenges}/{boardDescriptor.Challenges.Length}]",
            };
        }

        private void OnChallengeSelectedChange(int index, BaseChallengeDescriptor baseChallengeDescriptor, ChallengeBoardDescriptor boardDescriptor)
        {
            discordComponent.Presence = new RichPresence()
            {
                State = $"Browsing {boardDescriptor.boardName} [{boardDescriptor.CompletedChallenges}/{boardDescriptor.Challenges.Length}]",
                Details = $"c_{index:00} {(baseChallengeDescriptor.GetChallengeState() == BaseChallengeDescriptor.ChallengeState.Complete ? "COMPLETE" : "INCOMPLETE")}",
            };
        }

        private string GetCurrentlyPlayingSong(MetadataInfo songInfo, BeatmapInfo beatmapInfo = null)
        {
            return songInfo == null
                ? "Unknown"
                : $"[{songInfo.GetDifficulty(beatmapInfo?.difficulty)} {songInfo.tagData.Level}] {songInfo.artistUnicode} - {songInfo.titleUnicode}";
        }

        private Timestamps GetCurrentPlaybackTimestamps(RhythmTracker rhythmTracker, MetadataInfo songInfo)
        {
            if (rhythmGameContainer != null && rhythmGameContainer.isActiveAndEnabled)
            {
                var playbackTime = (rhythmTracker.TimelinePosition / FileStorage.beatmapOptions.songSpeed);
                Melon<Core>.Logger.Msg($"Playback time: {playbackTime}, Song length: {songInfo.tagData.SongLength}");
                var start = DateTime.UtcNow.AddMilliseconds(-playbackTime);
                var end = start.AddSeconds(songInfo.tagData.SongLength);
                return new Timestamps(start, end);
            }
            return null;
        }

        private void OnRythmPause()
        {
            string details = GetCurrentlyPlayingSong(rhythmGameContainer.RhythmController.beatmap.metadata);
            discordComponent.Presence = new RichPresence()
            {
                Type = ActivityType.Listening,
                Details = details,
                State = "Paused",
                Timestamps = Timestamps.Now,
            };
        }

        private void OnRhythmResume()
        {
            if (rhythmGameContainer != null && rhythmGameContainer.isActiveAndEnabled)
            {
                string details = GetCurrentlyPlayingSong(rhythmGameContainer.RhythmController.beatmap.metadata);
                Timestamps timestamps = GetCurrentPlaybackTimestamps(rhythmGameContainer.RhythmController.songTracker, rhythmGameContainer.RhythmController.beatmap.metadata);
                discordComponent.Presence = new RichPresence()
                {
                    Type = ActivityType.Listening,
                    Details = details,
                    State = "Jamming The Keys",
                    Timestamps = timestamps,
                };
            }
        }

        private void OnScoreEvent(SongResultChallenge.SongResultChallengeData songResult)
        {
            Beatmap beatmap = null;
            BeatmapInfo beatmapInfo = null;
            string songName = JeffBezosController.rhythmProgression.GetBeatmapPath().Split("/")[0];
            if (JeffBezosController.rhythmProgression is ArcadeProgression && ((ArcadeProgression)JeffBezosController.rhythmProgression).isCustomChart)
            {
                beatmap = BeatmapParser.ParseBeatmap(((ArcadeProgression)JeffBezosController.rhythmProgression).customChartPath);
            }
            else
            {
                BeatmapParser.ParseBeatmap(BeatmapIndex.defaultIndex, JeffBezosController.rhythmProgression.GetBeatmapPath(), out beatmapInfo, out beatmap, out songName, BeatmapParserEngine.SectionTypes.Metadata | BeatmapParserEngine.SectionTypes.TimingPoints);
            }

            string letterGradeArcade = HighScoreScreen.GetLetterGradeArcade(JeffBezosController.prevAccuracy, JeffBezosController.prevMiss == 0, !JeffBezosController.prevFail);
            MetadataInfo songInfo = beatmap.metadata;

            var paddedScore = JeffBezosController.prevScore.ToString().PadLeft(7, '0');
            var state = highScoreScreen?.previousHighScore != null && JeffBezosController.prevScore > highScoreScreen.previousHighScore.score ?
                "New Highscore" : "Song Scoring";
            discordComponent.Presence = new RichPresence()
            {
                Details = $"{paddedScore} {letterGradeArcade} [{songInfo.GetDifficulty(beatmapInfo?.difficulty)} {songInfo.tagData.Level}] {songInfo.artistUnicode} - {songInfo.titleUnicode}",
                State = state,
                Timestamps = activityTimestamps,
            };
        }
    }
}
