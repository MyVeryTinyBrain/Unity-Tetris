using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GameLogic : MonoBehaviour
{
    BlockSet handlingBlocks;
    Color[,] blockGrid;
    bool pause;

    private void Awake()
    {
        blockGrid = new Color[DeviceContext.SizeY, DeviceContext.SizeX];
        pause = false;
        ResetGame();
    }

    void ResetGame()
    {
        for (int y = 0; y < DeviceContext.SizeY; y++)
        {
            for (int x = 0; x < DeviceContext.SizeX; x++)
            {
                blockGrid[y, x] = DeviceContext.ClearColor;
            }
        }
        handlingBlocks = TryCreateRandomBlockSetAtTopCenter();
    }

    public bool OnTick(float deltaTime, IDeviceContext dc, ref bool redraw)
    {
        if (pause)
        {
            return true;
        }

        if (handlingBlocks != null)
        {
            DropOnce(dc);
        }
        ApplyBlockGridColor(dc);
        ApplyHandlingBlocksColor(dc);
        redraw = true;
        return true;
    }

    public void OnKeydown(KeyCode key, IDeviceContext dc, ref bool redraw)
    {
        if (pause)
        {
            return;
        }

        if (handlingBlocks == null)
        {
            return;
        }
        switch (key)
        {
            case KeyCode.UpArrow:
            TryRotateHandlingBlocks();
            break;

            case KeyCode.DownArrow:
            DropOnce(dc);
            break;

            case KeyCode.Space:
            DropStraight(dc);
            break;

            case KeyCode.LeftArrow:
            MoveHorizontally(-1);
            break;

            case KeyCode.RightArrow:
            MoveHorizontally(+1);
            break;

            case KeyCode.Escape:
            ResetGame();
            break;
        }
        switch (key)
        {
            case KeyCode.UpArrow:
            case KeyCode.DownArrow:
            case KeyCode.Space:
            case KeyCode.LeftArrow:
            case KeyCode.RightArrow:
            case KeyCode.Escape:
            ApplyBlockGridColor(dc);
            ApplyHandlingBlocksColor(dc);
            redraw = true;
            break;
        }
    }

    void DownBlocksTo(int targetY)
    {
        for (int y = targetY + 1; y < DeviceContext.SizeY; ++y)
        {
            for (int x = 0; x < DeviceContext.SizeX; ++x)
            {
                blockGrid[y - 1, x] = blockGrid[y, x];
            }
        }
    }

    IEnumerator RemoveLinesRoutine(IDeviceContext dc)
    {
        // 제거하는 동안 애니메이션을 재생하기에 모든 동작을 중지한다.
        pause = true;

        // 맨 위부터 지워야 할 줄 탐색
        List<int> removeYTargets = new List<int>();
        for (int y = DeviceContext.SizeY - 1; y >= 0; --y)
        {
            int count = 0;
            for (int x = 0; x < DeviceContext.SizeX; ++x)
            {
                if (blockGrid[y, x] != DeviceContext.ClearColor)
                {
                    ++count;
                }
            }
            if (count == DeviceContext.SizeX)
            {
                removeYTargets.Add(y);
            }
        }

        // 지워야 할 줄이 없으면 종료
        if (removeYTargets.Count == 0)
        {
            pause = false;
            yield return null;
        }

        // 지워야 할 줄에 애니메이션 재생
        DeviceContext DC = dc as DeviceContext;
        foreach (int y in removeYTargets)
        {
            for (int x = 0; x < DeviceContext.SizeX; ++x)
            {
                DC.SetBufferAnimation(new Vector2Int(x, y), AnimationType.PlayLoop);
            }
        }
        DC.Redraw();

        // 애니메이션 재생시간동안 대기
        yield return new WaitForSeconds(DeviceContext.AnimationDuration);

        // 애니메이션 중지
        foreach (int y in removeYTargets)
        {
            for (int x = 0; x < DeviceContext.SizeX; ++x)
            {
                DC.SetBufferAnimation(new Vector2Int(x, y), AnimationType.Stop);
            }
        }
        DC.Redraw();

        // 지워야 할 줄을 제거하고 블록을 아래로 내린다.
        foreach (int y in removeYTargets)
        {
            for (int x = 0; x < DeviceContext.SizeX; ++x)
            {
                blockGrid[y, x] = DeviceContext.ClearColor;
            }
            DownBlocksTo(y);
        }

        ApplyBlockGridColor(dc);
        ApplyHandlingBlocksColor(dc);
        DC.Redraw();

        // 동작 재개
        pause = false;
    }

    void MoveHorizontally(int horizontalDirection)
    {
        if (Movable(handlingBlocks, horizontalDirection))
        {
            handlingBlocks.Move(new Vector2Int(horizontalDirection, 0));
        }
    }

    void DropOnce(IDeviceContext dc)
    {
        if (Dropable(handlingBlocks) == false)
        {
            PlaceHandlingBlocks(dc);
            if (handlingBlocks == null)
            {
                handlingBlocks = TryCreateRandomBlockSetAtTopCenter();
            }
        }
        else
        {
            handlingBlocks.pivot.y -= 1;
        }
    }

    void DropStraight(IDeviceContext dc)
    {
        while (Dropable(handlingBlocks))
        {
            handlingBlocks.pivot.y -= 1;
        }
        PlaceHandlingBlocks(dc);
        if (handlingBlocks == null)
        {
            handlingBlocks = TryCreateRandomBlockSetAtTopCenter();
        }
    }

    void ApplyBlockGridColor(IDeviceContext dc)
    {
        for (int y = 0; y < DeviceContext.SizeY; y++)
        {
            for (int x = 0; x < DeviceContext.SizeX; x++)
            {
                dc.SetBufferColor(new Vector2Int(x, y), blockGrid[y, x]);
            }
        }
    }

    void ApplyHandlingBlocksColor(IDeviceContext dc)
    {
        if (handlingBlocks == null)
        {
            return;
        }

        // 조작중인 블럭들을 표시한다.
        int MinX = int.MaxValue, MaxX = int.MinValue, MaxY = int.MinValue;
        for (int i = 0; i < handlingBlocks.count; ++i)
        {
            Vector2Int worldPosition = handlingBlocks.GetBlockWorldPosition(i);
            dc.SetBufferColor(worldPosition, handlingBlocks.blocks[i].color);
            MinX = Mathf.Min(MinX, worldPosition.x);
            MaxX = Mathf.Max(MaxX, worldPosition.x);
            MaxY = Mathf.Max(MaxY, worldPosition.y);
        }

        // 조작중인 블럭이 존재하는 세로줄의 색상을 변경한다. (헬퍼 기능)
        for (int y = 0; y < DeviceContext.SizeY; y++)
        {
            for (int x = 0; x < DeviceContext.SizeX; x++)
            {
                Color borderColor = (x >= MinX && x <= MaxX && y <= MaxY ? new Color(0.5f, 0.5f, 0.5f, 1.0f) : DeviceContext.OriginBorderColor);
                dc.SetBufferBorderColor(new Vector2Int(x, y), borderColor);
            }
        }
    }

    void PlaceHandlingBlocks(IDeviceContext dc)
    {
        for (int i = 0; i < handlingBlocks.count; ++i)
        {
            Vector2Int worldPosition = handlingBlocks.GetBlockWorldPosition(i);
            blockGrid[worldPosition.y, worldPosition.x] = handlingBlocks.blocks[i].color;
        }
        handlingBlocks = null;

        StartCoroutine(RemoveLinesRoutine(dc));
    }

    bool EmptyArea(BlockSet blockSet)
    {
        for (int i = 0; i < blockSet.count; ++i)
        {
            Vector2Int worldPosition = blockSet.GetBlockWorldPosition(i);
            if (false == EmptyArea(worldPosition))
            {
                return false;
            }
        }
        return true;
    }

    bool EmptyArea(Vector2Int position)
    {
        if (position.x < 0 || position.y < 0 || position.x >= DeviceContext.SizeX || position.y >= DeviceContext.SizeY)
        {
            return false;
        }
        if (blockGrid[position.y, position.x] != DeviceContext.ClearColor)
        {
            return false;
        }
        return true;
    }

    bool Dropable(BlockSet blockSet)
    {
        for (int i = 0; i < blockSet.count; ++i)
        {
            Vector2Int worldPosition = blockSet.GetBlockWorldPosition(i);
            if (worldPosition.y == 0)
            {
                return false;
            }
            if (blockGrid[worldPosition.y - 1, worldPosition.x] != DeviceContext.ClearColor)
            {
                return false;
            }
        }
        return true;
    }

    bool Movable(BlockSet blockSet, int horizontalDirection)
    {
        horizontalDirection = Mathf.Clamp(horizontalDirection, -1, +1);
        for (int i = 0; i < blockSet.count; ++i)
        {
            Vector2Int worldPosition = blockSet.GetBlockWorldPosition(i) + new Vector2Int(horizontalDirection, 0);
            if (worldPosition.x < 0 || worldPosition.y < 0 || worldPosition.x >= DeviceContext.SizeX || worldPosition.y >= DeviceContext.SizeY)
            {
                return false;
            }
            if (blockGrid[worldPosition.y, worldPosition.x] != DeviceContext.ClearColor)
            {
                return false;
            }
        }
        return true;
    }

    public BlockSet TryCreateRandomBlockSetAtTopCenter()
    {
        List<Block> blocks = CreateRandomBlocks();
        Vector2Int bounds = MaxVector(blocks);
        Vector2Int pivot = Vector2Int.zero;
        pivot.x = (DeviceContext.SizeX - bounds.x - 1) / 2;
        pivot.y = DeviceContext.SizeY - bounds.y - 1;
        BlockSet blockSet = new BlockSet(blocks, pivot);

        // 최상단 중앙에 배치할 수 없다면 null을 반환합니다.
        if (EmptyArea(blockSet) == false)
        {
            return null;
        }
        return blockSet;
    }

    readonly Vector2Int[] FixDirections = new Vector2Int[]
    {
        new Vector2Int(-1,0),
        new Vector2Int(+1,0),
        new Vector2Int(0,-1),
        new Vector2Int(0,+1),
    };
    public void TryRotateHandlingBlocks()
    {
        BlockSet rotated = BlockSet.Rotate(handlingBlocks);
        if (EmptyArea(rotated) == false)
        {
            return;
        }
        handlingBlocks = rotated;
    }

    public static Vector2Int MaxVector(List<Block> blocks)
    {
        Vector2Int Max = Vector2Int.one * int.MinValue;
        foreach (Block block in blocks)
        {
            Max = Vector2Int.Max(block.position, Max);
        }
        return Max;
    }

    static readonly Color[] BlockColors = new Color[] { Color.cyan, Color.blue, new Color(1f, 0.33f, 0f), Color.yellow, Color.green, new Color(0.5f, 0f, 0.5f), Color.red };
    // 원점부터 양의 영역내에 랜덤한 블럭을 생성합니다.
    public static List<Block> CreateRandomBlocks()
    {
        int type = Random.Range(0, 7);
        Color color = BlockColors[type];
        List<Block> blocks = null;
        switch (type)
        {
            // Long block
            case 0:
            blocks = new List<Block>()
                {
                    new Block(0,0,color),
                    new Block(0,1,color),
                    new Block(0,2,color),
                    new Block(0,3,color),
                };
            break;

            // Reversed L block
            case 1:
            blocks = new List<Block>()
                {
                    new Block(0,0,color),
                    new Block(1,0,color),
                    new Block(1,1,color),
                    new Block(1,2,color),
                };
            break;

            // L block
            case 2:
            blocks = new List<Block>()
                {
                    new Block(0,0,color),
                    new Block(1,0,color),
                    new Block(0,1,color),
                    new Block(0,2,color),
                };
            break;

            // Square block
            case 3:
            blocks = new List<Block>()
                {
                    new Block(0,0,color),
                    new Block(1,0,color),
                    new Block(0,1,color),
                    new Block(1,1,color),
                };
            break;

            // S block
            case 4:
            blocks = new List<Block>()
                {
                    new Block(0,0,color),
                    new Block(1,0,color),
                    new Block(1,1,color),
                    new Block(2,1,color),
                };
            break;

            // T block
            case 5:
            blocks = new List<Block>()
                {
                    new Block(0,0,color),
                    new Block(1,0,color),
                    new Block(2,0,color),
                    new Block(1,1,color),
                };
            break;

            // Z block
            case 6:
            blocks = new List<Block>()
                {
                    new Block(0,1,color),
                    new Block(1,1,color),
                    new Block(1,0,color),
                    new Block(2,0,color),
                };
            break;
        }
        return blocks;
    }
}

public class Block
{
    public Vector2Int position;

    public Color color;

    public Block(int x, int y, Color color)
    {
        this.position = new Vector2Int(x, y);
        this.color = color;
    }

    public Block(Vector2Int position, Color color)
    {
        this.position = position;
        this.color = color;
    }
}

public class BlockSet
{
    public List<Block> blocks;

    public Vector2Int pivot;

    // 블럭들은 [(0,0),(bounds.x,bounds.y)] 영역을 차지합니다.
    public Vector2Int bounds;

    public int longestBoundsSide => Mathf.Max(bounds.x, bounds.y);

    public int count => blocks.Count;

    public BlockSet(List<Block> blocks, Vector2Int pivot)
    {
        this.blocks = blocks;
        this.pivot = pivot;
        bounds = GameLogic.MaxVector(blocks);
    }

    public Vector2Int GetBlockWorldPosition(int index)
    {
        return blocks[index].position + pivot;
    }

    public void Move(Vector2Int delta)
    {
        pivot += delta;
    }

    public static BlockSet Rotate(BlockSet set)
    {
        int longestSide = set.longestBoundsSide;
        List<Block> rotatedBlocks = new List<Block>(set.blocks.Count);
        foreach (Block block in set.blocks)
        {
            rotatedBlocks.Add(new Block(block.position, block.color));
        }
        Vector2Int Min = Vector2Int.one * int.MaxValue;
        foreach (Block block in rotatedBlocks)
        {
            Vector2Int position = block.position;
            block.position.x = position.y;
            block.position.y = longestSide - position.x;
            Min = Vector2Int.Min(Min, block.position);
        }
        // 블럭이 원점에 위치하도록 조정합니다.
        foreach (Block block in rotatedBlocks)
        {
            block.position -= Min;
        }
        return new BlockSet(rotatedBlocks, set.pivot);
        /*
            X = Y
            Y = longestSide - X - 1
        */
    }
}