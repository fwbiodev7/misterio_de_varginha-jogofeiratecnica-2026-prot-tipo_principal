using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Retratos compartilham os designs usados pelos personagens no mapa.</summary>
    public static class VarginhaDialoguePortraits
    {
        private static readonly Dictionary<string, Sprite> Cache = new();

        public static Sprite ForSpeaker(string speaker)
        {
            string key = Normalize(speaker);
            if (Cache.TryGetValue(key, out var cached) && cached != null && cached.texture != null) return cached;
            Sprite portrait = null;
            if (key == "edelzio") portrait = VarginhaStudentSprites.Portrait("Edelzio");
            else if (key == "padre fabio") portrait = VarginhaStudentSprites.Portrait("PadreFabio");
            else
            {
                foreach (var student in VarginhaPhase2Controller.StudentNames)
                    if (key == Normalize(student)) { portrait = VarginhaStudentSprites.Portrait(student); break; }
            }
            // Rodrigo speaks remotely; keep his established radio identity rather than
            // assigning him another character's face. Object/system messages use their icon.
            if (portrait == null)
            {
                string icon = key == "rodrigo" ? "Radio_Office" : key.Contains("notebook") ? "Notebook_TI"
                    : key.Contains("fusca") ? "Fusca_Fallback" : "Doc_Historical";
                portrait = VarginhaPixelArtSprites.Create(icon, new Color(.45f, .65f, .68f));
            }
            Cache[key] = portrait;
            return portrait;
        }

        private static string Normalize(string value)
        {
            var result = new StringBuilder();
            foreach (char c in (value ?? string.Empty).Normalize(NormalizationForm.FormD))
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) result.Append(char.ToLowerInvariant(c));
            return result.ToString().Trim();
        }

        public static void Draw(Rect area, Sprite sprite)
        {
            if (sprite == null || sprite.texture == null) return;
            Rect source = sprite.rect;
            float scale = Mathf.Min(area.width / source.width, area.height / source.height);
            Rect fitted = new Rect(Mathf.Round(area.center.x - source.width * scale * .5f),
                Mathf.Round(area.center.y - source.height * scale * .5f), source.width * scale, source.height * scale);
            GUI.DrawTextureWithTexCoords(fitted, sprite.texture, new Rect(source.x / sprite.texture.width,
                source.y / sprite.texture.height, source.width / sprite.texture.width, source.height / sprite.texture.height));
        }
    }
}
