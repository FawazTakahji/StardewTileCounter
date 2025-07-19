using Microsoft.Xna.Framework.Graphics;

namespace TileCounter;

using Microsoft.Xna.Framework;

public struct Textures
{
    public static Texture2D MainTexture = null!;
    public static bool Loaded = false;

    private const int TexturePieceSize = 64;

    public struct GreenBox
    {
        public static readonly Rectangle Complete = new(0, 0, TexturePieceSize, TexturePieceSize);

        public static readonly Rectangle TopLeft = new(0, 128, TexturePieceSize, TexturePieceSize);
        public static readonly Rectangle BottomLeft = new(0, 192, TexturePieceSize, TexturePieceSize);
        public static readonly Rectangle TopRight = new(0, 256, TexturePieceSize, TexturePieceSize);
        public static readonly Rectangle BottomRight = new(0, 320, TexturePieceSize, TexturePieceSize);
        public static readonly Rectangle Left = new(0, 384, TexturePieceSize, TexturePieceSize);
        public static readonly Rectangle Right = new(0, 448, TexturePieceSize, TexturePieceSize);
        public static readonly Rectangle Top = new(0, 512, TexturePieceSize, TexturePieceSize);
        public static readonly Rectangle Bottom = new(0, 576, TexturePieceSize, TexturePieceSize);
        public static readonly Rectangle TopEmpty = new(0, 640, TexturePieceSize, TexturePieceSize);
        public static readonly Rectangle BottomEmpty = new(0, 704, TexturePieceSize, TexturePieceSize);
        public static readonly Rectangle LeftEmpty = new(0, 768, TexturePieceSize, TexturePieceSize);
        public static readonly Rectangle RightEmpty = new(0, 832, TexturePieceSize, TexturePieceSize);
        public static readonly Rectangle TopBottomEmpty = new(0, 896, TexturePieceSize, TexturePieceSize);
        public static readonly Rectangle LeftRightEmpty = new(0, 960, TexturePieceSize, TexturePieceSize);
    }

    public struct RedBox
    {
        public static readonly Rectangle Complete = new(0, 64, TexturePieceSize, TexturePieceSize);
    }
}