using UnityEngine;
using RoR2;
using EntityStates;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace MonsterHunter.SkillStates.MonsterHunter.Skill
{
    public class ChargeTackleMobile : BaseMonsterHunterSkillState
    {
        protected override bool normalizeModel => true;

        public float baseDuration = 2f;

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

            base.PlayAnimation("Gesture, Override", "ChargeTackle", "Action.playbackRate", this.duration + 0.5f);

            this.chargeEffectInstance = GameObject.Instantiate(Modules.MonsterHunterAssets.swordChargeEffect, this.FindModelChild("Chest"));
            this.chargeEffectInstance.transform.localPosition = new Vector3(0f, 0f, -0.3f);
            this.chargeEffectInstance.transform.localRotation = Quaternion.identity;

            this.monsterHunter.SetZoom(3f, this.duration);
        }

        public override void OnExit()
        {
            base.OnExit();
            base.PlayAnimation("Gesture, Override", "BufferEmpty");
            this.characterMotor.mass *= 0.01f;
            if (NetworkServer.active) this.characterBody.RemoveBuff(RoR2Content.Buffs.ArmorBoost);
            if (this.chargeEffectInstance) Destroy(this.chargeEffectInstance);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.fixedDeltaTime;
            this.characterBody.outOfCombatStopwatch = 0f;
            this.characterBody.isSprinting = false;

            if (!this.charged) this.characterMotor.moveDirection *= 0.75f;
            else this.characterMotor.moveDirection *= 0.5f;

            if (this.stopwatch >= this.duration && !this.charged)
            {
                this.charged = true;

                this.PlaySound("sfx_mh_tackle_charge_max");
            }

            if (this.charged)
            {
                this.monsterHunter.SetZoom(-1f, 0.1f);
            }

            if (base.isAuthority)
            {
                if (this.inputBank && !this.inputBank.skill3.down && this.stopwatch >= 0.5f)
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