using System;
using System.Runtime.InteropServices;
using EntityStates.Barrel;
using RoR2;
using RoR2.Networking;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace MonsterHunter.Content.Components
{
    public sealed class MonsterPartInteraction : NetworkBehaviour, IInteractable, IDisplayNameProvider
    {
        public string displayNameToken = "rob_monHun_impEye";
        public string contextToken = "";
        public string identifier = "rob_monHun_impEye";
        public GameObject destroyOnOpen;

        [SyncVar]
        private bool opened;
        private bool triggered;

        bool IInteractable.ShouldProximityHighlight()
        {
            return false;
        }

        public string GetContextString(Interactor activator)
        {
            return Language.GetString(this.contextToken);
        }

        public override int GetNetworkChannel()
        {
            return QosChannelIndex.defaultReliable.intVal;
        }

        public Interactability GetInteractability(Interactor activator)
        {
            if (this.opened) return Interactability.Disabled;

            if (activator)
            {
                CharacterBody cb = activator.GetComponent<CharacterBody>();
                if (cb)
                {
                    MonsterHunterController mh = cb.GetComponent<MonsterHunterController>();
                    if (!mh) return Interactability.Disabled;
                }

                if (!this.triggered)
                {
                    this.triggered = true;
                    activator.AttemptInteraction(this.gameObject);
                }
            }

            return Interactability.Available;
        }

        [Server]
        public void OnInteractionBegin(Interactor activator)
        {
            if (!NetworkServer.active) return;

            if (!this.opened)
            {
                this.Networkopened = true;
                //EntityStateMachine esm = base.GetComponent<EntityStateMachine>();
                //if (esm) esm.SetNextState(new Opening());
                /*HunkController hunk = activator.GetComponent<HunkController>();
				if (hunk)
				{
					hunk.ServerGetAmmo(this.multiplier);
				}*/

                MonsterPartOrb orb = new MonsterPartOrb();
                orb.origin = this.transform.position;
                orb.target = Util.FindBodyMainHurtBox(activator.GetComponent<CharacterBody>());
                orb.identifier = this.identifier;
                OrbManager.instance.AddOrb(orb);

                if (this.destroyOnOpen) Destroy(this.destroyOnOpen);
                NetworkServer.Destroy(this.gameObject);
            }
        }

        public string GetDisplayName()
        {
            return Language.GetString(this.displayNameToken);
        }

        public bool ShouldIgnoreSpherecastForInteractibility(Interactor activator)
        {
            return false;
        }

        public void OnEnable()
        {
            // this helper is probably broken. oh well
            if (!Modules.Helpers.isLocalUserMonsterHunter)
			{
				Destroy(this.gameObject);
				return;
			}

            InstanceTracker.Add<MonsterPartInteraction>(this);
        }

        public void OnDisable()
        {
            InstanceTracker.Remove<MonsterPartInteraction>(this);
        }

        public bool ShouldShowOnScanner()
        {
            return !this.opened;
        }

        private void UNetVersion()
        {
        }

        public bool Networkopened
        {
            get
            {
                return this.opened;
            }
            [param: In]
            set
            {
                base.SetSyncVar<bool>(value, ref this.opened, 1U);
            }
        }

        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            if (forceAll)
            {
                writer.Write(this.opened);
                return true;
            }
            bool flag = false;
            if ((base.syncVarDirtyBits & 1U) != 0U)
            {
                if (!flag)
                {
                    writer.WritePackedUInt32(base.syncVarDirtyBits);
                    flag = true;
                }
                writer.Write(this.opened);
            }
            if (!flag)
            {
                writer.WritePackedUInt32(base.syncVarDirtyBits);
            }
            return flag;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                this.opened = reader.ReadBoolean();
                return;
            }
            int num = (int)reader.ReadPackedUInt32();
            if ((num & 1) != 0)
            {
                this.opened = reader.ReadBoolean();
            }
        }

        public override void PreStartClient()
        {
        }
    }

    public class MonsterPartOrb : Orb
    {
        public string identifier = "";

        private MonsterHunterController mh;

        public override void Begin()
        {
            base.duration = UnityEngine.Random.Range(0.2f, 0.3f);

            EffectData effectData = new EffectData
            {
                origin = this.origin,
                genericFloat = base.duration
            };

            effectData.SetHurtBoxReference(this.target);

            EffectManager.SpawnEffect(Modules.MonsterHunterAssets.monsterPartOrb, effectData, true);

            HurtBox hurtBox = this.target.GetComponent<HurtBox>();
            if (hurtBox)
            {
                this.mh = hurtBox.healthComponent.GetComponent<MonsterHunterController>();
            }
        }

        public override void OnArrival()
        {
            if (this.mh)
            {
                //this.mh.ServerGetAmmo(this.multiplier);
            }
        }
    }
}