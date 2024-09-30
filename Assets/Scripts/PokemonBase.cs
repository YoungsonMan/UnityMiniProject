using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new pokemon")]
public class PokemonBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;
    
    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    [SerializeField] int hp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int sAttack;
    [SerializeField] int sDefense;
    [SerializeField] int speed;

    [SerializeField] List<LearnableSkill> learnableSkills;

    public string Name
    {
        get { return name; }
    }
    public string Description
    {
        get { return description; }
    }
    public Sprite FrontSprite
    {
        get { return frontSprite; } 
    }
    public Sprite BackSprite
    {
        get { return backSprite; }
    }
    public PokemonType Type1
    {
        get { return type1; }
    }
    public PokemonType Type2
    {
        get { return type2; }
    }
    public int HP 
    {
        get { return hp; }
    }
    public int Attack
    {
        get { return attack; }
    }
    public int Defense
    {
        get { return defense; }
    }
    public int SpecialAttack
    {
        get { return sAttack; }
    }
    public int SpecialDefense
    {
        get { return sDefense; }
    }
    public int Speed
    {
        get { return speed; }
    }

    public List<LearnableSkill> LearnableSkills
    {
        get { return learnableSkills; }
    }
}


[System.Serializable]
public class LearnableSkill
{
    [SerializeField] SkillBase skillBase;
    [SerializeField] int level;

    public SkillBase Base
    {
        get { return skillBase; }
    }
 
    public int Level 
    {
        get { return level; } 
    }
}


public enum PokemonType
{
    Normal, Fire, Water, Electric, Grass, ice, Fighting, Posion, Ground, Flying, Psychic, Bug, Rock, Ghost, Dragon 
}
