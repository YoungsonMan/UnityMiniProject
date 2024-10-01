using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, BattleOver } // Busy = ������

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;

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
        currentSkill = Mathf.Clamp(currentSkill, 0, playerUnit.Pokemon.Skills.Count - 1); //����Ʈ�� 0���� �����ϴϱ� -1

        battleDialog.UpdateSkillSelection(currentSkill, playerUnit.Pokemon.Skills[currentSkill]);

        // ZŰ�� ����
        if (Input.GetKeyDown(KeyCode.Z)) 
        {
            battleDialog.EnableSkillSelector(false);
            battleDialog.EnableDialogText(true);
            StartCoroutine(PlayerMove());
        }
        // XŰ�� ���
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
            if (selectedMember.curHP <= 0) // �ؿ� ���ϸ� ���������� �������ϸ󸻰� �����ϱ� �ٲٸ鼭 �̰� �ȵ�
            {
                partyScreen.SetMessageText("Pokemon is FAINTED");
                return;
            }
            if (selectedMember == playerUnit.Pokemon) //�̰��ߵǴµ�
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

    // ���ϸ� ��ü|����
    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        if (playerUnit.Pokemon.curHP > 0) // ����ִ� ���ϸ� ����
        {
            yield return battleDialog.TypeDialog($"Come back {playerUnit.Pokemon.pBase.Name}");
            playerUnit.PlayFaintAnimation(); // ��ü�ִϸ��̼��� �����ص� ������(�������κ����ٴ���..) �ϴ� ������� ����
            yield return new WaitForSeconds(2f);
        }

        // ���������� �������ϸ� ������ �ڵ� ����
        playerUnit.Setup(newPokemon);

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

        // ���(�߻����ϸ�) ����
        enemyUnit.Setup(wildPokemon);

        // ��Ƽ��ũ��
        partyScreen.Init();


        // ��ų����Ʈ
        battleDialog.SetSkillNames(playerUnit.Pokemon.Skills);

        // �ڷ�ƾ �Ϸ�ɶ����� ��ٸ��� �Ϸ�Ǹ����
        yield return battleDialog.TypeDialog($"A wild {enemyUnit.Pokemon.pBase.Name} appeared."); 

        ActionSelection();
    }

    public void BattleOver(bool won) // GameManager���� ��Ʋ������ �˼��ְ�
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver()); // Foreach, Link�̿��� ª�� �ۼ�
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


    // �÷��̾� ---����---> ��
    IEnumerator PlayerMove()
    {
        // �÷��̾ ��� ��ų�����Ҽ���������, ���¸� Busy�� PerformMove�� ����
        state = BattleState.PerformMove;
        var skill = playerUnit.Pokemon.Skills[currentSkill];
        yield return RunMove(playerUnit, enemyUnit, skill);

        // ��Ʋ���°� RunMove�� �ٲ�°� �ƴϸ� ����
        if (state == BattleState.PerformMove)
        {
            StartCoroutine(EnemyMove());
        }
    }

    // �� ---����---> �÷��̾�
    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;
        // �ϴ� ������ų ����
        var skill = enemyUnit.Pokemon.GetRandomSkill();
        yield return RunMove(enemyUnit, playerUnit, skill);

        // ��Ʋ���°� RunMove�� �ٲ�°� �ƴϸ� ����
        if (state == BattleState.PerformMove)
        {
            ActionSelection();
        }
    }

    // �÷��̾� ���� | �� ������ ���������� ��ݵǴ� ����̿���
    // ���°��ݵ� ��ų�� ������ �� �߰��ϸ� �� ���������Ƿ� ĸ��ȭ
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Skill skill)
    {
        // ��ų ���� PP ����
        skill.PP--;
        // ������ ���ϱ�
        yield return battleDialog.TypeDialog($"{sourceUnit.Pokemon.pBase.Name} used {skill.Base.Name}");

        // ���ݾִϸ��̼�
        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f); // �Ǳ��̴½ð� ��޸���
        // ���� => ���� ����(���ٲ��)
        targetUnit.PlayHitAnimation();

        if (skill.Base.Category == SkillCategory.Status)
        {
            // ����/����� ���Ȯ��
            var effects = skill.Base.Effects;
            if (effects.Boosts != null)
            {
                if (skill.Base.Target == SkillTarget.Self)
                {   // ����
                    sourceUnit.Pokemon.ApplyBoosts(effects.Boosts); 
                }
                else
                {   // ����
                    targetUnit.Pokemon.ApplyBoosts(effects.Boosts);
                }
                yield return ShowStatusChanges(sourceUnit.Pokemon);
                yield return ShowStatusChanges(targetUnit.Pokemon);
            }
        }
        else
        {
            var damageDetails = targetUnit.Pokemon.TakeDamage(skill, sourceUnit.Pokemon);
            // ���ݹ������(��) ���������Ʈ(HP - DMG)
            yield return targetUnit.Hud.UpdateHP();
            // ������ȿ�� �ڷ�ƾ
            yield return ShowDamageDetails(damageDetails);

        }


        // ������ �޴ٰ� ����
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
        // �����Ѱ� �÷��̾����� ����������
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else
            {
                // ������ �÷��̾� �й�
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
