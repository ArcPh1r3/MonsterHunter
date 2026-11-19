using System.Collections.Generic;
using System;

namespace MonsterHunter.Modules
{
    public static class States
    {
        internal static List<Type> entityStates = new List<Type>();

        public static void RegisterStates()
        {
            entityStates.Add(typeof(SkillStates.FuckMyAss));

            entityStates.Add(typeof(SkillStates.BaseStates.BaseMeleeAttack));

            entityStates.Add(typeof(SkillStates.Emote.BaseEmote));
            entityStates.Add(typeof(SkillStates.Emote.Rest));
            entityStates.Add(typeof(SkillStates.Emote.Taunt));
            entityStates.Add(typeof(SkillStates.Emote.Dance));
            entityStates.Add(typeof(SkillStates.Emote.ToggleMusic));

            entityStates.Add(typeof(SkillStates.MonsterHunter.BaseMonsterHunterState));
            entityStates.Add(typeof(SkillStates.MonsterHunter.BaseMonsterHunterSkillState));
            entityStates.Add(typeof(SkillStates.MonsterHunter.MainState));
        }

        internal static void AddSkill(Type t)
        {
            entityStates.Add(t);
        }
    }
}