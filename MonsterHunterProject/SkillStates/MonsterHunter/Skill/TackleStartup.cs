using EntityStates;
using MonsterHunter.SkillStates.MonsterHunter.Skill;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MonsterHunter.SkillStates.MonsterHunter.Skill
{
    public class TackleStartup : BaseMonsterHunterSkillState
    {
        public float charge;

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = Util.Remap(this.charge, 0f, 1f, 0.15f, 0.4f);
            base.PlayAnimation("FullBody, Override", "ChargeTackle", "Action.playbackRate", this.duration, 0.1f);
            this.characterMotor.velocity *= 0f;

            // add temporaryoverlay
            if (this.modelLocator && this.modelLocator.modelTransform)
            {
                TemporaryOverlayInstance temporaryOverlay = TemporaryOverlayManager.AddOverlay(this.modelLocator.modelTransform.gameObject);
                temporaryOverlay.duration = this.duration;
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = Addressables.LoadAssetAsync<Material>("RoR2/DLC2/Chef/matYesChefOverlay.mat").WaitForCompletion();
                temporaryOverlay.inspectorCharacterModel = this.modelLocator.modelTransform.GetComponent<CharacterModel>();
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 30f, 1f, 0f);
                temporaryOverlay.animateShaderAlpha = true;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.monsterHunter.jumpLockoutStopwatch = 0.1f;
            this.characterMotor.moveDirection *= 0f;
            this.characterBody.outOfCombatStopwatch = 0f;
            this.characterMotor.velocity *= 0f;

            if (base.isAuthority && base.fixedAge >= this.duration)
            {
                this.outer.SetNextState(new Tackle
                {
                    charge = this.charge
                });
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}