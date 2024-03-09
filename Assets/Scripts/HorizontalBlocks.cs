using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalBlocks : MonoBehaviour
{
    [SerializeField]
    BlockSprite[] blocks;

    public BlockSprite GetBlock(int x)
    {
        return blocks[x];
    }
}
