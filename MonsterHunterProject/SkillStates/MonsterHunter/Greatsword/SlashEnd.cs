using UnityEngine;
using RoR2;
using EntityStates;
using UnityEngine.AddressableAssets;

namespace MonsterHunter.SkillStates.MonsterHunter.Greatsword
{
    public class SlashEnd : BaseMonsterHunterSkillState
    {
        public float baseDuration = 0.75f;

        private float duration;
        private float earlyExitTime = 0.25f;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration / this.attackSpeedStat;
            base.PlayAnimation("FullBody, Override", "DownslashEnd", "Action.playbackRate", this.duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.StartAimMode(2f);
            this.monsterHunter.jumpLockoutStopwatch = 0.1f;
            this.characterMotor.moveDirection *= 0f;
            this.characterBody.outOfCombatStopwatch = 0f;


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