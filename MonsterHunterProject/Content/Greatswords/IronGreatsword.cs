using MonsterHunter.Content.Greatswords;
using RoR2.Skills;
using UnityEngine;

namespace MonsterHunter.Content.Weapons
{
    public class IronGreatsword : BaseGreatsword<IronGreatsword>
    {
        public override string nameToken => "IRON_GREATSWORD";
        public override string name => "Buster Sword";
        public override string desc => "A crude iron greatsword that, despite its mass-produced quality, packs quite a punch.";
        public override string iconName => "texGreatswordIcon";
        public override Mesh mesh => Modules.MonsterHunterAssets.mainAssetBundle.LoadAsset<Mesh>("meshGreatsword");
        public override Material material => Modules.MonsterHunterAssets.CreateMaterial("matPlaceholder");
        public override SkillDef skillDef => Content.Survivors.MonsterHunter.chargeSlashSkillDef;
        public override float power => 1f;
        public override float attackSpeed => 1f;
        public override float chargeSpeed => 1f;
    }
}