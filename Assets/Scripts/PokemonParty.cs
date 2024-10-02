using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;


    public List<Pokemon> Pokemons
    {
        get  { return pokemons; } 
    }

    private void Start()
    {
        foreach (var pokemon in pokemons)
        {
            pokemon.Init();
        }
    }

    public Pokemon GetHealthyPokemon()
    {
        return pokemons.Where(x => x.curHP > 0).FirstOrDefault(); // Linq ���
    }
    public void AddPokemon(Pokemon newPokemon)
    {
        if (pokemons.Count < 6)
        {
            pokemons.Add(newPokemon);
        }
        else
        {
            // ���� �̼��� ��ǻ�ͷ� �����Ⱑ �ȵ�. ���ϸ�ڽ� �̱���. ���߿�...
        }
    }
}
