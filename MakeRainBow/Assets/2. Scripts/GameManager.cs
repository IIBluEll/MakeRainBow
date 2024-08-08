using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

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
                GenerateGrid();
                break;
            
            case GameState.SpawningBlocks :
                SpawnBlocks(round++ == 0 ? 1 : 2);  // 첫 라운드면 1개 아니면 2개 스폰
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
    }
    
    private void GenerateGrid()
    {
        round = 0;
        
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
}
