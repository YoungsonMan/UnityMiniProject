using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy, PartyScreen } // Busy = ������

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleHud playerHud;

    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud enemyHud;

    [SerializeField] BattleDialogBox battleDialog;

    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver; // bool�� �༭ ���и� ������ �ְ�

    BattleState state;
    int currentAction;      // ������ 0 = Fight, 1 = Run ���� ������, ��ü ������ �߰��ϸ� ����ɿ���
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
        // ��ų���°Ŷ� ��������� �ƽ����� �ȳѾ�¹��� ���ݴٸ���
        // �̰� �� �����ϰ� ���.
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
        currentSkill = Mathf.Clamp(currentSkill, 0, playerUnit.Pokemon.Skills.Count - 1); //����Ʈ�� 0���� �����ϴϱ� -1

        battleDialog.UpdateSkillSelection(currentSkill, playerUnit.Pokemon.Skills[currentSkill]);

        // ZŰ�� ����
        if (Input.GetKeyDown(KeyCode.Z)) 
        {
            battleDialog.EnableSkillSelector(false);
            battleDialog.EnableDialogText(true);
            StartCoroutine(PerformPlayerSkill());
        }
        // XŰ�� ���
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
        playerUnit.PlayFaintAnimation(); // ��ü�ִϸ��̼��� �����ص� ������(�������κ����ٴ���..) �ϴ� ������� ����
        yield return new WaitForSeconds(2f);

        // ���������� �������ϸ� ������ �ڵ� ����
        playerUnit.Setup(newPokemon);
        playerHud.SetData(newPokemon);

        battleDialog.SetSkillNames(newPokemon.Skills);

        // �ڷ�ƾ �Ϸ�ɶ����� ��ٸ��� �Ϸ�Ǹ����
        yield return battleDialog.TypeDialog($"Go, {newPokemon.pBase.Name}!!! ");

        // �������
        StartCoroutine(EnemyMove());

    }


    public IEnumerator SetUpBattle()
    {
        // Party�� ����ִ� ���ϸ� Ȯ���ϰ� ��ȯ
        playerUnit.Setup(playerParty.GetHealthyPokemon());
        playerHud.SetData(playerUnit.Pokemon);

        // ���(�߻����ϸ�) ����
        enemyUnit.Setup(wildPokemon);
        enemyHud.SetData(enemyUnit.Pokemon);

        // ��Ƽ��ũ��
        partyScreen.Init();


        // ��ų����Ʈ
        battleDialog.SetSkillNames(playerUnit.Pokemon.Skills);

        // �ڷ�ƾ �Ϸ�ɶ����� ��ٸ��� �Ϸ�Ǹ����
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


    // �÷��̾� ---����---> ��
    IEnumerator PerformPlayerSkill()
    {
        // �÷��̾ ��� ��ų�����Ҽ���������, ���¸� Busy��
        state = BattleState.Busy;

        var skill = playerUnit.Pokemon.Skills[currentSkill];
        // ��ų ���� PP ����
        skill.PP--;
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

            yield return new WaitForSeconds(2f);
            OnBattleOver(true); //�÷��̾� �¸�
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
        // ��ų ���� PP ����
        skill.PP--;
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

            yield return new WaitForSeconds(2f);


            // �ο�����ϸ� ������ ����ִ� ���ϸ��ֳ� ã�������� ��������
            var nextPokemon =  playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                playerUnit.Setup(nextPokemon);
                playerHud.SetData(nextPokemon);

                battleDialog.SetSkillNames(nextPokemon.Skills);

                // �ڷ�ƾ �Ϸ�ɶ����� ��ٸ��� �Ϸ�Ǹ����
                yield return battleDialog.TypeDialog($"Go REVENGE!!! {nextPokemon.pBase.Name}!!! ");

                PlayerAction();
            }
            else
            {
                OnBattleOver(false); //�÷��̾� �й�
            }

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
