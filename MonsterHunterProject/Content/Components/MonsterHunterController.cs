using EntityStates;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace MonsterHunter.Content.Components
{
    public class MonsterHunterController : NetworkBehaviour
    {
        public MeleeWeaponTrail swordTrail;
        public MeleeWeaponTrail swordTrailB;
        public float baseTurnSpeed { get; private set; }
        public HealthComponent healthComponent { get; private set; }
        public Animator animator { get; private set; }
        public CharacterBody characterBody { get; private set; }
        public CharacterDirection characterDirection { get; private set; }
        public ModelSkinController skinController { get; private set; }
        public CharacterModel characterModel { get; private set; }
        public SkillLocator skillLocator { get; private set; }
        public ChildLocator childLocator { get; private set; }
        public CameraRigController cameraController;
        public MonsterHunterPassive passive { get; private set; }
        public MonsterHunterTracker tracker { get; private set; }

        public static Action<Inventory> onInventoryUpdate;

        public float stopwatch;
        public float jumpLockoutStopwatch;
        private float notMovingStopwatch;

        private Inventory inventory
        {
            get
            {
                if (this.characterBody && this.characterBody.inventory) return this.characterBody.inventory;
                return null;
            }
        }

        private void Awake()
        {
            this.characterBody = this.GetComponent<CharacterBody>();
            this.characterDirection = this.GetComponent<CharacterDirection>();
            this.healthComponent = this.GetComponent<HealthComponent>();
            this.baseTurnSpeed = this.GetComponent<CharacterDirection>().turnSpeed;
            Transform modelTransform = this.GetComponent<ModelLocator>().modelTransform;
            this.animator = modelTransform.GetComponent<Animator>();
            this.childLocator = modelTransform.GetComponent<ChildLocator>();
            this.characterModel = modelTransform.GetComponent<CharacterModel>();
            this.skillLocator = this.GetComponent<SkillLocator>();
            this.skinController = this.GetComponentInChildren<ModelSkinController>();
            this.passive = this.GetComponent<MonsterHunterPassive>();
            this.tracker = this.GetComponent<MonsterHunterTracker>();

            this.Invoke("SetInventoryHook", 0.5f);
        }

        private void Start()
        {
            this.Invoke("ValidateWeapon", 0.1f);
        }

        private void ValidateWeapon()
        {
            //this.EquipWeapon(this.weaponTracker.equippedIndex);
        }

        private void SetInventoryHook()
        {
            if (this.inventory)
            {
                this.inventory.onInventoryChanged += this.Inventory_onInventoryChanged;
                this.inventory.onItemAddedClient += this.Inventory_onItemAddedClient;
            }
        }

        private void Inventory_onInventoryChanged()
        {
            if (this.inventory)
            {
            }

            if (onInventoryUpdate != null)
            {
                onInventoryUpdate(this.inventory);
            }
        }

        private void Inventory_onItemAddedClient(ItemIndex obj)
        {
            if (onInventoryUpdate != null)
            {
                onInventoryUpdate(this.inventory);
            }
        }

        private void Update()
        {
            /*if (Input.GetKeyDown(KeyCode.L))
            {
                if (WeaponPanel.instance)
                {
                    WeaponPanel.instance.targetHUD.gameObject.SetActive(!WeaponPanel.instance.targetHUD.gameObject.activeSelf);
                }
            }*/
        }

        private void FixedUpdate()
        {
            this.stopwatch += Time.fixedDeltaTime;
            this.jumpLockoutStopwatch -= Time.fixedDeltaTime;

            if (this.characterBody.hasAuthority)
            {
                if (this.characterBody.inputBank.moveVector.magnitude <= 0.1f && this.characterBody.characterMotor.isGrounded) this.notMovingStopwatch += Time.fixedDeltaTime;
                else this.notMovingStopwatch = 0f;
            }
        }

        public void ResetToNeutral()
        {
            foreach (EntityStateMachine e in this.gameObject.GetComponents<EntityStateMachine>())
            {
                if (e && e.customName == "Body")
                {
                    if (e.state.GetType() != typeof(SkillStates.MonsterHunter.MainState)) e.SetInterruptState(new SkillStates.MonsterHunter.MainState(), InterruptPriority.Frozen);
                }
            }
            foreach (EntityStateMachine e in this.gameObject.GetComponents<EntityStateMachine>())
            {
                if (e && e.customName == "Weapon")
                {
                    e.SetInterruptState(new EntityStates.Idle(), InterruptPriority.Frozen);
                }
            }
        }

        public bool SetWeaponState(EntityState newState, InterruptPriority i = InterruptPriority.Skill)
        {
            if (!this.characterBody.isPlayerControlled) i = InterruptPriority.Any;

            foreach (EntityStateMachine e in this.gameObject.GetComponents<EntityStateMachine>())
            {
                if (e && e.customName == "Weapon")
                {
                    return e.SetInterruptState(newState, i);
                };
            }

            return false;
        }
    }
}