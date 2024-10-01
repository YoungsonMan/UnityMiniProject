using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; //Dotween불러오기

public class BattleUnit : MonoBehaviour
{
    // 하드코딩으로 불러오는 방식에서 변경위해 이제 비활성화
    // [SerializeField] PokemonBase pBase;
    // [SerializeField] int level;
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;
    public bool IsPlayerUnit { get { return isPlayerUnit; } }
    public BattleHud Hud { get { return hud; } }


    public Pokemon Pokemon {  get; set; }

    //애니메이션 관련 
    Image image;
    Vector3 originalPos;
    Color originalColor;

    private void Awake()
    {
        image = GetComponent<Image>();

        // World에 구현되는게 아니라 Canvas에 구현위해 Local에다가
        // 등장전에 화면밖에서 등장위해서 오브젝트원래위치 저장
        originalPos = image.transform.localPosition;

        // 맞으면 색변경위해 오리지날색 저장
        originalColor = image.color;
    }
    public void Setup(Pokemon pokemon) // Parameter에서 받아오기
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
        {   // 플레이어, 등장전 밖에서 대기
            image.transform.localPosition = new Vector3(-580f, originalPos.y);
        }
        else
        {   // 상대방, 등장전 밖에서 대기
            image.transform.localPosition = new Vector3(510f, originalPos.y);
        }
        image.transform.DOLocalMoveX(originalPos.x, 1f);
    }

    public void PlayAttackAnimation()
    {
        // Dotween으로 시퀀스를 하나하나씩 재생가능
        var sequence = DOTween.Sequence();
        if (isPlayerUnit)
        {
            // 시퀀스에 추가해둬서 끝나면 바로 다음시퀀스나오게
            // 플레이어 공격(움직이는모션) 우로움직임
            // 추후 스킬에 따라 이펙트 더하기. (적도 동일)
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 50f, 0.25f));
        }
        else
        {
            // 상대방 공격(움직이는모션) 좌로움직임
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 50f, 0.25f));
        }
        // 움직였다 자리복귀
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
        // 죽을때 밑으로 쓕
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 150f, 0.5f));
        // 보이는값 죽여서 fade out & 안보이게
        sequence.Join(image.DOFade(0f, 0.5f)); //위에 내려가는거랑 함께 재생되게 Join함수
    }
}
