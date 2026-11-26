using RoR2;
using RoR2.Navigation;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace MonsterHunter.Modules
{
    public static class Helpers
    {
        internal const string agilePrefix = "<style=cIsUtility>Agile.</style> ";

        internal const string blackHex = "000000";
        internal const string whiteItemHex = "00FF66";
        internal const string greenItemHex = "00FF66";
        internal const string redItemHex = "FF0033";
        internal const string yellowItemHex = "FFFF00";
        internal const string lunarItemHex = "0066FF";
        internal const string voidItemHex = "C678B4";
        internal const string umbrellaHex = "C80202";
        internal const string devilHex = "7F364F";
        internal const string sinHex = "FF2E00";
        internal const string colorSuffix = "</color>";

        internal static Color whiteItemColor = new Color(1f, 1f, 1f);
        internal static Color greenItemColor = new Color(0f, 1f, 102f / 255f);
        internal static Color redItemColor = new Color(1f, 0f, 51f / 255f);
        internal static Color yellowItemColor = new Color(1f, 1f, 0f);
        internal static Color lunarItemColor = new Color(0f, 102f / 255f, 1f);
        internal static Color voidItemColor = new Color(198f / 255f, 120f / 255f, 180f / 255f);
        internal static Color badColor = new Color(127f / 255f, 0f, 0f);
        internal static Color devilColor = new Color(127f / 255f, 54f / 255f, 79f / 255f);

        public static void SetGhostPrefab(GameObject p, GameObject ghost)
        {
            ProjectileController pc = p.GetComponent<ProjectileController>();
            if (pc) SetGhostPrefab(pc, ghost);
        }

        public static void SetGhostPrefab(ProjectileController pc, GameObject ghost)
        {
            if (pc)
            {
                pc.ghostPrefab = ghost;
                pc.ghostPrefabAddress = null;
                pc.ghostAssetOrDirectReference = new RoR2.ContentManagement.AssetOrDirectReference<GameObject>
                {
                    address = null,
                    _address = null,
                    directRef = ghost,
                    _directRef = ghost,
                    loadOnAssigned = true
                };
            }
        }

        public static bool isLocalUserMonsterHunter
        {
            get
            {
                foreach (var player in PlayerCharacterMasterController.instances)
                {
                    if (player != null && player.networkUser != null)
                    {
                        GameObject bodyPrefab = BodyCatalog.GetBodyPrefab(player.networkUser.bodyIndexPreference);
                        if (bodyPrefab && bodyPrefab == Content.Survivors.MonsterHunter.characterPrefab && Util.HasEffectiveAuthority(player.gameObject)) return true;
                    }
                }
                return false;
            }
        }

        public static bool isMonsterHunterInPlay
        {
            get
            {
                foreach (var player in PlayerCharacterMasterController.instances)
                {
                    if (player != null && player.networkUser != null)
                    {
                        GameObject bodyPrefab = BodyCatalog.GetBodyPrefab(player.networkUser.bodyIndexPreference);
                        if (bodyPrefab)
                        {
                            if (bodyPrefab == Content.Survivors.MonsterHunter.characterPrefab) return true;
                        }
                    }
                }
                return false;
            }
        }

        public static CharacterBody getLocalUserBody
        {
            get
            {
                foreach (var player in PlayerCharacterMasterController.instances)
                {
                    if (player != null && player.networkUser != null && player.body)
                    {
                        return player.body;
                    }
                }
                return null;
            }
        }

        [Server]
        public static void CreateItemTakenOrb(Vector3 effectOrigin, GameObject targetObject, ItemIndex itemIndex)
        {
            if (!NetworkServer.active) return;

            GameObject effectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ItemTakenOrbEffect.prefab").WaitForCompletion();
            EffectData effectData = new EffectData
            {
                origin = effectOrigin,
                genericFloat = 0.25f,
                genericUInt = (uint)(itemIndex + 1)
            };
            effectData.SetChildLocatorTransformReference(targetObject, 0);
            EffectManager.SpawnEffect(effectPrefab, effectData, true);
        }

        internal static bool isSimulacrum
        {
            get
            {
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                return sceneName == "itancientloft" ||
                    sceneName == "itdampcave" ||
                    sceneName == "itfrozenwall" ||
                    sceneName == "itgolemplains" ||
                    sceneName == "itgoolake" ||
                    sceneName == "itmoon" ||
                    sceneName == "itskymeadow";
            }
        }

        public static Vector3 GetRandomNode()
        {
            NodeGraph groundNodes = SceneInfo.instance.groundNodes;

            bool cummed = false;
            int tries = 0;
            List<NodeGraph.NodeIndex> nodeList = groundNodes.FindNodesInRange(Vector3.zero, 0f, 2000f, HullMask.Human);
            while (!cummed)
            {
                tries++;
                System.Random random = new System.Random();
                NodeGraph.NodeIndex randomNode = nodeList[random.Next(nodeList.Count)];
                if (randomNode != null)
                {
                    Vector3 position;
                    groundNodes.GetNodePosition(randomNode, out position);
                    return position;
                }
                if (tries >= 1000) cummed = true;
            }

            return Vector3.zero;
        }

        public static Vector3 GetNearestNode(Vector3 startPosition, float startRadius, float maxRadius, bool groundOnly = true)
        {
            bool found = false;
            float radius = startRadius;
            while (!found)
            {
                if (groundOnly)
                {
                    System.Random random = new System.Random();
                    NodeGraph groundNodes = SceneInfo.instance.groundNodes;
                    List<NodeGraph.NodeIndex> nodeList = groundNodes.FindNodesInRange(startPosition, startRadius, radius, HullMask.Human);
                    if (nodeList.Count <= 0)
                    {
                        radius++;
                    }
                    else
                    {
                        NodeGraph.NodeIndex randomNode = nodeList[random.Next(nodeList.Count)];
                        if (randomNode != null)
                        {
                            Vector3 position;
                            groundNodes.GetNodePosition(randomNode, out position);
                            return position;
                        }
                    }

                    if (radius >= maxRadius)
                    {
                        break;
                    }
                }
                else
                {
                    System.Random random = new System.Random();
                    NodeGraph nodes = SceneInfo.instance.airNodes;
                    List<NodeGraph.NodeIndex> nodeList = nodes.FindNodesInRange(startPosition, startRadius, radius, HullMask.Human);
                    if (nodeList.Count <= 0)
                    {
                        radius++;
                    }
                    else
                    {
                        NodeGraph.NodeIndex randomNode = nodeList[random.Next(nodeList.Count)];
                        if (randomNode != null)
                        {
                            Vector3 position;
                            nodes.GetNodePosition(randomNode, out position);
                            return position;
                        }
                    }

                    if (radius >= maxRadius)
                    {
                        break;
                    }
                }
            }
            return startPosition;
        }

        internal static string ScepterDescription(string desc)
        {
            return "\n<color=#d299ff>SCEPTER: " + desc + "</color>";
        }
    }

    public static class IListExtensions
    {
        /// <summary>
        /// Shuffles the element order of the specified list.
        /// </summary>
        public static void Shuffle<T>(this System.Collections.Generic.IList<T> ts)
        {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = UnityEngine.Random.Range(i, count);
                var tmp = ts[i];
                ts[i] = ts[r];
                ts[r] = tmp;
            }
        }
    }
}