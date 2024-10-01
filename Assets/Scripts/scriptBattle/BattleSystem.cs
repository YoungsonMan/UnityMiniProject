using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, BattleOver } // Busy = 공격중

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;

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
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
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
        currentAction = Mathf.Clamp(currentAction, 0, 3);

        battleDialog.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                // Fight
                MoveSelection();
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
            StartCoroutine(PlayerMove());
        }
        // X키로 취소
        else if (Input.GetKeyDown(KeyCode.X))
        {
            battleDialog.EnableSkillSelector(false);
            battleDialog.EnableDialogText(true);
            ActionSelection();
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
            if (selectedMember.curHP <= 0) // 밑에 포켓몬 죽은다음에 다음포켓몬말고 선택하기 바꾸면서 이게 안됨
            {
                partyScreen.SetMessageText("Pokemon is FAINTED");
                return;
            }
            if (selectedMember == playerUnit.Pokemon) //이건잘되는데
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
            ActionSelection();
        }
    }

    // 포켓몬 교체|교대
    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        if (playerUnit.Pokemon.curHP > 0) // 살아있는 포켓몬만 교대
        {
            yield return battleDialog.TypeDialog($"Come back {playerUnit.Pokemon.pBase.Name}");
            playerUnit.PlayFaintAnimation(); // 교체애니메이션을 새로해도 되지만(왼쪽으로빠진다던가..) 일단 기절모션 재탕
            yield return new WaitForSeconds(2f);
        }

        // 기절했을때 다음포켓몬 나오는 코드 재탕
        playerUnit.Setup(newPokemon);

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

        // 상대(야생포켓몬) 등장
        enemyUnit.Setup(wildPokemon);

        // 파티스크린
        partyScreen.Init();


        // 스킬리스트
        battleDialog.SetSkillNames(playerUnit.Pokemon.Skills);

        // 코루틴 완료될때까지 기다리고 완료되면실행
        yield return battleDialog.TypeDialog($"A wild {enemyUnit.Pokemon.pBase.Name} appeared."); 

        ActionSelection();
    }

    public void BattleOver(bool won) // GameManager에서 배틀끝났나 알수있게
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver()); // Foreach, Link이용해 짧게 작성
        OnBattleOver(won);
    }

    public void ActionSelection()
    {
        state = BattleState.ActionSelection;
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

    public void MoveSelection()
    {
        state = BattleState.MoveSelection;
        battleDialog.EnableActionSelector(false);
        battleDialog.EnableDialogText(false);
        battleDialog.EnableSkillSelector(true);
    }


    // 플레이어 ---공격---> 적
    IEnumerator PlayerMove()
    {
        // 플레이어가 계속 스킬선택할수있음으로, 상태를 Busy로 PerformMove로 변경
        state = BattleState.PerformMove;
        var skill = playerUnit.Pokemon.Skills[currentSkill];
        yield return RunMove(playerUnit, enemyUnit, skill);

        // 배틀상태가 RunMove로 바뀌는거 아니면 진행
        if (state == BattleState.PerformMove)
        {
            StartCoroutine(EnemyMove());
        }
    }

    // 적 ---공격---> 플레이어
    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;
        // 일단 랜덤스킬 고르기
        var skill = enemyUnit.Pokemon.GetRandomSkill();
        yield return RunMove(enemyUnit, playerUnit, skill);

        // 배틀상태가 RunMove로 바뀌는거 아니면 진행
        if (state == BattleState.PerformMove)
        {
            ActionSelection();
        }
    }

    // 플레이어 공격 | 적 공격이 같은로직에 상반되는 방식이여서
    // 상태공격등 스킬의 변수들 더 추가하면 더 복잡해지므로 캡슐화
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Skill skill)
    {
        // 스킬 사용시 PP 감소
        skill.PP--;
        // 데미지 가하기
        yield return battleDialog.TypeDialog($"{sourceUnit.Pokemon.pBase.Name} used {skill.Base.Name}");

        // 공격애니메이션
        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f); // 피깎이는시간 기달리기
        // 공격 => 상대방 깜빡(색바뀌기)
        targetUnit.PlayHitAnimation();

        if (skill.Base.Category == SkillCategory.Status)
        {
            // 버프/디버프 대상확인
            var effects = skill.Base.Effects;
            if (effects.Boosts != null)
            {
                if (skill.Base.Target == SkillTarget.Self)
                {   // 본인
                    sourceUnit.Pokemon.ApplyBoosts(effects.Boosts); 
                }
                else
                {   // 상대방
                    targetUnit.Pokemon.ApplyBoosts(effects.Boosts);
                }
                yield return ShowStatusChanges(sourceUnit.Pokemon);
                yield return ShowStatusChanges(targetUnit.Pokemon);
            }
        }
        else
        {
            var damageDetails = targetUnit.Pokemon.TakeDamage(skill, sourceUnit.Pokemon);
            // 공격받은대상(적) 피통업데이트(HP - DMG)
            yield return targetUnit.Hud.UpdateHP();
            // 데미지효율 코루틴
            yield return ShowDamageDetails(damageDetails);

        }


        // 데미지 받다가 죽음
        if (targetUnit.Pokemon.curHP <= 0)
        {
            yield return battleDialog.TypeDialog($"{targetUnit.Pokemon.pBase.Name} Fainted");
            targetUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);

            CheckForBattleOver(targetUnit);

        }
    }

    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue(); 
            yield return battleDialog.TypeDialog(message);  
        } 

    }


    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        // 기절한게 플레이어인지 적인지구분
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else
            {
                // 없으면 플레이어 패배
                BattleOver(false);
            }
        }
        else
        {
            BattleOver(true);
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
