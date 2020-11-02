using CustomAvatar.Tracking;
using System;
using UnityEngine;

namespace MultiplayerExtensions.Avatars
{
    internal class MirrorInput : IAvatarInput
    {
        private readonly IAvatarInput _playerInput;

        internal MirrorInput(IAvatarInput playerInput)
        {
            _playerInput = playerInput;

            _playerInput.inputChanged += () => inputChanged?.Invoke();
        }

        public bool allowMaintainPelvisPosition => _playerInput.allowMaintainPelvisPosition;

        public event Action inputChanged;

        public bool TryGetFingerCurl(DeviceUse use, out FingerCurl curl)
        {
            switch (use)
            {
                case DeviceUse.LeftHand:
                    use = DeviceUse.RightHand;
                    break;

                case DeviceUse.RightHand:
                    use = DeviceUse.LeftHand;
                    break;
            }

            return _playerInput.TryGetFingerCurl(use, out curl);
        }

        public bool TryGetPose(DeviceUse use, out Pose pose)
        {
            switch (use)
            {
                case DeviceUse.LeftHand:
                    use = DeviceUse.RightHand;
                    break;

                case DeviceUse.RightHand:
                    use = DeviceUse.LeftHand;
                    break;

                case DeviceUse.LeftFoot:
                    use = DeviceUse.RightFoot;
                    break;

                case DeviceUse.RightFoot:
                    use = DeviceUse.LeftFoot;
                    break;
            }

            if (!_playerInput.TryGetPose(use, out pose)) return false;

            pose.position.x = -pose.position.x;

            pose.rotation.ToAngleAxis(out float angle, out Vector3 axis);

            axis.y *= -1;
            axis.z *= -1;

            pose.rotation = Quaternion.AngleAxis(angle, axis);

            return true;
        }
    }
}
