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


        // ��ų����
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
        // ���� ���ϸ���ӿ� ��ġ ���ϴ� ����.
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
         *  Power  = Effective Power of the used move (����� ������ ����)
         *  Modifier ���� ���� ���븶�� ������, ��ų �߰��� ���ݾ�, �������� ���� ���� �߰����ִ�.
         *  �����󿡼��� �ϴ� �̷��� �����ִµ� ������Ʈ ���� ������ �� �ֽ�ȭ�Ǿ ���ݴٸ���.
         *  Modifier = Targets * Weather * Badge * Critical * random * STAB * Type * Burn * other
         *  target �� �ϳ����� ������ 0.75, �ƴϸ� 1
         *  Weather = 1.5 WaterType & Rain | FireType & HarshSunLight
                   0.5 WaterType & HarshSunLight | FireType & Rain
         
            �ϴ� 2���� ��������...
            Damage = ((((2*Level/5) + 2) * Power * (A/D) / 50) * Item * Critical + 2) * Modifier
            Modifier => TK, Weather, Badge, STAB, Type, MoveMod, random, DoubleDmg
            Item = 1.1, ȿ���÷��ִ� ������ ����������. (Ex. Magnet, ElectricType)
            TK = 1, 2, 3 TripleKick ���
            STAB = 1.5, �ڼӺ��� => ���Ÿ�� == ������� ���ϸ� Ÿ�� (Ex. ���̸�(��Ÿ��) �ҼӼ�����)
                   �ڼӺ��� �ɷ����ִ� ���ϸ� ����. ������ �׳� 1
         
            �ϴ� �����󿡼� ���°ž��� ���� �����ϱ�.
          */

        // Damage = (( ((2*Level/5)+2) * Power * (A/D) / 50) + 2) * Modifier
        float modifiers = Random.Range(0.85f, 1f); // ���⼭�� �׳� ������ҷ� ��ġ�� ���µ� ���߿� ��ȸ�Ǹ� ����
        float a = (2 * attacker.Level + 10) / 250f; // ���߿� 50�����°� ���⼭ 250����.
        float d = a * skill.Base.Power * ((float)attacker.Attack / Defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        curHP -= damage;
        if (curHP <= 0)
        {
            // pokemon is fainted
            curHP = 0; // ���̳ʽ��� ü�� �Ȱ��� 0���� ����
            return true;
        }
        return false;
    }

    public Skill GetRandomSkill()
    {
        // �ϴ��� �����Լ��� ��ų���� ���ڻ������� ��ų����ϰ��ϰ�
        // ���߿� ��������(���� ����Ʈ�� ����������) ��� �����ϱ� 
        int r = Random.Range(0, Skills.Count);
        return Skills[r];
    }
}
