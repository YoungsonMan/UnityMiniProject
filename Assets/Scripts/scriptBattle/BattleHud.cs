using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;
    [SerializeField] HPBar hpBar;

    [Header("StatusColor")]
    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;

    Pokemon _pokeMonster;

    // 색깔dictionary에
    Dictionary<ConditionID, Color> statusColors;

    public void SetData(Pokemon pokemon)
    {
        _pokeMonster = pokemon;

        nameText.text = pokemon.pBase.Name;
        levelText.text = $"Lv. {pokemon.Level}"; // "Level" + pokemon.Level
        hpBar.SetHP((float) pokemon.curHP / pokemon.Hp);

        statusColors = new Dictionary<ConditionID, Color>()
        {
            { ConditionID.psn, psnColor },
            { ConditionID.brn, brnColor },
            { ConditionID.slp, slpColor },
            { ConditionID.par, parColor },
            { ConditionID.frz, frzColor }
        };


        SetStatusText();
        _pokeMonster.OnStatusChanged += SetStatusText; //상태이상 생기면 문자도같이
    }

    void SetStatusText()
    {
        if (_pokeMonster.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _pokeMonster.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_pokeMonster.Status.Id];    
        }
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
