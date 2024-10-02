using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "pokeMove", menuName = "Pokemon/Create new MOVE")]
public class SkillBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;
    [SerializeField] PokemonType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pp;
    [SerializeField] SkillCategory category;
    [SerializeField] SkillEffects effects;
    [SerializeField] SkillTarget target;

    public string Name
    {
        get { return name; }
    }
    public string Description
    {
        get { return description; }
    }
    public PokemonType Type
    {
        get { return type; }
    }
    public int Power
    {
        get { return power; }
    }
    public int Accuracy
    {
        get { return accuracy; }
    }
    public int PP
    {
        get { return pp; }
    }

    public SkillCategory Category
    {
        get { return category; }
    }
    public SkillEffects Effects
    {
        get { return effects; }
    }
    public SkillTarget Target
    {
        get { return target; }
    }

}

[System.Serializable]
public class SkillEffects
{
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID volatileStatus;

    public List<StatBoost> Boosts
    {
        get { return boosts; }
    }
    public ConditionID Status
    {
        get { return status; }
    }
    public ConditionID VolatileStatus
    {
        get { return volatileStatus; }
    }
}

[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}

public enum SkillCategory
{
    Physical, Special, Status
}

public enum SkillTarget
{
    Foe, Self
}

