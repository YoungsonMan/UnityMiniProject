using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy } // Busy = ������

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleHud playerHud;

    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud enemyHud;

    [SerializeField] BattleDialogBox battleDialog;

    BattleState state;
    int currentAction;      // ������ 0 = Fight, 1 = Run ���� ������, ��ü ������ �߰��ϸ� ����ɿ���
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

        // �ڷ�ƾ �Ϸ�ɶ����� ��ٸ��� �Ϸ�Ǹ����
        yield return battleDialog.TypeDialog($"A wild {enemyUnit.Pokemon.pBase.Name} appeared."); 
        yield return new WaitForSeconds(1f);

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


    // �÷��̾� ---����---> ��
    IEnumerator PerformPlayerSkill()
    {
        // �÷��̾ ��� ��ų�����Ҽ���������, ���¸� Busy��
        state = BattleState.Busy;

        var skill = playerUnit.Pokemon.Skills[currentSkill];
        // ������ ���ϱ�
        yield return battleDialog.TypeDialog($"{playerUnit.Pokemon.pBase.Name} used {skill.Base.Name}");

        yield return new WaitForSeconds(1f);

        bool isFainted = enemyUnit.Pokemon.TakeDamage(skill, playerUnit.Pokemon);
        // ���ݹ������(��) ���������Ʈ(HP - DMG)
        yield return enemyHud.UpdateHP();

        // ������ �޴ٰ� ����
        if (isFainted)
        {
            yield return battleDialog.TypeDialog($"{enemyUnit.Pokemon.pBase.Name} Fainted");
        }
        else  // ������ �ߵ�� �� ����
        {
            StartCoroutine(EnemyMove());
        }
    }


    // �� ---����---> �÷��̾�
    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        var skill = enemyUnit.Pokemon.GetRandomSkill();
        yield return battleDialog.TypeDialog($"{enemyUnit.Pokemon.pBase.Name} used {skill.Base.Name}");

        yield return new WaitForSeconds(1f);

        bool isFainted = playerUnit.Pokemon.TakeDamage(skill, enemyUnit.Pokemon);
        // ���ݹ������(�÷��̾�) ���������Ʈ(HP - DMG)
        yield return playerHud.UpdateHP();

        // ������ �޴ٰ� ����
        if (isFainted)
        {
            yield return battleDialog.TypeDialog($"{playerUnit.Pokemon.pBase.Name} Fainted");
        }
        else  // ������ �ߵ�� �� ����
        {
            PlayerAction();
        }
    }

}
