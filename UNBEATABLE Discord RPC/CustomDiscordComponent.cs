using System;
using System.Collections.Generic;
using System.Text;

namespace UNBEATABLE_Discord_RPC
{
    using Discord;
    using MelonLoader;
    using System.Collections;
    using UnityEngine;

    public class CustomDiscordComponent : MonoBehaviour
    {
        public global::Discord.Discord discord;

        public Activity activity;

        public ActivityManager activityManager;

        public static readonly long UNBEATABLEAppId = 1440143114873606294L;
        public static readonly long WhiteLabelAppId = 892184493899804682L;

        private long _appId = UNBEATABLEAppId;
        public long appId {
            get { return _appId; }
            set {
                discord.Dispose();
                _appId = value;
                connectToDiscord();
            }
        }

        public bool updateActivity;

        public float updateTime = .5f;

        public bool IsConnected { get; private set; }

        private void Awake()
        {
            Object.DontDestroyOnLoad(base.gameObject);
            connectToDiscord();
        }

        private void Start()
        {
            StartCoroutine(UpdateDiscord());
        }

        private void OnDestroy()
        {
            discord.Dispose();
        }

        private IEnumerator UpdateDiscord()
        {
            while (true)
            {
                yield return new WaitForSeconds(updateTime);
                if (IsConnected)
                {
                    discord.RunCallbacks();
                    if (updateActivity)
                    {
                        Melon<Core>.Logger.Msg($"Updating Discord activity to {activity.Details} {activity.State} {activity.Timestamps.Start} {activity.Timestamps.End}");
                        activityManager.UpdateActivity(activity, delegate
                        {
                        });
                        updateActivity = false;
                    }
                }
            }
        }

        private void connectToDiscord()
        {
            IsConnected = true;
            try
            {
                discord = new global::Discord.Discord(appId, (ulong) CreateFlags.NoRequireDiscord);
            }
            catch
            {
                IsConnected = false;
                return;
            }

            activityManager = discord.GetActivityManager();
            activity.Assets.SmallImage = (activity.Assets.LargeImage = "drp_icon");
            activity.Details = "";
            activity.State = "";
            updateActivity = true;
        }
    }
}
