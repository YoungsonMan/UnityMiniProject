using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BattleHud : MonoBehaviour
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
        hpBar.SetHP((float) pokemon.curHP / pokemon.Hp);
    }

    public IEnumerator UpdateHP()
    {
        if (_pokeMonster.HpChanged)
        {
            yield return hpBar.SetHPSmoothly((float)_pokeMonster.curHP / _pokeMonster.Hp); 
            _pokeMonster.HpChanged = false;
        }
    }

}
