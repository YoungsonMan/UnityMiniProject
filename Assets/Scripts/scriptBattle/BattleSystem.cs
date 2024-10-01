using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy } // Busy = 공격중

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleHud playerHud;

    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud enemyHud;

    [SerializeField] BattleDialogBox battleDialog;

    BattleState state;
    int currentAction;      // 현제는 0 = Fight, 1 = Run 추후 아이템, 교체 같은거 추가하면 변경될예정
    int currentSkill;

    private void Start()
    {
        StartCoroutine(SetUpBattle());
    }
    private void Update()
    {
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PlayerMove)
        {
            HandleSkillSelection();
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if(currentAction < 1)
                ++ currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 0) 
                -- currentAction;
        }
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
            if (currentSkill < playerUnit.Pokemon.Skills.Count - 1)
                ++currentSkill;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentSkill > 0)
                --currentSkill;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentSkill < playerUnit.Pokemon.Skills.Count - 2)
                currentSkill += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentSkill > 1)
                currentSkill -= 2;
        }

        battleDialog.UpdateSkillSelection(currentSkill, playerUnit.Pokemon.Skills[currentSkill]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            battleDialog.EnableSkillSelector(false);
            battleDialog.EnableDialogText(true);
            StartCoroutine(PerformPlayerSkill());
        }

    }


    public IEnumerator SetUpBattle()
    {
        playerUnit.Setup();
        playerHud.SetData(playerUnit.Pokemon);

        enemyUnit.Setup();
        enemyHud.SetData(enemyUnit.Pokemon);

        battleDialog.SetSkillNames(playerUnit.Pokemon.Skills);

        // 코루틴 완료될때까지 기다리고 완료되면실행
        yield return battleDialog.TypeDialog($"A wild {enemyUnit.Pokemon.pBase.Name} appeared."); 

        PlayerAction();
    }

    public void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(battleDialog.TypeDialog("Choose an action"));
        battleDialog.EnableActionSelector(true);
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
