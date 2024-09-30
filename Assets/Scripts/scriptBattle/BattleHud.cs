using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;

    public void SetData(Pokemon pokemon)
    {
        nameText.text = pokemon.pBase.Name;
        levelText.text = $"Level {pokemon.Level}";
        hpBar.SetHP((float) pokemon.curHP / pokemon.Hp);
    }


    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
