using UnityEngine;
using RoR2;
using EntityStates;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace MonsterHunter.SkillStates.MonsterHunter.Greatsword
{
    public class Quickstep : BaseState
    {
        public int charge;

        protected Vector3 slipVector = Vector3.zero;
        public float duration = 0.25f;
        public float speedCoefficient = 5.5f;
        private Vector3 cachedForward;

        public override void OnEnter()
        {
            base.OnEnter();
            this.slipVector = ((base.inputBank.moveVector == Vector3.zero) ? base.characterDirection.forward : base.inputBank.moveVector).normalized;
            this.cachedForward = this.characterDirection.forward;

            Animator anim = this.GetModelAnimator();

            Vector3 rhs = base.characterDirection ? base.characterDirection.forward : this.slipVector;
            Vector3 rhs2 = Vector3.Cross(Vector3.up, rhs);
            float num = Vector3.Dot(this.slipVector, rhs);
            float num2 = Vector3.Dot(this.slipVector, rhs2);
            anim.SetFloat("dashF", num);
            anim.SetFloat("dashR", num2);

            base.PlayAnimation("FullBody, Override", "Dash", "Action.playbackRate", this.duration);
            //base.PlayAnimation("Gesture, Override", "BufferEmpty");

            Util.PlaySound("PaladinDash", base.gameObject);

            this.ApplyBuff();
            this.CreateDashEffect();
        }

        public virtual void ApplyBuff()
        {
            if (NetworkServer.active) this.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
        }

        public virtual void CreateDashEffect()
        {
            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BoostJumpEffect"), new EffectData
            {
                origin = base.characterBody.corePosition,
                rotation = Util.QuaternionSafeLookRotation(this.slipVector)
            }, false);
            /*EffectData effectData = new EffectData();
            effectData.rotation = Util.QuaternionSafeLookRotation(this.slipVector);
            effectData.origin = base.characterBody.corePosition;

            EffectManager.SpawnEffect(Modules.Asset.dashFX, effectData, false);

            TemporaryOverlayInstance overlay = TemporaryOverlayManager.AddOverlay(this.modelLocator.modelTransform.gameObject);
            overlay.duration = 1.5f;
            overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 2f, 1f, 0f);
            overlay.animateShaderAlpha = true;
            overlay.destroyComponentOnEnd = true;
            overlay.originalMaterial = Modules.Asset.dashMat;
            overlay.AddToCharacterModel(this.GetModelTransform().GetComponent<CharacterModel>());*/
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.characterMotor.velocity = Vector3.zero;
            base.characterMotor.rootMotion += this.slipVector * (this.moveSpeedStat * this.speedCoefficient * Time.deltaTime) * Mathf.Cos(base.fixedAge / (this.duration * 1.3f) * 1.57079637f);

            if (base.isAuthority)
            {
                if (base.characterDirection)
                {
                    base.characterDirection.forward = this.cachedForward;
                }
            }

            if (base.isAuthority && base.fixedAge >= this.duration)
            {
                if (this.inputBank.skill2.down)
                {
                    this.outer.SetNextState(new ChargeSlash
                    {
                        charge = this.charge
                    });
                }
                else this.outer.SetNextStateToMain();
            }
        }

        public virtual void DampenVelocity()
        {
            base.characterMotor.velocity *= 0.4f;
        }

        public override void OnExit()
        {
            this.DampenVelocity();
            if (NetworkServer.active) this.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);

            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}