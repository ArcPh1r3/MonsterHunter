using RoR2;
using UnityEngine;

internal enum MonsterHunterCameraParams
{
    DEFAULT,
    AIM,
    EMOTE,
    CLOSE,
    FAR
}

namespace MonsterHunter.Modules
{
    internal static class CameraParams
    {
        internal static CharacterCameraParamsData defaultCameraParams;
        internal static CharacterCameraParamsData aimCameraParams;
        internal static CharacterCameraParamsData emoteCameraParams;
        internal static CharacterCameraParamsData closeCameraParams;
        internal static CharacterCameraParamsData farCameraParams;

        internal static void InitializeParams()
        {
            defaultCameraParams = NewCameraParams("ccpRobMonsterHunter", 70f, 1.37f, new Vector3(0f, 0f, -8.1f));
            aimCameraParams = NewCameraParams("ccpRobMonsterHunterAim", 70f, 1.37f, new Vector3(0f, -0.5f, -7f));
            emoteCameraParams = NewCameraParams("ccpRobMonsterHunterEmote", 70f, 0.4f, new Vector3(0f, 0f, -6f));
            closeCameraParams = NewCameraParams("ccpRobMonsterHunterClose", 70f, 1.17f, new Vector3(0f, -0.2f, -2f));
            farCameraParams = NewCameraParams("ccpRobMonsterHunterFar", 70f, 0.4f, new Vector3(0.5f, 1.15f, -10f));
        }

        private static CharacterCameraParamsData NewCameraParams(string name, float pitch, float pivotVerticalOffset, Vector3 standardPosition)
        {
            return NewCameraParams(name, pitch, pivotVerticalOffset, standardPosition, 0.1f);
        }

        private static CharacterCameraParamsData NewCameraParams(string name, float pitch, float pivotVerticalOffset, Vector3 idealPosition, float wallCushion)
        {
            CharacterCameraParamsData newParams = new CharacterCameraParamsData();

            newParams.maxPitch = pitch;
            newParams.minPitch = -pitch;
            newParams.pivotVerticalOffset = pivotVerticalOffset;
            newParams.idealLocalCameraPos = idealPosition;
            newParams.wallCushion = wallCushion;

            return newParams;
        }

        internal static CameraTargetParams.CameraParamsOverrideHandle OverrideCameraParams(CameraTargetParams camParams, MonsterHunterCameraParams camera, float transitionDuration = 0.5f)
        {
            CharacterCameraParamsData paramsData = GetNewParams(camera);

            CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
            {
                cameraParamsData = paramsData,
                priority = 0,
            };

            return camParams.AddParamsOverride(request, transitionDuration);
        }

        internal static CharacterCameraParams CreateCameraParamsWithData(MonsterHunterCameraParams camera)
        {
            CharacterCameraParams newCameraParams = ScriptableObject.CreateInstance<CharacterCameraParams>();

            newCameraParams.name = camera.ToString().ToLower() + "Params";

            newCameraParams.data = GetNewParams(camera);

            return newCameraParams;
        }

        internal static CharacterCameraParamsData GetNewParams(MonsterHunterCameraParams camera)
        {
            CharacterCameraParamsData paramsData = defaultCameraParams;

            switch (camera)
            {

                default:
                case MonsterHunterCameraParams.DEFAULT:
                    paramsData = defaultCameraParams;
                    break;
                case MonsterHunterCameraParams.AIM:
                    paramsData = aimCameraParams;
                    break;
                case MonsterHunterCameraParams.EMOTE:
                    paramsData = emoteCameraParams;
                    break;
                case MonsterHunterCameraParams.CLOSE:
                    paramsData = closeCameraParams;
                    break;
                case MonsterHunterCameraParams.FAR:
                    paramsData = farCameraParams;
                    break;
            }

            return paramsData;
        }
    }
}