using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Block : MonoBehaviour
{
   public Node node;
   public Block mergeBlock;   // 합병하는 블록

   public int rank;
   public bool merging;       // 합병 여부

   public Vector2 Pos => transform.position;
   
   [SerializeField] private SpriteRenderer renderer;

   public void Init(BlockType blockType)
   {
      rank = blockType.rank;
      renderer.color = blockType.color;
   }

   public void SetBlock(Node getnode)
   {
      // 이미 점유한 노드가 있다면 그 노드의 점유 블록을 null로 다시 바꿔줌 ( 합병할 때를 대비 )
      if (node != null)
      {
         node.useBlock = null;
      }

      node = getnode;
      node.useBlock = this;
   }

   public void MergeBlock(Block blockToMerge)
   {
      mergeBlock = blockToMerge;
      node.useBlock = null;
      blockToMerge.merging = true;
   }

   public bool CanMerge(int _rank)
   {
      return _rank == rank && !merging && mergeBlock == null;
   }
}

/// <summary>
///  블록 타입
/// </summary>
[Serializable]
public struct BlockType
{
   public int rank;
   public Color color;
}