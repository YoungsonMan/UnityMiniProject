using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;



[System.Serializable] //인스펙터에서만 보이게
public class Pokemon 
{
    [SerializeField] PokemonBase pokeBase;
    [SerializeField] int level;

    public Pokemon(PokemonBase _base, int pLevel)
    {
        pokeBase = _base;
        level = pLevel;

        Init();
    }

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
    public Skill CurrnetSkill { get; set; }

    public Dictionary<Stat, int> Stats { get; private set; }

    // 스탯변동 (+-6단계 있다고함)
    public Dictionary<Stat, int> StatBoosts { get; private set; }

    public Condition Status { get; private set; }
    public int StatusTime { get; set;}

    public Condition VolatileStatus { get; private set; }
    public int VolatileStatusTime { get; set; } 

    public Queue<string> StatusChanges { get; private set; }
    public bool HpChanged { get; set; }
    public event System.Action OnStatusChanged;

    public void Init() // 얘도 이제 이니셜라이즈 Initialization
    {

        // 스킬생성
        Skills = new List<Skill>();
        foreach (var skill in pBase.LearnableSkills)
        {
            if (skill.Level <= Level)                   // 스킬 배우는 레벨보다 높으면 추가
                Skills.Add(new Skill(skill.Base));
            if (Skills.Count >= 4)
                break;
        }
        CalculateStats();
        curHP = Hp;

        StatusChanges = new Queue<string>();

        ResetStatBoost();
        Status = null;
        VolatileStatus = null; 

    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((pBase.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((pBase.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpecialAttack, Mathf.FloorToInt((pBase.SpecialAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpecialDefense, Mathf.FloorToInt((pBase.SpecialDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((pBase.Speed * Level) / 100f) + 5);

        Hp = Mathf.FloorToInt((pBase.HP * Level) / 100f) + 10 + Level;
    }

    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0 },
            {Stat.Defense, 0 },
            {Stat.SpecialAttack, 0 },
            {Stat.SpecialDefense, 0 },
            {Stat.Speed, 0 },
        }; 
    }

    // stat에 +- 영향주는 버프/디버프형 스킬 복잡해지니까 이런식으로
    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        // 스탯부스트 된거 적용
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };
        // 스탯 변경하는데 6단계 가있다했는데 위에 숫자 순서대로 버프는곱하고 디버프는 나누기
        // Ex. 단단해지기 하면 방어력 하나씩 올라가는거
        if (boost >= 0)
        {
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        }
        else
        {
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);
        }

        return statVal;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost > 0)
            {
                StatusChanges.Enqueue($"{pBase.Name}'s {stat} ROSE!");
            }
            else
            {
                StatusChanges.Enqueue($"{pBase.Name}'s {stat} FELL!");
            }

            Debug.Log($"{stat} has been boosted to {StatBoosts[stat]}");
        }
    }


    // Dictionary사용으로 계속 계산안하게하기
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

        // Critical 확률 stage(0~4)마다 다름 (일단 2세대 기준으로 만들고있으니)
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

        // 특공베이스 스킬데미지 (2세대 기준, 송성공격은 다 특공베이스로들어감)
        // 특공수치의 공격이면 리턴스페셜
        // 특공은 특방수치로 댐감
        float attack = (skill.Base.Category == SkillCategory.Special) ? attacker.SpecialAttack : attacker.Attack;
        float defense = (skill.Base.Category == SkillCategory.Special) ? SpecialDefense : Defense;


        // 여기서도 그냥 랜덤요소로 퉁치고 가는데 나중에 기회되면 수정
        float modifiers = Random.Range(0.85f, 1f) * type * critical; 
        float a = (2 * attacker.Level + 10) / 250f; // 나중에 50나누는걸 여기서 250으로.
        float d = a * skill.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);


        UpdateHP(damage);

        return damageDetails;
    }
    public void UpdateHP(int damage)
    {
        curHP = Mathf.Clamp(curHP - damage, 0, Hp);
        HpChanged = true;
    }
    public void SetStatus(ConditionID conditionId)
    {
        if (Status != null)
        {
            return;
        }
        Status = ConditionsDB.Conditions[conditionId];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{pBase.Name} {Status.StartMessage}");

        OnStatusChanged?.Invoke(); 
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    public void SetVolatileStatus(ConditionID conditionId)
    {
        if (VolatileStatus != null)
        {
            return;
        }
        VolatileStatus = ConditionsDB.Conditions[conditionId];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{pBase.Name} {VolatileStatus.StartMessage}");
    }

    // VolatileStatus는 배틀끝나면 회복되는 (사실상 confusion만 아니고...)
    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }


    public Skill GetRandomSkill()
    {
        // 일단은 랜덤함수로 스킬갯수 숫자생성으로 스킬사용하게하고
        // 나중에 공격패턴(좀더 스마트한 공격을위한) 방식 구현하기 
        int r = Random.Range(0, Skills.Count);
        return Skills[r];
    }
    public bool OnBeforeMove()
    {
        bool canPerformMove = true;
        if (Status?.OnBeforeMove != null)
        {
            if(!Status.OnBeforeMove(this))
            {
                canPerformMove = false;
            } 
        }
        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }

        return canPerformMove  ;
    }
    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this); //sleep 이나 바로 효과나오는거 대비
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoost();
        Debug.Log("배틀시 스탯변경 초기화");
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical {  get; set; }
    public float TypeEffectiveness {  get; set; }
}
