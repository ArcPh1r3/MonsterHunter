using R2API;
using RoR2;
using RoR2.Skills;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MonsterHunter.Content.Greatswords
{
    public abstract class BaseGreatsword<T> : BaseGreatsword where T : BaseGreatsword<T>
    {
        public static T instance { get; private set; }

        public BaseGreatsword()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting BaseGreatsword was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class BaseGreatsword
    {
        public GreatswordDef greatswordDef { get; private set; }
        public abstract string nameToken { get; }
        public abstract string name { get; }
        public abstract string desc { get; }
        public abstract string iconName { get; }
        public abstract SkillDef skillDef { get; }
        public abstract float power { get; }
        public abstract float attackSpeed { get; }
        public abstract float chargeSpeed { get; }
        public abstract Mesh mesh { get; }
        public abstract Material material { get; }

        public virtual string nameTokenFull
        {
            get
            {
                return "ROB_MONSTERHUNTER_GREATSWORD_" + nameToken + "_NAME";
            }
        }

        public virtual string descToken
        {
            get
            {
                return "ROB_MONSTERHUNTER_GREATSWORD_" + nameToken + "_DESC";
            }
        }

        public virtual void Init()
        {
            CreateLang();
            CreateWeapon();
        }

        protected void CreateLang()
        {
            LanguageAPI.Add("ROB_MONSTERHUNTER_GREATSWORD_" + nameToken + "_NAME", name);
            LanguageAPI.Add("ROB_MONSTERHUNTER_GREATSWORD_" + nameToken + "_DESC", desc);
        }

        protected void CreateWeapon()
        {
            Sprite icon = null;
            if (iconName != "") icon = Modules.MonsterHunterAssets.mainAssetBundle.LoadAsset<Sprite>(iconName);

            greatswordDef = GreatswordDef.CreateGreatswordDefFromInfo(new GreatswordDefInfo
            {
                nameToken = "ROB_MONSTERHUNTER_GREATSWORD_" + nameToken + "_NAME",
                descriptionToken = "ROB_MONSTERHUNTER_GREATSWORD_" + nameToken + "_DESC",
                icon = icon,
                skillDef = skillDef,
                power = power,
                attackSpeed = attackSpeed,
                chargeSpeed = chargeSpeed,
                mesh = mesh,
                material = material
            });

            GreatswordCatalog.AddWeapon(greatswordDef);
        }
    }
}