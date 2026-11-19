using UnityEngine;
using RoR2;
using UnityEngine.AddressableAssets;

namespace MonsterHunter.Content.Components
{
    public class MonsterHunterAnimationEvents : MonoBehaviour
    {
        private TrailRenderer[] trails;
        private float trailLength = 0f;
        private float desiredTrailLength = 0f;
        private ChildLocator childLocator;

        private void Awake()
        {
            this.trails = this.GetComponentsInChildren<TrailRenderer>();
            this.childLocator = this.GetComponent<ChildLocator>();
        }

        private void FixedUpdate()
        {
            if (this.trails != null && this.trails.Length > 0)
            {
                Material mat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matTracerBright.mat").WaitForCompletion();
                this.trailLength = Mathf.Lerp(this.trailLength, this.desiredTrailLength, 4f * Time.fixedDeltaTime);
                foreach (TrailRenderer i in this.trails)
                {
                    i.material = mat;
                    i.time = this.trailLength;
                }
            }
        }

        public void PlaySound(string soundString)
        {
            Util.PlaySound(soundString, this.gameObject);
        }

        public void StartTrails()
        {
            this.desiredTrailLength = 3f;
        }

        public void EndTrails()
        {
            this.desiredTrailLength = 0f;
        }

        public void StartGunSpin()
        {
        }

        public void StopGunSpin()
        {
        }

        public void CreateLandingEffect()
        {
            EffectManager.SpawnEffect(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/CharacterLandImpact.prefab").WaitForCompletion(), new EffectData
            {
                origin = this.transform.position,
                rotation = Quaternion.identity,
                scale = 1f
            }, false);
        }

        public void CreateJetEffect()
        {
            //Transform jetL = this.childLocator.FindChild("JetL");
            //Transform jetR = this.childLocator.FindChild("JetR");
            //if (jetL) UnityEngine.Object.Instantiate<GameObject>(Modules.BelmontAssets.jetEffect, jetL);
            //if (jetR) UnityEngine.Object.Instantiate<GameObject>(Modules.BelmontAssets.jetEffect, jetR);
        }
    }
}