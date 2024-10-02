using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{

    public static void Init() //�ݺ��۾��ϴ� �Ǽ� �����ʰ� �׳� init
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
                // lambda function �̿�
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
                    if(Random.Range( 1, 5 ) == 1 ) // (1/4) Ȯ���� ��������
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
                    if(Random.Range( 1, 5 ) == 1 ) // (1/4) Ȯ���� �ص���
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
                // �ָ��� �ٷ� �ɸ��ϱ�
                OnStart = (Pokemon pokemon) =>
                {
                    // 1-3�� ���� ���� ��������
                    pokemon.StatusTime = Random.Range( 1, 4 );
                    Debug.Log($"Will be asleep for {pokemon.StatusTime} moves.");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.StatusTime <= 0)
                    {   // �ɸ� �� �� ������ �Ͼ�� 
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.pBase.Name} WOKE UP! ");
                        return true;
                    }

                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.pBase.Name} is SLEEPING.");
                    return false;
                }
            }
        },
        {   // Volatile Status, ��Ʋ������ ��������ȿ�� (ȥ��, ������, Ǯ����)
            ConditionID.confusion, new Condition()
            {
                Name = "Confusion",
                StartMessage = "has been confused",
                // �ָ��� �ٷ� �ɸ��ϱ�
                OnStart = (Pokemon pokemon) =>
                {
                    // 1-4�� ���� ȥ��, Ȯ���� ������ ����
                    pokemon.VolatileStatusTime = Random.Range( 1, 5 );
                    Debug.Log($"Will be confused for {pokemon.VolatileStatusTime} moves.");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.VolatileStatusTime <= 0)
                    {   // �ɸ� �� �� ������ �Ͼ�� 
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.pBase.Name} GOT OUT from confusion! ");
                        return true;
                    }

                    pokemon.VolatileStatusTime--;
                    // 50% Ȯ���� ��ų���� (7������� 33%�����)
                    if(Random.Range( 1, 3 ) == 1)
                    {
                        return true;
                    }
                    // ������ Ȯ�� ���� "������ �� ä �ڽ��� �����ߴ�!"
                    pokemon.StatusChanges.Enqueue($"{pokemon.pBase.Name} is CONFUSED.");
                    pokemon.UpdateHP(pokemon.Hp / 8);
                    pokemon.StatusChanges.Enqueue($"It HURT ITSELF due to confusion.");
                    return false;
                }
            }
        }
    };
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz, confusion
}
