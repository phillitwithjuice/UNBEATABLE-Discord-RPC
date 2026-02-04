using Arcade.UI.MenuStates;
using Arcade.UI.SongSelect;
using Challenges;
using MelonLoader;
using Rhythm;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UNBEATABLE_Discord_RPC.CustomEvents;

namespace UNBEATABLE_Discord_RPC
{
    public class CustomDiscordController : MonoBehaviour
    {
        public static readonly Dictionary<string, string> difficultyMap = new Dictionary<string, string>()
        {
            { "Beginner", "BEGINNER" },
            { "Easy", "NORMAL" },
            { "Normal", "HARD" },
            { "Hard", "Expert" },
            { "UNBEATABLE", "UNBEATABLE" },
        };

        public CustomDiscordComponent discordComponent;

        private string loadedSceneName = "";

        private float activityTimer;

        private RhythmGameContainer rhythmGameContainer;

        private ArcadeMenuStateMachine arcadeMenuStateMachine;

        private ArcadeSongList arcadeSongList;

        private HighScoreScreen highScoreScreen;

        public static List<EArcadeMenuStates> showSongInfoInMenuState = new List<EArcadeMenuStates>()
        {
        };

        private void Awake()
        {
            Melon<Core>.Logger.Msg("Custom Discord Controller injected");
            SceneManager.sceneLoaded += OnSceneLoad;
            CustomEvents.OnArcadeMenuStateChange += OnArcadeMenuStateChange;
            CustomEvents.OnChallengeSelectedChange += OnChallengeSelectedChange;
            CustomEvents.OnSetChallengeBoard += OnSetChallengeBoard;
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
            CustomEvents.OnRhythmResume -= OnRhythmResume;
            CustomEvents.OnStartStory -= OnStartStory;
            HighScoreScreen.ScoreScreenChallengeEvent -= OnScoreEvent;
        }

        private void Start()
        {
            discordComponent.activity.Details = "";
            discordComponent.activity.State = "Getting Ready";
            discordComponent.updateActivity = true;
        }

        private void OnSceneLoad(Scene LoadedScene, LoadSceneMode SceneMode)
        {
            activityTimer = 0f;
            UnityEngine.SceneManagement.Scene scene = (UnityEngine.SceneManagement.Scene)Convert.ChangeType(LoadedScene, typeof(UnityEngine.SceneManagement.Scene));
            loadedSceneName = scene.name;
            SetSceneState(afterLoad: true);
        }

        private void SetSceneState(bool afterLoad = false)
        {
            switch (JeffBezosController.rhythmGameType, loadedSceneName)
            {
                case (RhythmGameType.Story, "LogoScreen"):
                    discordComponent.activity.Details = "";
                    discordComponent.activity.State = "Getting Ready";
                    if (afterLoad)
                    {
                        StartActivityTimer();
                    }
                    discordComponent.updateActivity = true;
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

        private void StartActivityTimer()
        {
            discordComponent.activity.Timestamps.Start = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds);
            discordComponent.activity.Timestamps.End = 0;
            discordComponent.updateActivity = true;
        }

        private void ensureUNBEATABLEAppId()
        {
            if (discordComponent.appId != CustomDiscordComponent.UNBEATABLEAppId)
            {
                discordComponent.appId = CustomDiscordComponent.UNBEATABLEAppId;
            }
        }
        private void ensureWhiteLabelAppId()
        {
            if (discordComponent.appId != CustomDiscordComponent.WhiteLabelAppId)
            {
                discordComponent.appId = CustomDiscordComponent.WhiteLabelAppId;
            }
        }

        private void SetMainMenuState(bool afterLoad = false)
        {
            ensureUNBEATABLEAppId();
            if (afterLoad)
            {
                StartActivityTimer();
            }
            discordComponent.activity.Details = "";
            discordComponent.activity.State = "Main Menu";
            discordComponent.updateActivity = true;
        }

        private void SetStoryState(bool afterLoad = false)
        {
            ensureUNBEATABLEAppId();
            discordComponent.activity.Details = $"Slot {FileStorage.StorySaves.SelectedSlot + 1} Episode {FileStorage.variables.GetCurrentChapter() + 1}/{FileStorage.StorySaves.HighestReachedChapter + 1}";
            discordComponent.activity.State = $"Story Mode On {difficultyMap[FileStorage.variables.difficulty]}";
            discordComponent.updateActivity = true;
        }

        private void SetArcadeState(bool afterLoad = false)
        {
            ensureUNBEATABLEAppId();
            if (afterLoad)
            {
                rhythmGameContainer = FindAnyObjectByType<RhythmGameContainer>();
            }
            if (loadedSceneName == JeffBezosController.arcadeMenuScene)
            {
                // let OnArcadeMenuStateChange and OnSelectedSongChanged handle updating the Discord status
                if (afterLoad)
                {
                    StartActivityTimer();
                    ArcadeSongList.Instance.OnSelectedSongChanged += OnSelectedSongChanged;
                    arcadeMenuStateMachine = FindObjectOfType<ArcadeMenuStateMachine>();
                    arcadeSongList = FindObjectOfType<ArcadeSongList>();
                }
            }
            else if (rhythmGameContainer != null && rhythmGameContainer.isActiveAndEnabled)
            {
                SetCurrentPlayingSong(rhythmGameContainer.RhythmController.beatmap.metadata);
                SetCurrentPlaybackTimestamps(rhythmGameContainer.RhythmController.songTracker);
                discordComponent.activity.State = "Jamming The Keys";
                discordComponent.updateActivity = true;
            } else if (loadedSceneName == "ScoreScreenArcadeMode")
            {
                // let OnScoreEvent handle updating the Discord status
                if (afterLoad)
                {
                    StartActivityTimer();
                    highScoreScreen = FindObjectOfType<HighScoreScreen>();
                }
            }
        }

        private void SetWhiteLabelState(bool afterLoad = false)
        {
            ensureWhiteLabelAppId();
        }

        private void OnStartStory()
        {
            StartActivityTimer();
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
            switch (state?.StateName)
            {
                case EArcadeMenuStates.None:
                    break;
                case EArcadeMenuStates.TitleScreen:
                    discordComponent.activity.Details = "";
                    discordComponent.activity.State = "Entering The Arcade";
                    discordComponent.updateActivity = true;
                    break;
                case EArcadeMenuStates.MainMenu:
                    discordComponent.activity.Details = "";
                    discordComponent.activity.State = "Arcade Menu";
                    discordComponent.updateActivity = true;
                    break;
                case EArcadeMenuStates.SongSelect:
                    SetCurrentPlayingSong(songInfo, beatmapInfo);
                    discordComponent.activity.State = "Choosing A Song";
                    discordComponent.updateActivity = true;
                    break;
                case EArcadeMenuStates.DifficultySelect:
                    SetCurrentPlayingSong(songInfo, beatmapInfo);
                    discordComponent.activity.State = "Choosing A Difficulty";
                    discordComponent.updateActivity = true;
                    break;
                case EArcadeMenuStates.FilterSelect:
                    break;
                case EArcadeMenuStates.Leaderboard:
                    SetCurrentPlayingSong(songInfo, beatmapInfo);
                    discordComponent.activity.State = "🌠Stargazing🌠";
                    discordComponent.updateActivity = true;
                    break;
                case EArcadeMenuStates.Modifiers:
                    break;
                case EArcadeMenuStates.FolioView:
                    discordComponent.activity.Details = "";
                    discordComponent.activity.State = "Browsing Challenges";
                    discordComponent.updateActivity = true;
                    break;
                case EArcadeMenuStates.ChallengeView:
                    // let OnSetChallengeBoard and OnChallengeSelectedChange handle updating the Discord status
                    break;
                case EArcadeMenuStates.CharacterSelect:
                    break;
                case EArcadeMenuStates.PlayerLeaderboard:
                    discordComponent.activity.Details = "";
                    discordComponent.activity.State = "🌠Stargazing🌠";
                    discordComponent.updateActivity = true;
                    break;
                case EArcadeMenuStates.PlayerStats:
                    discordComponent.activity.Details = "";
                    discordComponent.activity.State = "🎵Seeing Someone Else🎵";
                    discordComponent.updateActivity = true;
                    break;
                default:
                    break;
            }
        }

        private void OnSetChallengeBoard(ChallengeBoardDescriptor boardDescriptor)
        {
            discordComponent.activity.State = $"Browsing {boardDescriptor.boardName} [{boardDescriptor.CompletedChallenges}/{boardDescriptor.Challenges.Length}]";
            discordComponent.updateActivity = true;
        }

        private void OnChallengeSelectedChange(int index, BaseChallengeDescriptor baseChallengeDescriptor)
        {
            discordComponent.activity.Details = $"c_{index:00}{(baseChallengeDescriptor.GetChallengeState() == BaseChallengeDescriptor.ChallengeState.Complete ? " Complete" : "")}";
            discordComponent.updateActivity = true;
        }

        private void SetCurrentPlayingSong(MetadataInfo songInfo, BeatmapInfo beatmapInfo = null)
        {
            if (songInfo == null)
            {
                discordComponent.activity.Details = "Unknown";
            } else
            {
                discordComponent.activity.Details = $"[{songInfo.GetDifficulty(beatmapInfo?.difficulty)} {songInfo.tagData.Level}] {songInfo.artistUnicode} - {songInfo.titleUnicode}";
            }
            discordComponent.updateActivity = true;
        }

        private void SetCurrentPlaybackTimestamps(RhythmTracker rhythmTracker)
        {
            if (rhythmGameContainer != null && rhythmGameContainer.isActiveAndEnabled)
            {
                var playbackTime = (int)(rhythmTracker.TimelinePosition / FileStorage.beatmapOptions.songSpeed) / 1000;
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var start = Convert.ToInt64((DateTime.UtcNow.AddSeconds(-playbackTime) - dateTime).TotalSeconds);
                discordComponent.activity.Timestamps = new Discord.ActivityTimestamps();
                discordComponent.activity.Timestamps.Start = start;
                if (playbackTime < 0)
                {
                    discordComponent.activity.Timestamps.End = start;
                }
                discordComponent.updateActivity = true;
            }
        }

        private void OnRhythmResume()
        {
            if (rhythmGameContainer != null && rhythmGameContainer.isActiveAndEnabled)
            {
                SetCurrentPlaybackTimestamps(rhythmGameContainer.RhythmController.songTracker);
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

            discordComponent.activity.Details = $"{JeffBezosController.prevScore} {letterGradeArcade} [{songInfo.GetDifficulty(beatmapInfo?.difficulty)} {songInfo.tagData.Level}] {songInfo.artistUnicode} - {songInfo.titleUnicode}";
            if (highScoreScreen?.previousHighScore != null && JeffBezosController.prevScore > highScoreScreen.previousHighScore.score)
            {
                discordComponent.activity.State = "New Highscore";
            }
            else
            {
                discordComponent.activity.State = "Song Scoring";
            }
            discordComponent.updateActivity = true;
        }

        private void Update()
        {
            /*
            if (rhythmController == null && discordComponent.IsConnected && loadedSceneName != null)
            {
                if (storyStateActivityTimer <= 0f)
                {
                    SetStoryState();
                    discordComponent.updateActivity = true;
                    storyStateActivityTimer = 90f;
                }

                storyStateActivityTimer -= Time.deltaTime;
            }
            else
            {
                if (!discordComponent.IsConnected || hasUpdatedSongActivity || rhythmController == null || rhythmController.beatmap == null)
                {
                    return;
                }

                for (int i = 0; i < activities.rhythmScenePairs.Length; i++)
                {
                    for (int j = 0; j < activities.rhythmScenePairs[i].sceneNames.Length; j++)
                    {
                        if (loadedSceneName == activities.rhythmScenePairs[i].sceneNames[j])
                        {
                            discordComponent.activity.Assets.LargeImage = activities.rhythmScenePairs[i].largeImage;
                            break;
                        }
                    }
                }

                discordComponent.activity.Details = rhythmController.beatmap.metadata.artist + " - " + rhythmController.beatmap.metadata.title;
                string text = $"{rhythmController.beatmap.metadata.GetDifficulty("Beginner")} {rhythmController.beatmap.metadata.tagData.Level}";
                if (JeffBezosController.GetNoFail() == 0 && JeffBezosController.GetAssistMode() == 0 && JeffBezosController.GetSongSpeed() == 0 && JeffBezosController.GetScrollSpeedIndex() == 12)
                {
                    text += "//no modifiers";
                }
                else
                {
                    if (JeffBezosController.GetNoFail() > 0)
                    {
                        text += "//no fail";
                    }

                    if (JeffBezosController.GetAssistMode() > 0)
                    {
                        text += "//assist mode";
                    }

                    if (JeffBezosController.GetSongSpeed() > 0)
                    {
                        int songSpeed = JeffBezosController.GetSongSpeed();
                        text += $"//{((songSpeed == 1) ? "half time" : "double time")}";
                    }

                    if (JeffBezosController.GetScrollSpeedIndex() != 4)
                    {
                        text += $"//scroll speed x{(float)(JeffBezosController.GetScrollSpeedIndex()) * 0.05f + 0.2f:0.00}";
                    }
                }

                discordComponent.activity.State = text;
                discordComponent.updateActivity = true;
                hasUpdatedSongActivity = true;
            }
            */
        }

        private long GetSongEndTimestamp(int SongLengthInSeconds)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((DateTime.UtcNow.AddSeconds(SongLengthInSeconds) - dateTime).TotalSeconds);
        }
    }
}
