using UnityEngine;
using RoR2;
using EntityStates;
using UnityEngine.AddressableAssets;

namespace MonsterHunter.SkillStates.MonsterHunter.Greatsword
{
    public class SlashEnd : BaseMonsterHunterSkillState
    {
        public float baseDuration = 2f;

        private float duration;
        private float earlyExitTime = 0.35f;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration / this.attackSpeedStat;
            base.PlayAnimation("FullBody, Override", "SlashEnd", "Action.playbackRate", this.duration);

            EffectManager.SpawnEffect(Modules.MonsterHunterAssets.groundPunchHeavyEffectPrefab, new EffectData
            {
                origin = this.characterBody.footPosition + (this.characterDirection.forward * 3f),
                scale = 1f
            }, true);

            Util.PlaySound("PaladinSwordGroundImpact", this.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.StartAimMode(2f);
            this.monsterHunter.jumpLockoutStopwatch = 0.1f;
            this.characterMotor.moveDirection *= 0f;
            this.characterBody.outOfCombatStopwatch = 0f;
            
            if (base.isAuthority)
            {
                if (base.fixedAge >= 0.5f * this.duration && this.inputBank.moveVector.magnitude > 0.1f)
                {
                    base.PlayCrossfade("FullBody, Override", "BufferEmpty", 0.2f);
                    this.outer.SetNextStateToMain();
                    return;
                }
            }

            if (base.isAuthority && base.fixedAge >= this.duration)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (base.fixedAge >= (this.duration * this.earlyExitTime)) return InterruptPriority.Any;
            return InterruptPriority.PrioritySkill;
        }
    }
}