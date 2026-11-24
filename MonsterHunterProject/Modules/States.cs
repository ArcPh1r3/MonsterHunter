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

            entityStates.Add(typeof(SkillStates.MonsterHunter.Skill.ChargeTackle));
            entityStates.Add(typeof(SkillStates.MonsterHunter.Skill.ChargeTackleMobile));
            entityStates.Add(typeof(SkillStates.MonsterHunter.Skill.TackleStartup));
            entityStates.Add(typeof(SkillStates.MonsterHunter.Skill.Tackle));
            entityStates.Add(typeof(SkillStates.MonsterHunter.Skill.TackleLanding));

            entityStates.Add(typeof(SkillStates.MonsterHunter.Greatsword.Combo));
            entityStates.Add(typeof(SkillStates.MonsterHunter.Greatsword.ChargeSlash));
            entityStates.Add(typeof(SkillStates.MonsterHunter.Greatsword.AirSlash));
            entityStates.Add(typeof(SkillStates.MonsterHunter.Greatsword.Slash));
            entityStates.Add(typeof(SkillStates.MonsterHunter.Greatsword.SlashStartup));
            entityStates.Add(typeof(SkillStates.MonsterHunter.Greatsword.SlashEnd));
            entityStates.Add(typeof(SkillStates.MonsterHunter.Greatsword.LeapSlash));
            entityStates.Add(typeof(SkillStates.MonsterHunter.Greatsword.Quickstep));

            entityStates.Add(typeof(SkillStates.MonsterHunter.HunterArts.Moonbreaker));
        }

        internal static void AddSkill(Type t)
        {
            entityStates.Add(t);
        }
    }
}