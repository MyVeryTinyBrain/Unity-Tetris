using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public enum AnimationType
{
    None, Play, PlayLoop, Stop
}

struct BlockBuffer
{
    public UnityEngine.Color color;
    public UnityEngine.Color borderColor;
    public AnimationType animation;
}

public interface IDeviceContext
{
    public void SetBufferEmpty(Vector2Int position);
    public void SetBufferColor(Vector2Int position, UnityEngine.Color color);
    public UnityEngine.Color GetColor(Vector2Int position);
    public void SetBufferBorderColor(Vector2Int position, UnityEngine.Color color);
    public void SetBufferAnimation(Vector2Int position, AnimationType animation);
    public bool IsPlayingAnimation(Vector2Int position);
}

public class DeviceContext : MonoBehaviour, IDeviceContext
{
    [SerializeField]
    Blocks blocks;

    public const int SizeX = 10;
    public const int SizeY = 20;
    public const float AnimationDuration = 0.3f;
    public static readonly Vector2Int Size = new Vector2Int(SizeX, SizeY);
    public static readonly UnityEngine.Color ClearColor = UnityEngine.Color.clear;
    public static readonly UnityEngine.Color OriginBorderColor = UnityEngine.Color.black;
    public static readonly AnimationType NoneAnimation = AnimationType.None;

    BlockBuffer[,] buffer = new BlockBuffer[SizeY, SizeX];

    private void Awake()
    {
        Clear();
    }

    public void SetBufferEmpty(Vector2Int position)
    {
        buffer[position.y, position.x].color = ClearColor;
    }

    public void SetBufferColor(Vector2Int position, UnityEngine.Color color)
    {
        buffer[position.y, position.x].color = color;
    }

    public UnityEngine.Color GetColor(Vector2Int position)
    {
        return blocks.GetBlock(position).fillColor;
    }

    public void SetBufferBorderColor(Vector2Int position, UnityEngine.Color color)
    {
        buffer[position.y, position.x].borderColor = color;
    }

    public void SetBufferAnimation(Vector2Int position, AnimationType animation)
    {
        buffer[position.y, position.x].animation = animation;
    }

    public bool IsPlayingAnimation(Vector2Int position)
    {
        return blocks.GetBlock(position).isPlayingAnimation;
    }

    public void Redraw()
    {
        for (int y = 0; y < SizeY; y++)
        {
            for (int x = 0; x < SizeX; x++)
            {
                ref BlockBuffer buf = ref buffer[y, x];
                blocks.GetBlock(new Vector2Int(x, y)).fillColor = buf.color;
                blocks.GetBlock(new Vector2Int(x, y)).borderColor = buf.borderColor;
                switch (buf.animation)
                {
                    case AnimationType.Play:
                    blocks.GetBlock(new Vector2Int(x, y)).PlayAnimation(AnimationDuration, false);
                    break;

                    case AnimationType.PlayLoop:
                    blocks.GetBlock(new Vector2Int(x, y)).PlayAnimation(AnimationDuration, true);
                    break;

                    case AnimationType.Stop:
                    blocks.GetBlock(new Vector2Int(x, y)).StopAnimation();
                    break;
                }
            }
        }
    }

    public void Clear()
    {
        for (int y = 0; y < SizeY; y++)
        {
            for (int x = 0; x < SizeX; x++)
            {
                ref BlockBuffer buf = ref buffer[y, x];
                blocks.GetBlock(new Vector2Int(x, y)).fillColor = ClearColor;
                blocks.GetBlock(new Vector2Int(x, y)).borderColor = OriginBorderColor;
                switch (buf.animation)
                {
                    case AnimationType.Play:
                    case AnimationType.PlayLoop:
                    case AnimationType.Stop:
                    blocks.GetBlock(new Vector2Int(x, y)).StopAnimation();
                    break;
                }
                buf.color = ClearColor;
                buf.borderColor = OriginBorderColor;
                buf.animation = NoneAnimation;
            }
        }
    }

    public void SetColorBuffer(Vector2Int position, UnityEngine.Color color)
    {
        throw new System.NotImplementedException();
    }

    public void SetAnimationBuffer(Vector2Int position, AnimationType animation)
    {
        throw new System.NotImplementedException();
    }
}
