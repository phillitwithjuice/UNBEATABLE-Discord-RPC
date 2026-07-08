namespace UNBEATABLE_Discord_RPC
{
    using DiscordRPC;
    using MelonLoader;
    using System.Collections;
    using UnityEngine;

    public class CustomDiscordComponent : MonoBehaviour
    {
        private Coroutine _updateCoroutine;

        public DiscordRpcClient Client { get; private set; }

        private RichPresence _presence = new RichPresence();
        public bool UpdatePresence { get; private set; }

        public RichPresence Presence {
            get { return _presence; }
            set
            {
                Melon<Core>.Logger.Msg($"Presence Setter");
                _presence = value;
                UpdatePresence = true;
            }
        }

        public static readonly string UNBEATABLEAppId = "1440143114873606294";
        public static readonly string WhiteLabelAppId = "892184493899804682";

        private string _appId = UNBEATABLEAppId;
        public string AppId {
            get { return _appId; }
            set {
                if (Client != null)
                {
                    Client.Dispose();
                }
                _appId = value;
                ConnectToDiscord();
            }
        }

        public float UpdateTime = .5f;

        public bool IsConnected { get; private set; }

        private void Awake()
        {
            Object.DontDestroyOnLoad(base.gameObject);
            ConnectToDiscord();
        }

        private void Start()
        {
            _updateCoroutine = StartCoroutine(UpdateDiscord());
        }

        private void OnDestroy()
        {
            Melon<Core>.Logger.Msg($"Stopping Discord loop {IsConnected}");
            StopCoroutine(_updateCoroutine);
            Client.Dispose();
        }

        private IEnumerator UpdateDiscord()
        {
            while (true)
            {
                if (IsConnected)
                {
                    if (!Client.AutoEvents) Client.Invoke();
                    if (UpdatePresence)
                    {
                        Melon<Core>.Logger.Msg($"Updating Discord presence to {Presence?.Type} {Presence?.Details} {Presence?.State} {Presence.Timestamps?.Start} {Presence.Timestamps?.End}");
                        Client.SetPresence(Presence);
                        UpdatePresence = false;
                    }
                }
                yield return new WaitForSecondsRealtime(UpdateTime);
            }
        }

        private void ConnectToDiscord()
        {
            IsConnected = false;
            try
            {
                Client = new DiscordRpcClient(_appId);
                Client.OnConnectionEstablished += (sender, e) =>
                {
                    Melon<Core>.Logger.Msg($"Connected to Discord with AppId {_appId}");
                };
                Client.OnReady += (sender, e) =>
                {
                    Melon<Core>.Logger.Msg($"Discord client ready");
                    IsConnected = true;
                    UpdatePresence = true;
                };
                Client.OnError += (sender, e) =>
                {
                    Melon<Core>.Logger.Msg($"Discord error");
                    IsConnected = false;
                };
                Client.Initialize();
            }
            catch
            {
                Melon<Core>.Logger.Msg($"Failed to connect to Discord");
                IsConnected = false;
                return;
            }
        }

        public void EnsureUNBEATABLEAppId()
        {
            if (AppId != UNBEATABLEAppId)
            {
                AppId = UNBEATABLEAppId;
            }
        }

        public void EnsureWhiteLabelAppId()
        {
            if (AppId != WhiteLabelAppId)
            {
                AppId = WhiteLabelAppId;
            }
        }
    }
}
