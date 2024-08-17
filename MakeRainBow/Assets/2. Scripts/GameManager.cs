using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;
using Object = UnityEngine.Object;

public enum GameState
{
    GenerateLevel,
    SpawningBlocks,
    WaitingInput,
    Moving,
    Win,
    Lose
}

public class GameManager : Singleton<GameManager>
{
    [Header("그리드 크기")] 
    [SerializeField] private int width = 4;
    [SerializeField] private int height = 4;

    [Space(10f), Header("참조 할당")] 
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private SpriteRenderer boardRender;

    [Space(10f), Header("블록 타입 설정")] 
    [SerializeField] private List<BlockType> blockTypes;

    [Space(10f), Header("블록 타입 설정")] 
    [SerializeField] private float animationTime = 0.2f;
    [SerializeField] private int winCondition = 7;
    
    private int round;

    private List<Node> nodes;
    private List<Block> blocks;
    private GameState state;
    
    private void ChangeState(GameState newState)
    {
        state = newState;

        switch (newState)
        {
            case GameState.GenerateLevel :
                Debug.Log(state);
                round = 0;
                GenerateGrid();
                break;
            
            case GameState.SpawningBlocks :
                Debug.Log(state);
                SpawnBlocks(round++ == 0 ? 2 : 1);  // 첫 라운드면 1개 아니면 2개 스폰
                break;
            
            case GameState.WaitingInput :
                Debug.Log(state);
                break;
            
            case GameState.Moving :
                Debug.Log(state);
                break;
            
            case GameState.Win :
                Debug.Log(state);
                Debug.Log("You Win");
                //TODO : 승리 UI
                break;
            
            case GameState.Lose :
                Debug.Log(state);
                Debug.Log("You Lose");
                //TODO : 패배 UI
                break;
            
        }
    }
    private void Start()
    {
        ChangeState(GameState.GenerateLevel);
    }

    private void Update()
    {
        if (state != GameState.WaitingInput)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveBlock(Vector2.left);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveBlock(Vector2.right);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveBlock(Vector2.up);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveBlock(Vector2.down);
        }
    }
    
    private void GenerateGrid()
    {
        nodes = new List<Node>();
        blocks = new List<Block>();
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Node node = Instantiate(nodePrefab, new Vector2(x, y), quaternion.identity);
                nodes.Add(node);
            }
        }

        Vector2 center = new Vector2((float)width / 2 - 0.5f, (float)height / 2 - 0.5f);

        SpriteRenderer board = Instantiate(boardRender, center, quaternion.identity);
        board.size = new Vector2(width, height);

        Camera.main.transform.position = new Vector3(center.x, center.y, -10);
        
        ChangeState(GameState.SpawningBlocks);
    }

    private BlockType GetBlockTypeValue(int rank)
    {
        for (int i = 0; i < blockTypes.Count; i++)
        {
            // 입력된 rank 값과 블록 타입의 값이 같으면 반환
            if (blockTypes[i].rank == rank)
            {
                return blockTypes[i];
            }
        }
        return default;
    }

    private void SpawnBlocks(int amount)
    {
        // 사용중인 블록이 없는 노드들을 선별하여 랜덤으로 리스트화
        List<Node> freeNodes = nodes.Where(n => n.useBlock == null).OrderBy(b => Random.value).ToList();

        // freeNodes 리스트에서 처음부터 amount까지 숫자만큼 스폰
        foreach (Node node in freeNodes.Take(amount))
        {
            SpawnBlock(node, 1);
        }
        
        if (freeNodes.Count() == 1)
        {
            ChangeState(GameState.Lose);
            return;
        }
    }

    private void SpawnBlock(Node node, int rank)
    {
        Block block = Instantiate(blockPrefab, node.POS, quaternion.identity);
        
        block.Init(GetBlockTypeValue(rank));
        block.SetBlock(node);

        blocks.Add(block);
        
        // blocks 리스트에 있는 어떤 블록이 목표 랭크에 달성하면 이김 아니면 입력 대기
        ChangeState(blocks.Any(b => b.rank == winCondition) ? GameState.Win : GameState.WaitingInput);
    }

    private void MoveBlock(Vector2 direction)
    {
        ChangeState(GameState.Moving);

        List<Block> blockOrder = blocks.OrderBy(n => n.Pos.y).ThenBy(n => n.Pos.x).ToList();

        if (direction == Vector2.up || direction == Vector2.left)
        {
            blockOrder.Reverse();
        }

        foreach (var block in blockOrder)
        {
            Node next = block.node;

            do
            {
                block.SetBlock(next);
                
                // 이동 방향의 다른 위치에 노드가 있는지 확인
                Node possibleNode = GetNodeAtPosition(next.POS + direction, nodes);

                if (possibleNode != null)
                {
                    // 병합 가능한 블록이 있는지 확인하고 병합
                    if (possibleNode.useBlock != null && possibleNode.useBlock.CanMerge(block.rank))
                    {
                        block.MergeBlock(possibleNode.useBlock);
                    }
                    else if (possibleNode.useBlock == null)
                    {
                        next = possibleNode;
                    }
                }
            } while (next != block.node);   // 더 이상 이동할 수 없을 때까지 반복
        }
        
        // DOTween 시퀀스
        var sequence = DOTween.Sequence();

        foreach (var block in blockOrder)
        {
            // 병합된 블록이 있는 경우 병합된 위치로 이동
            var movePoint = block.mergeBlock != null ? block.mergeBlock.node.POS : block.node.POS;

            sequence.Insert(0, block.transform.DOMove(movePoint, animationTime));
        }

        sequence.OnComplete(() =>
        {
            foreach (var block in blockOrder.Where(b => b.mergeBlock != null))
            {
                // 병합된 새로운 블록 생성
                SpawnBlock(block.mergeBlock.node, block.mergeBlock.rank + 1); // rank++은 안되고 +1은 된다?
                
                // 병합된 기존 블록 제거
                RemoveBlock(block.mergeBlock, blocks);
                RemoveBlock(block, blocks);
            }
            
            ChangeState(GameState.SpawningBlocks);
        });

    }

    private void RemoveBlock(Block block, List<Block> blocks)
    {
        blocks.Remove(block);
        Object.Destroy(block.gameObject);
    }

    private Node GetNodeAtPosition(Vector2 pos, List<Node> nodes)
    {
        // 해당 위치와 일치하는 첫 번째 노드 반환
        return nodes.FirstOrDefault(n => n.POS == pos);
    }
}
