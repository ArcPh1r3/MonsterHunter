using UnityEngine;
using RoR2;
using EntityStates;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using System.Runtime.CompilerServices;

namespace MonsterHunter.SkillStates.MonsterHunter.Skill
{
    public class ChargeTackle : BaseMonsterHunterSkillState
    {
        protected override bool normalizeModel => true;

        public float baseDuration = 3f;

        private float duration;
        private float stopwatch;
        private bool charged;
        private GameObject chargeEffectInstance;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration;// / this.attackSpeedStat;
            this.characterMotor.mass *= 100f;

            this.PlaySound("sfx_mh_tackle_charge");
            this.PlaySound("sfx_mh_tackle_charge_foley");

            if (NetworkServer.active) this.characterBody.AddBuff(RoR2Content.Buffs.ArmorBoost);

            base.PlayAnimation("FullBody, Override", "ChargeTackle", "Action.playbackRate", this.duration + 0.5f);

            this.chargeEffectInstance = GameObject.Instantiate(Modules.MonsterHunterAssets.swordChargeEffect, this.FindModelChild("Chest"));
            this.chargeEffectInstance.transform.localPosition = Vector3.zero;
            this.chargeEffectInstance.transform.localRotation = Quaternion.identity;

            this.monsterHunter.SetZoom(3f, this.duration);
        }

        public override void OnExit()
        {
            base.OnExit();
            this.characterMotor.mass *= 0.01f;
            if (NetworkServer.active) this.characterBody.RemoveBuff(RoR2Content.Buffs.ArmorBoost);
            if (this.chargeEffectInstance) Destroy(this.chargeEffectInstance);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.fixedDeltaTime;
            this.monsterHunter.jumpLockoutStopwatch = 0.1f;
            this.characterBody.outOfCombatStopwatch = 0f;
            this.characterBody.isSprinting = false;
            this.StartAimMode(0.1f);
            this.characterMotor.moveDirection *= 0f;
            this.characterMotor.velocity *= 0f;

            if (this.stopwatch >= this.duration && !this.charged)
            {
                this.charged = true;

                this.PlaySound("sfx_mh_tackle_charge_max");

                // add temporaryoverlay
                if (this.modelLocator && this.modelLocator.modelTransform)
                {
                    TemporaryOverlayInstance temporaryOverlay = TemporaryOverlayManager.AddOverlay(this.modelLocator.modelTransform.gameObject);
                    temporaryOverlay.duration = 0.5f;
                    temporaryOverlay.destroyComponentOnEnd = true;
                    temporaryOverlay.originalMaterial = Addressables.LoadAssetAsync<Material>("RoR2/DLC2/Chef/matYesChefOverlay.mat").WaitForCompletion();
                    temporaryOverlay.inspectorCharacterModel = this.modelLocator.modelTransform.GetComponent<CharacterModel>();
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlay.animateShaderAlpha = true;
                }
            }

            if (this.charged)
            {
                this.monsterHunter.SetZoom(-1f, 0.1f);
            }

            if (base.isAuthority)
            {
                if (this.inputBank && !this.inputBank.skill3.down && this.stopwatch >= 0.25f)
                {
                    this.outer.SetNextState(new Tackle
                    {
                        charge = Mathf.Clamp01(Util.Remap(this.stopwatch, 0f, this.duration, 0f, 1f))
                    });
                    return;
                }

                if (this.stopwatch >= this.duration + 0.5f)
                {
                    this.outer.SetNextState(new Tackle
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