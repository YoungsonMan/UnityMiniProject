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
    
    [SerializeField] Sprite type1;
    [SerializeField] Sprite type2;

    [SerializeField] int hp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int sAttack;
    [SerializeField] int sDefense;
    [SerializeField] int speed;
}

public enum PokemonType
{
    Normal, Fire, Water, Electric, Grass, ice, Fighting, Posion, Ground, Flying, Psychic, Bug, Rock, Ghost, Dragon 
}
