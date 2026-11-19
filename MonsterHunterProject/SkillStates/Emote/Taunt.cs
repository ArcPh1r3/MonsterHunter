namespace MonsterHunter.SkillStates.Emote
{
    public class Taunt : BaseEmote
    {
        public override void OnEnter()
        {
            base.OnEnter();

            this.PlayEmote("Taunt", "", 1.5f);
        }
    }
}