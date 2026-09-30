using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Persistent frames of the current Edelzio, imported with stable feet and body scale.</summary>
    public static class VarginhaInteractionSprites
    {
        public const string ResourcePath = "Varginha/HouseReference512/EdelzioInteractions";
        private static Sprite[] _frames;

        public static Sprite Frame(int row, int frame)
        {
            if (_frames == null || _frames[0] == null)
            {
                _frames = new Sprite[16];
                foreach (var sprite in Resources.LoadAll<Sprite>(ResourcePath))
                    for (int r = 0; r < 4; r++)
                    for (int f = 0; f < 4; f++)
                        if (sprite.name == "Edelzio_Interaction_" + r + "_" + f) _frames[r * 4 + f] = sprite;
            }
            return _frames[Mathf.Clamp(row, 0, 3) * 4 + (frame & 3)];
        }

        public static Sprite Seating(int frame, Vector2 direction) => Frame(direction.x > .5f ? 3 : 1, frame);

        public static Sprite Webcam(float time, bool reacting)
        {
            int frame = reacting ? 3 : time % 4.5f > 4.32f ? 1 : (Mathf.FloorToInt(time * 2f) & 1) == 0 ? 0 : 2;
            return Frame(2, frame);
        }
    }
}
