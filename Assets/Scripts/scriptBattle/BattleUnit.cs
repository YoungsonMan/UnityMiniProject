using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] PokemonBase pBase;
    [SerializeField] int level;
    [SerializeField] bool isPlayerUnit;

    public Pokemon Pokemon {  get; set; }

    public void Setup()
    {
        Pokemon = new Pokemon(pBase, level);
        if (isPlayerUnit)
            GetComponent<Image>().sprite = Pokemon.pBase.BackSprite;
        else
            GetComponent<Image>().sprite = Pokemon.pBase.FrontSprite;
    }
}
