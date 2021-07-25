using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using IPA.Utilities;
using MultiplayerExtensions.Emotes;
using MultiplayerExtensions.Environments;
using MultiplayerExtensions.Packets;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;
using System.Net;

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
        private readonly IPlatformUserModel platformUserModel;

        [UIComponent("emote-list-1")]
        public CustomListTableData emoteList1 = null!;

        [UIComponent("emote-list-2")]
        public CustomListTableData emoteList2 = null!;

        public EmotePanel(LobbyEnvironmentManager environmentManager, PacketManager packetManager, IPlatformUserModel platformUserModel)
        {
            this.packetManager = packetManager;
            this.environmentManager = environmentManager;
            this.platformUserModel = platformUserModel;
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
                floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(80, 30), true, new Vector3(0, 0.25f, 1), new Quaternion(0, 0, 0, 0));
                BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "MultiplayerExtensions.UI.EmotePanel.bsml"), floatingScreen.gameObject, this);
                floatingScreen.gameObject.SetActive(false);
                floatingScreen.gameObject.name = "MultiplayerEmotePanel";
                floatingScreen.transform.localEulerAngles = new Vector3(50, 0);
                screenPosition = floatingScreen.transform.position;
                screenAngles = floatingScreen.transform.localEulerAngles;
                LoadImages();
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

        private async Task LoadImages()
        {
            if (localEmoteImages.Count == 0)
            {
                UserInfo user = await platformUserModel.GetUserInfo();
                EmoteAPI emoteAPIResults = await EmoteAPI.GetEmoteAPIResultAsync(user.platformUserId);

                foreach (var url in emoteAPIResults.GlobalEmotes)
                {
                    if (!localEmoteImages.ContainsKey(url))
                    {
                        localEmoteImages.Add(url, new EmoteImage(url));
                    }
                }
            }
        }

        private async void ShowImages()
        {
            emoteList1.data.Clear();
            emoteList2.data.Clear();

            await LoadImages();
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
                    if (emoteList2.data.Count == 0 || emoteList2.data[emoteList2.data.Count - 1].text != "")
                    {
                        emoteList1.data.Add(new CustomCellInfo(emoteImage.Key, "", emoteImage.Value.Sprite));
                        emoteList2.data.Add(new CustomCellInfo("", "", BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite));
                    }
                    else
                    {
                        emoteList2.data[emoteList2.data.Count - 1] = new CustomCellInfo(emoteImage.Key, "", emoteImage.Value.Sprite);
                    }
                }
            }
            emoteList1.tableView.ReloadDataKeepingPosition();
            emoteList2.tableView.ReloadDataKeepingPosition();
            emoteList1.tableView.ScrollToCellWithIdx(0, TableView.ScrollPositionType.Beginning, false);
            emoteList2.tableView.ScrollToCellWithIdx(0, TableView.ScrollPositionType.Beginning, false);
            _ = ViewControllerMonkeyCleanup();
        }

        private void LocalImage_SpriteLoaded(object sender, EventArgs e)
        {
            if (sender is EmoteImage emoteImage)
            {
                if (emoteImage.SpriteWasLoaded)
                {
                    if (emoteList2.data.Count == 0 || emoteList2.data[emoteList2.data.Count - 1].text != "")
                    {
                        emoteList1.data.Add(new CustomCellInfo(emoteImage.imageID, "", emoteImage.Sprite));
                        emoteList1.tableView.ReloadDataKeepingPosition();

                        emoteList2.data.Add(new CustomCellInfo("", "", BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite));
                        emoteList2.tableView.ReloadDataKeepingPosition();

                        if (emoteList1.data.Count == 5)
                        {
                            emoteList1.tableView.AddCellToReusableCells(emoteList1.tableView.dataSource.CellForIdx(emoteList1.tableView, 3));
                        }

                        if (emoteList2.data.Count == 5)
                        {
                            emoteList2.tableView.AddCellToReusableCells(emoteList2.tableView.dataSource.CellForIdx(emoteList2.tableView, 3));
                        }
                    }
                    else
                    {
                        emoteList2.data[emoteList2.data.Count - 1] = new CustomCellInfo(emoteImage.imageID, "", emoteImage.Sprite);
                        emoteList2.tableView.ReloadDataKeepingPosition();
                    }
                    _ = ViewControllerMonkeyCleanup();
                }
                emoteImage.SpriteLoaded -= LocalImage_SpriteLoaded;
            }
        }

        private async Task ViewControllerMonkeyCleanup()
        {
            await SiraUtil.Utilities.PauseChamp;
            IEnumerable<ImageView> imageViews = emoteList1.tableView.GetComponentsInChildren<ImageView>(true).Concat(emoteList2.tableView.GetComponentsInChildren<ImageView>(true));
            foreach (var imageView in imageViews)
            {
                imageView.SetField("_skew", 0f);
            }
        }

        [UIAction("close-screen")]
        internal void CloseScreen() => floatingScreen?.gameObject?.SetActive(false);

        [UIAction("emote-select")]
        private void EmoteSelect(TableView tableView, int index)
        {
            tableView.ClearSelection();

            CustomListTableData selectedTableData;
            if (emoteList1.tableView == tableView)
            {
                selectedTableData = emoteList1;
            }
            else
            {
                selectedTableData = emoteList2;
            }

            if (selectedTableData.data[index].text != "")
            {
                FlyingEmote flyingEmote = new GameObject("FlyingEmote", typeof(FlyingEmote)).GetComponent<FlyingEmote>();
                flyingEmote.Setup(selectedTableData.data[index].icon, floatingScreen.transform.position, floatingScreen.transform.rotation);
                packetManager.Send(new EmotePacket() { source = selectedTableData.data[index].text, position = floatingScreen.transform.position, rotation = floatingScreen.transform.rotation });
                Plugin.Log.Debug($"Sent packet with emote {selectedTableData.data[index].text}");
            }
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
