using Microsoft.Xna.Framework.Graphics;

namespace TileCounter;

using Microsoft.Xna.Framework;

// Rotation is in radians
public struct Textures
{
    public static Texture2D MainTexture = null!;
    public static bool Loaded = false;

    private const int TexturePieceSize = 64;

    public struct GreenBox
    {
        public static readonly Rectangle Complete = new(0, 0, TexturePieceSize, TexturePieceSize);

        public struct Corner
        {
            public static readonly Rectangle Rect = new(64, 0, TexturePieceSize, TexturePieceSize);
            public const float TopLef = 0f;
            public const float TopRight = 1.57f;
            public const float BottomRight = 3.14f;
            public const float BottomLeft = 4.71f;
        }

        public struct Line
        {
            public static readonly Rectangle Rect = new(128, 0, TexturePieceSize, TexturePieceSize);
            public const float Left = 0f;
            public const float Top = 1.57f;
            public const float Right = 3.14f;
            public const float Bottom = 4.71f;
        }

        public struct TwoLines
        {
            public static readonly Rectangle Rect = new(192, 0, TexturePieceSize, TexturePieceSize);
            public const float LeftRight = 0f;
            public const float TopBottom = 1.57f;
        }

        public struct ThreeLines
        {
            public static readonly Rectangle Rect = new(256, 0, TexturePieceSize, TexturePieceSize);
            public const float TopBottomLeft = 0f;
            public const float TopRightLeft = 1.57f;
            public const float TopRightBottom = 3.14f;
            public const float BottomLeftRight = 4.71f;
        }
    }

    public struct RedBox
    {
        public static readonly Rectangle Complete = new(0, 64, TexturePieceSize, TexturePieceSize);
    }
}