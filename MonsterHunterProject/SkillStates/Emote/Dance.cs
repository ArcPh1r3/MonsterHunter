namespace MonsterHunter.SkillStates.Emote
{
    public class Dance : BaseEmote
    {
        public override void OnEnter()
        {
            base.OnEnter();

            this.PlayEmote("Dance");
        }

        public override void OnExit()
        {
            base.OnExit();

            if (this.modelLocator && this.modelLocator.modelTransform)
            {
                Content.Components.MonsterHunterAnimationEvents b = this.modelLocator.modelTransform.GetComponent<Content.Components.MonsterHunterAnimationEvents>();
                if (b) b.EndTrails();
            }
        }
    }
}