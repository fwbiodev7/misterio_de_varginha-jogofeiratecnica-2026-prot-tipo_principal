using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Game.Varginha
{
    /// <summary>
    /// Pause-owned backpack inventory unified with PixelHUDFrame / Pause Menu styling.
    /// </summary>
    public sealed class VarginhaBackpackInventory : MonoBehaviour
    {
        public bool IsOpen { get; private set; }
        private GameObject _canvasObject;
        private GameObject _ownedEventSystem;
        private TMP_FontAsset _font;
        private VarginhaStudentAllySquad _squad;
        private EdelzioTopDownController _player;
        public bool ShowingStudents { get; private set; }
        private int _inspected;
        private float _resumeScale;
        private int _openedFrame;

        private readonly UnityEngine.UI.Image[] _cells = new UnityEngine.UI.Image[9];
        private readonly TMP_Text[] _states = new TMP_Text[9];
        private readonly TMP_Text[] _cellNames = new TMP_Text[9];
        private readonly UnityEngine.UI.Image[] _cellPortraits = new UnityEngine.UI.Image[9];
        private UnityEngine.UI.Button _itemsTab, _studentsTab;
        private TMP_Text _itemsTabLabel, _studentsTabLabel;
        private TMP_Text _equipCaption;

        private static readonly string[] ItemNames = { "Mochila", "Chave do Fusca", "Caderno de 1996", "Notebook", "Documento de 1898", "Lanterna" };
        private static readonly string[] ItemArt = { "Backpack_Inventory", "Inventory_Key", "Inventory_Journal", "Notebook_Inventory", "Doc_Inventory", "Inventory_Flashlight" };
        private static readonly string[] ItemDescriptions = {
            "Guarda itens e organiza a equipe.",
            "Abre o Fusca.",
            "Anotações de 1996. Necessário para viajar.",
            "Usado na decodificação de dados.",
            "Pista histórica importante.",
            "Equipe na Hotbar [G] e ligue com [V]."
        };

        private TMP_Text _name, _description, _availability, _equipped, _items;
        private UnityEngine.UI.Image _portrait;
        private UnityEngine.UI.Button _equip;

        // Unified PixelHUDFrame color palette matching VarginhaGameHUD
        private static readonly Color PanelColor = new(.042f, .056f, .068f, .98f);
        private static readonly Color PanelBorder = new(.28f, .40f, .44f, .98f);
        private static readonly Color PaperColor = new(.92f, .91f, .82f);
        private static readonly Color AccentCyan = new(.64f, .84f, .86f);
        private static readonly Color MutedCyan = new(.48f, .59f, .61f);
        private static readonly Color FocusGold = new(.96f, .78f, .34f);

        // Cached UI 9-slice textures and sprites
        private static Sprite _modalFrameSprite;
        private static Sprite _subPanelSprite;
        private static Sprite _slotNormalSprite;
        private static Sprite _slotSelectedSprite;
        private static Sprite _buttonNormalSprite;
        private static Sprite _buttonActiveSprite;
        private static Sprite _separatorSprite;

        public bool Open(EdelzioTopDownController player)
        {
            if (IsOpen || player == null || !player.HasBackpack || player.IsInputLocked
                || player.CurrentSanity <= 0 || Time.timeScale <= 0
                || VarginhaTravelCinematic.IsTravelling
                || VarginhaGameHUD.Instance?.IsDialogueOpen == true
                || VarginhaGameHUD.Instance?.IsVictoryOpen == true) return false;

            _squad = Object.FindAnyObjectByType<VarginhaStudentAllySquad>();
            _player = player;
            ShowingStudents = false;
            _inspected = 0;
            player.GetComponent<VarginhaPlayerAttack>()?.EndHitstopForModal();
            _resumeScale = Time.timeScale;

            if (_canvasObject == null) Build();
            IsOpen = true;
            _openedFrame = Time.frameCount;
            Time.timeScale = 0;
            _canvasObject.SetActive(true);
            if (_ownedEventSystem != null) _ownedEventSystem.SetActive(true);

            string physical = "";
            string[] names = { "Mochila", "Chave do Fusca", "Caderno de 1996", "Notebook", "Documento de 1898" };
            for (int i = 1; i < 5; i++) if (player.HasInventoryItem(i)) physical += "\n• " + names[i];
            _items.text = physical.Length == 0 ? "Nenhum item pendente." : physical.TrimStart('\n');
            Refresh();
            return true;
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            if (_canvasObject != null) _canvasObject.SetActive(false);
            if (_ownedEventSystem != null) _ownedEventSystem.SetActive(false);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            if (Time.timeScale == 0 && Game.Managers.GameManager.Instance?.IsPaused != true)
                Time.timeScale = _resumeScale;
        }

        private void OnDisable() => Close();
        private void OnDestroy() => Close();

        private void Update()
        {
            if (!IsOpen || Time.frameCount == _openedFrame) return;
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.escapeKey.wasPressedThisFrame || keyboard.gKey.wasPressedThisFrame)
            {
                VarginhaGameHUD.Instance?.CloseBackpack();
                return;
            }

            if (keyboard.tabKey.wasPressedThisFrame)
            {
                ShowTab(!ShowingStudents);
                return;
            }

            int next = _inspected;
            if (keyboard.leftArrowKey.wasPressedThisFrame) next = (_inspected + 8) % 9;
            if (keyboard.rightArrowKey.wasPressedThisFrame) next = (_inspected + 1) % 9;
            if (keyboard.upArrowKey.wasPressedThisFrame) next = (_inspected + 6) % 9;
            if (keyboard.downArrowKey.wasPressedThisFrame) next = (_inspected + 3) % 9;
            if (next != _inspected) { _inspected = next; Refresh(); }

            if (keyboard.enterKey.wasPressedThisFrame) Equip();
        }

        private void Equip()
        {
            if (ShowingStudents && _squad != null && _squad.SelectStudent(_inspected))
            {
                Refresh();
                return;
            }

            if (!ShowingStudents && _inspected == 5 && _player != null && _player.HasFlashlight)
            {
                _player.IsFlashlightEquippedInHotbar = !_player.IsFlashlightEquippedInHotbar;
                Refresh();
            }
        }

        public void ShowTab(bool students)
        {
            ShowingStudents = students;
            _inspected = students ? Mathf.Max(0, _squad != null ? _squad.SelectedStudentIndex : 0) : 0;
            Refresh();
        }

        private void Refresh()
        {
            EnsureTextures();

            // Tab styling: active tab gets glowing cyan border & teal background
            if (_itemsTab != null)
            {
                _itemsTab.image.sprite = ShowingStudents ? _buttonNormalSprite : _buttonActiveSprite;
                _itemsTab.image.color = Color.white;
                if (_itemsTabLabel != null) _itemsTabLabel.color = ShowingStudents ? MutedCyan : AccentCyan;
            }
            if (_studentsTab != null)
            {
                _studentsTab.image.sprite = ShowingStudents ? _buttonActiveSprite : _buttonNormalSprite;
                _studentsTab.image.color = Color.white;
                if (_studentsTabLabel != null) _studentsTabLabel.color = ShowingStudents ? AccentCyan : MutedCyan;
            }

            _equipped.text = _squad?.SelectedStudent != null && _squad.SelectedStudent.IsActive
                ? $"<color=#{ColorUtility.ToHtmlStringRGB(FocusGold)}>EQUIPADO</color>\n<b>{_squad.SelectedStudent.StudentName}</b>\n\n<size=12><color=#{ColorUtility.ToHtmlStringRGB(AccentCyan)}>{VarginhaInputBindings.DisplayName(VarginhaInputAction.AllyCommand)}</color> para atacar</size>"
                : $"<color=#{ColorUtility.ToHtmlStringRGB(MutedCyan)}>NENHUM ALUNO EQUIPADO</color>\n<size=12>Selecione na aba Alunos</size>";

            if (!ShowingStudents)
            {
                for (int i = 0; i < 9; i++)
                {
                    bool owned = i < ItemNames.Length && _player != null && _player.HasInventoryItem(i);
                    bool selected = i == _inspected;
                    _cells[i].sprite = selected ? _slotSelectedSprite : _slotNormalSprite;
                    _cells[i].color = Color.white;
                    _cellPortraits[i].enabled = owned;
                    if (owned) _cellPortraits[i].sprite = VarginhaPixelArtSprites.Create(ItemArt[i], Color.gray);
                    _cellNames[i].text = owned ? ItemNames[i] : "";
                    _cellNames[i].color = PaperColor;
                    _states[i].text = owned
                        ? (i == 0 ? "EQUIPAMENTO"
                          : i == 5 ? (_player.IsFlashlightEquippedInHotbar ? "NA HOTBAR" : "NA MOCHILA")
                          : "GUARDADO")
                        : "VAZIO";
                    _states[i].color = owned
                        ? (i == 0 ? FocusGold
                          : i == 5 ? (_player.IsFlashlightEquippedInHotbar ? FocusGold : AccentCyan)
                          : AccentCyan)
                        : MutedCyan;
                }

                bool selectedOwned = _inspected < ItemNames.Length && _player != null && _player.HasInventoryItem(_inspected);
                _name.text = selectedOwned ? ItemNames[_inspected] : "Espaço livre";
                _name.color = selectedOwned ? AccentCyan : MutedCyan;
                _portrait.enabled = selectedOwned;
                if (selectedOwned) _portrait.sprite = VarginhaPixelArtSprites.Create(ItemArt[_inspected], Color.gray);
                _description.text = selectedOwned ? ItemDescriptions[_inspected] : "Nenhum objeto guardado neste espaço.";
                _description.color = PaperColor;
                if (_inspected == 5 && selectedOwned)
                {
                    bool inHotbar = _player.IsFlashlightEquippedInHotbar;
                    _availability.text = inHotbar
                        ? "Lanterna equipada na Hotbar! Selecione-a na barra rápida (tecla [6] ou clique) e ligue/desligue com [ESPAÇO], [ENTER] ou [V]."
                        : "Pressione [ENTER] ou clique abaixo para EQUIPAR a lanterna na Hotbar (barra rápida de itens).";
                    _availability.color = inHotbar ? FocusGold : AccentCyan;
                    _equip.interactable = true;
                    _equip.image.sprite = _buttonActiveSprite;
                    _equipCaption.text = inHotbar ? "DESEQUIPAR DA HOTBAR" : "EQUIPAR NA HOTBAR";
                    _equipCaption.color = PaperColor;
                }
                else
                {
                    _availability.text = "Objetos são usados ao interagir com o cenário. As pistas obtidas ficam salvas permanentemente no caderno.";
                    _availability.color = MutedCyan;
                    _equip.interactable = false;
                    _equip.image.sprite = _buttonNormalSprite;
                    _equipCaption.text = "USO NO CENÁRIO";
                    _equipCaption.color = MutedCyan;
                }
                return;
            }

            _portrait.enabled = true;
            _equipCaption.text = "EQUIPAR ALUNO";

            for (int i = 0; i < 9; i++)
            {
                _cellPortraits[i].enabled = true;
                _cellPortraits[i].sprite = VarginhaStudentAllySquad.Portrait(i);
                _cellNames[i].text = VarginhaPhase2Controller.StudentNames[i];
                _cellNames[i].color = PaperColor;

                var ally = _squad != null && i < _squad.Allies.Count ? _squad.Allies[i] : null;
                bool available = ally != null && ally.IsActive;
                bool equipped = available && _squad.SelectedStudentIndex == i;
                bool selected = i == _inspected;

                _cells[i].sprite = selected ? _slotSelectedSprite : _slotNormalSprite;
                _cells[i].color = Color.white;

                _states[i].text = !available ? "BLOQUEADO" : equipped ? "EQUIPADO" : (ally.ManualCooldownRemaining > 0
                    ? $"{ally.ManualCooldownRemaining:0.0}s" : "PRONTO");
                _states[i].color = equipped ? FocusGold : (available ? AccentCyan : MutedCyan);
            }

            string studentName = VarginhaPhase2Controller.StudentNames[_inspected];
            var selectedAlly = _squad != null && _inspected < _squad.Allies.Count ? _squad.Allies[_inspected] : null;
            bool unlocked = selectedAlly != null && selectedAlly.IsActive;

            _name.text = studentName;
            _name.color = unlocked ? AccentCyan : MutedCyan;
            _portrait.sprite = VarginhaStudentAllySquad.Portrait(_inspected);
            _description.text = VarginhaStudentAlly.DescribeAttack(studentName);
            _description.color = PaperColor;

            _availability.text = unlocked
                ? "Recarga individual: 5 segundos.\n\nTrocar de aluno não reinicia recargas. O comando de aliado chama o aluno equipado."
                : "Este aliado ainda não foi resgatado. Encontre a turma na escola; os especiais ficam ativos na igreja.";
            _availability.color = unlocked ? PaperColor : MutedCyan;

            _equip.interactable = unlocked;
            _equip.image.sprite = unlocked ? _buttonActiveSprite : _buttonNormalSprite;
            _equipCaption.color = unlocked ? PaperColor : MutedCyan;
        }

        private void Build()
        {
            EnsureTextures();
            _font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (_font == null) throw new System.InvalidOperationException("A mochila requer TMP Essential Resources (LiberationSans SDF).");

            _canvasObject = new GameObject("Mochila_Inventario", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvasObject.transform.SetParent(transform, false);

            var canvas = _canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            var scaler = _canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1120, 650);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            // Dim backdrop overlay matching Pause screen
            var backdrop = Panel(_canvasObject.transform, "Escurecer", 0, 0, 1120, 650, new Color(.005f, .01f, .015f, .78f));
            backdrop.rectTransform.anchorMin = Vector2.zero;
            backdrop.rectTransform.anchorMax = Vector2.one;
            backdrop.rectTransform.offsetMin = Vector2.zero;
            backdrop.rectTransform.offsetMax = Vector2.zero;
            backdrop.raycastTarget = true;

            // Main Modal Window framed with PixelHUDFrame metal style
            var page = new GameObject("PaginaModal", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image)).GetComponent<RectTransform>();
            page.SetParent(_canvasObject.transform, false);
            page.sizeDelta = new Vector2(1070, 610);
            page.anchorMin = page.anchorMax = new Vector2(.5f, .5f);
            var pageImg = page.GetComponent<UnityEngine.UI.Image>();
            pageImg.sprite = _modalFrameSprite;
            pageImg.type = UnityEngine.UI.Image.Type.Sliced;
            pageImg.color = Color.white;
            pageImg.raycastTarget = true;

            // Header matching PauseHeader style
            var title = Label(page, "MOCHILA", 36, 18, 500, 32, 24);
            title.fontStyle = FontStyles.Bold;
            title.color = PaperColor;
            Label(page, "INVESTIGAÇÃO • INVENTÁRIO E EQUIPE DE ALUNOS", 38, 48, 500, 20, 11).color = AccentCyan;

            // Tabs matching Pause button styling
            _itemsTab = CreateTabButton(page, "ITENS FÍSICOS", 600, 24, 200, 36, () => ShowTab(false), out _itemsTabLabel);
            _studentsTab = CreateTabButton(page, "ALUNOS", 814, 24, 200, 36, () => ShowTab(true), out _studentsTabLabel);

            // Top decorative divider line
            Separator(page, 32, 68, 1006, 2);

            // ================= LEFT COLUMN =================
            var leftPanel = Panel(page, "PainelEsquerdo", 32, 78, 230, 474, Color.white);
            leftPanel.sprite = _subPanelSprite;
            leftPanel.type = UnityEngine.UI.Image.Type.Sliced;

            Label(leftPanel.transform, "EQUIPAMENTO", 12, 12, 206, 20, 13, TextAlignmentOptions.Center).color = AccentCyan;
            var bagBox = Panel(leftPanel.transform, "MochilaBox", 61, 38, 108, 108, Color.white);
            bagBox.sprite = _slotNormalSprite;
            bagBox.type = UnityEngine.UI.Image.Type.Sliced;

            var bagIcon = Panel(bagBox.transform, "MochilaIcon", 10, 10, 88, 88, Color.white);
            bagIcon.sprite = VarginhaPixelArtSprites.Create("Backpack_Inventory", Color.gray);
            bagIcon.preserveAspect = true;

            _equipped = Label(leftPanel.transform, "", 12, 156, 206, 80, 14, TextAlignmentOptions.Center);

            Separator(leftPanel.transform, 16, 244, 198, 2);

            Label(leftPanel.transform, "ITENS FÍSICOS", 16, 254, 198, 18, 12).color = AccentCyan;
            _items = Label(leftPanel.transform, "", 16, 276, 198, 130, 13);
            _items.color = PaperColor;

            var footNote = Label(leftPanel.transform, "Itens usados saem.\nPistas permanecem.", 12, 424, 206, 36, 12, TextAlignmentOptions.Center);
            footNote.color = FocusGold;

            // ================= CENTER COLUMN (3x3 Grid) =================
            var centerPanel = Panel(page, "PainelCentro", 274, 78, 496, 474, Color.white);
            centerPanel.sprite = _subPanelSprite;
            centerPanel.type = UnityEngine.UI.Image.Type.Sliced;

            for (int i = 0; i < 9; i++)
            {
                int index = i;
                int col = i % 3;
                int row = i / 3;
                float x = 14 + col * 158;
                float y = 14 + row * 148;

                _cells[i] = Panel(centerPanel.transform, "Slot_" + i, x, y, 152, 142, Color.white);
                _cells[i].sprite = _slotNormalSprite;
                _cells[i].type = UnityEngine.UI.Image.Type.Sliced;
                _cells[i].raycastTarget = true;

                var button = _cells[i].gameObject.AddComponent<UnityEngine.UI.Button>();
                button.targetGraphic = _cells[i];
                button.navigation = new Navigation { mode = Navigation.Mode.None };
                button.onClick.AddListener(() => { _inspected = index; Refresh(); });

                // Frame for portrait
                var portBox = Panel(_cells[i].transform, "PortBox_" + i, 44, 8, 64, 64, new Color(0, 0, 0, .45f));
                var portrait = Panel(portBox.transform, "Retrato_" + i, 2, 2, 60, 60, Color.white);
                _cellPortraits[i] = portrait;
                portrait.sprite = VarginhaStudentAllySquad.Portrait(i);
                portrait.preserveAspect = true;

                _cellNames[i] = Label(_cells[i].transform, "", 4, 78, 144, 34, 13, TextAlignmentOptions.Center);
                _cellNames[i].color = PaperColor;

                _states[i] = Label(_cells[i].transform, "", 4, 114, 144, 20, 11, TextAlignmentOptions.Center);
            }

            // ================= RIGHT COLUMN (Inspection) =================
            var rightPanel = Panel(page, "PainelDireito", 782, 78, 256, 474, Color.white);
            rightPanel.sprite = _subPanelSprite;
            rightPanel.type = UnityEngine.UI.Image.Type.Sliced;

            _name = Label(rightPanel.transform, "", 12, 16, 232, 40, 20, TextAlignmentOptions.Center);
            _name.fontStyle = FontStyles.Bold;

            var inspectPortBox = Panel(rightPanel.transform, "InspectPortBox", 76, 62, 104, 104, Color.white);
            inspectPortBox.sprite = _slotNormalSprite;
            inspectPortBox.type = UnityEngine.UI.Image.Type.Sliced;

            _portrait = Panel(inspectPortBox.transform, "Retrato_Inspecionado", 4, 4, 96, 96, Color.white);
            _portrait.preserveAspect = true;

            _description = Label(rightPanel.transform, "", 16, 176, 224, 110, 14, TextAlignmentOptions.Center);

            Separator(rightPanel.transform, 16, 294, 224, 2);

            _availability = Label(rightPanel.transform, "", 16, 304, 224, 104, 12);

            _equip = Button(rightPanel.transform, "EQUIPAR ALUNO", 16, 420, 224, 40, Equip, out _equipCaption);

            // ================= FOOTER =================
            Separator(page, 32, 560, 1006, 2);

            Label(page, "<color=#a3c9cc>[SETAS]</color> Navegar    <color=#a3c9cc>[TAB]</color> Alternar Aba    <color=#a3c9cc>[ENTER]</color> Equipar    <color=#a3c9cc>[ESC] / [G]</color> Retomar",
                38, 570, 780, 26, 13);

            TMP_Text backLabel;
            Button(page, "VOLTAR", 894, 566, 144, 34, () => VarginhaGameHUD.Instance?.CloseBackpack(), out backLabel);

            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                _ownedEventSystem = new GameObject("Mochila_EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                _ownedEventSystem.transform.SetParent(transform, false);
                _ownedEventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
        }

        private static void EnsureTextures()
        {
            if (_modalFrameSprite != null && _modalFrameSprite.texture != null) return;

            _modalFrameSprite = CreateFramedSprite(64, 64, PanelColor, PanelBorder, true, new Vector4(8, 8, 8, 8));
            _subPanelSprite = CreateFramedSprite(32, 32, new Color(.028f, .036f, .044f, .96f), new Color(.18f, .26f, .30f, .85f), false, new Vector4(6, 6, 6, 6));
            _slotNormalSprite = CreateFramedSprite(32, 32, new Color(.042f, .056f, .068f, .96f), new Color(.22f, .32f, .36f, .85f), false, new Vector4(4, 4, 4, 4));
            _slotSelectedSprite = CreateFramedSprite(32, 32, new Color(.09f, .14f, .17f, .98f), AccentCyan, false, new Vector4(4, 4, 4, 4), true);
            _buttonNormalSprite = CreateFramedSprite(32, 32, PanelColor, PanelBorder, false, new Vector4(4, 4, 4, 4));
            _buttonActiveSprite = CreateButtonActiveSprite(32, 32, new Color(.10f, .15f, .17f, .98f), AccentCyan, new Vector4(4, 4, 4, 4));
            _separatorSprite = CreateSeparatorSprite();
        }

        private static Sprite CreateFramedSprite(int w, int h, Color bg, Color border, bool rivets, Vector4 slice, bool glow = false)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = $"HUD_Frame_{w}x{h}"
            };
            var pixels = new Color32[w * h];

            Color32 cBg = bg;
            Color32 cBorder = border;
            Color32 cShadow = new Color32(2, 3, 5, 200);
            Color32 cEdge = new Color32(6, 9, 12, 250);
            Color32 cHi = Color.Lerp(border, Color.white, .28f);
            Color32 cRivet = Color.Lerp(border, Color.white, .5f);

            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int val = (x * 17 + y * 29 + x * y * 3) % 23;
                int grain = val < 3 ? 10 : (val > 18 ? -12 : 0);
                byte r = (byte)Mathf.Clamp(cBg.r + grain, 0, 255);
                byte g = (byte)Mathf.Clamp(cBg.g + grain, 0, 255);
                byte b = (byte)Mathf.Clamp(cBg.b + grain, 0, 255);
                pixels[y * w + x] = new Color32(r, g, b, cBg.a);
            }

            // Outer edge & border
            for (int x = 0; x < w; x++)
            {
                pixels[x] = cShadow;
                pixels[(h - 1) * w + x] = cShadow;
                if (h > 2) { pixels[w + x] = cEdge; pixels[(h - 2) * w + x] = cEdge; }
                if (h > 4) { pixels[2 * w + x] = cBorder; pixels[(h - 3) * w + x] = cBorder; }
            }
            for (int y = 0; y < h; y++)
            {
                pixels[y * w] = cShadow;
                pixels[y * w + w - 1] = cShadow;
                if (w > 2) { pixels[y * w + 1] = cEdge; pixels[y * w + w - 2] = cEdge; }
                if (w > 4) { pixels[y * w + 2] = cBorder; pixels[y * w + w - 3] = cBorder; }
            }

            // Top-left highlight bevel
            for (int x = 3; x < w - 3; x++) pixels[(h - 4) * w + x] = cHi;
            for (int y = 3; y < h - 3; y++) pixels[y * w + 3] = cHi;

            if (rivets && w >= 32 && h >= 32)
            {
                int[] rx = { 6, w - 8 };
                int[] ry = { 6, h - 8 };
                foreach (int ox in rx) foreach (int oy in ry)
                {
                    pixels[oy * w + ox] = cRivet;
                    pixels[oy * w + ox + 1] = cRivet;
                    pixels[(oy + 1) * w + ox] = cRivet;
                    pixels[(oy + 1) * w + ox + 1] = cRivet;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect, slice);
        }

        private static Sprite CreateButtonActiveSprite(int w, int h, Color bg, Color border, Vector4 slice)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "HUD_BtnActive"
            };
            var pixels = new Color32[w * h];
            Color32 cBg = bg;
            Color32 cBorder = border;

            for (int i = 0; i < pixels.Length; i++) pixels[i] = cBg;

            // Border
            for (int x = 0; x < w; x++)
            {
                pixels[x] = cBorder;
                pixels[(h - 1) * w + x] = cBorder;
            }
            for (int y = 0; y < h; y++)
            {
                pixels[y * w] = cBorder;
                pixels[y * w + w - 1] = cBorder;
            }

            // Vertical 2px accent bar on left edge (matching PauseButton)
            for (int y = 2; y < h - 2; y++)
            {
                pixels[y * w + 2] = cBorder;
                pixels[y * w + 3] = cBorder;
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect, slice);
        }

        private static Sprite CreateSeparatorSprite()
        {
            var tex = new Texture2D(16, 2, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "HUD_Sep"
            };
            var pixels = new Color32[32];
            Color32 top = PanelBorder;
            Color32 bot = new Color32(10, 14, 18, 200);
            for (int i = 0; i < 16; i++) { pixels[16 + i] = top; pixels[i] = bot; }
            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, 16, 2), new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(2, 0, 2, 0));
        }

        private static RectTransform Place(GameObject go, Transform parent, float x, float y, float w, float h)
        {
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(w, h);
            return rect;
        }

        private static UnityEngine.UI.Image Panel(Transform parent, string name, float x, float y, float w, float h, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
            Place(go, parent, x, y, w, h);
            var graphic = go.GetComponent<UnityEngine.UI.Image>();
            graphic.color = color;
            graphic.raycastTarget = false;
            return graphic;
        }

        private static void Separator(Transform parent, float x, float y, float w, float h)
        {
            var sep = Panel(parent, "Separador", x, y, w, h, Color.white);
            sep.sprite = _separatorSprite;
            sep.type = UnityEngine.UI.Image.Type.Sliced;
        }

        private TMP_Text Label(Transform parent, string value, float x, float y, float w, float h, int size,
            TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft)
        {
            var go = new GameObject("Texto", typeof(RectTransform), typeof(CanvasRenderer));
            Place(go, parent, x, y, w, h);
            var label = go.AddComponent<TextMeshProUGUI>();
            label.font = _font;
            label.fontSize = size;
            label.enableAutoSizing = false;
            label.color = PaperColor;
            label.raycastTarget = false;
            label.alignment = alignment;
            label.text = value;
            return label;
        }

        private UnityEngine.UI.Button CreateTabButton(Transform parent, string caption, float x, float y, float w, float h,
            UnityEngine.Events.UnityAction action, out TMP_Text label)
        {
            var graphic = Panel(parent, "Tab_" + caption, x, y, w, h, Color.white);
            graphic.sprite = _buttonNormalSprite;
            graphic.type = UnityEngine.UI.Image.Type.Sliced;
            graphic.raycastTarget = true;

            var button = graphic.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = graphic;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(action);

            label = Label(graphic.transform, caption, 0, 0, w, h, 14, TextAlignmentOptions.Center);
            label.fontStyle = FontStyles.Bold;
            label.color = MutedCyan;
            return button;
        }

        private UnityEngine.UI.Button Button(Transform parent, string caption, float x, float y, float w, float h,
            UnityEngine.Events.UnityAction action, out TMP_Text label)
        {
            var graphic = Panel(parent, "Botao_" + caption, x, y, w, h, Color.white);
            graphic.sprite = _buttonNormalSprite;
            graphic.type = UnityEngine.UI.Image.Type.Sliced;
            graphic.raycastTarget = true;

            var button = graphic.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = graphic;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(action);

            label = Label(graphic.transform, caption, 0, 0, w, h, 14, TextAlignmentOptions.Center);
            label.fontStyle = FontStyles.Bold;
            label.color = PaperColor;
            return button;
        }
    }
}
