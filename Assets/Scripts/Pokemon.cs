using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;



[System.Serializable] //�ν����Ϳ����� ���̰�
public class Pokemon 
{
    [SerializeField] PokemonBase pokeBase;
    [SerializeField] int level;

    

    public PokemonBase pBase
    {
        get { return pokeBase; }
    }
    public int Level
    {
        get { return level; }
    }
    public int curHP {  get; set; }    

    public List<Skill> Skills {  get; set; }

    public Dictionary<Stat, int> Stats { get; private set; }

    public void Init() // �굵 ���� �̴ϼȶ����� Initialization
    {

        

        // ��ų����
        Skills = new List<Skill>();
        foreach (var skill in pBase.LearnableSkills)
        {
            if (skill.Level <= Level)
                Skills.Add(new Skill(skill.Base));
            if (Skills.Count >= 4)
                break;
        }
        CalculateStats();
        curHP = Hp;
    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((pBase.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((pBase.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpecialAttack, Mathf.FloorToInt((pBase.SpecialAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpecialDefense, Mathf.FloorToInt((pBase.SpecialDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((pBase.Speed * Level) / 100f) + 5);

        Hp = Mathf.FloorToInt((pBase.HP * Level) / 100f) + 10;
    }



    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        // stat�� +- �����ִ� ����/������� ��ų ���������ϱ� �̷�������

        return statVal;
    }
    // Dictionary������� ��� �����ϰ��ϱ�

    public int Hp { get; private set; }
    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }
    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }
    public int SpecialAttack
    {
        get { return GetStat(Stat.SpecialAttack); }
    }
    public int SpecialDefense
    {
        get { return GetStat(Stat.SpecialDefense); }
    }
    public int Speed
    {
        get { return GetStat(Stat.Speed); }
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
