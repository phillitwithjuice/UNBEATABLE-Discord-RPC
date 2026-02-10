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
            gameStartedTime = (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;

            var discord = GameObject.FindGameObjectWithTag("Discord");
            CustomDiscordComponent component;
            CustomDiscordController controller;

            if (discord == null)
            {
                discord = new GameObject("Discord", [typeof(CustomDiscordComponent), typeof(CustomDiscordController)]);
                discord.tag = "Discord";
                component = discord.GetComponent<CustomDiscordComponent>();
                controller = discord.GetComponent<CustomDiscordController>();
            }
            else
            {
                var discordActivityController = discord.GetComponent<DiscordActivityController>();
                if (discordActivityController !=  null && discordActivityController.enabled)
                {
                    LoggerInstance.Warning("The games internal Discord RPC controller is enabled!");
                    LoggerInstance.Warning("Will not patch custom Discord RPC controller into the game!");
                    LoggerInstance.Msg("Have a nice day!");
                    Unregister();
                    return;
                }

                var discordComponent = discord.GetComponent<DiscordComponent>();
                if (discordComponent != null)
                {
                    discordComponent.enabled = false;
                    discordComponent.discord?.Dispose();
                    UnityEngine.Object.Destroy(discordComponent);
                }

                component = discord.AddComponent<CustomDiscordComponent>();
                controller = discord.AddComponent<CustomDiscordController>();
            }
            controller.discordComponent = component;
        }
    }
}