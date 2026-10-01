using Game.Level;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Zoom e área visível de cada mapa, para cenas salvas e criadas em runtime.</summary>
    public static class VarginhaCameraFraming
    {
        public static bool Configure(CameraFollow2D follow, string sceneName)
        {
            Rect bounds;
            float size;
            switch (sceneName)
            {
                case VarginhaGameOverFlow.PhaseOneScene:
                    bounds = VarginhaHouseArchitecture.CameraBounds;
                    size = 3.8f;
                    break;
                case VarginhaTravelCinematic.SchoolScene:
                    bounds = Rect.MinMaxRect(-12.3f, -14f, 12.3f, 6f);
                    size = 4.2f;
                    break;
                case "Fase3_Igreja_Guardiao":
                    bounds = Rect.MinMaxRect(-8.5f, -6.5f, 9.5f, 6.5f);
                    size = 4f;
                    break;
                default: return false;
            }
            var player = Object.FindAnyObjectByType<EdelzioTopDownController>();
            var target = follow.Target != null ? follow.Target : player != null ? player.transform : null;
            follow.ConfigureMap(target, bounds, size);
            VarginhaPixelPresentation.Configure(follow.GetComponent<Camera>());
            return true;
        }
    }
}
