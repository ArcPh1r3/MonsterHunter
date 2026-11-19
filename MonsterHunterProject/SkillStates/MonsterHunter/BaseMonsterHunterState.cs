using EntityStates;
using MonsterHunter.Content.Components;
using RoR2;

namespace MonsterHunter.SkillStates.MonsterHunter
{
    public class BaseMonsterHunterState : BaseState
    {
        protected MonsterHunterController monsterHunter;

        protected virtual bool HasItem(ItemDef itemDef)
        {
            if (this.characterBody && this.characterBody.inventory)
            {
                return (this.characterBody.inventory.GetItemCountEffective(itemDef) > 0);
            }
            return false;
        }

        public override void OnEnter()
        {
            this.ValidateControllerReference();
            base.OnEnter();
        }

        protected void ValidateControllerReference()
        {
            this.monsterHunter = this.gameObject.GetComponent<MonsterHunterController>();
        }
    }
}