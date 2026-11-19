using UnityEngine;
using RoR2;
using EntityStates;
using UnityEngine.AddressableAssets;

namespace MonsterHunter.SkillStates.MonsterHunter.Greatsword
{
    public class ChargeSlash : BaseMonsterHunterSkillState
    {
        public float baseDuration = 0.9f;

        private float duration;
        private float lateWindow = 0.75f;
        private float stopwatch;
        private int charge = 0;
        private bool airSlash = false;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration / this.attackSpeedStat;
            base.PlayCrossfade("FullBody, Override", "ChargeDrive", "Action.playbackRate", 3f * this.duration, 0.1f);

            if (!this.isGrounded)
            {
                this.characterMotor.velocity.y = 10f;
                this.airSlash = true;
                this.duration *= 0.5f;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.fixedDeltaTime;

            if (this.characterMotor.velocity.y > 0f && this.airSlash) this.characterMotor.velocity.y *= this.monsterHunter.fallSpeedModifier;

            if (this.skillLocator && this.skillLocator.secondary && this.skillLocator.secondary.stock == 0)
            {
                if (this.stopwatch >= this.duration * this.lateWindow)
                {
                    // force slash with a penalty
                    if (base.isAuthority)
                    {
                        this.outer.SetNextState(new AirSlash
                        {
                            charge = this.charge
                        });
                        return;
                    }
                }
            }

            if (this.stopwatch >= this.duration)
            {
                this.AddCharge();
            }

            if (base.isAuthority)
            {
                if (!this.airSlash)
                {
                    if (this.inputBank && !this.inputBank.skill2.down)
                    {
                        // grounded slash
                        this.outer.SetNextState(new AirSlash
                        {
                            charge = this.charge
                        });
                        return;
                    }
                }
                else
                {
                    if (this.inputBank && !this.inputBank.skill2.down)
                    {
                        // standard aerial slash
                        this.outer.SetNextState(new AirSlash
                        {
                            charge = this.charge
                        });
                        return;
                    }

                    if (this.isGrounded)
                    {
                        // grounded slash, but instant
                        this.outer.SetNextState(new AirSlash
                        {
                            charge = this.charge
                        });
                        return;
                    }
                }
            }
        }

        private void AddCharge()
        {
            this.charge++;
            this.stopwatch = 0f;
            if (this.skillLocator && this.skillLocator.secondary) this.skillLocator.secondary.DeductStock(1);

            //if (this.airSlash)
            //{
                // slight lift if midair
                //if (this.characterMotor.velocity.y <= 2f) this.characterMotor.velocity.y = 2f;
            //}

            // spawn effect
            this.PlaySound("");
            if (!this.airSlash)
            {
                EffectManager.SpawnEffect(Modules.MonsterHunterAssets.groundPunchEffectPrefab, new EffectData
                {
                    origin = this.transform.position,
                    rotation = Quaternion.identity,
                    scale = 1f
                }, false);
            }

            // add temporaryoverlay
            if (this.modelLocator && this.modelLocator.modelTransform)
            {
                TemporaryOverlayInstance temporaryOverlay = TemporaryOverlayManager.AddOverlay(this.modelLocator.modelTransform.gameObject);
                temporaryOverlay.duration = 0.5f;
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/CritOnUse/matFullCrit.mat").WaitForCompletion();
                temporaryOverlay.inspectorCharacterModel = this.modelLocator.modelTransform.GetComponent<CharacterModel>();
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.animateShaderAlpha = true;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}