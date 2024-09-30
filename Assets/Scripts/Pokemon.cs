using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

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

    public bool TakeDamage(Skill skill, Pokemon attacker)
    {
        /* DAMAGE ALGORITHM
         * https://bulbapedia.bulbagarden.net/wiki/Damage#Damage_calculation
         * DamageCalculation provided from the Bulbapedia(PokemonWikiPedia)
         *  Damage = ((((2*Level/5) + 2) * Power * (A/D) / 50) + 2) * Modifier
         *  Level  = Level of attacking Pokemon
         *  A      = [Attacker's] ATTACK STAT, physical => ATTACK / special => SPEICIAL ATTACK
         *  D      = [Target's] DEFFENSE STAT, physical => DEFENSE / speical => SPEICAL DEFENSE
         *  Power  = Effective Power of the used move (기술에 정해진 위력)
         *  Modifier 같은 경우는 세대마다 아이템, 스킬 추가로 조금씩, 여러가지 점점 많이 추가되있다.
         *  참고영상에서는 일단 이렇게 나와있는데 웹사이트 가니 지금은 더 최신화되어서 조금다르다.
         *  Modifier = Targets * Weather * Badge * Critical * random * STAB * Type * Burn * other
         *  target 이 하나보다 많으면 0.75, 아니면 1
         *  Weather = 1.5 WaterType & Rain | FireType & HarshSunLight
                   0.5 WaterType & HarshSunLight | FireType & Rain
         
            일단 2세대 공식으로...
            Damage = ((((2*Level/5) + 2) * Power * (A/D) / 50) * Item * Critical + 2) * Modifier
            Modifier => TK, Weather, Badge, STAB, Type, MoveMod, random, DoubleDmg
            Item = 1.1, 효과올려주는 아이템 갖고있으면. (Ex. Magnet, ElectricType)
            TK = 1, 2, 3 TripleKick 기술
            STAB = 1.5, 자속보정 => 기술타입 == 기술쓰는 포켓몬 타입 (Ex. 파이리(불타입) 불속성공격)
                   자속보정 능력이있는 포켓몬 한정. 없으면 그냥 1
         
            일단 참고영상에서 나온거쓰고 추후 수정하기.
          */

        // Damage = (( ((2*Level/5)+2) * Power * (A/D) / 50) + 2) * Modifier
        float modifiers = Random.Range(0.85f, 1f); // 여기서도 그냥 랜덤요소로 퉁치고 가는데 나중에 기회되면 수정
        float a = (2 * attacker.Level + 10) / 250f; // 나중에 50나누는걸 여기서 250으로.
        float d = a * skill.Base.Power * ((float)attacker.Attack / Defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        curHP -= damage;
        if (curHP <= 0)
        {
            // pokemon is fainted
            curHP = 0; // 마이너스로 체력 안가게 0으로 설정
            return true;
        }
        return false;
    }

    public Skill GetRandomSkill()
    {
        // 일단은 랜덤함수로 스킬갯수 숫자생성으로 스킬사용하게하고
        // 나중에 공격패턴(좀더 스마트한 공격을위한) 방식 구현하기 
        int r = Random.Range(0, Skills.Count);
        return Skills[r];
    }
}
