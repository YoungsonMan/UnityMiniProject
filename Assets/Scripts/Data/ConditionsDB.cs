using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{

    public static void Init() //반복작업하다 실수 하지않게 그냥 init
    {
        foreach (var kvp in Conditions) // kvp = KeyValuePass
        {
            var conditonId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditonId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn, new Condition()
            {
                Name = "Poison", 
                StartMessage = "has been poisoned",
                // lambda function 이용
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHP(pokemon.Hp / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.pBase.Name} hurt itself due to poison.");
                }
            }
        },
        {
            ConditionID.brn, new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHP(pokemon.Hp / 16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.pBase.Name} hurt itself due to burn.");
                }
            }
        },
        {
            ConditionID.par, new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "has been paralyzed",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(Random.Range( 1, 5 ) == 1 ) // (1/4) 확률로 못움직임
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.pBase.Name} is paralyzed and UNABLE to move.");
                        return false;
                    }
                    return true;
                }
            }
        },
        {
            ConditionID.frz, new Condition()
            {
                Name = "Freeze",
                StartMessage = "has been frozen",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(Random.Range( 1, 5 ) == 1 ) // (1/4) 확률로 해동됨
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.pBase.Name} is not frozen anymore.");
                        return true;
                    }
                    return false;
                }
            }
        },
        {
            ConditionID.slp, new Condition()
            {
                Name = "Sleep",
                StartMessage = "has fallen asleep",
                // 최면은 바로 걸리니까
                OnStart = (Pokemon pokemon) =>
                {
                    // 1-3턴 동안 잠들어 못움직임
                    pokemon.StatusTime = Random.Range( 1, 4 );
                    Debug.Log($"Will be asleep for {pokemon.StatusTime} moves.");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.StatusTime <= 0)
                    {   // 걸린 턴 다 지나면 일어나기 
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.pBase.Name} WOKE UP! ");
                        return true;
                    }

                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.pBase.Name} is SLEEPING.");
                    return false;
                }
            }
        }
    };
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz
}
