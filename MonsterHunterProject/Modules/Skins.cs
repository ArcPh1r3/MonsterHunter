using R2API;
using RoR2;
using System;
using UnityEngine;

namespace MonsterHunter.Modules
{
    public static class Skins
    {
        public static SkinDef NewSkinDef(GameObject rootObject, string skinName, Sprite skinIcon, CharacterModel.RendererInfo[] rendererInfos)
        {
            return NewSkinDef(rootObject, skinName, skinIcon, rendererInfos, null);
        }

        public static SkinDef NewSkinDef(GameObject rootObject, string skinName, Sprite skinIcon, CharacterModel.RendererInfo[] rendererInfos, UnlockableDef unlockableDef)
        {
            SkinDef skinDef = (SkinDef)ScriptableObject.CreateInstance<SkinDef>();

            skinDef.nameToken = skinName;
            skinDef.icon = skinIcon;
            skinDef.rootObject = rootObject;
            skinDef.unlockableDef = unlockableDef;

            CharacterModel.RendererInfo[] newRendererInfos = new CharacterModel.RendererInfo[rendererInfos.Length];
            rendererInfos.CopyTo(newRendererInfos, 0);

            SkinDefParams skinDefParams = (SkinDefParams)ScriptableObject.CreateInstance<SkinDefParams>();
            skinDefParams.rendererInfos = newRendererInfos;
            skinDefParams.minionSkinReplacements = new SkinDefParams.MinionSkinReplacement[0];
            skinDefParams.projectileGhostReplacements = new SkinDefParams.ProjectileGhostReplacement[0];
            skinDefParams.meshReplacements = new SkinDefParams.MeshReplacement[0];
            skinDefParams.gameObjectActivations = new SkinDefParams.GameObjectActivation[0];

            skinDef.skinDefParams = skinDefParams;

            return skinDef;
        }

        public static SkinDef CreateSkinDef(string skinName, Sprite skinIcon, CharacterModel.RendererInfo[] rendererInfos, SkinnedMeshRenderer mainRenderer, GameObject root)
        {
            return CreateSkinDef(skinName, skinIcon, rendererInfos, mainRenderer, root, null);
        }

        public static SkinDef CreateSkinDef(string skinName, Sprite skinIcon, CharacterModel.RendererInfo[] rendererInfos, SkinnedMeshRenderer mainRenderer, GameObject root, UnlockableDef unlockableDef)
        {
            LoadoutAPI.SkinDefInfo skinDefInfo = new LoadoutAPI.SkinDefInfo
            {
                BaseSkins = Array.Empty<SkinDef>(),
                GameObjectActivations = new SkinDef.GameObjectActivation[0],
                Icon = skinIcon,
                MeshReplacements = new SkinDef.MeshReplacement[0],
                MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0],
                Name = skinName,
                NameToken = skinName,
                ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0],
                RendererInfos = rendererInfos,
                RootObject = root,
                UnlockableDef = unlockableDef
            };

            SkinDef skin = LoadoutAPI.CreateNewSkinDef(skinDefInfo);

            return skin;
        }
    }
}