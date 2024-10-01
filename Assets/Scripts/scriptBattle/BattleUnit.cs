using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; //Dotween�ҷ�����

public class BattleUnit : MonoBehaviour
{
    // �ϵ��ڵ����� �ҷ����� ��Ŀ��� �������� ���� ��Ȱ��ȭ
    // [SerializeField] PokemonBase pBase;
    // [SerializeField] int level;
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;
    public bool IsPlayerUnit { get { return isPlayerUnit; } }
    public BattleHud Hud { get { return hud; } }


    public Pokemon Pokemon {  get; set; }

    //�ִϸ��̼� ���� 
    Image image;
    Vector3 originalPos;
    Color originalColor;

    private void Awake()
    {
        image = GetComponent<Image>();

        // World�� �����Ǵ°� �ƴ϶� Canvas�� �������� Local���ٰ�
        // �������� ȭ��ۿ��� �������ؼ� ������Ʈ������ġ ����
        originalPos = image.transform.localPosition;

        // ������ ���������� ���������� ����
        originalColor = image.color;
    }
    public void Setup(Pokemon pokemon) // Parameter���� �޾ƿ���
    {
        Pokemon = pokemon;
        if (isPlayerUnit)
        {
            image.sprite = Pokemon.pBase.BackSprite;
        }
        else
        {
            image.sprite = Pokemon.pBase.FrontSprite;
        }

        hud.SetData(pokemon); 

        image.color = originalColor;
        PlayerEnterAnimation();
    }

    public void PlayerEnterAnimation()
    {

        if (isPlayerUnit)
        {   // �÷��̾�, ������ �ۿ��� ���
            image.transform.localPosition = new Vector3(-580f, originalPos.y);
        }
        else
        {   // ����, ������ �ۿ��� ���
            image.transform.localPosition = new Vector3(510f, originalPos.y);
        }
        image.transform.DOLocalMoveX(originalPos.x, 1f);
    }

    public void PlayAttackAnimation()
    {
        // Dotween���� �������� �ϳ��ϳ��� �������
        var sequence = DOTween.Sequence();
        if (isPlayerUnit)
        {
            // �������� �߰��صּ� ������ �ٷ� ����������������
            // �÷��̾� ����(�����̴¸��) ��ο�����
            // ���� ��ų�� ���� ����Ʈ ���ϱ�. (���� ����)
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 50f, 0.25f));
        }
        else
        {
            // ���� ����(�����̴¸��) �·ο�����
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 50f, 0.25f));
        }
        // �������� �ڸ�����
        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.25f));
    }
    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(originalColor, 01f));
    }
    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        // ������ ������ �p
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 150f, 0.5f));
        // ���̴°� �׿��� fade out & �Ⱥ��̰�
        sequence.Join(image.DOFade(0f, 0.5f)); //���� �������°Ŷ� �Բ� ����ǰ� Join�Լ�
    }
}
