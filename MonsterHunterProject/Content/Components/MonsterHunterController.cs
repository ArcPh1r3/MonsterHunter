using EntityStates;
using R2API.Networking;
using R2API.Networking.Interfaces;
using Rewired.Utils;
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
        public float fallSpeedModifier = 1.1665f;
        public float slashMovementModifier = 3f;
        public float boostTimer = 0f;
        private float zoom;
        private float targetZoom;
        public float zoomSmoothSpeed = 2f;

        private float z = 0.01337f;
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
        public CameraTargetParams cameraController;
        private CharacterCameraParams cameraParams;
        public MonsterHunterPassive passive { get; private set; }
        public MonsterHunterTracker tracker { get; private set; }

        public static Action<Inventory> onInventoryUpdate;

        public float stopwatch;
        public float jumpLockoutStopwatch;
        private float notMovingStopwatch;
        private float zoomResetStopwatch;

        public GameObject tackleEffect;

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
            this.cameraController = this.GetComponent<CameraTargetParams>();

            this.Invoke("SetInventoryHook", 0.5f);
        }

        private void Start()
        {
            this.Invoke("ValidateWeapon", 0.1f);
            this.CreateTackleEffect();
        }

        private void CreateTackleEffect()
        {
            this.tackleEffect = GameObject.Instantiate(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bison/BisonBody.prefab").WaitForCompletion().GetComponent<ModelLocator>().modelTransform.Find("BisonArmature/ROOT/SprintEffect").gameObject, this.childLocator.FindChild("Chest"));
            this.tackleEffect.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            this.tackleEffect.transform.localRotation = Quaternion.Euler(new Vector3(270f, 0f, 0f));
            this.tackleEffect.transform.localScale = Vector3.one;

            this.tackleEffect.GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/VoidSurvivor/matVoidBlinkTrailCorrupted.mat").WaitForCompletion();
            this.tackleEffect.GetComponent<ParticleSystemRenderer>().trailMaterial = Addressables.LoadAssetAsync<Material>("RoR2/DLC3/ExplosiveJunkBombDestructible/matJunkDroneBombExplosionChargePuffs.mat").WaitForCompletion();
            this.tackleEffect.transform.Find("SmokeRings").localScale = new Vector3(0.25f, 0.25f, 0.75f);
            this.tackleEffect.transform.Find("SmokeRings").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matDustDirectional.mat").WaitForCompletion();
            this.tackleEffect.transform.Find("Glow").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/DLC3/ExplosiveJunkBombDestructible/matJunkDroneBombExplosionChargePuffs.mat").WaitForCompletion();

            this.tackleEffect.SetActive(false);
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
            this.zoomResetStopwatch -= Time.fixedDeltaTime;
            this.boostTimer -= Time.fixedDeltaTime;

            if (this.hasAuthority)
            {
                if (this.characterBody.inputBank.moveVector.magnitude <= 0.1f && this.characterBody.characterMotor.isGrounded) this.notMovingStopwatch += Time.fixedDeltaTime;
                else this.notMovingStopwatch = 0f;

                if (this.cameraController)
                {
                    this.cameraParams = this.cameraController.cameraParams;
                    if (!this.cameraParams.IsNullOrDestroyed())
                    {
                        if (this.z == 0.01337f) this.z = this.cameraParams.data.idealLocalCameraPos.value.z;
                        if (this.zoomResetStopwatch <= 0f) this.targetZoom = 0f;
                        this.zoom = Mathf.Lerp(this.zoom, this.targetZoom, Time.fixedDeltaTime * this.zoomSmoothSpeed);
                        this.cameraParams.data.idealLocalCameraPos.value.z = this.z + this.zoom;
                    }
                }
            }
        }

        public void SetZoom(float newZoom, float duration)
        {
            this.targetZoom = newZoom;
            this.zoomResetStopwatch = duration;
        }

        private void OnDestroy()
        {
            if (this.z != 0.01337f && !this.cameraParams.IsNullOrDestroyed())
            {
                this.cameraParams.data.idealLocalCameraPos.value.z = this.z;
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