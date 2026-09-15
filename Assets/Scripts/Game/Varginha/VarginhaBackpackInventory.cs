using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Game.Varginha
{
    /// <summary>Pause-owned backpack: inspecting a student never spends a power or its cooldown.</summary>
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
        private TMP_Text _equipCaption;
        private static readonly string[] ItemNames = { "Mochila", "Chave do Fusca", "Caderno de 1996", "Notebook", "Documento de 1898" };
        private static readonly string[] ItemArt = { "Backpack_Inventory", "Inventory_Key", "Inventory_Journal", "Notebook_Inventory", "Doc_Inventory" };
        private static readonly string[] ItemDescriptions = { "Mochila cinza de Edelzio. Guarda os objetos da investigação e permite organizar os especiais da turma.",
            "Abre o Fusca. É usada somente ao confirmar a saída com o caderno de pesquisas.",
            "As anotações de 1996. Edelzio precisa delas para iniciar a viagem.",
            "Notebook cinza usado na decodificação. As informações obtidas ficam registradas após o uso.",
            "Documento histórico. A pista permanece conhecida depois da leitura." };
        private TMP_Text _name, _description, _availability, _equipped, _items;
        private UnityEngine.UI.Image _portrait;
        private UnityEngine.UI.Button _equip;
        private static readonly Color Ivory = new(.89f, .86f, .75f);
        private static readonly Color Gold = new(.72f, .58f, .32f);

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
            for (int i = 1; i < 5; i++) if (player.HasInventoryItem(i)) physical += "\n" + names[i];
            _items.text = "ITENS FÍSICOS\n" + (physical.Length == 0 ? "\nNenhum item pendente." : physical);
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
            { VarginhaGameHUD.Instance?.CloseBackpack(); return; }
            if (keyboard.tabKey.wasPressedThisFrame) { ShowTab(!ShowingStudents); return; }
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
            if (ShowingStudents && _squad != null && _squad.SelectStudent(_inspected)) Refresh();
        }

        public void ShowTab(bool students)
        {
            ShowingStudents = students;
            _inspected = students ? Mathf.Max(0, _squad != null ? _squad.SelectedStudentIndex : 0) : 0;
            Refresh();
        }

        private void Refresh()
        {
            _itemsTab.image.color = ShowingStudents ? new Color(.10f,.11f,.13f) : new Color(.29f,.26f,.20f);
            _studentsTab.image.color = ShowingStudents ? new Color(.29f,.26f,.20f) : new Color(.10f,.11f,.13f);
            _equipped.text = _squad?.SelectedStudent != null && _squad.SelectedStudent.IsActive
                ? "EQUIPADO\n" + _squad.SelectedStudent.StudentName + "\n\n" + VarginhaInputBindings.DisplayName(VarginhaInputAction.AllyCommand) + " para atacar"
                : "NENHUM ALUNO\nEQUIPADO\n\nEscolha na aba Alunos";
            if (!ShowingStudents)
            {
                for (int i = 0; i < 9; i++)
                {
                    bool owned = i < 5 && _player != null && _player.HasInventoryItem(i);
                    _cells[i].color = i == _inspected ? new Color(.22f,.22f,.23f) : new Color(.075f,.08f,.10f);
                    _cellPortraits[i].enabled = owned;
                    if (owned) _cellPortraits[i].sprite = VarginhaPixelArtSprites.Create(ItemArt[i], Color.gray);
                    _cellNames[i].text = owned ? ItemNames[i] : "";
                    _states[i].text = owned ? i == 0 ? "EQUIPAMENTO" : "GUARDADO" : "VAZIO";
                    _states[i].color = owned ? Ivory : Color.gray;
                }
                bool selectedOwned = _inspected < 5 && _player != null && _player.HasInventoryItem(_inspected);
                _name.text = selectedOwned ? ItemNames[_inspected] : "Espaço livre";
                _portrait.enabled = selectedOwned;
                if (selectedOwned) _portrait.sprite = VarginhaPixelArtSprites.Create(ItemArt[_inspected], Color.gray);
                _description.text = selectedOwned ? ItemDescriptions[_inspected] : "Nenhum objeto guardado neste espaço.";
                _availability.text = "Objetos são usados ao interagir com o cenário. Depois do uso, saem do inventário. As pistas da história são preservadas.";
                _equip.interactable = false; _equipCaption.text = "USO NO CENÁRIO";
                return;
            }
            _portrait.enabled = true;
            _equipCaption.text = "EQUIPAR ALUNO";
            for (int i = 0; i < 9; i++)
            {
                _cellPortraits[i].enabled = true;
                _cellPortraits[i].sprite = VarginhaStudentAllySquad.Portrait(i);
                _cellNames[i].text = VarginhaPhase2Controller.StudentNames[i];
                var ally = _squad != null && i < _squad.Allies.Count ? _squad.Allies[i] : null;
                bool available = ally != null && ally.IsActive;
                bool equipped = available && _squad.SelectedStudentIndex == i;
                _cells[i].color = i == _inspected ? new Color(.22f, .22f, .23f) : new Color(.075f, .08f, .10f);
                _states[i].text = !available ? "BLOQUEADO" : equipped ? "EQUIPADO" : ally.ManualCooldownRemaining > 0
                    ? $"{ally.ManualCooldownRemaining:0.0}s" : "DISPONÍVEL";
                _states[i].color = equipped ? Gold : available ? Ivory : Color.gray;
            }
            string studentName = VarginhaPhase2Controller.StudentNames[_inspected];
            var selected = _squad != null && _inspected < _squad.Allies.Count ? _squad.Allies[_inspected] : null;
            bool unlocked = selected != null && selected.IsActive;
            _name.text = studentName;
            _portrait.sprite = VarginhaStudentAllySquad.Portrait(_inspected);
            _description.text = VarginhaStudentAlly.DescribeAttack(studentName);
            _availability.text = unlocked
                ? "Recarga individual: 5 segundos.\n\nTrocar de aluno não reinicia recargas. O comando chama somente o aluno equipado."
                : "Este aliado ainda não está disponível nesta fase. Resgate a turma na escola; os especiais ficam disponíveis na igreja.";
            _equip.interactable = unlocked;
        }

        private void Build()
        {
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
            var backdrop = Panel(_canvasObject.transform, "Escurecer", 0, 0, 1120, 650, new Color(.007f, .009f, .018f, .97f));
            backdrop.rectTransform.anchorMin = Vector2.zero; backdrop.rectTransform.anchorMax = Vector2.one;
            backdrop.rectTransform.offsetMin = Vector2.zero; backdrop.rectTransform.offsetMax = Vector2.zero;
            backdrop.raycastTarget = true;
            var page = new GameObject("Pagina", typeof(RectTransform)).GetComponent<RectTransform>();
            page.SetParent(_canvasObject.transform, false); page.sizeDelta = new Vector2(1120, 650);
            page.anchorMin = page.anchorMax = new Vector2(.5f, .5f);
            Label(page, "MOCHILA", 36, 24, 1048, 48, 30, TextAlignmentOptions.Center);
            _itemsTab = Button(page, "ITENS FÍSICOS", 340, 76, 208, 29, () => ShowTab(false));
            _studentsTab = Button(page, "ALUNOS", 572, 76, 208, 29, () => ShowTab(true));
            Line(page, 38, 113, 1044, 1); Line(page, 38, 577, 1044, 1);
            Line(page, 266, 132, 1, 424); Line(page, 739, 132, 1, 424);
            // Original, restrained ornaments echo the reference's framed inventory layout.
            foreach (float x in new[] { 38f, 1080f }) foreach (float y in new[] { 112f, 576f })
                Panel(page, "Losango", x - 4, y - 4, 8, 8, Gold).transform.localRotation = Quaternion.Euler(0, 0, 45);
            var bag = Panel(page, "Mochila", 88, 144, 126, 126, Color.white);
            bag.sprite = VarginhaPixelArtSprites.Create("Backpack_Inventory", Color.gray); bag.preserveAspect = true;
            _equipped = Label(page, "", 54, 290, 192, 105, 18, TextAlignmentOptions.Center);
            _items = Label(page, "", 54, 405, 192, 118, 15);
            Label(page, "Itens usados saem.\nPistas permanecem.", 54, 531, 192, 34, 14).color = Gold;
            for (int i = 0; i < 9; i++)
            {
                int index = i;
                float x = 291 + (i % 3) * 144, y = 139 + (i / 3) * 139;
                _cells[i] = Panel(page, "Aluno_" + i, x, y, 132, 127, new Color(.075f, .08f, .10f));
                _cells[i].raycastTarget = true;
                var button = _cells[i].gameObject.AddComponent<UnityEngine.UI.Button>();
                button.targetGraphic = _cells[i]; button.navigation = new Navigation { mode = Navigation.Mode.None };
                button.onClick.AddListener(() => { _inspected = index; Refresh(); });
                var portrait = Panel(page, "Retrato_" + i, x + 34, y + 7, 64, 64, Color.white);
                _cellPortraits[i] = portrait;
                portrait.sprite = VarginhaStudentAllySquad.Portrait(i); portrait.preserveAspect = true;
                _cellNames[i] = Label(page, "", x + 4, y + 72, 124, 34, 14, TextAlignmentOptions.Center);
                _states[i] = Label(page, "", x + 3, y + 107, 126, 18, 12, TextAlignmentOptions.Center);
            }
            _name = Label(page, "", 767, 139, 291, 55, 26, TextAlignmentOptions.Center);
            _portrait = Panel(page, "Retrato_Inspecionado", 863, 201, 100, 100, Color.white); _portrait.preserveAspect = true;
            _description = Label(page, "", 767, 312, 291, 104, 19, TextAlignmentOptions.Center);
            _availability = Label(page, "", 780, 425, 270, 88, 15);
            _equip = Button(page, "EQUIPAR ALUNO", 790, 521, 243, 37, Equip);
            _equipCaption = page.GetChild(page.childCount-1).GetComponent<TMP_Text>();
            Label(page, "SETAS: navegar   TAB: aba   ENTER: equipar   ESC / G: fechar", 42, 594, 840, 28, 16);
            Button(page, "VOLTAR", 902, 590, 175, 36, () => VarginhaGameHUD.Instance?.CloseBackpack());
            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                _ownedEventSystem = new GameObject("Mochila_EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                _ownedEventSystem.transform.SetParent(transform, false);
                _ownedEventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
        }

        private static RectTransform Place(GameObject go, Transform parent, float x, float y, float w, float h)
        {
            var rect = go.GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(w, h); return rect;
        }
        private static UnityEngine.UI.Image Panel(Transform parent, string name, float x, float y, float w, float h, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
            Place(go, parent, x, y, w, h); var graphic = go.GetComponent<UnityEngine.UI.Image>();
            graphic.color = color; graphic.raycastTarget = false; return graphic;
        }
        private static void Line(Transform parent, float x, float y, float w, float h) => Panel(parent, "Filete", x, y, w, h, Gold);
        private TMP_Text Label(Transform parent, string value, float x, float y, float w, float h, int size,
            TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft)
        {
            var go = new GameObject("Texto", typeof(RectTransform), typeof(CanvasRenderer));
            Place(go, parent, x, y, w, h);
            var label = go.AddComponent<TextMeshProUGUI>(); label.font = _font; label.fontSize = size;
            label.enableAutoSizing = false; label.color = Ivory; label.raycastTarget = false;
            label.alignment = alignment; label.text = value; return label;
        }
        private UnityEngine.UI.Button Button(Transform parent, string caption, float x, float y, float w, float h, UnityEngine.Events.UnityAction action)
        {
            var graphic = Panel(parent, caption, x, y, w, h, new Color(.22f, .20f, .16f)); graphic.raycastTarget = true;
            var button = graphic.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = graphic;
            button.navigation = new Navigation { mode = Navigation.Mode.None }; button.onClick.AddListener(action);
            Label(parent, caption, x, y, w, h, 17, TextAlignmentOptions.Center); return button;
        }
    }
}
