using MonsterHunter.Content.Components;
using MonsterHunter.Modules;
using BepInEx.Configuration;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Skills;
using RoR2.UI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Rewired.ComponentControls.Effects;

namespace MonsterHunter.Content.Survivors
{
    public class MonsterHunter
    {
        internal static MonsterHunter instance;

        public static GameObject characterPrefab;
        internal static GameObject displayPrefab;

        internal static GameObject umbraMaster;

        //internal static ConfigEntry<bool> forceUnlock;
        //internal static ConfigEntry<bool> characterEnabled;

        public static Color characterColor = new Color(183f / 255f, 0f, 39f / 255f);

        public const string bodyName = "RobMonsterHunterBody";

        public static int bodyRendererIndex; // use this to store the rendererinfo index containing our character's body
                                             // keep it last in the rendererinfos because teleporter particles for some reason require this. hopoo pls

        internal static BodyIndex bodyIndex => BodyCatalog.FindBodyIndex(characterPrefab);

        // item display stuffs
        internal static ItemDisplayRuleSet itemDisplayRuleSet;
        internal static List<ItemDisplayRuleSet.KeyAssetRuleGroup> itemDisplayRules;

        // buff
        //public static BuffDef lockedOnBuff;

        // itemtier
        //public static ItemTierDef devilArmTier;

        // items
        //public static ItemDef keyItemDef;

        // cost types
        //public static CostTypeDef keyCostDef;
        //public static int keyCostTypeIndex;

        // interactables
        //public static GameObject devilShrinePrefab;
        //public static InteractableSpawnCard devilShrineInteractableCard;

        // night shrine stuff
        //public static HealthBarStyle devilHealthBarStyle;

        // achievements
        public static List<UnlockableDef> unlockableDefs = new List<UnlockableDef>();

        //internal static UnlockableDef characterUnlockableDef;
        internal static UnlockableDef masteryUnlockableDef;
        //internal static UnlockableDef grandMasteryUnlockableDef;
        internal static UnlockableDef completionUnlockableDef;
        internal static UnlockableDef earlySupporterUnlockableDef;

        /*internal static UnlockableDef queenUnlockableDef;
        internal static UnlockableDef titanUnlockableDef;
        internal static UnlockableDef colossusUnlockableDef;
        internal static UnlockableDef overlordUnlockableDef;
        internal static UnlockableDef yamatoUnlockableDef;
        internal static UnlockableDef spardaUnlockableDef;*/

        // skill overrides
        //internal static SkillDef confirmSkillDef;
        //internal static SkillDef cancelSkillDef;
        //internal static SkillDef lockOnSkillDef;
        //internal static SkillDef trickSkillDef;
        internal static SkillDef chargeSlashSkillDef;

        // scepter
        //public static SkillDef holyWaterScepterSkillDef;

        // stage props
        //public static GameObject spardaInteractable;

        public const string bodyNameToken = "ROB_MONSTERHUNTER_BODY_NAME";

        internal void CreateCharacter()
        {
            instance = this;

            //forceUnlock = Modules.Config.ForceUnlockConfig("Dante");

            string nameFormat = "ACHIEVEMENT_{0}_NAME";
            string descFormat = "ACHIEVEMENT_{0}_DESCRIPTION";

            LanguageAPI.Add(string.Format(nameFormat, Achievements.MonsterHunterCompletionAchievement.IDENTIFIER), "Hunter: Quest Complete");
            LanguageAPI.Add(string.Format(descFormat, Achievements.MonsterHunterCompletionAchievement.IDENTIFIER), "As <color=#B70027>Hunter</color>, beat the game.");

            LanguageAPI.Add(string.Format(nameFormat, Achievements.MonsterHunterMasteryAchievement.IDENTIFIER), "Hunter: Mastery");
            LanguageAPI.Add(string.Format(descFormat, Achievements.MonsterHunterMasteryAchievement.IDENTIFIER), "As <color=#B70027>Hunter</color>, beat the game or obliterate on Monsoon.");

            LanguageAPI.Add(string.Format(nameFormat, Achievements.MonsterHunterEarlySupporterAchievement.IDENTIFIER), "Hunter: Early Supporter");
            LanguageAPI.Add(string.Format(descFormat, Achievements.MonsterHunterEarlySupporterAchievement.IDENTIFIER), "Play <color=#B70027>Hunter</color> in beta.");

            masteryUnlockableDef = CreateAndAddUnlockableDef(Content.Achievements.MonsterHunterMasteryAchievement.IDENTIFIER, Content.Achievements.MonsterHunterMasteryAchievement.UNLOCKABLE_IDENTIFIER, Content.Achievements.MonsterHunterMasteryAchievement.Sprite);
            completionUnlockableDef = CreateAndAddUnlockableDef(Content.Achievements.MonsterHunterCompletionAchievement.IDENTIFIER, Content.Achievements.MonsterHunterCompletionAchievement.UNLOCKABLE_IDENTIFIER, Content.Achievements.MonsterHunterCompletionAchievement.Sprite);
            earlySupporterUnlockableDef = CreateAndAddUnlockableDef(Content.Achievements.MonsterHunterEarlySupporterAchievement.IDENTIFIER, Content.Achievements.MonsterHunterEarlySupporterAchievement.UNLOCKABLE_IDENTIFIER, Content.Achievements.MonsterHunterEarlySupporterAchievement.Sprite);

            //if (!forceUnlock.Value) characterUnlockableDef = R2API.UnlockableAPI.AddUnlockable<Achievements.DriverUnlockAchievement>();

            characterPrefab = CreateBodyPrefab(true);

            displayPrefab = Modules.Prefabs.CreateDisplayPrefab("MonsterHunterDisplay", characterPrefab);
            displayPrefab.GetComponentInChildren<CharacterModel>().gameObject.AddComponent<ModelSkinController>().skins = characterPrefab.GetComponentInChildren<ModelSkinController>().skins.ToArray();
            Modules.MonsterHunterAssets.ConvertAllRenderersToHopooShader(displayPrefab);

            Modules.Prefabs.RegisterNewSurvivor(characterPrefab, displayPrefab, "MONSTERHUNTER");
            //if (forceUnlock.Value) Modules.Prefabs.RegisterNewSurvivor(characterPrefab, displayPrefab, "DANTE");
            //else Modules.Prefabs.RegisterNewSurvivor(characterPrefab, displayPrefab, "DANTE", characterUnlockableDef);

            umbraMaster = CreateMaster(characterPrefab, "RobMonsterHunterMonsterMaster");

            //lockedOnBuff = Modules.Buffs.AddNewBuff("bdLockedOn", null, Color.white, false, false, true);

            Hook();
        }

        public static void CreateSparda()
        {
            /*GameObject spardaModel = Modules.MonsterHunterAssets.mainAssetBundle.LoadAsset<GameObject>("SpardaProp");
            Modules.MonsterHunterAssets.ConvertAllRenderersToHopooShader(spardaModel);

            spardaInteractable = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Barrel1/Barrel1.prefab").WaitForCompletion().InstantiateClone("RobDanteSpardaProp", true);
            spardaInteractable.GetComponent<Highlight>().targetRenderer.enabled = false;
            spardaInteractable.GetComponent<Highlight>().targetRenderer.GetComponent<SkinnedMeshRenderer>().sharedMesh = null;

            MainPlugin.Destroy(spardaInteractable.GetComponent<BarrelInteraction>());
            MainPlugin.Destroy(spardaInteractable.GetComponent<GenericDisplayNameProvider>());

            spardaInteractable.GetComponent<SfxLocator>().openSound = "";

            Transform modelTransform = spardaInteractable.GetComponent<ModelLocator>().modelTransform;
            spardaInteractable.AddComponent<SpardaInteractable>();
            modelTransform.GetComponent<Animator>().enabled = false;
            modelTransform.Find("BarrelMesh").GetComponent<SkinnedMeshRenderer>().enabled = false;

            spardaModel.transform.parent = spardaInteractable.GetComponent<Highlight>().targetRenderer.transform;
            spardaModel.transform.localPosition = new Vector3(0f, 0f, 0f);
            spardaModel.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
            spardaModel.transform.localScale = Vector3.one;

            spardaInteractable.GetComponent<Highlight>().targetRenderer = spardaModel.GetComponentInChildren<MeshRenderer>();

            GameObject.Destroy(spardaInteractable.transform.Find("HologramPivot"));*/
        }

        internal static UnlockableDef CreateAndAddUnlockableDef(string identifier, string unlockableIdentifier, Sprite achievementIcon)
        {
            var unlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockableDef.cachedName = unlockableIdentifier.ToUpperInvariant();
            unlockableDef.nameToken = "ACHIEVEMENT_" + identifier.ToUpperInvariant() + "_NAME";
            unlockableDef.achievementIcon = achievementIcon;

            unlockableDefs.Add(unlockableDef);

            return unlockableDef;
        }

        private static InspectDef NewInspectDef(Sprite icon, string nameToken, string descToken)
        {
            InspectDef inspectDef = ScriptableObject.CreateInstance<InspectDef>();

            inspectDef.Info = new InspectInfo
            {
                Visual = icon,
                TitleToken = nameToken,
                DescriptionToken = descToken,
                FlavorToken = descToken,
                ForceShowInfo = false,
                isConsumedItem = false,
                isDynamicInspectInfo = false,
                TitleColor = Color.white
            };

            return inspectDef;
        }

        private static GameObject CreateBodyPrefab(bool isPlayer)
        {
            #region Body
            GameObject newPrefab = Modules.Prefabs.CreatePrefab(bodyName, "mdlMonsterHunter", new BodyInfo
            {
                armor = 30f,
                armorGrowth = 0f,
                bodyName = "RobMonsterHunterBody",
                bodyNameToken = bodyNameToken,
                bodyColor = characterColor,
                characterPortrait = Modules.MonsterHunterAssets.mainAssetBundle.LoadAsset<Texture>("texMonsterHunterIcon"),
                crosshair = Modules.MonsterHunterAssets.crosshairPrefab,
                damage = 12f,
                healthGrowth = 64f,
                healthRegen = 1f,
                jumpCount = 1,
                maxHealth = 180f,
                subtitleNameToken = "ROB_MONSTERHUNTER_BODY_SUBTITLE",
                podPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"),
                moveSpeed = 7f,
                acceleration = 80f,
                jumpPower = 15f,
                attackSpeed = 1f,
                crit = 1f,
                modelBasePosition = Modules.Prefabs.defaultBodyInfo.modelBasePosition,
                cameraPivot = Modules.Prefabs.defaultBodyInfo.cameraPivot,
                aimOrigin = Modules.Prefabs.defaultBodyInfo.aimOrigin
            });

            ChildLocator childLocator = newPrefab.GetComponentInChildren<ChildLocator>();

            //childLocator.gameObject.AddComponent<Content.Components.DanteKatanaSfxHandler>();

            CharacterBody body = newPrefab.GetComponent<CharacterBody>();
            //body.preferredInitialStateType = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Dante.Revert));
            //body.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            //body.bodyFlags |= CharacterBody.BodyFlags.SprintAnyDirection;
            //body.sprintingSpeedMultiplier = 1.75f;

            MonsterHunterController dante = newPrefab.AddComponent<Content.Components.MonsterHunterController>();

            // sword trail
            MeleeWeaponTrail trail = childLocator.FindChild("TrailBase").gameObject.AddComponent<MeleeWeaponTrail>();

            Material trailMat = Material.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Brother/matBrotherSwingDistortion.mat").WaitForCompletion());

            trail._material = trailMat;
            trail._lifeTime = 0.1f;
            trail._colors = new Color[]
            {
                Color.white
            };
            trail._sizes = new float[]
            {
                1f
            };
            trail._base = childLocator.FindChild("TrailBase");
            trail._tip = childLocator.FindChild("TrailTip");
            trail.Emit = false;

            dante.swordTrail = trail;

            trail = childLocator.FindChild("TrailBaseB").gameObject.AddComponent<MeleeWeaponTrail>();

            trail._material = trailMat;
            trail._lifeTime = 0.1f;
            trail._colors = new Color[]
            {
                Color.white
            };
            trail._sizes = new float[]
            {
                1f
            };
            trail._base = childLocator.FindChild("TrailBaseB");
            trail._tip = childLocator.FindChild("TrailTipB");
            trail.Emit = false;

            dante.swordTrailB = trail;

            //dante.trailMat = trailMat;

            trailMat = Material.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Huntress/matHuntressSwingTrail.mat").WaitForCompletion());

            trailMat.SetTexture("_MainTex", Addressables.LoadAssetAsync<Texture>("RoR2/DLC2/FalseSonBoss/texLunarGazePlusFireLaser4.png").WaitForCompletion());
            trailMat.SetTexture("_Cloud1Tex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/TiledTextures/texCloudCrackedIceInverted.png").WaitForCompletion());
            trailMat.SetTexture("_Cloud2Tex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/TiledTextures/texCloudOrganic1.png").WaitForCompletion());
            trailMat.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/DLC3/texRampFire02.png").WaitForCompletion());
            trailMat.SetFloat("_AlphaBoost", 15f);
            trailMat.SetFloat("_Boost", 5f);
            trailMat.SetColor("_TintColor", Color.white);

            trail._material = trailMat;
            //dante.flameTrailMat = trailMat;

            //SfxLocator sfx = newPrefab.GetComponent<SfxLocator>();
            //sfx.barkSound = "";
            //sfx.landingSound = "";
            //sfx.deathSound = "";
            //sfx.fallDamageSound = "";

            //FootstepHandler footstep = newPrefab.GetComponentInChildren<FootstepHandler>();
            //footstep.footstepDustPrefab = Resources.Load<GameObject>("Prefabs/GenericHugeFootstepDust");
            //footstep.baseFootstepString = "Play_moonBrother_step";
            //footstep.sprintFootstepOverrideString = "Play_moonBrother_sprint";

            CharacterMotor characterMotor = newPrefab.GetComponent<CharacterMotor>();
            characterMotor.mass = 300f;
            //KinematicCharacterMotor characterController = newPrefab.GetComponent<KinematicCharacterMotor>();
            //characterController.CapsuleRadius = 4f;
            //characterController.CapsuleHeight = 9f;

            //CharacterDirection direction = newPrefab.GetComponent<CharacterDirection>();
            //direction.turnSoundName = "BBBBELMONT";
            //direction.turnSpeed = 135f;

            //Interactor interactor = newPrefab.GetComponent<Interactor>();
            //interactor.maxInteractionDistance = 8f;

            //newPrefab.GetComponent<CameraTargetParams>().cameraParams = Modules.CameraParams.CreateCameraParamsWithData(DriverCameraParams.DEFAULT);

            //newPrefab.GetComponent<CharacterDirection>().turnSpeed = 720f;

            foreach (EntityStateMachine i in newPrefab.GetComponents<EntityStateMachine>())
            {
                //if (i.customName == "Body") i.initialStateType = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Dante.Revert));
                if (i.customName == "Body") i.mainStateType = new EntityStates.SerializableEntityStateType(typeof(SkillStates.MonsterHunter.MainState));
            }
            //EntityStateMachine passiveController = newPrefab.AddComponent<EntityStateMachine>();
            //passiveController.initialStateType = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Ravager.WallJump));
            //passiveController.mainStateType = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Ravager.WallJump));
            //passiveController.customName = "Passive";

            //var state = isPlayer ? typeof(EntityStates.SpawnTeleporterState) : typeof(SpawnState);
            //newPrefab.GetComponent<EntityStateMachine>().initialStateType = new EntityStates.SerializableEntityStateType(state);

            // schizophrenia
            newPrefab.GetComponent<CharacterDeathBehavior>().deathState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.FuckMyAss));

            newPrefab.AddComponent<Content.Components.MonsterHunterTracker>();
            //newPrefab.AddComponent<Content.Components.EnemyStepController>();
            newPrefab.AddComponent<Content.Components.MomentumController>();
            #endregion

            #region Model
            bodyRendererIndex = 0;

            Material bodyMat = Modules.MonsterHunterAssets.CreateMaterial("matMonsterHunter");
            Material swordMat = Modules.MonsterHunterAssets.CreateMaterial("matPlaceholder");

            Modules.Prefabs.SetupCharacterModel(newPrefab, new CustomRendererInfo[] {
                new CustomRendererInfo
                {
                    childName = "Model",
                    material = bodyMat
                },
                new CustomRendererInfo
                {
                    childName = "SwordModel",
                    material = swordMat
                }}, bodyRendererIndex);
            #endregion

            CreateHitboxes(newPrefab);
            SetupHurtboxes(newPrefab);
            CreateSkills(newPrefab);
            CreateSkins(newPrefab);
            InitializeItemDisplays(newPrefab);

            return newPrefab;
        }

        private static void SetupHurtboxes(GameObject bodyPrefab)
        {
            HurtBoxGroup hurtboxGroup = bodyPrefab.GetComponentInChildren<HurtBoxGroup>();
            List<HurtBox> hurtboxes = new List<HurtBox>();

            hurtboxes.Add(bodyPrefab.GetComponentInChildren<ChildLocator>().FindChild("MainHurtbox").GetComponent<HurtBox>());

            HealthComponent healthComponent = bodyPrefab.GetComponent<HealthComponent>();

            foreach (Collider i in bodyPrefab.GetComponent<ModelLocator>().modelTransform.GetComponentsInChildren<Collider>())
            {
                if (i && i.gameObject.name != "MainHurtbox" && i.gameObject.name.Contains("Hurtbox"))
                {
                    HurtBox hurtbox = i.gameObject.AddComponent<HurtBox>();
                    hurtbox.gameObject.layer = LayerIndex.entityPrecise.intVal;
                    hurtbox.healthComponent = healthComponent;
                    hurtbox.isBullseye = false;
                    hurtbox.damageModifier = HurtBox.DamageModifier.Normal;
                    hurtbox.hurtBoxGroup = hurtboxGroup;

                    if (i.gameObject.name.Contains("Head"))
                    {
                        hurtbox.isSniperTarget = true;
                    }

                    hurtboxes.Add(hurtbox);
                }
            }

            hurtboxGroup.hurtBoxes = hurtboxes.ToArray();
        }

        private static GameObject CreateMaster(GameObject bodyPrefab, string masterName)
        {
            GameObject newMaster = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterMasters/MercMonsterMaster"), masterName, true);
            newMaster.GetComponent<CharacterMaster>().bodyPrefab = bodyPrefab;

            Modules.Prefabs.masterPrefabs.Add(newMaster);

            return newMaster;
        }

        private static void CreateHitboxes(GameObject prefab)
        {
            ChildLocator childLocator = prefab.GetComponentInChildren<ChildLocator>();
            GameObject model = childLocator.gameObject;

            Transform hitboxTransform = childLocator.FindChild("SlashHitbox");
            Modules.Prefabs.SetupHitbox(model, hitboxTransform, childLocator.FindChild("SlashHitbox2"), "Sword");

            hitboxTransform = childLocator.FindChild("TackleHitbox");
            Modules.Prefabs.SetupHitbox(model, hitboxTransform, "Body");
            /*hitboxTransform = childLocator.FindChild("KickHitbox");
            Modules.Prefabs.SetupHitbox(model, hitboxTransform, "Kick");

            hitboxTransform = childLocator.FindChild("WhipHitbox");
            Modules.Prefabs.SetupHitbox(model, hitboxTransform, "Whip");

            hitboxTransform = childLocator.FindChild("CrossHitbox");
            Transform ch1 = hitboxTransform.Find("Hitbox1");
            Transform ch2 = hitboxTransform.Find("Hitbox2");
            Transform ch3 = hitboxTransform.Find("Hitbox3");
            Transform ch4 = hitboxTransform.Find("Hitbox4");
            HitBoxGroup hitBoxGroup = model.AddComponent<HitBoxGroup>();

            HitBox hitBox1 = ch1.gameObject.AddComponent<HitBox>();
            ch1.gameObject.layer = LayerIndex.projectile.intVal;

            HitBox hitBox2 = ch2.gameObject.AddComponent<HitBox>();
            ch2.gameObject.layer = LayerIndex.projectile.intVal;

            HitBox hitBox3 = ch3.gameObject.AddComponent<HitBox>();
            ch3.gameObject.layer = LayerIndex.projectile.intVal;

            HitBox hitBox4 = ch4.gameObject.AddComponent<HitBox>();
            ch4.gameObject.layer = LayerIndex.projectile.intVal;

            hitBoxGroup.hitBoxes = new HitBox[]
            {
                hitBox1,
                hitBox2,
                hitBox3,
                hitBox4
            };

            hitBoxGroup.groupName = "HolyCross";*/
        }

        private static void CreateSkills(GameObject prefab)
        {
            MonsterHunterPassive passive = prefab.AddComponent<MonsterHunterPassive>();
            Modules.Skills.CreateSkillFamilies(prefab);

            SkillLocator skillLocator = prefab.GetComponent<SkillLocator>();

            skillLocator.passiveSkill.enabled = true;
            skillLocator.passiveSkill.skillNameToken = "ROB_MONSTERHUNTER_BODY_PASSIVE_NAME";
            skillLocator.passiveSkill.skillDescriptionToken = "ROB_MONSTERHUNTER_BODY_PASSIVE_DESCRIPTION";
            skillLocator.passiveSkill.icon = Modules.MonsterHunterAssets.mainAssetBundle.LoadAsset<Sprite>("texPassiveIcon");
            skillLocator.passiveSkill.keywordToken = "KEYWORD_SONOFSPARDA";

            #region Generic
            /*confirmSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_BELMONT_BODY_CONFIRM_NAME",
                skillNameToken = prefix + "_BELMONT_BODY_CONFIRM_NAME",
                skillDescriptionToken = prefix + "_BELMONT_BODY_CONFIRM_DESCRIPTION",
                icon = Modules.DanteAssets.mainAssetBundle.LoadAsset<Sprite>("texConfirmIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Idle)),
                activationStateMachineName = "fuck",
                baseMaxStock = 1,
                baseRechargeInterval = 0f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Any,
                resetCooldownTimerOnUse = false,
                isCombatSkill = false,
                mustKeyPress = false,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 0,
            });

            cancelSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_BELMONT_BODY_CANCEL_NAME",
                skillNameToken = prefix + "_BELMONT_BODY_CANCEL_NAME",
                skillDescriptionToken = prefix + "_BELMONT_BODY_CANCEL_DESCRIPTION",
                icon = Modules.DanteAssets.mainAssetBundle.LoadAsset<Sprite>("texCancelIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Idle)),
                activationStateMachineName = "fuck",
                baseMaxStock = 1,
                baseRechargeInterval = 0f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Any,
                resetCooldownTimerOnUse = false,
                isCombatSkill = false,
                mustKeyPress = false,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 0,
            });*/
            #endregion

            #region Primary
            SkillDef primary = Modules.Skills.CreatePrimarySteppedSkillDef(new EntityStates.SerializableEntityStateType(typeof(SkillStates.MonsterHunter.Greatsword.Combo)),
                "Weapon",
                "ROB_MONSTERHUNTER_BODY_PRIMARY_NAME",
                "ROB_MONSTERHUNTER_BODY_PRIMARY_DESCRIPTION",
                Modules.MonsterHunterAssets.mainAssetBundle.LoadAsset<Sprite>("texWIPIcon"), false);

            Modules.Skills.AddPrimarySkills(prefab, primary);
            #endregion

            #region Secondary
            chargeSlashSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {   
                skillName = "MonsterHunterChargeSlash",
                skillNameToken = "ROB_MONSTERHUNTER_BODY_SECONDARY_NAME",
                skillDescriptionToken = "ROB_MONSTERHUNTER_BODY_SECONDARY_DESCRIPTION",
                icon = Modules.MonsterHunterAssets.mainAssetBundle.LoadAsset<Sprite>("texWIPIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.MonsterHunter.Greatsword.ChargeSlash)),
                activationStateMachineName = "Weapon",
                baseMaxStock = 3,
                baseRechargeInterval = 3f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Skill,
                resetCooldownTimerOnUse = true,
                isCombatSkill = true,
                mustKeyPress = false,
                cancelSprintingOnActivation = true,
                rechargeStock = 3,
                requiredStock = 1,
                stockToConsume = 0,
                keywordTokens = new string[0]
            });

            chargeSlashSkillDef.autoHandleLuminousShot = false;
            chargeSlashSkillDef.attackSpeedBuffsRestockSpeed = true;

            Modules.Skills.AddSecondarySkills(prefab, chargeSlashSkillDef);
            #endregion

            #region Utility
            SkillDef tackleSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
                {
                    skillName = "MonsterHunterTackle",
                    skillNameToken = "ROB_MONSTERHUNTER_BODY_TACKLE_NAME",
                    skillDescriptionToken = "ROB_MONSTERHUNTER_BODY_TACKLE_DESCRIPTION",
                    icon = Modules.MonsterHunterAssets.mainAssetBundle.LoadAsset<Sprite>("texWIPIcon"),
                    activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.MonsterHunter.Skill.ChargeTackleMobile)),
                    activationStateMachineName = "Weapon",
                    baseMaxStock = 1,
                    baseRechargeInterval = 10f,
                    beginSkillCooldownOnSkillEnd = true,
                    canceledFromSprinting = false,
                    forceSprintDuringState = false,
                    fullRestockOnAssign = true,
                    interruptPriority = EntityStates.InterruptPriority.PrioritySkill,
                    resetCooldownTimerOnUse = true,
                    isCombatSkill = true,
                    mustKeyPress = true,
                    cancelSprintingOnActivation = true,
                    rechargeStock = 1,
                    requiredStock = 1,
                    stockToConsume = 1,
                    keywordTokens = new string[0]
                });

            Modules.Skills.AddUtilitySkills(prefab, tackleSkillDef);

            //Modules.Skills.AddUnlockablesToFamily(skillLocator.utility.skillFamily, null);
            #endregion

            #region Special
            SkillDef devilTriggerSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "MonsterHunterSpecial",
                skillNameToken = "ROB_MONSTERHUNTER_BODY_SPECIAL_NAME",
                skillDescriptionToken = "ROB_MONSTERHUNTER_BODY_SPECIAL_DESCRIPTION",
                icon = Modules.MonsterHunterAssets.mainAssetBundle.LoadAsset<Sprite>("texWIPIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.MonsterHunter.MainState)),
                activationStateMachineName = "fart",
                baseMaxStock = 1,
                baseRechargeInterval = 0.5f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Frozen,
                resetCooldownTimerOnUse = true,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
                keywordTokens = new string[0]
            });

            Modules.Skills.AddSpecialSkills(prefab, devilTriggerSkillDef);

            //Modules.Skills.AddUnlockablesToFamily(skillLocator.special.skillFamily, null, crossUnlockableDef, pistolUnlockableDef);
            #endregion

            //if (MainPlugin.scepterInstalled) InitializeScepterSkills();
        }

        private static void InitializeScepterSkills()
        {
            //AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(holyWaterScepterSkillDef, bodyName, SkillSlot.Special, 0);
            //AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(crossScepterSkillDef, bodyName, SkillSlot.Special, 1);
            //AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(pistolScepterSkillDef, bodyName, SkillSlot.Special, 2);
        }

        private static void CreateSkins(GameObject prefab)
        {
            GameObject model = prefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            ModelSkinController skinController = model.AddComponent<ModelSkinController>();
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            SkinnedMeshRenderer mainRenderer = characterModel.mainSkinnedMeshRenderer;

            CharacterModel.RendererInfo[] defaultRenderers = characterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin

            SkinDef defaultSkin = Modules.Skins.NewSkinDef(model,
                "ROB_MONSTERHUNTER_BODY_DEFAULT_SKIN_NAME",
                Modules.MonsterHunterAssets.mainAssetBundle.LoadAsset<Sprite>("texDefaultSkin"),
                defaultRenderers);

            skins.Add(defaultSkin);
            #endregion
            #region HyperSkin

            SkinDef hyperSkin = Modules.Skins.NewSkinDef(model,
                "ROB_MONSTERHUNTER_BODY_HYPER_SKIN_NAME",
                Modules.MonsterHunterAssets.mainAssetBundle.LoadAsset<Sprite>("texDefaultSkin"),
                defaultRenderers);

            hyperSkin.skinDefParams.meshReplacements = new SkinDefParams.MeshReplacement[]
            {
                new SkinDefParams.MeshReplacement
                {
                    mesh = Modules.MonsterHunterAssets.mainAssetBundle.LoadAsset<Mesh>("meshHyperGuy"),
                    renderer = mainRenderer,
                }
            };

            Material hyperMat = Modules.MonsterHunterAssets.CreateMaterial("matPlaceholder");
            hyperSkin.skinDefParams.rendererInfos[0].defaultMaterial = hyperMat;

            skins.Add(hyperSkin);
            #endregion


            skinController.skins = skins.ToArray();
        }

        private static void InitializeItemDisplays(GameObject prefab)
        {
            CharacterModel characterModel = prefab.GetComponentInChildren<CharacterModel>();

            if (itemDisplayRuleSet == null)
            {
                itemDisplayRuleSet = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
                itemDisplayRuleSet.name = "idrs" + bodyName;
            }

            characterModel.itemDisplayRuleSet = itemDisplayRuleSet;
            characterModel.itemDisplayRuleSet.keyAssetRuleGroups = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().itemDisplayRuleSet.keyAssetRuleGroups;// itemDisplayRuleSet;
            itemDisplayRules = itemDisplayRuleSet.keyAssetRuleGroups.ToList();
        }

        private static void Hook()
        {
            // hud shit
            RoR2.UI.HUD.onHudTargetChangedGlobal += HUDSetup;

            // place stuff
            On.RoR2.SceneDirector.Start += SceneDirector_Start;

            // devil arm drops
            RoR2.GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
        }

        private static void AttemptSpawnSparda(string stageName)
        {
            if (stageName == "mysteryspace")
            {
                //GameObject sparda = GameObject.Instantiate(spardaInteractable, new Vector3(363.5f, -149.8f, 204f), Quaternion.Euler(new Vector3(15f, 0f, 180f)));
                //NetworkServer.Spawn(sparda);
            }
        }

        private static void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            if (damageReport.attackerBody && damageReport.attackerMaster && damageReport.victim)
            {
            }
        }

        private static void BaseMainMenuScreen_Awake(On.RoR2.UI.MainMenu.BaseMainMenuScreen.orig_Awake orig, RoR2.UI.MainMenu.BaseMainMenuScreen self)
        {
            orig(self);

            displayPrefab.GetComponentInChildren<ModelSkinController>().skins = characterPrefab.GetComponentInChildren<ModelSkinController>().skins.ToArray();
            On.RoR2.UI.MainMenu.BaseMainMenuScreen.Awake -= BaseMainMenuScreen_Awake;
        }

        private static void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);

            if (Modules.Helpers.isMonsterHunterInPlay)
            {
                AttemptSpawnSparda(SceneManager.GetActiveScene().name);
            }
        }

        internal static void HUDSetup(RoR2.UI.HUD hud)
        {
            if (!hud) return;
            if (!hud.GetComponent<ChildLocator>()) return;

            Transform crosshairExtras = hud.GetComponent<ChildLocator>().FindChild("CrosshairExtras");

            if (!hud.targetMaster) return;
            if (!hud.targetMaster.bodyPrefab) return;

            if (hud.targetBodyObject)
            {
                if (!hud.targetMaster.hasAuthority) return;

                /*if (MainPlugin.riskUIInstalled)
                {
                    RiskUIHudSetup(hud);
                    return;
                }*/
            }
        }

        internal static void RiskUIHudSetup(RoR2.UI.HUD hud)
        {
            if (hud.targetBodyObject && hud.targetMaster.bodyPrefab == MonsterHunter.characterPrefab && hud.targetMaster.hasAuthority)
            {
            }
        }
    }
}