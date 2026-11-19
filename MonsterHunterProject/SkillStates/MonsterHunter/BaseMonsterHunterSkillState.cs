using EntityStates;
using MonsterHunter.Content.Components;
using RoR2;
using UnityEngine;

namespace MonsterHunter.SkillStates.MonsterHunter
{
    public class BaseMonsterHunterSkillState : BaseSkillState
    {
        protected virtual bool noCollision => false;
        protected virtual bool isWeapon => true;
        protected MonsterHunterController monsterHunter;
        private Animator _animator;

        protected virtual void CancelAttack() { }

        protected void PlaySound(string soundString, bool useOverrides = true)
        {
            /*if (useOverrides && this.dante && this.dante.weaponDef)
            {
                if (this.dante.weaponDef.soundStringOverrides != null && this.dante.weaponDef.soundStringOverrides.Length > 0)
                {
                    for (int i = 0; i < this.dante.weaponDef.soundStringOverrides.Length; i++)
                    {
                        if (this.dante.weaponDef.soundStringOverrides[i].originalSoundString == soundString)
                        {
                            soundString = this.dante.weaponDef.soundStringOverrides[i].newSoundString;
                            break;
                        }
                    }
                }
            }*/

            Util.PlaySound(soundString, this.gameObject);
        }

        protected virtual bool HasItem(ItemDef itemDef)
        {
            if (this.characterBody && this.characterBody.inventory)
            {
                return (this.characterBody.inventory.GetItemCountEffective(itemDef) > 0);
            }
            return false;
        }

        protected virtual bool turningAllowed
        {
            get
            {
                return true;
            }
        }

        protected virtual bool normalizeModel
        {
            get
            {
                return false;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (this._animator)
            {
                if (this.isGrounded) this._animator.SetFloat("airBlend", 0f);
                else this._animator.SetFloat("airBlend", 1f);
            }
        }

        public override void OnEnter()
        {
            this.ValidateControllerReference();
            base.OnEnter();
            this._animator = this.GetModelAnimator();
            if (!this.turningAllowed) this.characterDirection.turnSpeed = 0f;
            if (this.normalizeModel) this.modelLocator.normalizeToFloor = true;
            if (this.noCollision)
            {
                this.gameObject.layer = LayerIndex.fakeActor.intVal;
                this.characterMotor.Motor.RebuildCollidableLayers();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (!this.turningAllowed) this.characterDirection.turnSpeed = this.monsterHunter.baseTurnSpeed;
            if (this.normalizeModel) this.modelLocator.normalizeToFloor = false;
            if (this.noCollision)
            {
                this.gameObject.layer = LayerIndex.defaultLayer.intVal;
                this.characterMotor.Motor.RebuildCollidableLayers();
            }
        }

        protected void ValidateControllerReference()
        {
            this.monsterHunter = this.gameObject.GetComponent<MonsterHunterController>();
        }

        public enum InputDirection
        {
            None,
            Forward,
            Back,
            Left,
            Right
        }

        protected InputDirection inputDirection
        {
            get
            {
                if (this.inputBank && this.inputBank.moveVector == Vector3.zero) return InputDirection.None;

                bool cameraRelative = true;

                if (cameraRelative)
                {
                    Vector3 targetPosition = this.transform.position + (this.inputBank.aimDirection * 5f);
                    targetPosition.y = this.transform.position.y;

                    Vector3 relativePosition = targetPosition - this.transform.position;
                    float angle = Vector3.Angle(this.inputBank.moveVector, relativePosition);

                    /*if (this.dante && this.dante.isWeaponPrimary && this.isWeapon)
                    {
                        if (this.dante.wasSprinting && angle <= 20f) return InputDirection.Forward;
                    }
                    else */if (angle <= 20f)
                    {
                        return InputDirection.Forward;
                    }

                    if (angle >= 150f && angle <= 210f)
                    {
                        return InputDirection.Back;
                    }
                }

                return InputDirection.None;
            }
        }
    }
}