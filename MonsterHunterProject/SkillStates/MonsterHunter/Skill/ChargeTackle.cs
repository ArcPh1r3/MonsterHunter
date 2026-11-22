using UnityEngine;
using RoR2;
using EntityStates;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace MonsterHunter.SkillStates.MonsterHunter.Skill
{
    public class ChargeTackle : BaseMonsterHunterSkillState
    {
        protected override bool normalizeModel => true;

        public float baseDuration = 1f;

        private float duration;
        private float stopwatch;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration;// / this.attackSpeedStat;
            this.characterMotor.mass *= 100f;

            this.PlaySound("sfx_mh_charge_start");

            if (NetworkServer.active) this.characterBody.AddBuff(RoR2Content.Buffs.ArmorBoost);

            this.PlaySound("sfx_mh_foley_02");
            base.PlayAnimation("FullBody, Override", "ChargeAir", "Action.playbackRate", this.duration);
        }

        public override void OnExit()
        {
            base.OnExit();
            this.characterMotor.mass *= 0.01f;
            if (NetworkServer.active) this.characterBody.RemoveBuff(RoR2Content.Buffs.ArmorBoost);
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

            if (base.isAuthority)
            {
                if (this.inputBank && !this.inputBank.skill3.down && base.fixedAge >= 0.25f)
                {
                    this.outer.SetNextStateToMain();
                    return;
                }

                if (this.fixedAge >= this.duration)
                {
                    this.outer.SetNextStateToMain();
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