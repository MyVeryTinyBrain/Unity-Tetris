using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blocks : MonoBehaviour
{
    [SerializeField]
    HorizontalBlocks[] horizontalBlocks;

    public BlockSprite GetBlock(Vector2Int position)
    {
        return horizontalBlocks[position.y].GetBlock(position.x);
    }
}
