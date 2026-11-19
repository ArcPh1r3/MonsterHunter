using UnityEngine;
using MonsterHunter.Content.Components;
using EntityStates;
using UnityEngine.Networking;

namespace MonsterHunter.SkillStates.Emote
{
    public class ToggleMusic : BaseState
    {
        private float duration = 0.6f;
        private bool boom = false;
        private int rng;

        public override void OnEnter()
        {
            this.rng = Mathf.RoundToInt(Random.value * 100f);
            base.OnEnter();

            if (this.GetComponent<MonsterHunterController>()) base.PlayCrossfade("Gesture Additive, Left", "ToggleMusic", "Emote.playbackRate", this.duration, 0.05f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!this.boom && base.fixedAge >= 0.25f * this.duration)
            {
                this.boom = true;
                this.TryToggleMusic();
                RoR2.Util.PlaySound("sfx_hunk_button_click", this.gameObject);
            }

            if (base.isAuthority && base.fixedAge >= this.duration) this.outer.SetNextStateToMain();
        }

        private void TryToggleMusic()
        {
            Transform headphones = this.FindModelChild("HeadphonesModel");
            // attempt to add music player component
            // first check if there already is one, and toggle it off if so
            MusicPlayer m = this.GetComponent<MusicPlayer>();
            if (m)
            {
                Destroy(m);
                if (headphones) headphones.gameObject.SetActive(false);
                return;
            }
            else
            {
                if (headphones) headphones.gameObject.SetActive(true);

                if (base.isAuthority)
                {
                    m = this.gameObject.AddComponent<MusicPlayer>();

                    m.bgmString = "Play_Dante_SDT_System_Devils_Never_Cry";
                    if (this.rng < 60) m.bgmString = "Play_Dante_SDT_System_Fighting_Breath";
                    if (this.rng < 40) m.bgmString = "Play_Dante_SDT_System_Devil_May_Cry_Anime";
                    if (this.rng < 20) m.bgmString = "Play_Dante_SDT_System_Burn_In_Hell";
                    if (this.rng < 10) m.bgmString = "Play_Dante_SDT_System_Number_One";

                    /*if (this.rng < 6) m.bgmString = "Sin_Devil_Trigger";
                    else if (this.rng < 50) m.bgmString = "Bury_The_Light_System";
                    else m.bgmString = "Devil_Trigger";*/
                }
                else
                {
                    m = this.gameObject.AddComponent<MusicPlayer>();

                    m.bgmString = "Play_Dante_SDT_System_Devils_Never_Cry_Local";
                    if (this.rng < 60) m.bgmString = "Play_Dante_SDT_System_Fighting_Breath_Local";
                    if (this.rng < 40) m.bgmString = "Play_Dante_SDT_System_Devil_May_Cry_Anime_Local";
                    if (this.rng < 20) m.bgmString = "Play_Dante_SDT_System_Burn_In_Hell_Local";
                    if (this.rng < 10) m.bgmString = "Play_Dante_SDT_System_Number_One_Local";
                }
            }
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            writer.Write(this.rng);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            this.rng = reader.ReadInt32();
        }
    }
}