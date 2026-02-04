using Discord;
using MelonLoader;
using System.Text.RegularExpressions;
using UnityEngine;

[assembly: MelonInfo(typeof(UNBEATABLE_Discord_RPC.Core), "UNBEATABLE Discord RPC", "1.0.0", "phillitwithjuice", null)]
[assembly: MelonGame("D-CELL GAMES", "UNBEATABLE")]

namespace UNBEATABLE_Discord_RPC
{
    public class Core : MelonMod
    {

        public static readonly Regex storyModePattern = new("C\\d+_.+");

        public DiscordComponent discordComponent;
        private bool gameClosing;
        public bool GameStarted { get; private set; }
        public long gameStartedTime { get; private set; }
        public Rhythm.MetadataInfo songinfo { get; private set; }

        public override void OnInitializeMelon()
        {
            MelonEvents.OnApplicationLateStart.Subscribe(OnApplicationLateStart);
        }

        public new void OnApplicationLateStart()
        {
            GameStarted = true;
            gameStartedTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

            var discord = GameObject.FindGameObjectWithTag("Discord");
            var discordActivityController = discord.GetComponent<DiscordActivityController>();
            if (discordActivityController.enabled)
            {
                LoggerInstance.Warning("The games internal Discord RPC controller is enabled!");
                LoggerInstance.Warning("Will not patch custom Discord RPC controller into the game!");
                LoggerInstance.Msg("Have a nice day!");
                Unregister();
                return;
            }
            discordComponent = discord.GetComponent<DiscordComponent>();
            discordComponent.enabled = false;
            discordComponent.discord.Dispose();
            UnityEngine.Object.Destroy(discordComponent);

            var customDiscordComponent = discord.AddComponent<CustomDiscordComponent>();
            discordComponent.enabled = true;

            var controller = discord.AddComponent<CustomDiscordController>();
            controller.discordComponent = customDiscordComponent;
            controller.enabled = true;
        }
    }
}