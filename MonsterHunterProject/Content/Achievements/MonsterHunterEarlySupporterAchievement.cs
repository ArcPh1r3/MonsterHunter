using RoR2;
using RoR2.Achievements;
using UnityEngine;

namespace MonsterHunter.Content.Achievements
{
    //string identifier, string unlockableRewardIdentifier, string prerequisiteAchievementIdentifier, uint lunarCoinReward, Type serverTrackerType = null
    //automatically creates language tokens "ACHIEVEMENT_{identifier.ToUpper()}_NAME" and "ACHIEVEMENT_{identifier.ToUpper()}_DESCRIPTION" 
    [RegisterAchievement(IDENTIFIER, UNLOCKABLE_IDENTIFIER, null, 99, null)]
    internal class MonsterHunterEarlySupporterAchievement : BaseAchievement
    {
        public const string IDENTIFIER = "ROB_MONSTERHUNTER_SUPPORTER_ACHIEVEMENT_ID";
        public const string UNLOCKABLE_IDENTIFIER = "ROB_MONSTERHUNTER_SUPPORTER_UNLOCKABLE_REWARD_ID";
        public static Sprite Sprite => Modules.MonsterHunterAssets.mainAssetBundle.LoadAsset<Sprite>("texSuperSkin");

        public override BodyIndex LookUpRequiredBodyIndex() => Survivors.MonsterHunter.bodyIndex;

        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody characterBody)
        {
            if (characterBody.baseNameToken == Content.Survivors.MonsterHunter.bodyNameToken && base.meetsBodyRequirement)
            {
                base.Grant();
            }
        }

        public override void OnBodyRequirementBroken()
        {
            base.OnBodyRequirementBroken();
            CharacterBody.onBodyStartGlobal -= CharacterBody_onBodyStartGlobal;
        }
    }
}