using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using HG.Reflection;
using MonsterHunter.Modules;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace MonsterHunter.Content.MonsterParts
{
    // TO-DO: figure out networking proper. this is a hurdle in shipmod too atp, but just because its entirely untested there. literally 0 clue how itll work in mp as-is but shouldnt be dangerous..
    // probably just a matter of RPCing get/set/add/subtract. we'll see soon, holding off until we get pickups for these
    public abstract class BaseMonsterPart
    {
        // TO-DO: associate with a pickup. needs a gameobj model prefab and texture pickup icon
        public BaseMonsterPart()
        {
            partIndex = PartCatalog.FindPartIndex(GetType());
        }

        public PartIndex partIndex { get; private set; }

        private sealed class PartAmount
        {
            public uint value;
        }

        private readonly ConditionalWeakTable<UserProfile, PartAmount> partAmounts = new ConditionalWeakTable<UserProfile, PartAmount>();

        public abstract string xmlKey { get; }
        public abstract Color32 partColor { get; }
        public abstract string partName { get; }

        public void Initialize()
        {
            CreateHUDElement();
        }

        public uint GetAmount(UserProfile profile)
        {
            return profile != null && partAmounts.TryGetValue(profile, out PartAmount partAmount) ? partAmount.value : 0u;
        }

        public void SetAmount(UserProfile profile, uint amount)
        {
            partAmounts.GetOrCreateValue(profile).value = amount;
            // we dont NEED to force a save right this second
            profile.RequestEventualSave();
            // but try to anyway:
            if (profile.canSave && PlatformSystems.saveSystem != null)
            {
                PlatformSystems.saveSystem.Save(profile, false);
            }
        }

        public void AddPart(UserProfile profile, uint amount = 1)
        {
            ulong total = (ulong)GetAmount(profile) + amount;
            SetAmount(profile, (uint)Math.Min(total, (ulong)uint.MaxValue));
        }

        public bool TrySpend(UserProfile profile, uint amount)
        {
            uint current = GetAmount(profile);
            if (current < amount)
            {
                return false;
            }

            SetAmount(profile, current - amount);
            return true;
        }

        private void CreateHUDElement()
        {
            GameObject vanillaHUD = Addressables.LoadAssetAsync<GameObject>("d79990e6848003d438cabcf79e7e5bf7").WaitForCompletion();

            if (vanillaHUD && vanillaHUD.TryGetComponent(out HUD hud) && hud.voidCoinContainer)
            {
                GameObject container = GameObject.Instantiate(hud.voidCoinContainer.gameObject);
                container.name = this.GetType().Name + "Root";
                container.transform.SetParent(hud.voidCoinContainer.transform.parent, worldPositionStays: false);
                
                if (container.transform.Find("BackgroundPanel").TryGetComponent(out RawImage img))
                {
                    img.color = partColor;
                }

                if (container.transform.Find("ValueText").TryGetComponent(out HGTextMeshProUGUI hgtmpugui))
                {
                    hgtmpugui.color = partColor;
                }

                if (container.transform.Find("VoidCoinSign").TryGetComponent(out HGTextMeshProUGUI hgtmpugui2))
                {
                    hgtmpugui2.color = partColor;
                    hgtmpugui2.text = partName; // TO-DO: this should ideally be a sprite of the part or smth lol
                    hgtmpugui2.fontSize = 24;
                    hgtmpugui2.gameObject.name = "DollarSign";
                }
            }
        }
    }

    public class MonsterPartPanelController : MonoBehaviour
    {
        public MoneyText moneyText;
        public UserProfile profile;
        // public static List<MonsterPartPanelController> instances = new List<MonsterPartPanelController>(); idk why i have instance tracking for these in shipmod either
        public BaseMonsterPart partToTrack;

        public void Awake()
        {
            profile = LocalUserManager.GetFirstLocalUser().userProfile;
            if (profile != null && TryGetComponent(out MoneyText mt))
            {
                moneyText = mt;
                moneyText.targetValue = (int)partToTrack.GetAmount(profile);
            }

            if (Config.showAllParts.Value == true)
            {
                gameObject.SetActive(true);
            }
        }

        // would be nice to only update as needed but this is also what vanilla does even for lunar coins :')
        //  feels wrong but no real downside to doing it constantly so whatever right
        public void FixedUpdate()
        {
            if (profile != null && moneyText != null)
            {
                moneyText.targetValue = (int)partToTrack.GetAmount(profile);
            }
        }

        // actually por que
        //public void OnEnable()
        //{
        //    if (!instances.Contains(this))
        //    {
        //        instances.Add(this);
        //    }
        //}

        //public void OnDisable()
        //{
        //    if (instances.Contains(this))
        //    {
        //        instances.Remove(this);
        //    }
        //}
    }
}
