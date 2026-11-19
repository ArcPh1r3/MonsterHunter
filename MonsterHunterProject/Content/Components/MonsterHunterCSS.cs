using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MonsterHunter.Content.Components
{
    public class MonsterHunterCSS : MonoBehaviour
    {
        private ChildLocator childLocator;

        private void Awake()
        {
            this.childLocator = this.GetComponent<ChildLocator>();
        }

        private void Update()
        {
        }
    }
}