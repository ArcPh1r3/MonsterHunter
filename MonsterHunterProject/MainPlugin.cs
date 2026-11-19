using System;
using BepInEx;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using R2API.Networking;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine.AddressableAssets;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace MonsterHunter
{
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    //[BepInDependency("bubbet.riskui", BepInDependency.DependencyFlags.SoftDependency)]
    //[BepInDependency("com.DestroyedClone.AncientScepter", BepInDependency.DependencyFlags.SoftDependency)]
    //[BepInDependency("com.Viliger.EnemiesReturns", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]

    public class MainPlugin : BaseUnityPlugin
    {
        public const string MODUID = "com.rob.MonsterHunter";
        public const string MODNAME = "MonsterHunter";
        public const string MODVERSION = "0.0.1";

        public static MainPlugin instance;

        //public static bool scepterInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.DestroyedClone.AncientScepter");
        public static bool rooInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");
        //public static bool hunkInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rob.Hunk");
        //public static bool greenAlienHeadInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Borbo.GreenAlienHead");
        //public static bool enemiesReturnsInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Viliger.EnemiesReturns");
        /*public static bool riskUIInstalled
        {
            get
            {
                if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("bubbet.riskui"))
                {
                    if (MainPlugin.RiskUIEnabled()) return true;
                }
                return false;
            }
        }*/

        private void Awake()
        {
            instance = this;
            Modules.Files.PluginInfo = this.Info;

            Modules.Config.myConfig = Config;

            Log.Init(Logger);
            Modules.Config.ReadConfig();
            Modules.MonsterHunterAssets.PopulateAssets();
            Modules.CameraParams.InitializeParams();
            Modules.States.RegisterStates();
            Modules.Projectiles.RegisterProjectiles();
            new Modules.LanguageTokens();

            //NetworkingAPI.RegisterMessageType<Content.Components.SyncGainGauge>();

            //devilArmTier = (ItemTierDef)ScriptableObject.CreateInstance<ItemTierDef
            /*Content.Survivors.MonsterHunter.devilArmTier = ItemTierDef.Instantiate(Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/BossTierDef.asset").WaitForCompletion());
            Content.Survivors.MonsterHunter.devilArmTier.name = "DevilArmTierDef";
            Content.Survivors.MonsterHunter.devilArmTier.pickupRules = ItemTierDef.PickupRules.ConfirmAll;
            Content.Survivors.MonsterHunter.devilArmTier.isDroppable = false;
            Content.Survivors.MonsterHunter.devilArmTier.colorIndex = ColorCatalog.ColorIndex.VoidCoin;
            Content.Survivors.MonsterHunter.devilArmTier.darkColorIndex = ColorCatalog.ColorIndex.VoidCoin;
            Content.Survivors.MonsterHunter.devilArmTier.canScrap = false;
            Content.Survivors.MonsterHunter.devilArmTier.canRestack = false;
            Content.Survivors.MonsterHunter.devilArmTier.tier = ItemTier.AssignedAtRuntime;
            Content.Survivors.MonsterHunter.devilArmTier._tier = ItemTier.AssignedAtRuntime;*/
            //devilArmTier.bgIconTexture = null;
            //devilArmTier.canRestack

            new Content.Survivors.MonsterHunter().CreateCharacter();

            Hook();

            new Modules.ContentPacks().Initialize();

            RoR2.ContentManagement.ContentManager.onContentPacksAssigned += LateSetup;
        }

        private void LateSetup(global::HG.ReadOnlyArray<RoR2.ContentManagement.ReadOnlyContentPack> obj)
        {
            //Content.Survivors.Dante.SetItemDisplays();
        }

        private void Hook()
        {
        }

        public static float GetICBMDamageMult(CharacterBody body)
        {
            float mult = 1f;
            if (body && body.inventory)
            {
                int itemcount = body.inventory.GetItemCountEffective(DLC1Content.Items.MoreMissile);
                int stack = itemcount - 1;
                if (stack > 0) mult += stack * 0.5f;
            }
            return mult;
        }

        /*private static bool RiskUIEnabled()
        {
            return MaterialHud.RiskUIPlugin.Enabled.Value;
        }*/
    }
}