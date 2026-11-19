using UnityEngine;
using RoR2;
using UnityEngine.AddressableAssets;

namespace MonsterHunter.Content.Components
{
    public class MusicPlayer : MonoBehaviour
    {
        public string bgmString = "";
        public bool disableBGM = true;

        private uint playID = 0u;
        private MusicTrackOverride musicTrackOverride;

        private void OnEnable()
        {
            if (this.disableBGM)
            {
                this.musicTrackOverride = this.gameObject.AddComponent<MusicTrackOverride>();
                this.musicTrackOverride.track = Addressables.LoadAssetAsync<MusicTrackDef>("RoR2/Base/Common/MusicTrackDefs/muNone.asset").WaitForCompletion();
                this.musicTrackOverride.priority = 90000;
            }
            else
            {
                if (this.playID == 0u) this.playID = Util.PlaySound(this.bgmString, this.gameObject);
            }
        }

        private void Start()
        {
            if (this.playID == 0u) this.playID = Util.PlaySound(this.bgmString, this.gameObject);
        }

        private void OnDestroy()
        {
            AkSoundEngine.StopPlayingID(this.playID);
            if (this.musicTrackOverride) Destroy(this.musicTrackOverride);
        }

        private void OnDisable()
        {
            AkSoundEngine.StopPlayingID(this.playID);
            if (this.musicTrackOverride) Destroy(this.musicTrackOverride);
        }
    }
}