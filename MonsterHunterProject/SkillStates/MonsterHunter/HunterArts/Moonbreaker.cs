using EntityStates;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace MonsterHunter.SkillStates.MonsterHunter.HunterArts
{
    public class Moonbreaker : BaseMonsterHunterSkillState
    {
        public static float damageCoefficient = 3.5f;
        public float baseDuration = 1.5f;
        public static float attackRecoil = 5f;
        public static float hitHopVelocity = 6f;
        private const float attackStartTime = 0.134f;
        private const float attackRefreshTime1 = 0.25f;
        private const float attackRefreshTime2 = 0.35f;
        private const float attackEndTime = 0.417f;
        public static float earlyExitTime = 0.5f;
        public int swingIndex;

        private bool inCombo;
        private float earlyExitDuration;
        private float duration;
        private bool hasFired;
        private float hitPauseTimer;
        protected OverlapAttack attack;
        private bool inHitPause;
        private bool hasHopped;
        private float stopwatch;
        private bool cancelling;
        private Animator animator;
        private BaseState.HitStopCachedState hitStopCachedState;
        private Vector3 storedVelocity;
        private GameObject swingEffectInstance;
        private bool storedJump;
        private bool refresh1;
        private bool refresh2;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration / this.attackSpeedStat;
            this.earlyExitDuration = this.duration * Moonbreaker.earlyExitTime;
            this.hasFired = false;
            this.cancelling = false;
            this.animator = base.GetModelAnimator();
            base.StartAimMode(0.5f + this.duration, false);
            base.characterBody.outOfCombatStopwatch = 0f;
            this.characterDirection.forward = this.characterDirection.moveVector;

            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();

            if (modelTransform)
            {
                hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Sword");
            }

            base.PlayCrossfade("FullBody, Override", "Moonbreaker", "Action.playbackRate", this.duration, 0.1f);

            float dmg = Moonbreaker.damageCoefficient;

            this.attack = new OverlapAttack();
            this.attack.damageType = DamageTypeCombo.GenericSpecial;
            this.attack.attacker = base.gameObject;
            this.attack.inflictor = base.gameObject;
            this.attack.teamIndex = base.GetTeam();
            this.attack.damage = dmg * this.damageStat;
            this.attack.procCoefficient = 1;
            this.attack.hitEffectPrefab = Modules.MonsterHunterAssets.slashImpactEffect;// this.swordController.hitEffect;
            this.attack.forceVector = Vector3.zero;
            this.attack.pushAwayForce = 750f;
            this.attack.hitBoxGroup = hitBoxGroup;
            this.attack.isCrit = base.RollCrit();
            this.attack.impactSound = Modules.MonsterHunterAssets.swordImpactSoundEvent.index;

            if (this.attack.isCrit) this.attack.hitEffectPrefab = Modules.MonsterHunterAssets.critImpactEffect;

            // add temporaryoverlay
            this.PlaySound("sfx_mh_dash");
            this.PlaySound("sfx_mh_foley_03");
        }

        public override void OnExit()
        {
            base.OnExit();

            if (this.inHitPause)
            {
                base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
                this.inHitPause = false;
            }
        }

        protected virtual void OnHitAuthority()
        {
            if (!this.hasHopped)
            {
                this.hasHopped = true;
                this.skillLocator.secondary.RechargeFraction(1f, true);
            }

            if (!this.inHitPause)
            {
                if (base.characterMotor.velocity != Vector3.zero) this.storedVelocity = base.characterMotor.velocity;
                this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, "Action.playbackRate");
                this.hitPauseTimer = 0.1f / this.attackSpeedStat;
                this.inHitPause = true;

                if (this.swingEffectInstance)
                {
                    ScaleParticleSystemDuration fuck = this.swingEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                    if (fuck) fuck.newDuration = 20f;
                }
            }
        }

        public void FireAttack()
        {
            // wah
            if (base.isAuthority)
            {
                Vector3 direction = this.GetAimRay().direction;
                direction.y = Mathf.Max(direction.y, direction.y * 0.5f);
                this.FindModelChild("MeleePivot").rotation = Util.QuaternionSafeLookRotation(direction);
            }

            if (!this.hasFired)
            {
                this.hasFired = true;

                this.PlaySound("sfx_mh_swing");

                if (this.characterMotor)
                {
                    this.characterMotor.Motor.ForceUnground();
                    this.characterMotor.velocity *= 0.75f;
                    this.characterMotor.velocity.y += 6f;
                }

                if (base.isAuthority)
                {
                    base.AddRecoil(-1f * Moonbreaker.attackRecoil, -2f * Moonbreaker.attackRecoil, -0.5f * Moonbreaker.attackRecoil, 0.5f * Moonbreaker.attackRecoil);
                }

                GameObject fx = Modules.MonsterHunterAssets.swingEffect;
                if (this.attack.isCrit) fx = Modules.MonsterHunterAssets.critSwingEffect;
                string muzzleString = "Swing3Muzzle";

                Transform muzzleTransform = this.FindModelChild(muzzleString);
                if (muzzleTransform)
                {
                    this.swingEffectInstance = UnityEngine.Object.Instantiate<GameObject>(fx, muzzleTransform);
                    ScaleParticleSystemDuration fuck = this.swingEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                    if (fuck) fuck.newDuration = fuck.initialDuration;
                }
            }

            if (base.isAuthority)
            {
                if (this.attack.Fire())
                {
                    this.OnHitAuthority();
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (this.animator) this.animator.SetBool("inCombat", true);
            this.hitPauseTimer -= Time.deltaTime;

            if (!this.hasFired)
            {
                this.characterMotor.Motor.ForceUnground();
                this.characterMotor.velocity = this.characterDirection.forward * this.monsterHunter.leapSpeed + (Vector3.up * 6f);
                this.characterMotor.moveDirection *= 0f;
            }

            if (this.characterMotor.velocity.y < 0f && !this.hasFired) this.characterMotor.velocity.y *= 0.95f;

            if (this.hitPauseTimer <= 0f && this.inHitPause)
            {
                base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
                this.inHitPause = false;
                if (this.storedVelocity != Vector3.zero)
                {
                    characterMotor.Motor.ForceUnground();
                    base.characterMotor.velocity = this.storedVelocity;
                }
                if (this.swingEffectInstance)
                {
                    ScaleParticleSystemDuration fuck = this.swingEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                    if (fuck) fuck.newDuration = fuck.initialDuration;
                }
            }

            if (!this.inHitPause)
            {
                this.stopwatch += Time.deltaTime;
            }
            else
            {
                this.stopwatch += 0.05f * Time.deltaTime;
                if (base.characterMotor) base.characterMotor.velocity = Vector3.zero;
                if (this.animator) this.animator.SetFloat("Action.playbackRate", 0.05f);
            }

            if (!this.refresh1 && this.stopwatch >= this.duration * Moonbreaker.attackRefreshTime1)
            {
                this.refresh1 = true;
                this.attack.ResetIgnoredHealthComponents();

                GameObject fx = Modules.MonsterHunterAssets.swingEffect;
                if (this.attack.isCrit) fx = Modules.MonsterHunterAssets.critSwingEffect;
                string muzzleString = "Swing3Muzzle";

                Transform muzzleTransform = this.FindModelChild(muzzleString);
                if (muzzleTransform) UnityEngine.Object.Instantiate<GameObject>(fx, muzzleTransform);

                this.PlaySound("sfx_mh_swing");
            }

            if (!this.refresh2 && this.stopwatch >= this.duration * Moonbreaker.attackRefreshTime2)
            {
                this.refresh2 = true;
                this.attack.ResetIgnoredHealthComponents();

                GameObject fx = Modules.MonsterHunterAssets.swingEffect;
                if (this.attack.isCrit) fx = Modules.MonsterHunterAssets.critSwingEffect;
                string muzzleString = "Swing3Muzzle";

                Transform muzzleTransform = this.FindModelChild(muzzleString);
                if (muzzleTransform) UnityEngine.Object.Instantiate<GameObject>(fx, muzzleTransform);

                this.PlaySound("sfx_mh_swing");
            }

            float startTime = Moonbreaker.attackStartTime;
            if (this.stopwatch >= this.duration * startTime && this.stopwatch <= this.duration * Moonbreaker.attackEndTime)
            {
                this.FireAttack();
            }

            if (base.isAuthority)
            {
                //if (base.fixedAge >= this.earlyExitDuration && base.inputBank.skill1.down)
                //{
                //    var nextSwing = new Slash();
                //    nextSwing.swingIndex = this.swingIndex + 1;
                //    this.outer.SetNextState(nextSwing);
                //    return;
                //} 

                if (this.stopwatch >= this.duration)
                {
                    this.outer.SetNextStateToMain();
                    return;
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (this.cancelling)
                return InterruptPriority.Any;

            if (this.stopwatch >= this.earlyExitDuration)
                return InterruptPriority.Any;

            if (this.refresh2 && this.hasHopped)
                return InterruptPriority.Skill;

            return InterruptPriority.PrioritySkill;
        }

        public void SetStep(int i)
        {
            this.swingIndex = i;

            if (this.swingIndex > 1)
            {
                ((SteppedSkillDef.InstanceData)activatorSkillSlot.skillInstanceData).step = 0;
                this.swingIndex = 0;
                this.inCombo = true;
            }
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write((byte)this.swingIndex);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            this.swingIndex = (int)reader.ReadByte();
        }
    }
}