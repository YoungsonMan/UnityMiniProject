using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy, PartyScreen } // Busy = 공격중

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleHud playerHud;

    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud enemyHud;

    [SerializeField] BattleDialogBox battleDialog;

    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver; // bool을 줘서 승패를 가릴수 있게

    BattleState state;
    int currentAction;      // 현제는 0 = Fight, 1 = Run 추후 아이템, 교체 같은거 추가하면 변경될예정
    int currentSkill;
    int currentMember;

    PokemonParty playerParty;
    Pokemon wildPokemon;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetUpBattle());
    }
    public void HandleUpdate()
    {
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PlayerMove)
        {
            HandleSkillSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentAction += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentAction -= 2;
        }
        // 스킬고르는거랑 비슷하지만 맥스갯수 안넘어가는법을 조금다르게
        // 이게 더 심플하고 깔끔.
        currentAction = Mathf.Clamp(currentAction, 0, 3);

        battleDialog.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                // Fight
                PlayerMove();
            }
            else if (currentAction == 1)
            {
                // Bag
            }
            else if (currentAction == 2)
            {
                // Pokemon
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                // Run
            }
        }
    }

    void HandleSkillSelection()
    {
        //     0   |   1
        //     2   |   3

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentSkill;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentSkill;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSkill += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSkill -= 2;
        }
        currentSkill = Mathf.Clamp(currentSkill, 0, playerUnit.Pokemon.Skills.Count - 1); //리스트가 0부터 시작하니까 -1

        battleDialog.UpdateSkillSelection(currentSkill, playerUnit.Pokemon.Skills[currentSkill]);

        // Z키로 선택
        if (Input.GetKeyDown(KeyCode.Z)) 
        {
            battleDialog.EnableSkillSelector(false);
            battleDialog.EnableDialogText(true);
            StartCoroutine(PerformPlayerSkill());
        }
        // X키로 취소
        else if (Input.GetKeyDown(KeyCode.X))
        {
            battleDialog.EnableSkillSelector(false);
            battleDialog.EnableDialogText(true);
            PlayerAction();
        }
    }


    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMember += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMember -= 2;
        }
        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Pokemons[currentMember];
            if (selectedMember.Hp <= 0)
            {
                partyScreen.SetMessageText("Pokemon is FAINTED");
                return;
            }
            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("Pokemon is ALREADY out");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            StartCoroutine(SwitchPokemon(selectedMember));
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            PlayerAction();
        }
    }
    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        yield return battleDialog.TypeDialog($"Come back {playerUnit.Pokemon.pBase.Name}");
        playerUnit.PlayFaintAnimation(); // 교체애니메이션을 새로해도 되지만(왼쪽으로빠진다던가..) 일단 기절모션 재탕
        yield return new WaitForSeconds(2f);

        // 기절했을때 다음포켓몬 나오는 코드 재탕
        playerUnit.Setup(newPokemon);
        playerHud.SetData(newPokemon);

        battleDialog.SetSkillNames(newPokemon.Skills);

        // 코루틴 완료될때까지 기다리고 완료되면실행
        yield return battleDialog.TypeDialog($"Go, {newPokemon.pBase.Name}!!! ");

        // 상대차례
        StartCoroutine(EnemyMove());

    }


    public IEnumerator SetUpBattle()
    {
        // Party에 살아있는 포켓몬 확인하고 소환
        playerUnit.Setup(playerParty.GetHealthyPokemon());
        playerHud.SetData(playerUnit.Pokemon);

        // 상대(야생포켓몬) 등장
        enemyUnit.Setup(wildPokemon);
        enemyHud.SetData(enemyUnit.Pokemon);

        // 파티스크린
        partyScreen.Init();


        // 스킬리스트
        battleDialog.SetSkillNames(playerUnit.Pokemon.Skills);

        // 코루틴 완료될때까지 기다리고 완료되면실행
        yield return battleDialog.TypeDialog($"A wild {enemyUnit.Pokemon.pBase.Name} appeared."); 

        PlayerAction();
    }

    public void PlayerAction()
    {
        state = BattleState.PlayerAction;
        battleDialog.SetDialog("Choose an action");
        battleDialog.EnableActionSelector(true);
    }


    public void OpenPartyScreen()
    {
        Debug.Log("LoadPartyScreen");
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    public void PlayerMove()
    {
        state = BattleState.PlayerMove;
        battleDialog.EnableActionSelector(false);
        battleDialog.EnableDialogText(false);
        battleDialog.EnableSkillSelector(true);
    }


    // 플레이어 ---공격---> 적
    IEnumerator PerformPlayerSkill()
    {
        // 플레이어가 계속 스킬선택할수있음으로, 상태를 Busy로
        state = BattleState.Busy;

        var skill = playerUnit.Pokemon.Skills[currentSkill];
        // 스킬 사용시 PP 감소
        skill.PP--;
        // 데미지 가하기
        yield return battleDialog.TypeDialog($"{playerUnit.Pokemon.pBase.Name} used {skill.Base.Name}");

        // 공격애니메이션
        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f); // 피깎이는시간 기달리기
        // 공격 => 상대방 깜빡(색바뀌기)
        enemyUnit.PlayHitAnimation();

        var damageDetails = enemyUnit.Pokemon.TakeDamage(skill, playerUnit.Pokemon);
        // 공격받은대상(적) 피통업데이트(HP - DMG)
        yield return enemyHud.UpdateHP();
        // 데미지효율 코루틴
        yield return ShowDamageDetails(damageDetails);

        // 데미지 받다가 죽음
        if (damageDetails.Fainted)
        {
            yield return battleDialog.TypeDialog($"{enemyUnit.Pokemon.pBase.Name} Fainted");
            enemyUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            OnBattleOver(true); //플레이어 승리
        }
        else  // 데미지 견디면 적 차례
        {
            StartCoroutine(EnemyMove());
        }
    }


    // 적 ---공격---> 플레이어
    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        // 일단 랜덤스킬 고르기
        var skill = enemyUnit.Pokemon.GetRandomSkill();
        // 스킬 사용시 PP 감소
        skill.PP--;
        //적 -> 플레이어, 데미지 가하기
        yield return battleDialog.TypeDialog($"{enemyUnit.Pokemon.pBase.Name} used {skill.Base.Name}");


        // 공격애니메이션
        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f); // 피깎이는시간 기달리기
        // 공격 => 상대방(player) 깜빡(색바뀌기)
        playerUnit.PlayHitAnimation();

        var damageDetails = playerUnit.Pokemon.TakeDamage(skill, enemyUnit.Pokemon);
        // 공격받은대상(플레이어) 피통업데이트(HP - DMG)
        yield return playerHud.UpdateHP();
        // 데미지효율 코루틴
        yield return ShowDamageDetails(damageDetails);

        // 데미지 받다가 죽음
        if (damageDetails.Fainted)
        {
            yield return battleDialog.TypeDialog($"{playerUnit.Pokemon.pBase.Name} Fainted");
            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);


            // 싸우던포켓몬 죽으면 살아있는 포켓몬있나 찾고있으면 내보내기
            var nextPokemon =  playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                playerUnit.Setup(nextPokemon);
                playerHud.SetData(nextPokemon);

                battleDialog.SetSkillNames(nextPokemon.Skills);

                // 코루틴 완료될때까지 기다리고 완료되면실행
                yield return battleDialog.TypeDialog($"Go REVENGE!!! {nextPokemon.pBase.Name}!!! ");

                PlayerAction();
            }
            else
            {
                OnBattleOver(false); //플레이어 패배
            }

        }
        else  // 데미지 견디면 적 차례
        {
            PlayerAction();
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            yield return battleDialog.TypeDialog("A CRITICAL HIT!!!");
        }
        if (damageDetails.TypeEffectiveness > 1f)
        {
            yield return battleDialog.TypeDialog("It was SUPER effective!");
        }
        else if (damageDetails.TypeEffectiveness < 1f)
        {
            yield return battleDialog.TypeDialog("It wasn't that effective...");
        }

    }

}
