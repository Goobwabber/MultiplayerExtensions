using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using MultiplayerExtensions.Emotes;
using MultiplayerExtensions.Environments;
using MultiplayerExtensions.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using UnityEngine;
using Zenject;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;

namespace MultiplayerExtensions.UI
{
    public class EmotePanel : IInitializable, IDisposable
    {
        private FloatingScreen floatingScreen = null!;
        private Vector3 screenPosition;
        private Vector3 screenAngles;

        private bool parsed;
        private SemaphoreSlim flyingEmoteSemaphore;
        private Dictionary<string, EmoteImage> localEmoteImages = null!;
        private Dictionary<string, EmoteImage> remoteEmoteImages = null!;

        private readonly PacketManager packetManager;
        private readonly LobbyEnvironmentManager environmentManager;

        [UIComponent("emote-list")]
        public CustomListTableData customListTableData = null!;

        public EmotePanel(LobbyEnvironmentManager environmentManager, PacketManager packetManager)
        {
            this.packetManager = packetManager;
            this.environmentManager = environmentManager;
        }

        public void Initialize()
        {
            parsed = false;
            flyingEmoteSemaphore = new SemaphoreSlim(1, 1);
            localEmoteImages = new Dictionary<string, EmoteImage>();
            remoteEmoteImages = new Dictionary<string, EmoteImage>();

            packetManager.RegisterCallback<EmotePacket>(HandleEmotePacket);
        }

        public void Dispose()
        {
            packetManager.UnregisterCallback<EmotePacket>();
        }

        private void Parse()
        {
            if (!parsed)
            {
                parsed = true;
                floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(75, 30), true, new Vector3(0, 0.25f, 1), new Quaternion(0, 0, 0, 0));
                BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "MultiplayerExtensions.UI.EmotePanel.bsml"), floatingScreen.gameObject, this);
                floatingScreen.gameObject.SetActive(false);
                floatingScreen.gameObject.name = "MultiplayerEmotePanel";
                floatingScreen.transform.localEulerAngles = new Vector3(50, 0);
                screenPosition = floatingScreen.transform.position;
                screenAngles = floatingScreen.transform.localEulerAngles;
            }
            // Restore position so it respawns where we expect it to
            floatingScreen.transform.position = screenPosition;
            floatingScreen.transform.localEulerAngles = screenAngles;
        }

        internal void ToggleActive()
        {
            Parse();
            if (floatingScreen.gameObject.activeSelf)
            {
                floatingScreen.gameObject.SetActive(false);
            }
            else
            {
                floatingScreen.gameObject.SetActive(true);
                ShowImages();
            }
        }

        private void LoadImages()
        {
            foreach (var url in Plugin.Config.EmoteURLs)
            {
                if (!localEmoteImages.ContainsKey(url))
                {
                    localEmoteImages.Add(url, new EmoteImage(url));
                }
            }
        }

        private async void ShowImages()
        {
            customListTableData.data.Clear();

            LoadImages();
            foreach (var emoteImage in localEmoteImages)
            {
                if (!emoteImage.Value.SpriteWasLoaded && !emoteImage.Value.Blacklist)
                {
                    emoteImage.Value.SpriteLoaded += LocalImage_SpriteLoaded;
                    await emoteImage.Value.DownloadImage();
                    _ = emoteImage.Value.Sprite;
                }
                else if (emoteImage.Value.SpriteWasLoaded)
                {
                    customListTableData.data.Add(new CustomCellInfo(emoteImage.Key, "", emoteImage.Value.Sprite));
                }
            }
            customListTableData.tableView.ReloadData();
            customListTableData.tableView.ScrollToCellWithIdx(0, TableView.ScrollPositionType.Beginning, false);
        }

        private void LocalImage_SpriteLoaded(object sender, EventArgs e)
        {
            if (sender is EmoteImage emoteImage)
            {
                if (emoteImage.SpriteWasLoaded)
                {
                    customListTableData.data.Add(new CustomCellInfo(emoteImage.URL, "", emoteImage.Sprite));
                    customListTableData.tableView.ReloadData();
                }
                emoteImage.SpriteLoaded -= LocalImage_SpriteLoaded;
            }
        }

        [UIAction("close-screen")]
        internal void CloseScreen() => floatingScreen?.gameObject?.SetActive(false);

        [UIAction("emote-select")]
        private void EmoteSelect(TableView _, int index)
        {
            customListTableData.tableView.ClearSelection();
            FlyingEmote flyingEmote = new GameObject("FlyingEmote", typeof(FlyingEmote)).GetComponent<FlyingEmote>();
            flyingEmote.Setup(customListTableData.data[index].icon, floatingScreen.transform.position, floatingScreen.transform.rotation);
            packetManager.Send(new EmotePacket() { source = customListTableData.data[index].text, position = floatingScreen.transform.position, rotation = floatingScreen.transform.rotation });
            Plugin.Log.Debug($"Sent packet with emote {customListTableData.data[index].text}");
        }

        private async void HandleEmotePacket(EmotePacket packet, IConnectedPlayer player)
        {
            Plugin.Log.Debug($"Recieved packet with emote {packet.source}");
            await flyingEmoteSemaphore.WaitAsync();
            
            Vector3 playerPosition = environmentManager.GetPositionOfPlayer(player);
            Quaternion playerRotation = environmentManager.GetRotationOfPlayer(player);

            Vector3 position = playerRotation * packet.position + playerPosition;
            Quaternion rotation = playerRotation * packet.rotation;

            FlyingEmote flyingEmote = new GameObject("FlyingEmote", typeof(FlyingEmote)).GetComponent<FlyingEmote>();
            
            if (localEmoteImages.TryGetValue(packet.source, out EmoteImage localEmoteImage))
            {
                if (localEmoteImage.SpriteWasLoaded)
                {
                    flyingEmote.Setup(localEmoteImage.Sprite, position, rotation);
                }
                else if (!localEmoteImage.Blacklist)
                {
                    await localEmoteImage.WaitSpriteLoadAsync();
                    if (localEmoteImage.SpriteWasLoaded)
                    {
                        flyingEmote.Setup(localEmoteImage.Sprite, position, rotation);
                    }
                }
            }
            else if (remoteEmoteImages.TryGetValue(packet.source, out EmoteImage remoteEmoteImage))
            {
                if (remoteEmoteImage.SpriteWasLoaded)
                {
                    flyingEmote.Setup(remoteEmoteImage.Sprite, position, rotation);
                }
            }
            else
            {
                EmoteImage emoteImage = new EmoteImage(packet.source);
                await emoteImage.DownloadImage();
                _ = emoteImage.Sprite;
                remoteEmoteImages.Add(packet.source, emoteImage);
                await emoteImage.WaitSpriteLoadAsync();
                if (emoteImage.SpriteWasLoaded)
                {
                    flyingEmote.Setup(emoteImage.Sprite, position, rotation);
                }
            }

            flyingEmoteSemaphore.Release();
            Plugin.Log.Debug($"Finished displaying packet with emote {packet.source}");
        }
    }
}
