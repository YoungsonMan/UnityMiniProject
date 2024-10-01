using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{



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
                // lambda function 이용
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHP(pokemon.Hp / 16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.pBase.Name} hurt itself due to burn.");
                }
            }
        }
    };
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz
}
