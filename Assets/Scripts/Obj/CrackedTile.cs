using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "CrackedTile", menuName = "CustomTiles/CrackedTile")]
public class CrackedTile : Tile
{
    [Header("균열 단계별 스프라이트 (0: 정상, 1~n: 균열)")]
    public Sprite[] crackStages;

    [HideInInspector] public int currentStage = 0;
    public int maxStage => crackStages.Length - 1;

    private void OnEnable()
    {
        //  실행 시 항상 첫 번째 스프라이트로 초기화
        if (crackStages != null && crackStages.Length > 0)
            sprite = crackStages[0];
    }

    public void DamageTile()
    {
        if (currentStage < maxStage)
        {
            currentStage++;
            sprite = crackStages[currentStage];
        }
    }

    public bool IsBroken()
    {
        return currentStage >= maxStage;
    }
}
