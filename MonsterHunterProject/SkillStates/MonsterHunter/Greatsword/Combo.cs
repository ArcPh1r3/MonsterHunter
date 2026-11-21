using EntityStates;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace MonsterHunter.SkillStates.MonsterHunter.Greatsword
{
    public class Combo : BaseMonsterHunterSkillState, SteppedSkillDef.IStepSetter
    {
        public static float damageCoefficient = 4.44f;
        public float baseDuration = 1.2f;
        public static float attackRecoil = 3f;
        public static float hitHopVelocity = 6f;
        private const float attackStartTime = 0.384f;
        private const float attackEndTime = 0.5f;
        public static float earlyExitTime = 0.67f;
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

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration / this.attackSpeedStat;
            this.earlyExitDuration = this.duration * Combo.earlyExitTime;
            this.hasFired = false;
            this.cancelling = false;
            this.animator = base.GetModelAnimator();
            base.StartAimMode(0.5f + this.duration, false);
            base.characterBody.isSprinting = false;
            base.characterBody.outOfCombatStopwatch = 0f;

            this.characterBody.OnJump += this.CharacterBody_OnJump;

            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();

            if (modelTransform)
            {
                hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Sword");
            }

            //if (this.swingIndex > 1) {
            //    ((SteppedSkillDef.InstanceData)activatorSkillSlot.skillInstanceData).step = 0;
            //    this.swingIndex = 0;
            //   this.inCombo = true;
            //}

            //Util.PlaySound(Modules.Sounds.Cloth1, base.gameObject);

            //Slash1 is first swing, Slash2 is second swing
            string animString = "Combo1";//"Slash" + (1 + swingIndex).ToString();
            //SlashCombo1 is Slash1 but coming off Slash2
            base.PlayCrossfade("FullBody, Override", animString, "Action.playbackRate", this.duration, 0.05f * duration);

            float dmg = Combo.damageCoefficient;

            this.attack = new OverlapAttack();
            this.attack.damageType = DamageTypeCombo.GenericPrimary;
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
        }

        private void CharacterBody_OnJump()
        {
            if (this.inHitPause)
            {
                this.storedVelocity = this.characterMotor.velocity;
                this.storedJump = true;
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (!this.hasFired) this.FireAttack();

            if (this.inHitPause)
            {
                base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
                this.inHitPause = false;
            }

            //base.PlayAnimation("FullBody, Override", "BufferEmpty");
            //base.PlayAnimation("Gesture, Override", "BufferEmpty");
        }

        protected virtual void OnHitAuthority()
        {
            if (!this.hasHopped)
            {
                if (base.characterMotor && !base.characterMotor.isGrounded)
                {
                    //base.SmallHop(base.characterMotor, Combo.hitHopVelocity);
                }

                //if (base.skillLocator.utility.skillDef.skillNameToken == "PALADIN_UTILITY_DASH_NAME") base.skillLocator.utility.RunRecharge(1f);

                this.hasHopped = true;
            }

            if (!this.inHitPause)
            {
                if (base.characterMotor.velocity != Vector3.zero) this.storedVelocity = base.characterMotor.velocity;
                this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, "Action.playbackRate");
                this.hitPauseTimer = 0.25f / this.attackSpeedStat;
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

                Util.PlaySound("PaladinHeavySwing", this.gameObject);
                if (this.characterMotor)
                {
                    this.characterMotor.Motor.ForceUnground();
                    this.characterMotor.velocity = (this.inputBank.moveVector * 10f) + (this.characterDirection.forward * 4f);
                    this.characterMotor.velocity.y = 10f;
                }

                if (base.isAuthority)
                {
                    string muzzleString = null;
                    if (this.swingIndex == 0) muzzleString = "Swing1Muzzle";
                    else muzzleString = "Swing2Muzzle";

                    base.AddRecoil(-1f * Combo.attackRecoil, -2f * Combo.attackRecoil, -0.5f * Combo.attackRecoil, 0.5f * Combo.attackRecoil);

                    Transform muzzleTransform = this.FindModelChild(muzzleString);
                    if (muzzleTransform)
                    {
                        this.swingEffectInstance = UnityEngine.Object.Instantiate<GameObject>(Modules.MonsterHunterAssets.swingEffect, muzzleTransform);
                        ScaleParticleSystemDuration fuck = this.swingEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                        if (fuck) fuck.newDuration = fuck.initialDuration;
                    }
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

            if (!this.hasFired) this.characterMotor.moveDirection *= 0.2f;

            if (this.characterMotor.velocity.y < 0f && !this.hasFired) this.characterMotor.velocity.y *= 0.9f;

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

            float startTime = Combo.attackStartTime;
            if (this.stopwatch >= this.duration * startTime && this.stopwatch <= this.duration * Combo.attackEndTime)
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

            return InterruptPriority.Skill;
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