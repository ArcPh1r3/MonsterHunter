using EntityStates;
using RoR2;
using RoR2.Projectile;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace MonsterHunter.SkillStates.MonsterHunter.Greatsword
{
    public class LeapSlash : BaseMonsterHunterSkillState
    {
        public static float damageCoefficient = 8f;
        public static float leapDuration = 0.6f;
        public static float dropVelocity = 18f;

        public int charge;
        private float duration;
        private bool hasFired;
        private bool hasLanded;
        private float hitPauseTimer;
        private OverlapAttack attack;
        private bool inHitPause;
        private float stopwatch;
        private Animator animator;
        private BaseState.HitStopCachedState hitStopCachedState;
        private float previousAirControl;
        private bool hasHit;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = LeapSlash.leapDuration;// / (0.75f + (0.25f * this.attackSpeedStat));
            this.hasFired = false;
            this.hasLanded = false;
            this.animator = base.GetModelAnimator();

            this.previousAirControl = base.characterMotor.airControl;
            base.characterMotor.airControl = EntityStates.Croco.Leap.airControl;

            Vector3 direction = base.GetAimRay().direction;

            if (base.isAuthority)
            {
                base.characterBody.isSprinting = true;

                direction.y = Mathf.Max(direction.y, 1.5f * EntityStates.Croco.Leap.minimumY);
                Vector3 a = direction.normalized * (0.75f * EntityStates.Croco.Leap.aimVelocity) * this.moveSpeedStat;
                Vector3 b = Vector3.up * 0.95f * EntityStates.Croco.Leap.upwardVelocity;
                Vector3 b2 = new Vector3(direction.x, 0f, direction.z).normalized * (0.95f * EntityStates.Croco.Leap.forwardVelocity);

                base.characterMotor.Motor.ForceUnground();
                base.characterMotor.velocity = a + b + b2;

                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BoostJumpEffect"), new EffectData
                {
                    origin = base.characterBody.footPosition,
                    rotation = Util.QuaternionSafeLookRotation(base.characterMotor.velocity)
                }, true);
            }

            base.characterDirection.moveVector = direction;

            base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();

            string hitboxString = "Sword";

            if (modelTransform)
            {
                hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == hitboxString);
            }

            base.PlayAnimation("FullBody, Override", "ChargeAir", "Action.playbackRate", this.duration);
            //Util.PlaySound(Modules.Sounds.Lunge, base.gameObject);
            //Util.PlaySound(Modules.Sounds.Cloth2, base.gameObject);

            float dmg = LeapSlash.damageCoefficient;

            this.attack = new OverlapAttack();
            this.attack.damageType = DamageType.Generic;
            this.attack.damageType.damageSource = DamageSource.Secondary;
            this.attack.attacker = base.gameObject;
            this.attack.inflictor = base.gameObject;
            this.attack.teamIndex = base.GetTeam();
            this.attack.damage = dmg * this.damageStat * (1 + this.charge);
            this.attack.procCoefficient = 1;
            this.attack.hitEffectPrefab = Modules.MonsterHunterAssets.slashImpactEffect;
            this.attack.forceVector = -Vector3.up * 6000f;
            this.attack.pushAwayForce = 500f;
            this.attack.hitBoxGroup = hitBoxGroup;
            this.attack.isCrit = base.RollCrit();
            this.attack.impactSound = Modules.MonsterHunterAssets.swordImpactSoundEvent.index;

            if (this.charge >= 1)
            {
                this.attack.hitEffectPrefab = Modules.MonsterHunterAssets.finisherImpactEffect;
                this.attack.damageType = DamageTypeCombo.GenericSecondary | DamageType.Stun1s;
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
            base.characterMotor.airControl = this.previousAirControl;
        }

        public void FireAttack()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;
                Util.PlaySound("PaladinHeavySwing", this.gameObject);
                base.PlayAnimation("FullBody, Override", "AirSlash", "Action.playbackRate", 0.25f);

                if (base.isAuthority)
                {
                    base.AddRecoil(-1f * 3f, -2f * 3f, -0.5f * 3f, 0.5f * 3f);
                    //EffectManager.SimpleMuzzleFlash(this.swordController.swingEffect, base.gameObject, "SwingDown", true);
                    //UnityEngine.Object.Instantiate(this.swordController.swingEffect, FindModelChild("SwingDown"));

                    base.characterMotor.velocity *= 0.25f;
                    base.characterMotor.velocity += Vector3.up * -LeapSlash.dropVelocity;
                }
            }

            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();

                if (this.attack.Fire())
                {
                    if (!this.inHitPause)
                    {
                        this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, "Action.playbackRate");
                        this.hitPauseTimer = 0.4f / this.attackSpeedStat;
                        this.inHitPause = true;
                    }

                    if (!this.hasHit)
                    {
                        this.hasHit = true;
                        //if (base.skillLocator.utility.skillDef.skillNameToken == "PALADIN_UTILITY_DASH_NAME") base.skillLocator.utility.RunRecharge(1f);
                    }
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.StartAimMode(0.5f, false);
            this.stopwatch += Time.deltaTime;
            this.monsterHunter.jumpLockoutStopwatch = 0.1f;
            this.characterBody.outOfCombatStopwatch = 0f;

            if (this.stopwatch >= this.duration)
            {
                this.FireAttack();

                /*if (base.isAuthority && base.inputBank.skill2.down)
                {
                    if (base.skillLocator.secondary.stock > 0)
                    {
                        EntityState nextState = new AirSlamAlt();
                        this.outer.SetNextState(nextState);
                        return;
                    }
                }*/
            }

            if (this.stopwatch >= this.duration && base.isAuthority && base.characterMotor.isGrounded)
            {
                this.GroundImpact();
                this.outer.SetNextState(new SlashEnd());
                return;
            }
        }

        private void FireShockwave()
        {
            /*Vector3 shockwavePosition = base.characterBody.footPosition;
            Vector3 forward = base.characterDirection.forward;

            ProjectileManager.instance.FireProjectile(
                Modules.Projectiles.shockwave,
                shockwavePosition,
                Util.QuaternionSafeLookRotation(forward),
                base.gameObject,
                base.characterBody.damage * StaticValues.beamDamageCoefficient,
                EntityStates.BrotherMonster.WeaponSlam.waveProjectileForce,
                base.RollCrit(),
                DamageColorIndex.Default,
                null,
                -1f,
                DamageTypeCombo.GenericSecondary);*/
        }

        private void GroundImpact()
        {
            if (!this.hasLanded)
            {
                this.hasLanded = true;

                if (this.charge >= 3) this.PlaySound("sfx_mh_impact_ground_strong");

                this.FireShockwave();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}