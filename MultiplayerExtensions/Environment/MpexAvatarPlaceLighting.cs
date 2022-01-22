using IPA.Utilities;
using MultiplayerExtensions.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Environments
{
    public class MpexAvatarPlaceLighting : MonoBehaviour
    {
        public const float SmoothTime = 2f;
        public Color TargetColor { get; private set; } = Color.black;
        private List<TubeBloomPrePassLight> _lights = new List<TubeBloomPrePassLight>();

        private readonly FieldAccessor<MultiplayerLobbyAvatarPlaceManager, float>.Accessor _innerCircleRadius
            = FieldAccessor<MultiplayerLobbyAvatarPlaceManager, float>
                .GetAccessor(nameof(_innerCircleRadius));
        private readonly FieldAccessor<MultiplayerLobbyAvatarPlaceManager, float>.Accessor _minOuterCircleRadius
            = FieldAccessor<MultiplayerLobbyAvatarPlaceManager, float>
                .GetAccessor(nameof(_minOuterCircleRadius));

        private IMultiplayerSessionManager _sessionManager = null!;
        private MultiplayerLobbyAvatarPlaceManager _avatarPlaceManager = null!;
        private ILobbyStateDataModel _lobbyStateDataModel = null!;
        private MenuLightsManager _lightsManager = null!;
        private MpexPlayerManager _mpexPlayerManager = null!;
        private Config _config = null!;

        [Inject]
        internal void Construct(
            IMultiplayerSessionManager sessionManager,
            MultiplayerLobbyAvatarPlaceManager avatarPlaceManager,
            ILobbyStateDataModel lobbyStateDataModel,
            MenuLightsManager lightsManager,
            MpexPlayerManager mpexPlayerManager,
            Config config)
        {
            _sessionManager = sessionManager;
            _avatarPlaceManager = avatarPlaceManager;
            _lobbyStateDataModel = lobbyStateDataModel;
            _lightsManager = lightsManager;
            _mpexPlayerManager = mpexPlayerManager;
            _config = config;
        }

        private void Awake()
        {
            _lights = GetComponentsInChildren<TubeBloomPrePassLight>().ToList();

            foreach (var player in _sessionManager.connectedPlayers)
            {
                if (player.sortIndex != -1 && GetPosition(player.sortIndex) == transform.position)
                    SetColor(player.isMe ? _config.PlayerColor : _mpexPlayerManager.GetPlayer(player.userId)?.Color ?? Config.DefaultPlayerColor, true);
            }
        }

        private void OnEnable()
        {
            _mpexPlayerManager.PlayerConnectedEvent += HandlePlayerData;
            _sessionManager.playerConnectedEvent += HandlePlayerConnected;
            _sessionManager.playerDisconnectedEvent += HandlePlayerDisconnected;
        }

        private void OnDisable()
        {
            _mpexPlayerManager.PlayerConnectedEvent -= HandlePlayerData;
            _sessionManager.playerConnectedEvent -= HandlePlayerConnected;
            _sessionManager.playerDisconnectedEvent -= HandlePlayerDisconnected;
        }

        private void HandlePlayerData(IConnectedPlayer player, MpexPlayerData data)
        {
            if (GetPosition(player.sortIndex) == transform.position)
                SetColor(data.Color, false);
        }

        private void HandlePlayerConnected(IConnectedPlayer player)
        {
            if (GetPosition(player.sortIndex) == transform.position)
                SetColor(Config.DefaultPlayerColor, false);
        }

        private void HandlePlayerDisconnected(IConnectedPlayer player)
        {
            if (GetPosition(player.sortIndex) == transform.position)
                SetColor(Color.black, false);
        }

        private Vector3 GetPosition(int sortIndex)
        {
            float angleBetweenPlayersWithEvenAdjustment = MultiplayerPlayerPlacement.GetAngleBetweenPlayersWithEvenAdjustment(_lobbyStateDataModel.maxPartySize, MultiplayerPlayerLayout.Circle);
            float outerCircleRadius = Mathf.Max(MultiplayerPlayerPlacement.GetOuterCircleRadius(angleBetweenPlayersWithEvenAdjustment, _innerCircleRadius(ref _avatarPlaceManager)), _minOuterCircleRadius(ref _avatarPlaceManager));
            float outerCirclePositionAngleForPlayer = MultiplayerPlayerPlacement.GetOuterCirclePositionAngleForPlayer(sortIndex, _lobbyStateDataModel.localPlayer.sortIndex, angleBetweenPlayersWithEvenAdjustment);
            return MultiplayerPlayerPlacement.GetPlayerWorldPosition(outerCircleRadius, outerCirclePositionAngleForPlayer, MultiplayerPlayerLayout.Circle);
        }

        private void Update()
        {
            Color current = GetColor();
            if (current == TargetColor)
                return;
            if (_lightsManager.IsColorVeryCloseToColor(current, TargetColor))
                SetColor(TargetColor);
            else
                SetColor(Color.Lerp(current, TargetColor, Time.deltaTime * SmoothTime));
        }

        public void SetColor(Color color, bool immediate)
        {
            TargetColor = color;
            if (immediate)
                SetColor(color);
        }

        public Color GetColor()
        {
            if (_lights.Count > 0)
                return _lights[0].color;
            return Color.black;
        }

        private void SetColor(Color color)
        {
            foreach(TubeBloomPrePassLight light in _lights)
            {
                light.color = color;
                light.Refresh();
            }
        }
    }
}
