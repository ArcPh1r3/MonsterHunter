using EntityStates;
using MonsterHunter.SkillStates.MonsterHunter.Greatsword;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MonsterHunter.SkillStates.MonsterHunter.Skill
{
    public class Tackle : BaseMonsterHunterSkillState
    {
        public float charge;

        public float maxSpeed = 7f;
        public float minSpeed = 3f;
        public float minDamage = 1f;
        public float maxDamage = 2.8f;

        private float speed;
        private float duration = 0.35f;
        private Vector3 slipVector;
        private OverlapAttack attack;
        private float previousAirControl;

        public override void OnEnter()
        {
            base.OnEnter();
            this.slipVector = this.GetAimRay().direction;
            this.speed = Util.Remap(this.charge, 0f, 1f, this.minSpeed, this.maxSpeed);

            this.PlaySound("sfx_mh_tackle");
            base.PlayAnimation("FullBody, Override", "Tackle", "Action.playbackRate", this.duration);

            this.previousAirControl = base.characterMotor.airControl;
            base.characterMotor.airControl = EntityStates.Croco.Leap.airControl;
            base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();

            string hitboxString = "Body";

            if (modelTransform)
            {
                hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == hitboxString);
            }

            this.attack = new OverlapAttack();
            this.attack.damageType = DamageTypeCombo.GenericUtility | DamageType.Stun1s;
            this.attack.damageType.damageSource = DamageSource.Utility;
            this.attack.attacker = base.gameObject;
            this.attack.inflictor = base.gameObject;
            this.attack.teamIndex = base.GetTeam();
            this.attack.damage = Util.Remap(this.charge, 0f, 1f, this.minDamage, this.maxDamage) * this.damageStat;
            this.attack.procCoefficient = 1;
            this.attack.hitEffectPrefab = Modules.MonsterHunterAssets.kickImpactEffect;
            this.attack.forceVector = Vector3.up * -800f;
            this.attack.pushAwayForce = 500f;
            this.attack.hitBoxGroup = hitBoxGroup;
            this.attack.isCrit = base.RollCrit();
            this.attack.impactSound = Modules.MonsterHunterAssets.tackleImpactSoundEvent.index;

            this.characterMotor.Motor.ForceUnground();
            this.characterMotor.velocity = this.slipVector * this.speed * this.moveSpeedStat;
            this.monsterHunter.tackleEffect.SetActive(true);

            EffectManager.SpawnEffect(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarGolem/MuzzleflashLunarGolemTwinShot.prefab").WaitForCompletion(), new EffectData
            {
                origin = base.characterBody.corePosition,
                rotation = Util.QuaternionSafeLookRotation(this.slipVector)
            }, false);

            EffectManager.SpawnEffect(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniExplosionVFX.prefab").WaitForCompletion(), new EffectData
            {
                origin = base.characterBody.corePosition,
                rotation = Util.QuaternionSafeLookRotation(this.slipVector)
            }, false);

            EffectManager.SpawnEffect(Modules.MonsterHunterAssets.tackleEffect, new EffectData
            {
                origin = base.characterBody.corePosition,
                rotation = Util.QuaternionSafeLookRotation(this.slipVector)
            }, false);

            // add temporaryoverlay
            if (this.modelLocator && this.modelLocator.modelTransform)
            {
                TemporaryOverlayInstance temporaryOverlay = TemporaryOverlayManager.AddOverlay(this.modelLocator.modelTransform.gameObject);
                temporaryOverlay.duration = 3f;
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = Addressables.LoadAssetAsync<Material>("RoR2/DLC2/Chef/matYesChefOverlay.mat").WaitForCompletion();
                temporaryOverlay.inspectorCharacterModel = this.modelLocator.modelTransform.GetComponent<CharacterModel>();
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 5f, 1f, 0f);
                temporaryOverlay.animateShaderAlpha = true;
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
            base.characterMotor.airControl = this.previousAirControl;
            this.monsterHunter.tackleEffect.SetActive(false);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.monsterHunter.jumpLockoutStopwatch = 0.1f;
            this.characterBody.outOfCombatStopwatch = 0f;
            this.characterBody.isSprinting = true;
            this.characterDirection.forward = this.slipVector;
            this.characterDirection.moveVector = this.slipVector;

            if (base.isAuthority)
            {
                if (this.attack.Fire())
                {
                    this.monsterHunter.boostTimer = 3f;
                    this.characterMotor.Motor.ForceUnground();
                    this.characterMotor.velocity = (Vector3.up * 25f) + (this.slipVector * -5f);
                    this.skillLocator.utility.RechargeFraction(0.5f, true);
                    this.PlayAnimation("FullBody, Override", "TackleImpact");
                    this.outer.SetNextStateToMain();
                    return;
                }
            }

            if (this.fixedAge >= this.duration)
            {
                if (base.isAuthority)
                {
                    if (this.isGrounded)
                    {
                        this.outer.SetNextState(new TackleLanding());
                        return;
                    }
                }
            }
            else
            {
                this.characterMotor.Motor.ForceUnground();
                this.characterMotor.velocity = this.slipVector * this.speed * this.moveSpeedStat;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}