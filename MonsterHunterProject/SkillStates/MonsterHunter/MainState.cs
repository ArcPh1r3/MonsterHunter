using UnityEngine;
using RoR2;
using EntityStates;
using MonsterHunter.Modules;
using MonsterHunter.SkillStates.Emote;
using BepInEx.Configuration;
using MonsterHunter.Content.Components;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace MonsterHunter.SkillStates.MonsterHunter
{
	public class MainState : GenericCharacterMain
	{
		private Animator animator;
		public LocalUser localUser;

		private MonsterHunterController monsterHunter;
		private MomentumController momentumController;
		private float sprintTimer;
		private bool wasSprinting;
		private bool wasGrounded2;
        private bool isSliding;
		private GameObject slideEffectInstance;
		private uint slidePlayID;

        public override void OnEnter()
		{
			base.OnEnter();
            this.animator = this.modelAnimator;
			this.monsterHunter = this.GetComponent<MonsterHunterController>();
			this.momentumController = this.GetComponent<MomentumController>();
			this.FindLocalUser();
			//if (this.characterBody.modelLocator) this.characterBody.modelLocator.normalizeToFloor = false;
		}

        public override void OnExit()
        {
            base.OnExit();
			//if (this.characterBody.modelLocator) this.characterBody.modelLocator.normalizeToFloor = false;
			AkSoundEngine.StopPlayingID(this.slidePlayID);
		}

        public override void FixedUpdate()
		{
			base.FixedUpdate();

			/*if (this.inputBank.moveVector != Vector3.zero && this.characterBody.isSprinting)
			{
				if (this.isGrounded) this.sprintTimer += Time.fixedDeltaTime;
				else if (this.sprintTimer >= 1f)
                {
					if (this.wasGrounded2)
                    {
						Util.PlaySound("sfx_dante_jump", this.gameObject);
						base.PlayCrossfade("Body", "Hop", 0.05f);
						this.SmallHop(this.characterMotor, 6f);
                    }
                }
			}
			else this.sprintTimer = 0f;
			if (this.characterBody.outOfCombatStopwatch <= 0.1f) this.sprintTimer = 0f;
			this.wasGrounded2 = this.isGrounded;*/

			//this.characterBody.isSprinting = this.sprintTimer >= 1f;

			if (this.animator)
			{
				bool cock = false;
				if (!this.characterBody.outOfDanger || !this.characterBody.outOfCombat) cock = true;

				this.animator.SetBool("inCombat", cock);

				if (this.isGrounded) this.animator.SetFloat("airBlend", 0f);
				else this.animator.SetFloat("airBlend", 1f);

				this.animator.SetFloat("aimDir", this.inputBank.aimDirection.y);

				if (this.inputBank.moveVector != Vector3.zero || !this.isGrounded) this.animator.SetFloat("moving", Mathf.Lerp(this.animator.GetFloat("moving"), 1f, Time.fixedDeltaTime * 5f));
				else this.animator.SetFloat("moving", Mathf.Lerp(this.animator.GetFloat("moving"), 0f, Time.fixedDeltaTime * 5f));
			}

			if (this.sprintTimer >= 1f)
            {
				if (!this.wasSprinting)
                {
					Util.PlaySound("sfx_dante_sprint", this.gameObject);

					EffectData effectData = new EffectData();
					effectData.rotation = Util.QuaternionSafeLookRotation(this.characterDirection.forward);
					effectData.origin = base.characterBody.corePosition;

					//EffectManager.SpawnEffect(Modules.MonsterHunterAssets.dashEffectSimple, effectData, true);

					//if (NetworkServer.active) this.characterBody.AddBuff(Content.Survivors.MonsterHunter.superSprintBuff);
				}

				this.wasSprinting = true;
				this.animator.SetBool("isSuperSprinting", true);
            }
			else
            {
				//if (NetworkServer.active && this.wasSprinting) this.characterBody.RemoveBuff(Content.Survivors.MonsterHunter.superSprintBuff);
				this.wasSprinting = false;
				this.animator.SetBool("isSuperSprinting", false);
			}

			this.HandleSlide();
		}

		private void HandleSlide()
        {
			this.characterBody.sprintingSpeedMultiplier = 1.45f;

            if (this.sprintTimer >= 1f)
			{
				this.momentumController.shouldCalc = true;
				if (this.momentumController.desiredMomentum >= 0.5f) this.isSliding = true;
				else this.isSliding = false;
			}
			else this.isSliding = false;

			//if (this.characterBody.modelLocator) this.characterBody.modelLocator.normalizeToFloor = this.sprintTimer >= 1f;

			this.animator.SetBool("isSliding", this.isSliding);

			if (this.isSliding)
            {
				this.characterBody.sprintingSpeedMultiplier = 1.75f;
				if (!this.slideEffectInstance)
                {
					this.characterBody.RecalculateStats();
					if (EntityStates.Commando.SlideState.slideEffectPrefab) this.slideEffectInstance = UnityEngine.Object.Instantiate<GameObject>(EntityStates.Commando.SlideState.slideEffectPrefab, this.FindModelChild("Root"));
					this.slidePlayID = Util.PlaySound("sfx_dante_slide_loop", this.gameObject);
				}
            }
			else
            {
				if (this.slideEffectInstance)
                {
                    this.characterBody.RecalculateStats();
                    Destroy(this.slideEffectInstance);
				}
				AkSoundEngine.StopPlayingID(this.slidePlayID);
            }
        }

		public override void Update()
        {
            base.Update();
			//this.characterBody.isSprinting = this.sprintTimer >= 1f;

			if (base.isAuthority)
            {
				// emotes
				if (base.characterMotor.isGrounded)
				{
					this.CheckEmote<Rest>(Config.restKey);
					this.CheckEmote<Taunt>(Config.tauntKey);
				}

				this.CheckMovingEmote<ToggleMusic>(Config.danceKey);
			}
		}

        public override void ProcessJump()
        {
			if (this.monsterHunter && this.monsterHunter.jumpLockoutStopwatch > 0f) return;
            base.ProcessJump();
        }

        public override bool TryPerformJump(float horizontalBonus, float verticalBonus)
        {
			Util.PlaySound("sfx_mh_jump", this.gameObject);
            return base.TryPerformJump(horizontalBonus, verticalBonus);
        }

		private bool HasItem(ItemDef itemDef)
        {
			if (this.characterBody && this.characterBody.inventory)
			{
				return this.characterBody.inventory.GetItemCount(itemDef) > 0;
			}
			return false;
        }

        private void CheckEmote(KeyCode keybind, EntityState state)
		{
			if (Input.GetKeyDown(keybind))
			{
				if (!this.localUser.isUIFocused)
				{
					this.outer.SetInterruptState(state, InterruptPriority.Any);
				}
			}
		}

		private void CheckEmote<T>(ConfigEntry<KeyboardShortcut> keybind) where T : EntityState, new()
		{
			if (Modules.Config.GetKeyPressed(keybind))
			{
				this.FindLocalUser();

				if (this.localUser != null && !this.localUser.isUIFocused)
				{
					this.outer.SetInterruptState(new T(), InterruptPriority.Any);
				}
			}
		}

		private void CheckMovingEmote<T>(ConfigEntry<KeyboardShortcut> keybind) where T : EntityState, new()
		{
			if (Modules.Config.GetKeyPressed(keybind))
			{
				this.FindLocalUser();

				if (this.localUser != null && !this.localUser.isUIFocused)
				{
					foreach (EntityStateMachine i in this.gameObject.GetComponents<EntityStateMachine>())
					{
						if (i.customName == "Weapon") i.SetInterruptState(new T(), InterruptPriority.Any);
					}
				}
			}
		}

		private void FindLocalUser()
		{
			if (this.localUser == null)
			{
				if (base.characterBody)
				{
					foreach (LocalUser lu in LocalUserManager.readOnlyLocalUsersList)
					{
						if (lu.cachedBody == base.characterBody)
						{
							this.localUser = lu;
							break;
						}
					}
				}
			}
		}
	}
}