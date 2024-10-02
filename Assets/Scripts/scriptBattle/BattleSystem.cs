using DG.Tweening;
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


    [SerializeField] GameObject pokeballSprite; 


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
            if (selectedMember.curHP <= 0) 
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
            ActionSelection();
        }
    }

    // ���ϸ� ��ü|���� �Լ�
    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        bool currentPokemonFainted = true;
        if (playerUnit.Pokemon.curHP > 0) // ����ִ� ���ϸ� ����
        {
            currentPokemonFainted = false;
            yield return battleDialog.TypeDialog($"Come back {playerUnit.Pokemon.pBase.Name}");
            playerUnit.PlayFaintAnimation(); // ��ü�ִϸ��̼��� �����ص� ������(�������κ����ٴ���..) �ϴ� ������� ����
            yield return new WaitForSeconds(2f);
        }

        // ���������� �������ϸ� ������ �ڵ� ����
        playerUnit.Setup(newPokemon);

        battleDialog.SetSkillNames(newPokemon.Skills);

        // �ڷ�ƾ �Ϸ�ɶ����� ��ٸ��� �Ϸ�Ǹ����
        yield return battleDialog.TypeDialog($"Go, {newPokemon.pBase.Name}!!! ");

        if (currentPokemonFainted)
        {   // �������ϸ� �����ؼ� ����� ���°Ÿ� �÷��̾���
            ChooseFirstTurn();
        }
        else
        {   // ��ü�ؼ� ���°Ÿ�
            // �������
            StartCoroutine(EnemyMove());
        }
    }

    // ��Ʋ�����ϱ� �Լ�
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

        ChooseFirstTurn();
    }

    // �����Լ�
    public void ChooseFirstTurn()
    {
        if (playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed)
        {
            ActionSelection();
        }
        else
        {
            StartCoroutine(EnemyMove());    
        }
    } //���Ŀ� '������ȭ'���� �����Ǽ��������� ��ų �߰��ϸ� ����...

    // ��Ʋ��|���� �Լ�
    public void BattleOver(bool won) 
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver()); // Foreach, Link�̿��� ª�� �ۼ�
        OnBattleOver(won);
    }


    // �ൿ���� �Լ�
    public void ActionSelection()
    {
        state = BattleState.ActionSelection;
        battleDialog.SetDialog("Choose an action");
        battleDialog.EnableActionSelector(true);
    }

    // ��Ƽâ���� �Լ�
    public void OpenPartyScreen()
    {
        Debug.Log("LoadPartyScreen");
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    // ������� �Լ�
    public void MoveSelection()
    {
        state = BattleState.MoveSelection;
        battleDialog.EnableActionSelector(false);
        battleDialog.EnableDialogText(false);
        battleDialog.EnableSkillSelector(true);
    }


    // �÷��̾����� �Լ�: �÷��̾� ---����---> ��
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

    // ������ �Լ�: �� ---����---> �÷��̾�
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
    // �ൿ�Լ�
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Skill skill)
    {
        bool canRunSkill =  sourceUnit.Pokemon.OnBeforeMove();
        if (!canRunSkill)
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon); //���� �ɷ��� 1/4Ȯ���θ� �������̴ϱ� 

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
            yield return RunSkillEffects(skill, sourceUnit.Pokemon, targetUnit.Pokemon);
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

        // �����̻� ������ ������ �Լ�
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateHP();
        // �����̻����ε� ������ �޴� ������ ������ �������ְ� ����
        if (sourceUnit.Pokemon.curHP <= 0)
        {
            yield return battleDialog.TypeDialog($"{sourceUnit.Pokemon.pBase.Name} Fainted");
            sourceUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
            CheckForBattleOver(sourceUnit);
        }
    }
    IEnumerator RunSkillEffects(Skill skill, Pokemon source, Pokemon target)
    {
        
        var effects = skill.Base.Effects;

        // ����/����� ���Ȯ��
        if (effects.Boosts != null)
        {
            if (skill.Base.Target == SkillTarget.Self)
            {   // ����
                source.ApplyBoosts(effects.Boosts);
            }
            else
            {   // ����
                target.ApplyBoosts(effects.Boosts);
            }

            //�����̻�
            if (effects.Status != ConditionID.none)
            {
                target.SetStatus(effects.Status);   
            }
            //���º�ȭ Volatile Status Condition
            if (effects.VolatileStatus != ConditionID.none)
            {
                target.SetVolatileStatus(effects.VolatileStatus);
            }


            yield return ShowStatusChanges(source);
            yield return ShowStatusChanges(target);
        }
    }

    // ���Ⱥ��� �Լ�
    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue(); 
            yield return battleDialog.TypeDialog(message);  
        } 

    }

    // ��Ʋ����|������ üũ
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
                Debug.Log("��Ʋ�� �й��ߴ�. ������ ����������..\n ���ϸ��� ȸ���� ��Ȱ");
            }
        }
        else
        {
            BattleOver(true);
        }
    }

    // ���������λ��� �Լ�
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

    IEnumerator ThrowPokeball()
    {
        state = BattleState.Busy;
        yield return battleDialog.TypeDialog("Player used Pokeball!");

        var pokeballObject = Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(2,0), Quaternion.identity);

        var pokeball = pokeballObject.GetComponent<SpriteRenderer>();

        // Animation
        yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0,2), 2f, 1, 1f).WaitForCompletion(); // ���ͺ� ���󰡴¸��
        yield return enemyUnit.PlayCaptureAnimation(); // ���ϸ� ���ͺ��� �������� ���
        yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 1.3f, 0.5f).WaitForCompletion();

        for (int i = 0; i < 3; i++) // ������ �������� ��鸮�� Ƚ�� (3)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }
    }

    int TryToCatchPokemon(Pokemon pokemon)
    {

    }

}
