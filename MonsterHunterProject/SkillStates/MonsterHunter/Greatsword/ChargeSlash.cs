using UnityEngine;
using RoR2;
using EntityStates;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace MonsterHunter.SkillStates.MonsterHunter.Greatsword
{
    public class ChargeSlash : BaseMonsterHunterSkillState
    {
        protected override bool normalizeModel => true;
        protected override bool turningAllowed
            { get { return this.airSlash; } }

        public float baseDuration = 1f;

        private float duration;
        private float lateWindow = 0.9f;
        private float stopwatch;
        public int charge = 0;
        private bool airSlash = false;
        private CharacterCameraParams cameraParams;
        private bool isBoosted = false;
        private float cap = 3f;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration;// / this.attackSpeedStat;
            this.characterMotor.mass *= 100f;

            this.isBoosted = this.monsterHunter.boostTimer > 0f;
            this.monsterHunter.boostTimer = 0f;

            this.PlaySound("sfx_mh_charge_start");

            if (NetworkServer.active) this.characterBody.AddBuff(RoR2Content.Buffs.ArmorBoost);

            if (!this.isGrounded)
            {
                this.PlaySound("sfx_mh_foley_02");
                if (this.isBoosted) base.PlayAnimation("FullBody, Override", "ChargeAirBoosted", "Action.playbackRate", this.duration);
                else base.PlayAnimation("FullBody, Override", "ChargeAir", "Action.playbackRate", this.duration);
                this.characterMotor.velocity *= 0.25f;
                this.characterMotor.velocity.y = 3f;
                this.airSlash = true;
                this.duration = 0.36f;
            }
            else
            {
                this.PlaySound("sfx_mh_foley_01");
                base.PlayCrossfade("FullBody, Override", "ChargeGroundStart", "Action.playbackRate", this.duration * 0.6f, 0.1f);
            }

            //this.cameraParams = this.GetComponent<CameraTargetParams>().cameraParams;
            //if (this.cameraParams && base.isAuthority) this.z = this.cameraParams.data.idealLocalCameraPos.value.z;
        }

        public override void OnExit()
        {
            base.OnExit();
            //if (this.cameraParams && base.isAuthority) this.cameraParams.data.idealLocalCameraPos.value.z = this.z;
            this.characterMotor.mass *= 0.01f;
            if (NetworkServer.active) this.characterBody.RemoveBuff(RoR2Content.Buffs.ArmorBoost);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.fixedDeltaTime;
            this.monsterHunter.jumpLockoutStopwatch = 0.1f;
            this.characterBody.outOfCombatStopwatch = 0f;
            this.characterBody.isSprinting = false;
            if (this.airSlash) this.StartAimMode(0.1f);

            this.monsterHunter.SetZoom(0.25f + (this.charge * -2f), 1f);

            if (this.characterMotor.velocity.y >= this.cap) this.characterMotor.velocity.y = this.cap;

            if (this.isGrounded) this.characterMotor.moveDirection *= 0f;
            else this.characterMotor.moveDirection *= 0.75f;

            if (!this.airSlash && base.isAuthority)
            {
                if (this.inputBank.jump.justPressed && base.fixedAge >= 0.5f)
                {
                    this.outer.SetNextState(new Quickstep
                    {
                        charge = this.charge
                    });
                    return;
                }
            }

            if (this.characterMotor.velocity.y > 0f && this.airSlash) this.characterMotor.velocity.y *= this.monsterHunter.fallSpeedModifier;

            if (this.skillLocator && this.skillLocator.secondary && this.skillLocator.secondary.stock == 0)
            {
                if (this.stopwatch >= this.duration * this.lateWindow)
                {
                    // force slash with a penalty
                    if (base.isAuthority)
                    {
                        if (this.airSlash)
                        {
                            // no penalty for aerial slash (:
                            this.outer.SetNextState(new AirSlash
                            {
                                charge = this.charge
                            });
                            return;
                        }
                        else
                        {
                            int newCharge = Mathf.RoundToInt(Mathf.Clamp(this.charge - 1, 0, Mathf.Infinity));

                            if (this.inputBank.moveVector.magnitude > 0.1f)
                            {
                                this.outer.SetNextState(new LeapSlash
                                {
                                    charge = newCharge
                                });
                            }
                            else
                            {
                                this.outer.SetNextState(new SlashStartup
                                {
                                    charge = newCharge
                                });
                            }
                            return;
                        }
                    }
                }
            }

            float chargeTimer = this.duration;
            if (this.charge == 0 && !this.airSlash) chargeTimer *= 0.6f;
            if (this.stopwatch >= chargeTimer)
            {
                this.AddCharge();
            }

            //if (this.cameraParams && base.isAuthority)
            //{
                //float desiredZ = this.z * Util.Remap(this.charge, 0f, 1f, 1f, 1.75f);
                //this.cameraParams.data.idealLocalCameraPos.value.z = desiredZ;
            //}

            if (base.isAuthority)
            {
                if (!this.airSlash)
                {
                    if (this.inputBank && !this.inputBank.skill2.down && base.fixedAge >= 0.4f)
                    {
                        // grounded slash
                        if (this.inputBank.moveVector.magnitude > 0.1f)
                        {
                            this.outer.SetNextState(new LeapSlash
                            {
                                charge = this.charge
                            });
                        }
                        else
                        {
                            this.outer.SetNextState(new SlashStartup
                            {
                                charge = this.charge
                            });
                        }
                        return;
                    }
                }
                else
                {
                    if (this.inputBank && !this.inputBank.skill2.down && base.fixedAge >= 0.25f)
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

            // woo!
            this.duration *= 0.95f;

            // animate
            if (!this.airSlash)
            {
                base.PlayAnimation("FullBody, Override", "ChargeGround", "Action.playbackRate", this.duration);
            }

            //if (this.airSlash)
            //{
            //      slight lift if midair
            //      if (this.characterMotor.velocity.y <= 2f) this.characterMotor.velocity.y = 2f;
            //}

            float pitch = 1f + (this.charge * 0.5f);
            // spawn effect
            if (!this.airSlash)
            {
                this.PlaySound("sfx_mh_charge", pitch);
                EffectManager.SpawnEffect(Modules.MonsterHunterAssets.groundPunchEffectPrefab, new EffectData
                {
                    origin = this.transform.position + (Vector3.up * -1.1f),
                    rotation = Quaternion.identity,
                    scale = 1f
                }, false);
            }
            else this.PlaySound("sfx_mh_charge_air", pitch);

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