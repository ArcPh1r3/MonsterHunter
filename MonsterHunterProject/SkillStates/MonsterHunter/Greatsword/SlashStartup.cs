using EntityStates;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MonsterHunter.SkillStates.MonsterHunter.Greatsword
{
    public class SlashStartup : BaseMonsterHunterSkillState
    {
        public static float damageCoefficient = 7f;

        protected override bool normalizeModel => true;

        public float baseDuration = 0.4f;
        public int charge;

        private float duration;
        private OverlapAttack attack;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration;// / this.attackSpeedStat;
            base.PlayCrossfade("FullBody, Override", "Slash", "Action.playbackRate", this.duration, 0.1f);

            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();

            string hitboxString = "Sword";

            if (modelTransform)
            {
                hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == hitboxString);
            }

            //Util.PlaySound(Modules.Sounds.Lunge, base.gameObject);
            //Util.PlaySound(Modules.Sounds.Cloth2, base.gameObject);

            float dmg = SlashStartup.damageCoefficient;

            this.attack = new OverlapAttack();
            this.attack.damageType = DamageType.Generic;
            this.attack.damageType.damageSource = DamageSource.Secondary;
            this.attack.attacker = base.gameObject;
            this.attack.inflictor = base.gameObject;
            this.attack.teamIndex = base.GetTeam();
            this.attack.damage = dmg * this.damageStat * (1 + this.charge) * this.attackSpeedStat;
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
            if (base.isAuthority) this.attack.Fire();
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.StartAimMode(2f);
            this.monsterHunter.jumpLockoutStopwatch = 0.1f;
            this.characterMotor.moveDirection *= 0f;
            this.characterBody.outOfCombatStopwatch = 0f;
            this.characterMotor.velocity = this.characterDirection.forward * this.monsterHunter.slashMovementModifier;

            this.monsterHunter.SetZoom(0.5f + (this.charge * 0.75f), 1f);

            if (base.fixedAge >= (0.95f * this.duration))
            {
                if (base.isAuthority) this.attack.Fire();
            }

            if (base.isAuthority && base.fixedAge >= this.duration)
            {
                if (this.charge >= 3)
                {
                    this.PlaySound("sfx_mh_impact_ground_strong");

                    EffectManager.SpawnEffect(Modules.MonsterHunterAssets.groundSlamImpact, new EffectData
                    {
                        origin = this.characterBody.footPosition + (this.characterDirection.forward * 3f),
                        scale = 1f
                    }, false);
                }
                this.outer.SetNextState(new SlashEnd());
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}