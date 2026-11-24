using R2API;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.PostProcessing;
using ThreeEyedGames;
using RoR2.UI;
using UnityEngine.UI;
using MonsterHunter.Content.Components;

namespace MonsterHunter.Modules
{
    public static class MonsterHunterAssets
    {
        internal static List<ItemDef> itemDefs = new List<ItemDef>();

        public static AssetBundle mainAssetBundle;

        internal static Shader hotpoo = Resources.Load<Shader>("Shaders/Deferred/HGStandard");
        internal static Material commandoMat;

        internal static List<EffectDef> effectDefs = new List<EffectDef>();
        internal static List<NetworkSoundEventDef> networkSoundEventDefs = new List<NetworkSoundEventDef>();

        internal static GameObject crosshairPrefab;

        internal static GameObject swingEffect;
        internal static GameObject critSwingEffect;
        internal static GameObject swingEffectFinisher;
        internal static GameObject critSwingEffectFinisher;

        internal static GameObject kickSwingEffect;

        internal static GameObject slashImpactEffect;
        internal static GameObject redSlashImpactEffect;
        internal static GameObject finisherImpactEffect;
        internal static GameObject kickImpactEffect;

        public static GameObject groundPunchEffectPrefab;
        public static GameObject groundPunchHeavyEffectPrefab;

        public static GameObject swordChargeEffect;

        public static GameObject groundSlamImpact;
        public static GameObject groundSlamImpactLunar;

        public static GameObject tackleEffect;

        public static GameObject trackerPrefab;

        internal static NetworkSoundEventDef swordImpactSoundEvent;
        internal static NetworkSoundEventDef kickImpactSoundEvent;
        internal static NetworkSoundEventDef tackleImpactSoundEvent;

        //public static Dictionary<ItemDef, string> customItemInteractionNotifications = new Dictionary<ItemDef, string>();

        internal static void PopulateAssets()
        {
            mainAssetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(MainPlugin.instance.Info.Location), "assetbundles", "robmonsterhunter"));

            Modules.Config.InitROO(MonsterHunterAssets.mainAssetBundle.LoadAsset<Sprite>("texMonsterHunterIcon"), " AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAHHHHHHHHHHHHHHHHHHHHHHHHH");

            //

            crosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/SimpleDotCrosshair.prefab").WaitForCompletion().InstantiateClone("RobMonsterHunterCrosshair", false);
            //crosshairPrefab.GetComponent<RawImage>().enabled = false;

            //

            swingEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlash.prefab").WaitForCompletion().InstantiateClone("RobDanteBladeSwing", false);

            Material swingMat = Material.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Huntress/matHuntressSwingTrail.mat").WaitForCompletion());
            swingMat.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampMagmaWorm.png").WaitForCompletion());
            swingMat.SetFloat("_Boost", 2f);
            swingMat.SetFloat("_AlphaBoost", 3f);

            swingEffect.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = swingMat;
            swingEffect.transform.GetChild(0).localScale = new Vector3(1.5f, 1.5f, 3f); 

            //

            critSwingEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlash.prefab").WaitForCompletion().InstantiateClone("RobDanteBladeSwingCrit", false);

            Material critSwingMat = Material.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Huntress/matHuntressSwingTrail.mat").WaitForCompletion());
            critSwingMat.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampGolem.png").WaitForCompletion());
            critSwingMat.SetFloat("_Boost", 10f);
            critSwingMat.SetFloat("_AlphaBoost", 3f);

            critSwingEffect.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = critSwingMat;

            //

            swingEffectFinisher = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordFinisherSlash.prefab").WaitForCompletion().InstantiateClone("RobDanteBladeSwingFinisher", false);

            swingEffectFinisher.transform.GetChild(1).GetComponent<ParticleSystemRenderer>().material = swingMat;

            //

            critSwingEffectFinisher = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordFinisherSlash.prefab").WaitForCompletion().InstantiateClone("RobDanteCritBladeSwingFinisher", false);

            critSwingEffectFinisher.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Titan/matTitanLaserGlob.mat").WaitForCompletion();
            critSwingEffectFinisher.transform.GetChild(1).GetComponent<ParticleSystemRenderer>().material = critSwingMat;

            //

            kickSwingEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlash.prefab").WaitForCompletion().InstantiateClone("RobDanteKickSwing", false);
            swingMat = Material.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Brother/matBrotherSwing.mat").WaitForCompletion());
            //swingMat.SetColor("_TintColor", new Color(91f / 255f, 49f / 255f, 0f, 1f));
            swingMat.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampHuntressSoft2.png").WaitForCompletion());
            swingMat.SetFloat("_Boost", 4f);
            kickSwingEffect.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = swingMat;

            //

            slashImpactEffect = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniImpactVFXSlash.prefab").WaitForCompletion(), "RobMonsterHunterSlashImpactVFX", false);

            slashImpactEffect.transform.Find("Dash, Bright").localScale = Vector3.one * 5f;
            slashImpactEffect.transform.Find("Flash, Hard").localScale = Vector3.one * 4f;
            slashImpactEffect.transform.Find("Scaled Hitspark 3, Radial (Random Color)").localScale = Vector3.one * 3f;
            slashImpactEffect.transform.Find("Impact Slash").localScale = Vector3.one * 4f;
            slashImpactEffect.transform.Find("Scaled Hitspark 2 (Random Color)").localScale = Vector3.one * 2f;
            slashImpactEffect.transform.Find("Flash, Hard").localScale = Vector3.one * 4f;
            slashImpactEffect.transform.Find("ScaledSmokeRing, Mesh").localScale = Vector3.one * 4f;
            slashImpactEffect.GetComponent<OmniEffect>().enabled = false;

            slashImpactEffect.transform.Find("Scaled Hitspark 3, Radial (Random Color)").gameObject.SetActive(true);
            slashImpactEffect.transform.Find("Flash, Hard").gameObject.SetActive(true);
            slashImpactEffect.transform.Find("Dash, Bright").gameObject.SetActive(true);
            slashImpactEffect.transform.Find("ScaledSmokeRing, Mesh").gameObject.SetActive(true);
            slashImpactEffect.transform.Find("Impact Slash").gameObject.SetActive(true);
            slashImpactEffect.transform.Find("Scaled Hitspark 2 (Random Color)").gameObject.SetActive(true);

            foreach (ParticleSystem poopfart00 in slashImpactEffect.GetComponentsInChildren<ParticleSystem>())
            {
                if (poopfart00)
                {
                    var shitandpiss = poopfart00.main;
                    shitandpiss.playOnAwake = true;
                }
            }

            VFXAttributes v = slashImpactEffect.GetComponent<VFXAttributes>();
            v.vfxPriority = VFXAttributes.VFXPriority.Always;
            v.DoNotPool = true;

            /*slashImpactEffect.transform.Find("Flash, Hard").localScale = new Vector3(4f, 4f, 4f);
            slashImpactEffect.transform.Find("Dash, Bright").localScale = new Vector3(5f, 3f, 3f);
            slashImpactEffect.transform.Find("ScaledSmokeRing, Mesh").localScale = new Vector3(4f, 4f, 4f);
            slashImpactEffect.transform.Find("ScaledSmokeRing, Mesh").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Junk/LunarWisp/matLunarWispBombClouds.mat").WaitForCompletion();
            slashImpactEffect.transform.Find("Impact Slash").localScale = new Vector3(2f, 2f, 2f);
            slashImpactEffect.transform.Find("Scaled Hitspark 4, Directional (Random Color)").localScale = new Vector3(2f, 2f, 2f);
            slashImpactEffect.transform.Find("Scaled Hitspark 3, Radial (Random Color)").localScale = new Vector3(2f, 2f, 2f);
            slashImpactEffect.transform.Find("Scaled Hitspark 2 (Random Color)").localScale = new Vector3(2f, 2f, 2f);*/

            AddNewEffectDef(slashImpactEffect);

            //

            finisherImpactEffect = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/OmniImpactVFXSlashMerc.prefab").WaitForCompletion(), "RobMonsterHunterFinisherImpactVFX", false);

            finisherImpactEffect.transform.Find("Flash, Hard").localScale = new Vector3(9f, 9f, 9f);
            finisherImpactEffect.transform.Find("Flash, Hard").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/VoidSurvivor/matVoidSurvivorBlasterCoreCorrupted.mat").WaitForCompletion();
            finisherImpactEffect.transform.Find("Dash, Bright").localScale = new Vector3(7f, 7f, 7f);
            finisherImpactEffect.transform.Find("Dash, Bright").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matBloodHumanLarge.mat").WaitForCompletion();
            finisherImpactEffect.transform.Find("ScaledSmokeRing, Mesh").localScale = new Vector3(8f, 8f,8f);
            finisherImpactEffect.transform.Find("ScaledSmokeRing, Mesh").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matBloodHumanLarge.mat").WaitForCompletion();
            finisherImpactEffect.transform.Find("Impact Slash").localScale = new Vector3(6f, 6f, 6f);
            finisherImpactEffect.transform.Find("Impact Slash").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matBloodHumanLarge.mat").WaitForCompletion();
            finisherImpactEffect.transform.Find("Scaled Hitspark 4, Directional (Random Color)").localScale = new Vector3(2f, 2f, 2f);
            finisherImpactEffect.transform.Find("Scaled Hitspark 3, Radial (Random Color)").localScale = new Vector3(4f, 4f, 4f);
            finisherImpactEffect.transform.Find("Scaled Hitspark 3, Radial (Random Color)").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/DLC3/DroneScrapper/matDroneScrapperHitSpark.mat").WaitForCompletion();
            finisherImpactEffect.transform.Find("Scaled Hitspark 2 (Random Color)").localScale = new Vector3(2f, 2f, 2f);
            finisherImpactEffect.GetComponent<OmniEffect>().enabled = false;

            var sex2 = finisherImpactEffect.transform.Find("Impact Slash").GetComponent<ParticleSystem>().main;
            sex2.startLifetimeMultiplier *= 8f;
            sex2 = finisherImpactEffect.transform.Find("Dash, Bright").GetComponent<ParticleSystem>().main;
            sex2.startLifetimeMultiplier *= 10f;
            sex2 = finisherImpactEffect.transform.Find("ScaledSmokeRing, Mesh").GetComponent<ParticleSystem>().main;
            sex2.startLifetimeMultiplier *= 8f;
            sex2 = finisherImpactEffect.transform.Find("Flash, Hard").GetComponent<ParticleSystem>().main;
            sex2.startLifetimeMultiplier *= 2f;

            finisherImpactEffect.transform.Find("Scaled Hitspark 3, Radial (Random Color)").gameObject.SetActive(true);
            finisherImpactEffect.transform.Find("Flash, Hard").gameObject.SetActive(true);
            finisherImpactEffect.transform.Find("Dash, Bright").gameObject.SetActive(true);
            finisherImpactEffect.transform.Find("ScaledSmokeRing, Mesh").gameObject.SetActive(true);
            finisherImpactEffect.transform.Find("Impact Slash").gameObject.SetActive(true);
            finisherImpactEffect.transform.Find("Scaled Hitspark 2 (Random Color)").gameObject.SetActive(true);
            finisherImpactEffect.transform.Find("Flash, Hard").gameObject.SetActive(true);

            foreach (ParticleSystem poopfart00 in finisherImpactEffect.GetComponentsInChildren<ParticleSystem>())
            {
                if (poopfart00)
                {
                    var shitandpiss = poopfart00.main;
                    shitandpiss.playOnAwake = true;
                }
            }

            v = finisherImpactEffect.GetComponent<VFXAttributes>();
            v.vfxPriority = VFXAttributes.VFXPriority.Always;
            v.DoNotPool = true;

            AddNewEffectDef(finisherImpactEffect, "");

            //


            redSlashImpactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/OmniImpactVFXSlashMerc.prefab").WaitForCompletion().InstantiateClone("RobDanteRedSlashImpactVFX", false);
            redSlashImpactEffect.GetComponent<OmniEffect>().enabled = false;

            Material hitsparkMat = Material.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Merc/matOmniHitspark3Merc.mat").WaitForCompletion());
            hitsparkMat.SetColor("_TintColor", Color.red);

            redSlashImpactEffect.transform.GetChild(1).gameObject.GetComponent<ParticleSystemRenderer>().material = hitsparkMat;

            redSlashImpactEffect.transform.GetChild(2).localScale = Vector3.one * 1.5f;
            redSlashImpactEffect.transform.GetChild(2).gameObject.GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/VoidSurvivor/matVoidSurvivorBlasterFireCorrupted.mat").WaitForCompletion();

            Material slashMat = Material.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Merc/matOmniRadialSlash1Merc.mat").WaitForCompletion());
            slashMat.SetColor("_TintColor", Color.red);

            redSlashImpactEffect.transform.GetChild(5).gameObject.GetComponent<ParticleSystemRenderer>().material = slashMat;

            redSlashImpactEffect.transform.GetChild(4).localScale = Vector3.one * 3f;
            redSlashImpactEffect.transform.GetChild(4).gameObject.GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpDust.mat").WaitForCompletion();

            redSlashImpactEffect.transform.GetChild(6).GetChild(0).gameObject.GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/Common/Void/matOmniHitspark1Void.mat").WaitForCompletion();
            redSlashImpactEffect.transform.GetChild(6).gameObject.GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/Common/Void/matOmniHitspark2Void.mat").WaitForCompletion();

            redSlashImpactEffect.transform.GetChild(1).localScale = Vector3.one * 1.5f;

            redSlashImpactEffect.transform.GetChild(1).gameObject.SetActive(true);
            redSlashImpactEffect.transform.GetChild(2).gameObject.SetActive(true);
            redSlashImpactEffect.transform.GetChild(3).gameObject.SetActive(true);
            redSlashImpactEffect.transform.GetChild(4).gameObject.SetActive(true);
            redSlashImpactEffect.transform.GetChild(5).gameObject.SetActive(true);
            redSlashImpactEffect.transform.GetChild(6).gameObject.SetActive(true);
            redSlashImpactEffect.transform.GetChild(6).GetChild(0).gameObject.SetActive(true);

            redSlashImpactEffect.transform.GetChild(6).transform.localScale = new Vector3(1f, 1f, 3f);

            redSlashImpactEffect.transform.localScale = Vector3.one * 1.5f;

            AddNewEffectDef(redSlashImpactEffect);

            //

            kickImpactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Loader/OmniImpactVFXLoader.prefab").WaitForCompletion().InstantiateClone("RobMonsterHunterKickImpact", false);
            kickImpactEffect.GetComponent<OmniEffect>().enabled = false;

            hitsparkMat = Material.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Huntress/matOmniHitspark1Huntress.mat").WaitForCompletion());
            hitsparkMat.SetColor("_TintColor", Color.red);

            kickImpactEffect.transform.GetChild(0).gameObject.GetComponent<ParticleSystemRenderer>().material = hitsparkMat;

            hitsparkMat = Material.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Huntress/matOmniHitspark3Huntress.mat").WaitForCompletion());
            hitsparkMat.SetColor("_TintColor", Color.red);
            kickImpactEffect.transform.GetChild(1).gameObject.GetComponent<ParticleSystemRenderer>().material = hitsparkMat;

            kickImpactEffect.transform.GetChild(3).gameObject.GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/Railgunner/matRailgunTracerHead.mat").WaitForCompletion();

            hitsparkMat = Material.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOmniRing2Generic.mat").WaitForCompletion());
            hitsparkMat.SetColor("_TintColor", Color.red);
            kickImpactEffect.transform.GetChild(5).gameObject.GetComponent<ParticleSystemRenderer>().material = hitsparkMat;

            kickImpactEffect.transform.GetChild(4).gameObject.GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Toolbot/matToolbotImpact.mat").WaitForCompletion();

            kickImpactEffect.transform.GetChild(0).gameObject.SetActive(true);
            kickImpactEffect.transform.GetChild(1).gameObject.SetActive(true);
            kickImpactEffect.transform.GetChild(2).gameObject.SetActive(true);
            kickImpactEffect.transform.GetChild(3).gameObject.SetActive(true);
            kickImpactEffect.transform.GetChild(4).gameObject.SetActive(true);
            kickImpactEffect.transform.GetChild(5).gameObject.SetActive(true);

            AddNewEffectDef(kickImpactEffect);

            //

            //

            swordImpactSoundEvent = CreateNetworkSoundEventDef("sfx_mh_sword_impact");
            kickImpactSoundEvent = CreateNetworkSoundEventDef("sfx_mh_sword_impact");
            tackleImpactSoundEvent = CreateNetworkSoundEventDef("sfx_mh_tackle_impact");

            //

            groundPunchEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BeetleGuard/BeetleGuardGroundSlam.prefab").WaitForCompletion().InstantiateClone("RobMonsterHunterGroundPunchImpact", false);

            groundPunchEffectPrefab.transform.Find("ParticleInitial/Debris, 3D").localScale *= 0.35f;
            groundPunchEffectPrefab.transform.Find("ParticleInitial/Debris").localScale *= 0.35f;
            groundPunchEffectPrefab.transform.Find("ParticleInitial/DustRing").localScale *= 0.2f;
            groundPunchEffectPrefab.transform.Find("ParticleInitial/Decal").localScale *= 0.25f;

            groundPunchEffectPrefab.transform.Find("ParticleInitial/Spikes, Large").gameObject.SetActive(false);
            groundPunchEffectPrefab.transform.Find("ParticleInitial/Spikes, Small").gameObject.SetActive(false);
            groundPunchEffectPrefab.transform.Find("ParticleInitial/Flash").transform.localScale *= 0.3f;
            groundPunchEffectPrefab.transform.Find("ParticleInitial/Dust").transform.localScale *= 0.2f;

            groundPunchEffectPrefab.GetComponentInChildren<AnimateShaderAlpha>().timeMax = 10f;
            groundPunchEffectPrefab.GetComponent<ShakeEmitter>().wave.amplitude *= 0.2f;

            MainPlugin.Destroy(groundPunchEffectPrefab.GetComponent<DestroyOnParticleEnd>());
            groundPunchEffectPrefab.AddComponent<DestroyOnTimer>().duration = 10f;

            Modules.MonsterHunterAssets.AddNewEffectDef(groundPunchEffectPrefab);

            //

            groundPunchHeavyEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BeetleGuard/BeetleGuardGroundSlam.prefab").WaitForCompletion().InstantiateClone("RobMonsterHunterGroundPunchHeavyImpact", false);

            groundPunchHeavyEffectPrefab.transform.Find("ParticleInitial/Debris, 3D").localScale *= 0.5f;
            groundPunchHeavyEffectPrefab.transform.Find("ParticleInitial/Debris").localScale *= 0.5f;
            groundPunchHeavyEffectPrefab.transform.Find("ParticleInitial/DustRing").localScale *= 0.5f;
            groundPunchHeavyEffectPrefab.transform.Find("ParticleInitial/Decal").localScale *= 0.5f;

            groundPunchHeavyEffectPrefab.transform.Find("ParticleInitial/Spikes, Large").gameObject.SetActive(false);
            groundPunchHeavyEffectPrefab.transform.Find("ParticleInitial/Spikes, Small").gameObject.SetActive(false);
            groundPunchHeavyEffectPrefab.transform.Find("ParticleInitial/Flash").transform.localScale *= 0.3f;
            groundPunchHeavyEffectPrefab.transform.Find("ParticleInitial/Dust").transform.localScale *= 0.5f;

            groundPunchHeavyEffectPrefab.GetComponentInChildren<AnimateShaderAlpha>().timeMax = 10f;
            groundPunchHeavyEffectPrefab.GetComponent<ShakeEmitter>().wave.amplitude *= 0.5f;

            MainPlugin.Destroy(groundPunchHeavyEffectPrefab.GetComponent<DestroyOnParticleEnd>());
            groundPunchHeavyEffectPrefab.AddComponent<DestroyOnTimer>().duration = 10f;

            Modules.MonsterHunterAssets.AddNewEffectDef(groundPunchHeavyEffectPrefab);

            //

            trackerPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/HuntressTrackingIndicator"), "RobMonsterHunterTrackerPrefab", false);
            trackerPrefab.transform.Find("Core Pip").GetComponent<SpriteRenderer>().sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUICircleOutline.png").WaitForCompletion();
            trackerPrefab.transform.Find("Core Pip").GetComponent<SpriteRenderer>().color = new Color(105f / 255f, 221f / 255f, 1f, 45f / 255f);
            trackerPrefab.transform.Find("Core Pip").localScale = new Vector3(0.11f, 0.11f, 0.11f);

            trackerPrefab.transform.Find("Core, Dark").GetComponent<SpriteRenderer>().sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Captain/texCaptainCrosshairOuter.png").WaitForCompletion();
            //new Color(144f / 255f, 0f, 0f, 220f / 255f);
            trackerPrefab.transform.Find("Core, Dark").GetComponent<SpriteRenderer>().color = new Color(81f / 255f, 248f / 255f, 1f, 75f / 255f);
            trackerPrefab.transform.Find("Core, Dark").localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
            trackerPrefab.transform.Find("Core, Dark").localScale = Vector3.one * 0.12f;
            //trackerPrefab.transform.Find("Core, Dark").localScale = new Vector3(0.1f, 0.1f, 0.1f);

            //trackerPrefab.transform.Find("Holder").gameObject.SetActive(false);

            foreach (SpriteRenderer i in trackerPrefab.transform.Find("Holder").gameObject.GetComponentsInChildren<SpriteRenderer>())
            {
                if (i)
                {
                    i.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Engi/texUIEngiMissileLockedOn.png").WaitForCompletion();
                    i.gameObject.transform.localPosition = new Vector3(0.19f, 0f, 0f);
                    i.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 270f));
                    i.gameObject.transform.localScale = new Vector3(0.035f, 0.04f, 1f);
                    i.color = new Color(255f / 255f, 215f / 255f, 134f / 255f, 40f / 255f);
                }
            }

            //
            
            groundSlamImpact = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherSlamImpact.prefab").WaitForCompletion().InstantiateClone("RobMonsterHunterGroundSlamImpact", false);

            groundSlamImpact.transform.Find("Spikes, Small").gameObject.SetActive(false);
            groundSlamImpact.transform.Find("PP").gameObject.SetActive(false);
            groundSlamImpact.transform.Find("Point light").gameObject.SetActive(false);
            groundSlamImpact.transform.Find("Flash Lines, Fire").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matFirePillarParticle.mat").WaitForCompletion();
            groundSlamImpact.transform.Find("Fire").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matFirePillarParticle.mat").WaitForCompletion();
            groundSlamImpact.transform.Find("Physics").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/MagmaWorm/matFracturedGround.mat").WaitForCompletion();
            groundSlamImpact.transform.Find("Decal").GetComponent<Decal>().Material = Addressables.LoadAssetAsync<Material>("RoR2/Base/BeetleGroup/matBeetleGuardSlamDecal.mat").WaitForCompletion();

            AddNewEffectDef(groundSlamImpact);

            //

            groundSlamImpactLunar = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherSlamImpact.prefab").WaitForCompletion().InstantiateClone("RobMonsterHunterGroundSlamImpactLunar", false);
            AddNewEffectDef(groundSlamImpactLunar);

            //

            swordChargeEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/FalseSon/FalseSonLaserCharge.prefab").WaitForCompletion().InstantiateClone("RobMonsterHunterChargeEffect", false);

            Material poopfart6767 = Material.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/DLC2/FalseSonBoss/matLunarGazeFireLaser3.mat").WaitForCompletion());
            poopfart6767.SetColor("_TintColor", new Color(48f / 255f, 146f / 255f, 1f, 1f));
            poopfart6767.SetFloat("_InvFade", 3f);

            Material poopfart676767 = Material.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/LunarWisp/matLunarWispMuzzleBurst.mat").WaitForCompletion());
            poopfart676767.SetTexture("_MainTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/VFX/ParticleMasks/texSoftGlowRing.png").WaitForCompletion());

            swordChargeEffect.transform.Find("Sparks").localScale = Vector3.one * 0.35f;
            swordChargeEffect.transform.Find("Sparks/Backdrop").localScale = Vector3.one * 0.5f;
            swordChargeEffect.transform.Find("Sparks/Backdrop").GetComponent<ParticleSystemRenderer>().material = poopfart676767;
            swordChargeEffect.transform.Find("Sparks/Flare (3)").localScale = Vector3.one * 0.5f;
            swordChargeEffect.transform.Find("Sparks/Flare (3)").GetComponent<ParticleSystemRenderer>().material = poopfart6767;
            //swordChargeEffect.transform.Find("FlameTrails, World").GetComponent<ParticleSystemRenderer>().trailMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/DeathMark/matDeathMarkSkull.mat").WaitForCompletion();
            //swordChargeEffect.transform.Find("FlameBillboards, Local").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/VoidSurvivor/matVoidBlinkBillboardCorrupted.mat").WaitForCompletion();
            //swordChargeEffect.transform.Find("FlameBillboards, Local").localScale *= 0.5f;
            //swordChargeEffect.transform.Find("SmokeBillboard").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Brother/matBrotherSwingDistortion.mat").WaitForCompletion();
            //swordChargeEffect.transform.Find("SmokeBillboard").localScale *= 0.35f;
            //swordChargeEffect.transform.Find("Sparks").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/DeathMark/matDeathMarkFire.mat").WaitForCompletion();
            //swordChargeEffect.transform.Find("Ring, Dark").gameObject.SetActive(false);//.GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/moon2/matMoonBatteryTrailBloodSiphon.mat").WaitForCompletion();
            //swordChargeEffect.GetComponentInChildren<Light>().color = Color.red;
            //swordChargeEffect.GetComponentInChildren<Light>().range *= 0.25f;

            //

            tackleEffect = PrefabAPI.InstantiateClone(RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BrotherDashEffect"), "RobMonsterHunterTackleEffect", false);
            tackleEffect.GetComponent<EffectComponent>().applyScale = true;
            tackleEffect.transform.localScale *= 0.5f;

            //tackleEffect.transform.Find("Dash").gameObject.SetActive(false);
            //tackleEffect.transform.Find("Flash, White").gameObject.SetActive(false);
            tackleEffect.transform.Find("Donut").transform.localScale = Vector3.one * 0.35f;
            tackleEffect.transform.Find("Donut").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOpaqueDustSpeckledHuge.mat").WaitForCompletion();
            tackleEffect.transform.Find("Donut, Distortion").transform.localScale = Vector3.one * 0.35f;
            //tackleEffect.GetComponentInChildren<Light>().color = Color.red;

            AddNewEffectDef(tackleEffect);

            //Material bloodMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matBloodHumanLarge.mat").WaitForCompletion();
            //Material bloodMat2 = Addressables.LoadAssetAsync<Material>("RoR2/Base/moon2/matBloodSiphon.mat").WaitForCompletion();

            /*customNotificationPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/NotificationPanel2.prefab").WaitForCompletion().InstantiateClone("DanteNotification", false);
            DanteNotification _new = customNotificationPrefab.AddComponent<DanteNotification>();
            GenericNotification _old = customNotificationPrefab.GetComponent<GenericNotification>();

            _new.titleText = _old.titleText;
            _new.titleTMP = _old.titleTMP;
            _new.descriptionText = _old.descriptionText;
            _new.iconImage = _old.iconImage;
            _new.previousIconImage = _old.previousIconImage;
            _new.canvasGroup = _old.canvasGroup;
            _new.fadeOutT = _old.fadeOutT;

            _old.enabled = false;*/
        }

        public static void DontPool(this GameObject gob)
        {
            VFXAttributes vfx = gob.GetComponent<VFXAttributes>();
            if (vfx == null) vfx = gob.AddComponent<VFXAttributes>();
            vfx.DoNotPool = true;
        }

        private static GameObject CreateBloodExplosionEffect(string effectName, Material bloodMat, float scale = 1f, Material decalMat = null)
        {
            GameObject newEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherSlamImpact.prefab").WaitForCompletion().InstantiateClone(effectName, true);

            var huh = newEffect.GetComponent<DestroyOnTimer>();
            if (huh) huh.duration = 360f;

            newEffect.transform.Find("Spikes, Small").gameObject.SetActive(false);

            newEffect.transform.Find("PP").gameObject.SetActive(false);//.GetComponent<PostProcessVolume>().sharedProfile = Addressables.LoadAssetAsync<PostProcessProfile>("RoR2/Base/title/ppLocalMagmaWorm.asset").WaitForCompletion();
            newEffect.transform.Find("Point light").gameObject.SetActive(false);//.GetComponent<Light>().color = Color.yellow;
            newEffect.transform.Find("Flash Lines").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOpaqueDustLargeDirectional.mat").WaitForCompletion();

            newEffect.transform.GetChild(3).GetComponent<ParticleSystemRenderer>().material = bloodMat;
            newEffect.transform.Find("Flash Lines, Fire").GetComponent<ParticleSystemRenderer>().material = bloodMat;
            newEffect.transform.GetChild(6).GetComponent<ParticleSystemRenderer>().material = bloodMat;
            newEffect.transform.Find("Fire").GetComponent<ParticleSystemRenderer>().material = bloodMat;

            var sex = newEffect.transform.Find("Fire").GetComponent<ParticleSystem>().main;
            sex.startLifetimeMultiplier = 0.7f;
            sex = newEffect.transform.Find("Flash Lines, Fire").GetComponent<ParticleSystem>().main;
            sex.startLifetimeMultiplier = 0.5f;
            sex = newEffect.transform.GetChild(6).GetComponent<ParticleSystem>().main;
            sex.startLifetimeMultiplier = 0.6f;

            newEffect.transform.Find("Physics").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/MagmaWorm/matFracturedGround.mat").WaitForCompletion();

            newEffect.transform.Find("Decal").GetComponent<Decal>().Material = decalMat;
            newEffect.transform.Find("Decal").GetComponent<AnimateShaderAlpha>().timeMax = 360f;
            newEffect.transform.Find("Decal").transform.localPosition = Vector3.zero;
            newEffect.transform.Find("Decal").transform.localScale = Vector3.one * 8f;

            newEffect.transform.Find("FoamSplash").gameObject.SetActive(false);
            newEffect.transform.Find("FoamBilllboard").gameObject.SetActive(false);
            newEffect.transform.Find("Dust").gameObject.SetActive(false);
            newEffect.transform.Find("Dust, Directional").gameObject.SetActive(false);

            newEffect.transform.localScale = Vector3.one * scale;

            AddNewEffectDef(newEffect, "sfx_ravager_gore");

            ParticleSystemColorFromEffectData fuck = newEffect.AddComponent<ParticleSystemColorFromEffectData>();
            fuck.particleSystems = new ParticleSystem[]
            {
                newEffect.transform.Find("Fire").GetComponent<ParticleSystem>(),
                newEffect.transform.Find("Flash Lines, Fire").GetComponent<ParticleSystem>(),
                newEffect.transform.GetChild(6).GetComponent<ParticleSystem>(),
                newEffect.transform.GetChild(3).GetComponent<ParticleSystem>()
            };
            fuck.effectComponent = newEffect.GetComponent<EffectComponent>();

            return newEffect;
        }

        internal static GameObject CreateTextPopupEffect(string prefabName, string token, string soundName = "")
        {
            GameObject i = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BearProc").InstantiateClone(prefabName, true);

            i.GetComponent<EffectComponent>().soundName = soundName;
            if (!i.GetComponent<NetworkIdentity>()) i.AddComponent<NetworkIdentity>();

            i.GetComponentInChildren<RoR2.UI.LanguageTextMeshController>().token = token;

            MonsterHunterAssets.AddNewEffectDef(i);

            return i;
        }

        internal static NetworkSoundEventDef CreateNetworkSoundEventDef(string eventName)
        {
            NetworkSoundEventDef networkSoundEventDef = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            networkSoundEventDef.akId = AkSoundEngine.GetIDFromString(eventName);
            networkSoundEventDef.eventName = eventName;

            networkSoundEventDefs.Add(networkSoundEventDef);

            return networkSoundEventDef;
        }

        internal static void ConvertAllRenderersToHopooShader(GameObject objectToConvert, float emPower = 0f)
        {
            foreach (Renderer i in objectToConvert.GetComponentsInChildren<Renderer>())
            {
                if (i)
                {
                    if (i.material && i.material.shader && i.material.shader != hotpoo)
                    {
                        // exclude lines from this for obvious reasons
                        if (!i.GetComponent<LineRenderer>() && !i.gameObject.name.Contains("meshVial"))
                        {
                            Texture normalMap = null;
                            if (i.material.HasProperty("_BumpMap"))
                            {
                                // transfer normal maps cuz im not gay
                                normalMap = i.material.GetTexture("_BumpMap");
                            }

                            Texture emissionMap = null;
                            if (i.material.HasProperty("_EmissionMap"))
                            {
                                // transfer emission maps cuz im not gay
                                emissionMap = i.material.GetTexture("_EmissionMap");
                                if (emPower == 0f) emPower = 1f;
                            }

                            i.material.shader = hotpoo;

                            if (normalMap != null)
                            {
                                i.material.SetTexture("_NormalTex", normalMap);
                                i.material.SetFloat("_NormalStrength", 1f);
                            }

                            if (emissionMap != null)
                            {
                                i.material.SetTexture("_EmTex", emissionMap);
                                i.material.SetFloat("_NormalStrength", 1f);
                                i.material.SetFloat("_EmPower", emPower);
                                i.material.SetColor("_EmColor", Color.white);
                            }
                        }
                    }
                }
            }
        }

        internal static CharacterModel.RendererInfo[] SetupRendererInfos(GameObject obj)
        {
            MeshRenderer[] meshes = obj.GetComponentsInChildren<MeshRenderer>();
            CharacterModel.RendererInfo[] rendererInfos = new CharacterModel.RendererInfo[meshes.Length];

            for (int i = 0; i < meshes.Length; i++)
            {
                rendererInfos[i] = new CharacterModel.RendererInfo
                {
                    defaultMaterial = meshes[i].material,
                    renderer = meshes[i],
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                };
            }

            return rendererInfos;
        }

        private static GameObject LoadEffect(string resourceName)
        {
            return LoadEffect(resourceName, "", false);
        }

        private static GameObject LoadEffect(string resourceName, string soundName)
        {
            return LoadEffect(resourceName, soundName, false);
        }

        private static GameObject LoadEffect(string resourceName, bool parentToTransform)
        {
            return LoadEffect(resourceName, "", parentToTransform);
        }

        private static GameObject LoadEffect(string resourceName, string soundName, bool parentToTransform)
        {
            GameObject newEffect = mainAssetBundle.LoadAsset<GameObject>(resourceName);

            newEffect.AddComponent<DestroyOnTimer>().duration = 10f;
            newEffect.AddComponent<NetworkIdentity>();
            newEffect.AddComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
            var effect = newEffect.AddComponent<EffectComponent>();
            effect.applyScale = false;
            effect.effectIndex = EffectIndex.Invalid;
            effect.parentToReferencedTransform = parentToTransform;
            effect.positionAtReferencedTransform = true;
            effect.soundName = soundName;

            AddNewEffectDef(newEffect, soundName);

            return newEffect;
        }

        internal static void AddNewEffectDef(GameObject effectPrefab)
        {
            AddNewEffectDef(effectPrefab, "");
        }

        internal static void AddNewEffectDef(GameObject effectPrefab, string soundName)
        {
            EffectComponent ec = effectPrefab.GetComponent<EffectComponent>();
            if (!ec) ec = effectPrefab.AddComponent<EffectComponent>();

            VFXAttributes va = effectPrefab.GetComponent<VFXAttributes>();
            if (!va) va = effectPrefab.AddComponent<VFXAttributes>();

            EffectDef newEffectDef = new EffectDef();
            newEffectDef.prefab = effectPrefab;
            newEffectDef.prefabEffectComponent = ec;
            newEffectDef.prefabName = effectPrefab.name;
            newEffectDef.prefabVfxAttributes = va;
            newEffectDef.spawnSoundEventName = soundName;

            effectDefs.Add(newEffectDef);
        }

        public static Material CreateMaterial(string materialName, float emission, Color emissionColor, float normalStrength)
        {
            if (!commandoMat) commandoMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Commando/matCommandoDualies.mat").WaitForCompletion();

            Material mat = UnityEngine.Object.Instantiate<Material>(commandoMat);
            Material tempMat = MonsterHunterAssets.mainAssetBundle.LoadAsset<Material>(materialName);

            if (!tempMat) return commandoMat;
            if (tempMat.shader == hotpoo) return tempMat;

            mat.name = materialName;
            mat.SetColor("_Color", tempMat.GetColor("_Color"));
            mat.SetTexture("_MainTex", tempMat.GetTexture("_MainTex"));
            mat.SetColor("_EmColor", emissionColor);
            mat.SetFloat("_EmPower", emission);
            mat.SetTexture("_EmTex", tempMat.GetTexture("_EmissionMap"));
            if (normalStrength > 0f) mat.SetTexture("_NormalTex", tempMat.GetTexture("_BumpMap"));
            mat.SetFloat("_NormalStrength", normalStrength);

            //mat.DisableKeyword("DITHER");

            return mat;
        }

        public static Material CreateMaterial(string materialName)
        {
            return MonsterHunterAssets.CreateMaterial(materialName, 0f);
        }

        public static Material CreateMaterial(string materialName, float emission)
        {
            return MonsterHunterAssets.CreateMaterial(materialName, emission, Color.white);
        }

        public static Material CreateMaterial(string materialName, float emission, Color emissionColor)
        {
            return MonsterHunterAssets.CreateMaterial(materialName, emission, emissionColor, 0f);
        }
    }
}