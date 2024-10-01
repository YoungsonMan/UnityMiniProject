using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;

    Pokemon _pokeMonster;

    public void SetData(Pokemon pokemon)
    {
        _pokeMonster = pokemon;

        nameText.text = pokemon.pBase.Name;
        levelText.text = $"Lv. {pokemon.Level}"; // "Level" + pokemon.Level
        hpBar.SetHP((float)pokemon.curHP / pokemon.Hp);
    }
}
