using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterHunter.Content.Greatswords
{
    public static class GreatswordCatalog
    {
        public static GreatswordDef[] greatswordDefs = new GreatswordDef[0];

        public static void AddWeapon(GreatswordDef weaponDef, bool addItem = true)
        {
            Array.Resize(ref greatswordDefs, greatswordDefs.Length + 1);

            int index = greatswordDefs.Length - 1;
            weaponDef.index = (ushort)index;

            greatswordDefs[index] = weaponDef;
            weaponDef.index = (ushort)index;

            // set default icon
            /*if (!weaponDef.icon)
            {
                switch (weaponDef.tier)
                {
                    case DriverWeaponTier.Common:
                        weaponDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Texture>("texGenericWeaponGrey");
                        break;
                    case DriverWeaponTier.Uncommon:
                        weaponDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Texture>("texGenericWeaponGreen");
                        break;
                    case DriverWeaponTier.Legendary:
                        weaponDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Texture>("texGenericWeaponRed");
                        break;
                    case DriverWeaponTier.Unique:
                        weaponDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Texture>("texGenericWeaponYellow");
                        break;
                    case DriverWeaponTier.Lunar:
                        weaponDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Texture>("texGenericWeaponBlue");
                        break;
                    case DriverWeaponTier.Void:
                        weaponDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Texture>("texGenericWeaponPurple");
                        break;
                }
            }*/

            //if (addItem) itemDefs.Add(weaponDef.itemDef);

            Debug.Log("Added " + weaponDef.nameToken + " to Greatsword catalog with index: " + weaponDef.index);
        }

        public static GreatswordDef GetWeaponFromIndex(int index)
        {
            return greatswordDefs[index];
        }
    }
}