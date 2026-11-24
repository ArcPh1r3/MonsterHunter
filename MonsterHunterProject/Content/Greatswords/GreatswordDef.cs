using UnityEngine;
using RoR2;
using RoR2.Skills;

[CreateAssetMenu(fileName = "gs", menuName = "ScriptableObjects/GreatswordDef", order = 1)]
public class GreatswordDef : ScriptableObject
{
    [Header("General")]
    public string nameToken = "";
    public string descriptionToken = "";
    public Sprite icon = null;

    [Header("Properties")]
    public SkillDef skillDef = null;
    public float power = 1f;
    public float attackSpeed = 1f;
    public float chargeSpeed = 1f;

    [Header("Visuals")]
    public Mesh mesh = null;
    public Material material = null;
    public GameObject swingEffectOverride = null;
    public GameObject impactEffectOverride = null;
    public MonsterHunterSoundOverride[] soundStringOverrides = new MonsterHunterSoundOverride[0];

    [HideInInspector]
    public ushort index; // assigned at runtime

    public static GreatswordDef CreateGreatswordDefFromInfo(GreatswordDefInfo greatswordDefInfo)
    {
        GreatswordDef weaponDef = (GreatswordDef)ScriptableObject.CreateInstance(typeof(GreatswordDef));
        weaponDef.name = greatswordDefInfo.nameToken;

        weaponDef.nameToken = greatswordDefInfo.nameToken;
        weaponDef.descriptionToken = greatswordDefInfo.descriptionToken;
        weaponDef.icon = greatswordDefInfo.icon;

        weaponDef.skillDef = greatswordDefInfo.skillDef;
        weaponDef.power = greatswordDefInfo.power;
        weaponDef.attackSpeed = greatswordDefInfo.attackSpeed;
        weaponDef.chargeSpeed = greatswordDefInfo.chargeSpeed;

        weaponDef.mesh = greatswordDefInfo.mesh;
        weaponDef.material = greatswordDefInfo.material;

        return weaponDef;
    }
}

[System.Serializable]
public struct GreatswordDefInfo
{
    public string nameToken;
    public string descriptionToken;
    public Sprite icon;

    public SkillDef skillDef;
    public float power;
    public float attackSpeed;
    public float chargeSpeed;

    public Mesh mesh;
    public Material material;
}

[System.Serializable]
public struct MonsterHunterSoundOverride
{
    public string originalSoundString;
    public string newSoundString;
}