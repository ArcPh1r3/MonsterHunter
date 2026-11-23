using UnityEngine;
using RoR2;
using EntityStates;
using UnityEngine.AddressableAssets;

namespace MonsterHunter.SkillStates.MonsterHunter.Greatsword
{
    public class TackleLanding : BaseMonsterHunterSkillState
    {
        protected override bool normalizeModel => true;
        protected override bool turningAllowed => false;

        public float baseDuration = 2f;

        private float duration;
        private float earlyExitTime = 0.35f;
        private Animator modelAnimator;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration / this.attackSpeedStat;
            this.characterBody.aimTimer = -1f;
            this.characterMotor.velocity *= 0f;
            this.modelAnimator = this.GetModelAnimator();
            base.PlayAnimation("FullBody, Override", "TackleLanding", "Action.playbackRate", this.duration);

            EffectManager.SpawnEffect(Modules.MonsterHunterAssets.groundPunchHeavyEffectPrefab, new EffectData
            {
                origin = this.characterBody.footPosition + (this.characterDirection.forward * 0.5f),
                scale = 1f
            }, false);

            Util.PlaySound("sfx_mh_impact_ground", this.gameObject);
        }

        public override void OnExit()
        {
            base.OnExit();

            if (this.modelAnimator)
            {
                this.modelAnimator.SetFloat(AnimationParameters.aimWeight, 1f);
                this.modelAnimator.SetLayerWeight(this.modelAnimator.GetLayerIndex("AimPitch"), 1f);
                this.modelAnimator.SetLayerWeight(this.modelAnimator.GetLayerIndex("AimYaw"), 1f);
            }

            if (this.inputBank.moveVector.magnitude > 0.1f) this.characterBody.isSprinting = true;
        }

        public override void Update()
        {
            base.Update();

            if (this.modelAnimator)
            {
                float value = Mathf.Clamp01(Util.Remap(this.age, 0.5f, this.duration, 0f, 1f));
                this.modelAnimator.SetFloat(AnimationParameters.aimWeight, value);
                this.modelAnimator.SetLayerWeight(this.modelAnimator.GetLayerIndex("AimPitch"), value);
                this.modelAnimator.SetLayerWeight(this.modelAnimator.GetLayerIndex("AimYaw"), value);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
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