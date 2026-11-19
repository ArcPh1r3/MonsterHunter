using RoR2;
using UnityEngine;

namespace MonsterHunter.Content.Achievements
{
    //string identifier, string unlockableRewardIdentifier, string prerequisiteAchievementIdentifier, uint lunarCoinReward, Type serverTrackerType = null
    //automatically creates language tokens "ACHIEVEMENT_{identifier.ToUpper()}_NAME" and "ACHIEVEMENT_{identifier.ToUpper()}_DESCRIPTION" 
    [RegisterAchievement(IDENTIFIER, UNLOCKABLE_IDENTIFIER, null, 10, null)]
    internal class MonsterHunterMasteryAchievement : BaseMasteryAchievement
    {
        public const string IDENTIFIER = "ROB_MONSTERHUNTER_MASTERY_UNLOCKABLE_ACHIEVEMENT_ID";
        public const string UNLOCKABLE_IDENTIFIER = "ROB_MONSTERHUNTER_MASTERY_UNLOCKABLE_REWARD_ID";

        public static Sprite Sprite => Modules.MonsterHunterAssets.mainAssetBundle.LoadAsset<Sprite>("texMasterySkin");

        //difficulty coeff 3 is monsoon. 3.5 is typhoon for grandmastery skins
        public override float RequiredDifficultyCoefficient => 3;
        public override DifficultyIndex RequiredEclipseLevel => DifficultyIndex.Eclipse1;
    }
}