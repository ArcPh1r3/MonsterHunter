using UnityEngine;
using RoR2;
using EntityStates;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using MonsterHunter.SkillStates.MonsterHunter.Greatsword;

namespace MonsterHunter.SkillStates.MonsterHunter.Skill
{
    public class ChargeTackleMobile : BaseMonsterHunterSkillState
    {
        public float baseDuration = 3f;

        private float duration;
        private float stopwatch;
        private bool charged;
        private GameObject chargeEffectInstance;
        private uint playID;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration / this.attackSpeedStat;
            this.characterMotor.mass *= 100f;

            this.PlaySound("sfx_mh_tackle_charge");
            this.PlaySound("sfx_mh_tackle_charge_foley");
            this.playID = this.PlaySound("sfx_mh_tackle_loop");

            if (NetworkServer.active) this.characterBody.AddBuff(RoR2Content.Buffs.ArmorBoost);

            base.PlayCrossfade("FullBody, Override", "BufferEmpty", 0.1f);
            base.PlayAnimation("Gesture, Override", "ChargeTackle", "Action.playbackRate", this.duration + 0.5f);

            this.chargeEffectInstance = GameObject.Instantiate(Modules.MonsterHunterAssets.swordChargeEffect, this.FindModelChild("Chest"));
            this.chargeEffectInstance.transform.localPosition = new Vector3(0f, 0f, -0.3f);
            this.chargeEffectInstance.transform.localRotation = Quaternion.identity;

            this.monsterHunter.SetZoom(1.5f, this.duration);
            // add temporaryoverlay
            /*if (this.modelLocator && this.modelLocator.modelTransform)
            {
                TemporaryOverlayInstance temporaryOverlay = TemporaryOverlayManager.AddOverlay(this.modelLocator.modelTransform.gameObject);
                temporaryOverlay.duration = this.duration;
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = Addressables.LoadAssetAsync<Material>("RoR2/DLC2/Chef/matYesChefOverlay.mat").WaitForCompletion();
                temporaryOverlay.inspectorCharacterModel = this.modelLocator.modelTransform.GetComponent<CharacterModel>();
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                temporaryOverlay.animateShaderAlpha = true;
            }*/
        }

        public override void OnExit()
        {
            base.OnExit();
            base.PlayAnimation("Gesture, Override", "BufferEmpty");
            this.characterMotor.mass *= 0.01f;
            if (NetworkServer.active) this.characterBody.RemoveBuff(RoR2Content.Buffs.ArmorBoost);
            if (this.chargeEffectInstance) Destroy(this.chargeEffectInstance);
            AkSoundEngine.StopPlayingID(this.playID);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.fixedDeltaTime;
            this.characterBody.outOfCombatStopwatch = 0f;
            this.characterBody.isSprinting = false;
            this.StartAimMode(0.1f);

            if (!this.charged) this.characterMotor.moveDirection *= 0.75f;
            else this.characterMotor.moveDirection *= 0.5f;

            if (this.stopwatch >= this.duration && !this.charged)
            {
                this.charged = true;

                this.PlaySound("sfx_mh_charge_max");

                // add temporaryoverlay
                if (this.modelLocator && this.modelLocator.modelTransform)
                {
                    TemporaryOverlayInstance temporaryOverlay = TemporaryOverlayManager.AddOverlay(this.modelLocator.modelTransform.gameObject);
                    temporaryOverlay.duration = 1f;
                    temporaryOverlay.destroyComponentOnEnd = true;
                    temporaryOverlay.originalMaterial = Addressables.LoadAssetAsync<Material>("RoR2/DLC2/Chef/matYesChefOverlay.mat").WaitForCompletion();
                    temporaryOverlay.inspectorCharacterModel = this.modelLocator.modelTransform.GetComponent<CharacterModel>();
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 5f, 1f, 0f);
                    temporaryOverlay.animateShaderAlpha = true;
                }

                if (this.chargeEffectInstance)
                {
                    this.chargeEffectInstance.transform.Find("Sparks").localScale = Vector3.one * 0.1f;
                    this.chargeEffectInstance.transform.Find("Sparks/Backdrop").localScale = Vector3.one * 0.75f;
                    this.chargeEffectInstance.transform.Find("Sparks/Flare (3)").localScale = Vector3.one * 1.25f;

                    EffectManager.SpawnEffect(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarGolem/MuzzleflashLunarGolemTwinShot.prefab").WaitForCompletion(), new EffectData
                    {
                        origin = this.chargeEffectInstance.transform.position,
                        rotation = this.chargeEffectInstance.transform.rotation
                    }, false);
                }
            }

            if (this.charged)
            {
                this.monsterHunter.SetZoom(-3f, 0.1f);
            }

            if (base.isAuthority)
            {
                if (this.inputBank && !this.inputBank.skill3.down && this.stopwatch >= 0.1)
                {
                    this.outer.SetNextState(new TackleStartup
                    {
                        charge = Mathf.Clamp01(Util.Remap(this.stopwatch, 0f, this.duration, 0f, 1f))
                    });
                    return;
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}