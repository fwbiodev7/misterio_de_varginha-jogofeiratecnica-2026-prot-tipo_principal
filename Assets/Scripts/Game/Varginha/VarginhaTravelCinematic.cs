using System.Collections;
using System;
using Game.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Varginha
{
    /// <summary>Viagem persistente sobre o carregamento assíncrono real da próxima fase.</summary>
    public sealed class VarginhaTravelCinematic : MonoBehaviour
    {
        public const string SchoolScene = "Fase2_Escola_Resgate";
        public const string ChurchScene = "Fase3_Igreja_Guardiao";
        private const float MinimumTravelSeconds = 6.4f;
        public static bool IsTravelling { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() => IsTravelling = false;
        private bool _students;
        private float _elapsed, _fade, _progress;
        private Texture2D _frame;
        private Color32[] _frameColors;
        private VarginhaTravelPixelArt _art;
        private int _lastTick = -1;

        public static bool Begin(bool withStudents)
        {
            if (IsTravelling) return false;
            string destination = withStudents ? ChurchScene : SchoolScene;
            if (!Application.CanStreamedLevelBeLoaded(destination))
            {
                Debug.LogError("Cena de destino ausente: " + destination);
                return false;
            }
            IsTravelling = true;
            var go = new GameObject("Viagem_Cinematica_Fusca");
            DontDestroyOnLoad(go);
            var travel = go.AddComponent<VarginhaTravelCinematic>();
            travel._students = withStudents;
            travel.StartCoroutine(travel.Travel(destination));
            return true;
        }

        private IEnumerator Travel(string destination)
        {
            VarginhaGameHUD.Instance?.CloseDialogue();
            Time.timeScale = 0f;
            var operation = SceneManager.LoadSceneAsync(destination);
            operation.allowSceneActivation = false;
            // A tomada precisa respirar, mas não deve prender o jogador por dez
            // segundos quando a cena já terminou de carregar.
            while (_elapsed < MinimumTravelSeconds || operation.progress < .9f)
            {
                _elapsed += Time.unscaledDeltaTime;
                _progress = Mathf.Min(Mathf.Clamp01(_elapsed / MinimumTravelSeconds), operation.progress / .9f);
                _fade = Mathf.Clamp01(_elapsed / .5f);
                yield return null;
            }
            Time.timeScale = 1f;
            operation.allowSceneActivation = true;
            while (!operation.isDone) yield return null;
            GameManager.Instance?.StartGame();
            float fadeOut = 0f;
            while (fadeOut < .65f)
            {
                fadeOut += Time.unscaledDeltaTime;
                _fade = 1f - Mathf.Clamp01(fadeOut / .65f);
                yield return null;
            }
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            IsTravelling = false;
            Time.timeScale = 1f;
            if (_frame != null) Destroy(_frame);
        }

        private void EnsureArt()
        {
            if (_art != null) return;
            _art = new VarginhaTravelPixelArt(Load("TravelPixel/NightRoad"), Load("TravelPixel/CabinStudents"),
                Load("TravelPixel/FuscaReference"), Load("TravelPixel/TreeReference"), Load("EdelzioTopDownV3"));
            _frame = new Texture2D(VarginhaTravelPixelArt.Width,VarginhaTravelPixelArt.Height,TextureFormat.RGBA32,false)
            { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "Loading_PixelArt_384x216" };
            _frameColors = new Color32[VarginhaTravelPixelArt.Width * VarginhaTravelPixelArt.Height];
        }
        private static VarginhaTravelPixelArt.Picture Load(string name)
        {
            var texture = Resources.Load<Texture2D>("Varginha/" + name);
            if (texture == null) throw new System.InvalidOperationException("Arte de viagem ausente: " + name);
            texture.filterMode = FilterMode.Point;
            Color32[] colors;
            try
            {
                colors = texture.GetPixels32();
            }
            catch (ArgumentException)
            {
                // Texturas antigas podem continuar em memória com isReadable desligado até
                // o próximo reimport. Fazemos uma cópia GPU->CPU uma única vez no loading.
                var previous = RenderTexture.active;
                var temporary = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(texture, temporary);
                RenderTexture.active = temporary;
                var readable = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
                readable.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0, false);
                readable.Apply(false, false);
                colors = readable.GetPixels32();
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(temporary);
                Destroy(readable);
            }
            var pixels = new uint[colors.Length];
            for(int y=0;y<texture.height;y++) for(int x=0;x<texture.width;x++)
            {
                Color32 c=colors[(texture.height-1-y)*texture.width+x];
                pixels[y*texture.width+x]=((uint)c.a<<24)|((uint)c.r<<16)|((uint)c.g<<8)|c.b;
            }
            return new VarginhaTravelPixelArt.Picture(texture.width,texture.height,pixels);
        }
        private void Update()
        {
            EnsureArt();
            int tick=Mathf.FloorToInt(_elapsed*12f);
            if(_lastTick==tick)return;
            _lastTick=tick;
            string difficulty=VarginhaDifficulty.Selected==InvestigationDifficulty.Easy ? "FACIL"
                : VarginhaDifficulty.Selected==InvestigationDifficulty.Hard ? "DIFICIL" : "MEDIO";
            var pixels=_art.Render(_elapsed,_students,_progress,difficulty);
            for(int y=0;y<VarginhaTravelPixelArt.Height;y++)for(int x=0;x<VarginhaTravelPixelArt.Width;x++)
            {
                uint c=pixels[y*VarginhaTravelPixelArt.Width+x];
                _frameColors[(VarginhaTravelPixelArt.Height-1-y)*VarginhaTravelPixelArt.Width+x]=new Color32((byte)(c>>16),(byte)(c>>8),(byte)c,255);
            }
            _frame.SetPixels32(_frameColors);_frame.Apply(false);
        }
        private void OnGUI()
        {
            if(_frame==null)return;
            int depth=GUI.depth;var matrix=GUI.matrix;var color=GUI.color;
            GUI.depth=-10000;GUI.matrix=Matrix4x4.identity;GUI.color=Color.black;
            GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);
            // Escala inteira e letterbox: nenhum pixel fracionado em telas grandes ou ultrawide.
            int scale=Mathf.Max(1,Mathf.FloorToInt(Mathf.Min(Screen.width/(float)VarginhaTravelPixelArt.Width,Screen.height/(float)VarginhaTravelPixelArt.Height)));
            float width=VarginhaTravelPixelArt.Width*scale,height=VarginhaTravelPixelArt.Height*scale;
            GUI.color=new Color(1,1,1,_fade);
            GUI.DrawTexture(new Rect(Mathf.Floor((Screen.width-width)/2),Mathf.Floor((Screen.height-height)/2),width,height),_frame);
            GUI.depth=depth;GUI.matrix=matrix;GUI.color=color;
        }
    }
}
