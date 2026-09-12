using System;
using System.Collections;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Encenação de cada golpe: antecipação, ação, impacto único e saída; nunca aplica dano por frame.</summary>
    public sealed class VarginhaAllyAttackPresentation : MonoBehaviour
    {
        private GameObject _root;
        private string _caption;
        private Vector3 _captionPosition;
        private Color _color;
        private GUIStyle _label;
        private static Sprite _ring, _star, _disc;
        public static Sprite MarkerSprite { get { EnsureShapes(); return _ring; } }
        public bool IsPresenting => _root != null;

        public IEnumerator Play(string student, VarginhaStudentAllyStyle style, Color color,
            Vector3 source, Vector3 destination, Func<bool> canAdvance, Action impact)
        {
            Cancel(); EnsureShapes();
            _color = color;
            _caption = student.ToUpperInvariant() + " • " + Signature(student, style);
            _root = new GameObject("Animacao_" + student);
            _captionPosition = destination + Vector3.up * 2.3f;
            var actor = Part("Aluno", VarginhaPixelArtSprites.Create("Student_" + student, color), Color.white, 1.55f, 110);
            var weapon = Part("Instrumento", VarginhaPixelArtSprites.Create("StudentAttack_" + style, color), Color.white,
                style == VarginhaStudentAllyStyle.FallingPiano ? 2.5f : 1.45f, 112);
            var ball = Part("Bola_ping_pong", _disc, new Color(1f,.95f,.7f), .3f, 114);
            ball.enabled = style == VarginhaStudentAllyStyle.PingPong;
            var warning = Part("Area_do_golpe", _ring, new Color(color.r,color.g,color.b,.55f), 2.2f, 101);
            warning.transform.position = destination;
            var burst = Part("Impacto_cartoon", _star, color, .01f, 113);
            var halo = Part("Onda_de_impacto", _ring, color, .01f, 109);
            var trailSprite = VarginhaPixelArtSprites.Create("StudentAttackTrail_" + style, Color.Lerp(color, Color.white, .25f));
            var trails = new SpriteRenderer[4];
            for (int i = 0; i < trails.Length; i++)
            {
                trails[i] = Part("Rastro_pixel_" + i, trailSprite, new Color(color.r, color.g, color.b, .45f - i * .08f), .45f + i * .08f, 108);
                trails[i].enabled = false;
            }
            SpriteRenderer[] particles = new SpriteRenderer[12];
            for (int i = 0; i < particles.Length; i++)
                particles[i] = Part("Particula", i % 3 == 0 ? _star : _disc,
                    style == VarginhaStudentAllyStyle.Art ? Color.HSVToRGB(i / 12f,.7f,1f) : Color.Lerp(color,Color.white,i%3*.3f), .01f, 115);
            float anticipation = .32f;
            float action = style == VarginhaStudentAllyStyle.FallingPiano ? .65f : .38f;
            const float recovery = .55f;
            float time = 0; bool struck = false;
            Vector3 start = destination + (source - destination).normalized * 2.1f;
            if ((start - destination).sqrMagnitude < .01f) start = destination + Vector3.left * 2.1f;
            float sign = destination.x >= source.x ? 1 : -1;
            while (time < anticipation + action + recovery)
            {
                if (!canAdvance()) { yield return null; continue; }
                time += Time.deltaTime;
                float wind = Mathf.Clamp01(time / anticipation);
                float move = Mathf.Clamp01((time - anticipation) / action);
                float after = Mathf.Clamp01((time - anticipation - action) / recovery);
                float ease = Mathf.SmoothStep(0,1,move);
                Vector3 actorPosition = Vector3.Lerp(start, destination + Vector3.left * sign * .65f,ease);
                actor.transform.position = actorPosition;
                actor.transform.localScale = new Vector3(1.55f*(1 + .18f*Mathf.Sin(wind*Mathf.PI)),1.55f*(1 - .17f*Mathf.Sin(wind*Mathf.PI)),1);
                actor.flipX = sign < 0;
                weapon.transform.position = actorPosition + Vector3.right * sign * .65f;
                weapon.transform.rotation = Quaternion.Euler(0,0,Mathf.Lerp(-65*sign,45*sign,ease));
                warning.transform.localScale = Vector3.one * (2.1f + .12f * Mathf.Sin(time * 18));
                switch (style)
                {
                    case VarginhaStudentAllyStyle.JiuJitsu:
                        actor.transform.position += Vector3.up * Mathf.Sin(move*Mathf.PI) * 1.4f;
                        actor.transform.rotation = Quaternion.Euler(0,0,-sign*360*ease);
                        weapon.enabled = false;
                        break;
                    case VarginhaStudentAllyStyle.Katana:
                        actor.transform.position = Vector3.Lerp(start,destination + Vector3.right*sign*1.1f,Mathf.Pow(move,3));
                        weapon.transform.position = actor.transform.position + Vector3.right*sign*.7f;
                        weapon.transform.rotation = Quaternion.Euler(0,0,120 - 240*ease);
                        weapon.transform.localScale = new Vector3(2.4f,.9f,1);
                        break;
                    case VarginhaStudentAllyStyle.PingPong:
                        actor.transform.position = start;
                        weapon.transform.position = start + Vector3.right * sign * .65f;
                        weapon.transform.rotation = Quaternion.Euler(0,0,Mathf.Lerp(-100,65,Mathf.Clamp01(move*3))*sign);
                        ball.transform.position = Vector3.Lerp(start,destination,ease) + Vector3.up*Mathf.Abs(Mathf.Sin(move*Mathf.PI*(student == "Anna Sabia" ? 2 : 3)))*1.2f;
                        break;
                    case VarginhaStudentAllyStyle.Guitar:
                        actor.transform.position += Vector3.up*Mathf.Sin(move*Mathf.PI)*.65f;
                        weapon.transform.position = actor.transform.position + new Vector3(sign*.6f, .8f*(1-ease),0);
                        weapon.transform.rotation = Quaternion.Euler(0,0,Mathf.Lerp(-140,65,ease)*sign);
                        weapon.transform.localScale = Vector3.one*2f;
                        break;
                    case VarginhaStudentAllyStyle.Art:
                        weapon.transform.position = Vector3.Lerp(start,destination,ease) + new Vector3(Mathf.Sin(move*12)*.45f,Mathf.Cos(move*12)*.45f,0);
                        weapon.transform.rotation = Quaternion.Euler(0,0,move*360);
                        break;
                    case VarginhaStudentAllyStyle.Microphone:
                        actor.transform.position = start;
                        weapon.transform.position = start + Vector3.right*sign*.7f + Vector3.up*.25f;
                        weapon.transform.rotation = Quaternion.Euler(0,0,-20*sign);
                        halo.transform.position = Vector3.Lerp(start,destination,ease);
                        halo.transform.localScale = Vector3.one*(.3f + move*2f);
                        break;
                    case VarginhaStudentAllyStyle.FallingPiano:
                        actor.transform.position = start + Vector3.up*.3f;
                        weapon.transform.position = destination + Vector3.up*(3.8f*(1-move*move));
                        weapon.transform.rotation = Quaternion.Euler(0,0,Mathf.Sin(time*10)*8*(1-move));
                        break;
                    case VarginhaStudentAllyStyle.Support:
                        actor.transform.position += Vector3.up*Mathf.Sin(move*Mathf.PI)*.3f;
                        weapon.transform.position = actor.transform.position + Vector3.right*sign*.7f;
                        weapon.transform.localScale = Vector3.one*(1.3f + move*.7f);
                        break;
                }
                DrawPixelTrail(trails, actor.transform.position, destination, sign, move, time, style);
                if (move >= 1 && !struck)
                {
                    struck = true;
                    impact?.Invoke();
                    PlayImpactSound(style);
                    var camera = Camera.main;
                    if (camera != null) (camera.GetComponent<VarginhaCameraShake>() ?? camera.gameObject.AddComponent<VarginhaCameraShake>()).Shake(.13f,.06f);
                }
                if (struck)
                {
                    warning.enabled = false;
                    burst.transform.position = destination;
                    burst.transform.localScale = Vector3.one*(2.6f + after*.8f);
                    burst.transform.rotation = Quaternion.Euler(0,0,after*35);
                    burst.color = new Color(color.r,color.g,color.b,(1-after)*.85f);
                    halo.transform.position = destination;
                    halo.transform.localScale = Vector3.one*(1.1f + after*3.2f);
                    halo.color = new Color(1,1,1,1-after);
                    actor.transform.position += Vector3.up*after*.8f;
                    actor.color = new Color(1,1,1,1-after);
                    weapon.color = new Color(1,1,1,1-after);
                    ball.color = new Color(1,1,.7f,1-after);
                    for (int i = 0; i < particles.Length; i++)
                    {
                        float angle = i*Mathf.PI*2/particles.Length;
                        particles[i].transform.position = destination + new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0)*(after*2f + .3f);
                        particles[i].transform.localScale = Vector3.one*((i%3==0 ? .35f:.16f)*(1-after));
                        particles[i].transform.Rotate(0,0,Time.deltaTime*220);
                    }
                    for (int i = 0; i < trails.Length; i++) trails[i].color = new Color(color.r, color.g, color.b, (1 - after) * (.45f - i * .08f));
                }
                yield return null;
            }
            Cancel();
        }

        private static void DrawPixelTrail(SpriteRenderer[] trails, Vector3 actor, Vector3 target, float sign,
            float move, float time, VarginhaStudentAllyStyle style)
        {
            bool active = move > .05f && move < 1f;
            for (int i = 0; i < trails.Length; i++)
            {
                if (!active) { trails[i].enabled = false; continue; }
                trails[i].enabled = true;
                float offset = (i + 1) * .25f;
                Vector3 position = style == VarginhaStudentAllyStyle.PingPong
                    ? Vector3.Lerp(actor, target, Mathf.Clamp01(move - offset * .1f)) + Vector3.up * Mathf.Abs(Mathf.Sin((move - offset * .1f) * Mathf.PI * 3))
                    : Vector3.Lerp(actor, target, Mathf.Clamp01(move - offset * .1f)) - Vector3.right * sign * offset;
                trails[i].transform.position = position;
                trails[i].transform.rotation = Quaternion.Euler(0, 0, (time * 180f + i * 45f) * sign);
                trails[i].transform.localScale = Vector3.one * (.35f + (1f - move) * .18f);
            }
        }

        private static readonly System.Collections.Generic.Dictionary<VarginhaStudentAllyStyle, AudioClip> Sounds = new();
        private void PlayImpactSound(VarginhaStudentAllyStyle style)
        {
            if (!Sounds.TryGetValue(style, out var clip) || clip == null)
            {
                const int rate = 22050;
                int count = Mathf.RoundToInt(rate * .32f);
                var samples = new float[count];
                float frequency = style == VarginhaStudentAllyStyle.PingPong ? 820 : style == VarginhaStudentAllyStyle.FallingPiano ? 130 : 220 + (int)style * 65;
                for (int i = 0; i < count; i++)
                {
                    float t = i / (float)rate;
                    float envelope = Mathf.Sin(Mathf.PI * i / count) * Mathf.Exp(-t * 12);
                    samples[i] = (Mathf.Sin(2*Mathf.PI*frequency*t*(1-t)) + .35f*Mathf.Sin(2*Mathf.PI*frequency*1.5f*t))*envelope*.4f;
                }
                clip=AudioClip.Create("Impacto_"+style,count,1,rate,false);clip.SetData(samples,0);Sounds[style]=clip;
            }
            var audio=_root.AddComponent<AudioSource>();audio.playOnAwake=false;audio.spatialBlend=0;audio.volume=.35f;audio.PlayOneShot(clip);
        }

        public static string Signature(string name, VarginhaStudentAllyStyle style)
        {
            switch (style)
            {
                case VarginhaStudentAllyStyle.JiuJitsu: return "IPPON!";
                case VarginhaStudentAllyStyle.PingPong: return name == "Anna Sabia" ? "TOPSPIN!" : "SMASH!";
                case VarginhaStudentAllyStyle.Guitar: return "ACORDE BRUTAL!";
                case VarginhaStudentAllyStyle.Art: return "EXPLOSÃO DE CORES!";
                case VarginhaStudentAllyStyle.Microphone: return "SOLTA A VOZ!";
                case VarginhaStudentAllyStyle.Katana: return "CORTE RELÂMPAGO!";
                case VarginhaStudentAllyStyle.FallingPiano: return "GRANDE FINALE!";
                default: return "FORÇA, PROFESSOR!";
            }
        }
        private SpriteRenderer Part(string name, Sprite sprite, Color color, float size, int order)
        {
            var go = new GameObject(name); go.transform.SetParent(_root.transform);
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.color = color; renderer.sortingOrder = order;
            go.transform.localScale = Vector3.one * size;
            return renderer;
        }
        public void Cancel() { if (_root != null) Destroy(_root); _root = null; }
        private void OnDisable() => Cancel();
        private void OnGUI()
        {
            if (Game.Varginha.VarginhaTravelCinematic.IsTravelling) return;
            if (_root == null || Camera.main == null) return;
            if (_label == null) _label = new GUIStyle(GUI.skin.label) { fontSize = 17, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            Vector3 point = Camera.main.WorldToScreenPoint(_captionPosition);
            if (point.z <= 0) return;
            var rect = new Rect(Mathf.Clamp(point.x-220,0,Mathf.Max(0,Screen.width-440)),Mathf.Clamp(Screen.height-point.y,20,Screen.height-55),440,40);
            var previous = GUI.color;
            GUI.color = new Color(.015f,.025f,.06f,.9f); GUI.DrawTexture(rect,Texture2D.whiteTexture);
            GUI.color = _color; GUI.DrawTexture(new Rect(rect.x,rect.yMax-3,rect.width,3),Texture2D.whiteTexture);
            GUI.color = Color.white; GUI.Label(rect,_caption,_label); GUI.color = previous;
        }
        private static void EnsureShapes()
        {
            if (_ring != null) return;
            _ring = Shape(0); _star = Shape(1); _disc = Shape(2);
        }
        private static Sprite Shape(int shape)
        {
            // Máscaras também são sprites de baixa resolução; cada bloco é um pixel visível.
            const int size = 32;
            var texture = new Texture2D(size,size,TextureFormat.RGBA32,false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color[size*size];
            for (int y=0;y<size;y++) for (int x=0;x<size;x++)
            {
                float dx=(x-15.5f)/15f, dy=(y-15.5f)/15f;
                float radius=Mathf.Sqrt(dx*dx+dy*dy);
                bool on = shape == 2 ? radius <= .52f
                    : shape == 0 ? radius >= .73f && radius <= .94f
                    : radius <= .72f && (Mathf.Abs(dx) + Mathf.Abs(dy) > .35f || (x + y) % 5 == 0);
                pixels[y*size+x]=on ? Color.white : Color.clear;
            }
            texture.SetPixels(pixels);texture.Apply();
            return Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),32);
        }
    }
}
