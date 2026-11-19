using RoR2;
using System.Linq;
using UnityEngine;

namespace MonsterHunter.Content.Components
{
    public class MonsterHunterTracker : MonoBehaviour
    {
        public float maxTrackingDistance = 90f;
        public float maxTrackingAngle = 40f;
        public float trackerUpdateFrequency = 10f;

        private HurtBox trackingTarget;
        private CharacterBody characterBody;
        private TeamComponent teamComponent;
        private InputBankTest inputBank;
        private float trackerUpdateStopwatch;
        private Indicator indicator;
        private readonly BullseyeSearch search = new BullseyeSearch();

        private void Awake()
        {
            this.indicator = new Indicator(base.gameObject, Modules.MonsterHunterAssets.trackerPrefab);
        }

        private void Start()
        {
            this.characterBody = base.GetComponent<CharacterBody>();
            this.inputBank = base.GetComponent<InputBankTest>();
            this.teamComponent = base.GetComponent<TeamComponent>();
        }

        public HurtBox GetTrackingTarget()
        {
            return this.trackingTarget;
        }

        private void OnEnable()
        {
            if (this.indicator != null) this.indicator.active = true;
        }

        private void OnDisable()
        {
            if (this.indicator != null) this.indicator.active = false;
        }

        private void OnDestroy()
        {
            if (this.indicator != null) this.indicator.active = false;
        }

        private void FixedUpdate()
        {
            this.trackerUpdateStopwatch += Time.fixedDeltaTime;

            if (this.trackerUpdateStopwatch >= 1f / this.trackerUpdateFrequency)
            {
                this.trackerUpdateStopwatch -= 1f / this.trackerUpdateFrequency;

                Ray aimRay = new Ray(this.inputBank.aimOrigin, this.inputBank.aimDirection);
                this.SearchForTarget(aimRay);

                if (this.indicator != null) this.indicator.targetTransform = (this.trackingTarget ? this.trackingTarget.transform : null);
            }

            if (this.indicator != null && this.indicator.visualizerInstance)
            {
                /*if (this.dante)
                {
                    if (this.dante.lockOnTarget)
                    {
                        this.indicator.targetTransform = this.dante.lockOnTarget.transform;
                        this.indicator.visualizerInstance.SetActive(true);
                    }
                    else this.indicator.visualizerInstance.SetActive(false);
                }
                else if (this.sdt)
                {
                    if (this.GetTrackingTarget())
                    {
                        this.sdt.lockOnTarget = this.GetTrackingTarget().gameObject;
                        this.indicator.targetTransform = this.sdt.lockOnTarget.transform;
                        this.indicator.visualizerInstance.SetActive(true);
                    }
                    else this.indicator.visualizerInstance.SetActive(false);
                }
                else
                {
                    this.indicator.visualizerInstance.SetActive(this.trackingTarget);
                }*/
            }
        }

        private void SearchForTarget(Ray aimRay)
        {
            this.search.teamMaskFilter = TeamMask.GetUnprotectedTeams(this.teamComponent.teamIndex);
            this.search.filterByLoS = true;
            this.search.searchOrigin = aimRay.origin;
            this.search.searchDirection = aimRay.direction;
            this.search.sortMode = BullseyeSearch.SortMode.Distance;
            this.search.maxDistanceFilter = this.maxTrackingDistance;
            this.search.maxAngleFilter = this.maxTrackingAngle;
            this.search.RefreshCandidates();
            this.search.FilterOutGameObject(base.gameObject);

            this.trackingTarget = this.search.GetResults().FirstOrDefault<HurtBox>();
        }
    }
}