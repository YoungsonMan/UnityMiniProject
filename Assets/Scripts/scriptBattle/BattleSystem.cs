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

        // ���ݾִϸ��̼�
        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f); // �Ǳ��̴½ð� ��޸���
        // ���� => ���� ����(���ٲ��)
        enemyUnit.PlayHitAnimation();

        var damageDetails = enemyUnit.Pokemon.TakeDamage(skill, playerUnit.Pokemon);
        // ���ݹ������(��) ���������Ʈ(HP - DMG)
        yield return enemyHud.UpdateHP();
        // ������ȿ�� �ڷ�ƾ
        yield return ShowDamageDetails(damageDetails);

        // ������ �޴ٰ� ����
        if (damageDetails.Fainted)
        {
            yield return battleDialog.TypeDialog($"{enemyUnit.Pokemon.pBase.Name} Fainted");
            enemyUnit.PlayFaintAnimation();
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

        // �ϴ� ������ų ����
        var skill = enemyUnit.Pokemon.GetRandomSkill();

        //�� -> �÷��̾�, ������ ���ϱ�
        yield return battleDialog.TypeDialog($"{enemyUnit.Pokemon.pBase.Name} used {skill.Base.Name}");


        // ���ݾִϸ��̼�
        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f); // �Ǳ��̴½ð� ��޸���
        // ���� => ����(player) ����(���ٲ��)
        playerUnit.PlayHitAnimation();

        var damageDetails = playerUnit.Pokemon.TakeDamage(skill, enemyUnit.Pokemon);
        // ���ݹ������(�÷��̾�) ���������Ʈ(HP - DMG)
        yield return playerHud.UpdateHP();
        // ������ȿ�� �ڷ�ƾ
        yield return ShowDamageDetails(damageDetails);

        // ������ �޴ٰ� ����
        if (damageDetails.Fainted)
        {
            yield return battleDialog.TypeDialog($"{playerUnit.Pokemon.pBase.Name} Fainted");
            playerUnit.PlayFaintAnimation();
        }
        else  // ������ �ߵ�� �� ����
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
