using BS_Utils.Utilities;
using CustomAvatar.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerExtensions.Avatars
{
    class MultiplayerInput : IAvatarInput
    {
        private readonly AvatarPoseController _poseController;

        private Pose head = new Pose();
        private Pose rightHand = new Pose();
        private Pose leftHand = new Pose();

        internal MultiplayerInput(AvatarPoseController poseController)
        {
            _poseController = poseController;

            _poseController.didUpdatePoseEvent += (Vector3 headPosition) => inputChanged?.Invoke();
        }

        private void OnInputChanged(Vector3 newHeadPosition)
        {
            Transform headTransform = _poseController.GetField<Transform>("_headTransform");
            Transform rightHandTransform = _poseController.GetField<Transform>("_rightHandTransform");
            Transform leftHandTransform = _poseController.GetField<Transform>("_leftHandTransform");

            head.position = newHeadPosition;
            head.rotation = headTransform.rotation;
            rightHand.position = rightHandTransform.position;
            rightHand.rotation = rightHandTransform.rotation;
            leftHand.position = leftHandTransform.position;
            leftHand.rotation = leftHandTransform.rotation;

            inputChanged();
        }

        public bool allowMaintainPelvisPosition => throw new NotImplementedException();

        public event Action inputChanged;

        public bool TryGetFingerCurl(DeviceUse use, out FingerCurl curl)
        {
            curl = new FingerCurl(0f, 0f, 0f, 0f, 0f);
            return false;
        }

        public bool TryGetPose(DeviceUse use, out Pose pose)
        {
            switch (use)
            {
                case DeviceUse.Head:
                    pose = head;
                    return true;
                case DeviceUse.RightHand:
                    pose = rightHand;
                    return true;
                case DeviceUse.LeftHand:
                    pose = leftHand;
                    return true;
                default:
                    pose = new Pose();
                    return false;
            }
        }
    }
}
