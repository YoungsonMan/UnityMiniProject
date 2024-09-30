using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pokemon 
{
    public PokemonBase pBase { get; set; }
    public int Level { get; set; }

    public int curHP {  get; set; }    

    public List<Skill> Skills {  get; set; }
    public Pokemon(PokemonBase pokeBase, int pokeLevel)
    {
        pBase = pokeBase;
        Level = pokeLevel;
        curHP = Hp;


        // 스킬생성
        Skills = new List<Skill>();
        foreach (var skill in pBase.LearnableSkills)
        {
            if (skill.Level <= Level)
                Skills.Add(new Skill(skill.Base));
            if (Skills.Count >= 4)
                break;
        }
    }

    public int Hp
    {
        // 실제 포켓몬게임에 수치 정하는 공식.
        get { return Mathf.FloorToInt((pBase.HP * Level) / 100f) + 10; }
    }
    public int Attack
    {
        get { return Mathf.FloorToInt((pBase.Attack * Level) / 100f) + 5; }
    }
    public int Defense
    {
        get { return Mathf.FloorToInt((pBase.Defense * Level) / 100f) + 5; }
    }
    public int SpecialAttack
    {
        get { return Mathf.FloorToInt((pBase.SpecialAttack * Level) / 100f) + 5; }
    }
    public int SpecialDefense
    {
        get { return Mathf.FloorToInt((pBase.SpecialDefense * Level) / 100f) + 5; }
    }
    public int Speed
    {
        get { return Mathf.FloorToInt((pBase.Speed * Level) / 100f) + 5; }
    }

}
