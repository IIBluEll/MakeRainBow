using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 보드의 각 그리드 셀을 나타냄
/// 각 셀의 위치와 그 셀을 쓰는 블록을 관리함
/// </summary>
public class Node : MonoBehaviour
{
    public Vector2 POS
    {
        get => transform.position;      // 현재 노드 반환
    }

    public Block useBlock;                // 현재 사용중인 블록
}
