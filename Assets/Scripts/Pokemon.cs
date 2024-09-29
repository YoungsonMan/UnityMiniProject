using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pokemon 
{
    PokemonBase pbase;
    int level;

    public Pokemon(PokemonBase pokeBase, int pokeLevel)
    {
        pbase = pokeBase;
        level = pokeLevel;
    }

    public int Hp
    {
        // 실제 포켓몬게임에 수치 정하는 공식.
        get { return Mathf.FloorToInt((pbase.HP * level) / 100f) + 10; }
    }
    public int Attack
    {
        get { return Mathf.FloorToInt((pbase.Attack * level) / 100f) + 5; }
    }
    public int Defense
    {
        get { return Mathf.FloorToInt((pbase.Defense * level) / 100f) + 5; }
    }
    public int SpecialAttack
    {
        get { return Mathf.FloorToInt((pbase.SpecialAttack * level) / 100f) + 5; }
    }
    public int SpecialDefense
    {
        get { return Mathf.FloorToInt((pbase.SpecialDefense * level) / 100f) + 5; }
    }
    public int Speed
    {
        get { return Mathf.FloorToInt((pbase.Speed * level) / 100f) + 5; }
    }

}
