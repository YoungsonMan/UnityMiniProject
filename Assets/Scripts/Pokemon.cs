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

    public DamageDetails TakeDamage(Skill skill, Pokemon attacker)
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

        // Critical Ȯ�� stage(0~4)���� �ٸ� (�ϴ� 2���� �������� �����������)
        // 0 = 1/16, 1 = 1/8, 2 = 1/4, 3 = 1/3, 4 = 1/2
        // 6.25% => 12.5% => 25% => 50%
        float critical = 1f;
        if (Random.value * 100f <= 6.25f)
        {
            critical = 2f;
        }

        // Damage = (( ((2*Level/5)+2) * Power * (A/D) / 50) + 2) * Modifier
        float type = TypeChart.GetEffectiveness(skill.Base.Type, this.pBase.Type1) * TypeChart.GetEffectiveness(skill.Base.Type, this.pBase.Type2);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false
        };

        // Ư�����̽� ��ų������ (2���� ����, �ۼ������� �� Ư�����̽��ε�)
        // Ư����ġ�� �����̸� ���Ͻ����
        // Ư���� Ư���ġ�� �ﰨ
        float attack = (skill.Base.IsSpecial) ? attacker.SpecialAttack : attacker.Attack;
        float defense = (skill.Base.IsSpecial) ? SpecialDefense : Defense;


        // ���⼭�� �׳� ������ҷ� ��ġ�� ���µ� ���߿� ��ȸ�Ǹ� ����
        float modifiers = Random.Range(0.85f, 1f) * type * critical; 
        float a = (2 * attacker.Level + 10) / 250f; // ���߿� 50�����°� ���⼭ 250����.
        float d = a * skill.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);


        curHP -= damage;
        if (curHP <= 0)
        {
            // pokemon is fainted
            curHP = 0; // ���̳ʽ��� ü�� �Ȱ��� 0���� ����
            damageDetails.Fainted = true;
        }
        return damageDetails;
    }

    public Skill GetRandomSkill()
    {
        // �ϴ��� �����Լ��� ��ų���� ���ڻ������� ��ų����ϰ��ϰ�
        // ���߿� ��������(���� ����Ʈ�� ����������) ��� �����ϱ� 
        int r = Random.Range(0, Skills.Count);
        return Skills[r];
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical {  get; set; }
    public float TypeEffectiveness {  get; set; }
}
