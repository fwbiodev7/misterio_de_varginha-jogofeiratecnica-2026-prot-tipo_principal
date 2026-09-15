using UnityEngine;
using System;

namespace Game.Varginha
{
    public enum PropType
    {
        ToyBoxUnderBed, // Caixa antiga de 1996 com Caderno e Chave
        Backpack,       // Mochila do Edelzio
        NotebookLaptop, // Notebook para decodificar
        CoffeeOrFood,   // Café que restaura sanidade
        FuscaVehicle,   // Fusca para escapar
        OldDocument,    // Pista opcional
        FuseBox,        // Caixa de força / eletricidade
        PadreFabio,     // Padre Fábio e o Livro do Tombo Secreto
        SecretTome      // Livro do Tombo da diocese
    }

    /// <summary>
    /// Objeto interativo de investigação no cenário estilo Pokémon / RPG clássico.
    /// Pressione E ou Espaço próximo ao objeto para examinar.
    /// </summary>
    public class InteractableProp : MonoBehaviour
    {
        [Header("Prop Info")]
        [SerializeField] private PropType propType;
        [SerializeField] private string propName = "Objeto Misterioso";
        [TextArea(2, 4)]
        [SerializeField] private string inspectMessage = "Você examina o objeto.";

        [Header("State")]
        [SerializeField] private bool canInteractMultipleTimes = false;
        private bool _hasInteracted;

        public PropType Type => propType;
        public string PropName => propName;
        public string InspectMessage => inspectMessage;
        public bool CanInteract => !_hasInteracted || canInteractMultipleTimes;

        public event Action<EdelzioTopDownController> OnInteracted;

        /// <summary>Configura props criadas pelo construtor de uma fase runtime.</summary>
        public void Configure(PropType type, string name, string message, bool multipleTimes = false)
        {
            propType = type;
            propName = string.IsNullOrWhiteSpace(name) ? "Objeto Misterioso" : name;
            inspectMessage = string.IsNullOrWhiteSpace(message) ? "Você examina o objeto." : message;
            canInteractMultipleTimes = multipleTimes;
            _hasInteracted = false;
            EnsureVisualPresentation();
        }

        private void Awake()
        {
            EnsureVisualPresentation();
        }

        /// <summary>
        /// Corrige props de cenas antigas que perderam o sprite ou foram salvos com
        /// ordem de desenho atrás do piso. O fallback é procedural e mantém o estilo
        /// pixel art do jogo, sem reexibir itens já coletados.
        /// </summary>
        private void EnsureVisualPresentation()
        {
            if (!CanInteract) return;

            var renderer = GetComponent<SpriteRenderer>();
            if (renderer == null) renderer = gameObject.AddComponent<SpriteRenderer>();

            Sprite fallback = null;
            int sortingOrder = renderer.sortingOrder;
            switch (propType)
            {
                case PropType.ToyBoxUnderBed:
                    fallback = VarginhaPixelArtSprites.Create("ToyBox_Prop", new Color(.55f, .30f, .15f));
                    sortingOrder = Mathf.Max(sortingOrder, 4);
                    break;
                case PropType.Backpack:
                    fallback = VarginhaPixelArtSprites.Create("Backpack_Prop", new Color(.46f, .48f, .50f));
                    sortingOrder = Mathf.Max(sortingOrder, 7);
                    break;
                case PropType.NotebookLaptop:
                    fallback = VarginhaPixelArtSprites.Create("Notebook_TI", new Color(.18f, .52f, .70f));
                    sortingOrder = Mathf.Max(sortingOrder, 7);
                    break;
                case PropType.CoffeeOrFood:
                    fallback = VarginhaPixelArtSprites.Create("Coffee_Hot", new Color(.72f, .40f, .18f));
                    sortingOrder = Mathf.Max(sortingOrder, 6);
                    break;
                case PropType.FuscaVehicle:
                    // Preserva a folha de sprites do Fusca quando ela existe.
                    if (renderer.sprite == null)
                        fallback = VarginhaPixelArtSprites.Create("Fusca_Fallback", new Color(.20f, .60f, .85f));
                    sortingOrder = Mathf.Max(sortingOrder, 5);
                    break;
                case PropType.OldDocument:
                    fallback = VarginhaPixelArtSprites.Create("Doc_Prop", new Color(.78f, .67f, .42f));
                    sortingOrder = Mathf.Max(sortingOrder, 6);
                    break;
                case PropType.FuseBox:
                    fallback = VarginhaPixelArtSprites.Create("FuseBox_Prop", new Color(.35f, .42f, .48f));
                    sortingOrder = Mathf.Max(sortingOrder, 5);
                    break;
                case PropType.PadreFabio:
                    fallback = VarginhaPixelArtSprites.Create("Padre_Fabio", new Color(.52f, .40f, .28f));
                    sortingOrder = Mathf.Max(sortingOrder, 7);
                    break;
                case PropType.SecretTome:
                    fallback = VarginhaPixelArtSprites.Create("Tome_Prop", new Color(.48f, .22f, .12f));
                    sortingOrder = Mathf.Max(sortingOrder, 7);
                    break;
            }

            if (fallback != null) renderer.sprite = fallback;
            renderer.enabled = true;
            renderer.sortingOrder = sortingOrder;

            foreach (var collider in GetComponents<Collider2D>()) collider.enabled = true;
            var body = GetComponent<Rigidbody2D>();
            if (body != null) body.simulated = true;
        }

        // Fallbacks evitam que uma referência estática perdida por reload de domínio silencie as interfaces.
        private static VarginhaGameHUD GetHud()
        {
            return VarginhaGameHUD.Instance != null
                ? VarginhaGameHUD.Instance
                : UnityEngine.Object.FindAnyObjectByType<VarginhaGameHUD>();
        }

        private static VarginhaNotebookQuiz GetQuiz()
        {
            return VarginhaNotebookQuiz.Instance != null
                ? VarginhaNotebookQuiz.Instance
                : UnityEngine.Object.FindAnyObjectByType<VarginhaNotebookQuiz>();
        }

        public void Interact(EdelzioTopDownController edelzio)
        {
            if (!CanInteract || edelzio == null || edelzio.IsInputLocked) return;
            _hasInteracted = true;

            string message = inspectMessage;

            switch (propType)
            {
                case PropType.ToyBoxUnderBed:
                    GetComponent<ToyBoxOpenAnimation>()?.PlayOpen(edelzio);
                    edelzio.HasFuscaKey = true;
                    edelzio.HasResearchNotebook = true;
                    message = "📦 Você puxa a caixa debaixo da cama...\nEncontrou a CHAVE DO FUSCA e o CADERNO DE 1996!\nNa última página está escrito: 'ELA AINDA ESTÁ AQUI'!";
                    GetHud()?.ShowDialogue("Edelzio", message);
                    GetHud()?.ShowRodrigoHint("Rodrigo: 'O que foi isso?! As luzes começaram a piscar! Pegue seu notebook e vá até o Fusca no quintal rápido!'");
                    EntityManifestationAI.AwakenEntity();
                    break;

                case PropType.Backpack:
                    edelzio.HasBackpack = true;
                    var pickupAnimation = GetComponent<BackpackPickupAnimation>();
                    if (pickupAnimation != null) pickupAnimation.PlayPickup(edelzio);
                    else
                    {
                        edelzio.EquipBackpack();
                        HideCollectedWorldObject();
                    }
                    message = "🎒 Você pegou a sua MOCHILA! Seus itens e caderno agora estão guardados em segurança.";
                    GetHud()?.ShowDialogue("Edelzio", message);
                    break;

                case PropType.NotebookLaptop:
                    if (edelzio.HasDecodedData)
                    {
                        GetHud()?.ShowDialogue("NOTEBOOK", "O notebook ja esta decodificado. As coordenadas apontam para o Fusca.");
                        break;
                    }
                    var quiz = GetQuiz();
                    if (quiz != null)
                    {
                        // Sair do quiz sem concluir deve permitir tentar novamente.
                        _hasInteracted = false;
                        var action = edelzio.GetComponent<VarginhaPlayerActionAnimation>();
                        if (action != null)
                            action.PlayNotebookSession(transform, () => GetQuiz()?.Open(edelzio, this));
                        else
                            quiz.Open(edelzio, this);
                        return;
                    }

                    // Proteção para cenas antigas que ainda não receberam o gerenciador do quiz.
                    edelzio.HasDecodedData = true;
                    edelzio.TryConsumeInventoryItem(3);
                    HideCollectedWorldObject();
                    message = "O notebook terminou a decodificacao. As coordenadas apontam para o Fusca.";
                    GetHud()?.ShowDialogue("NOTEBOOK", message);
                    GetHud()?.ShowRodrigoHint("Rodrigo: 'Pegue o Fusca antes que a entidade bloqueie o caminho.'");
                    break;

                case PropType.CoffeeOrFood:
                    var coffeeAction = edelzio.GetComponent<VarginhaPlayerActionAnimation>();
                    if (coffeeAction != null)
                    {
                        coffeeAction.PlayDrinkCoffee(transform, () =>
                        {
                            edelzio.RestoreOneHeart();
                            var cupRenderer = GetComponent<SpriteRenderer>();
                            if (cupRenderer != null)
                                cupRenderer.sprite = VarginhaPixelArtSprites.Create("Coffee_Empty", new Color(.8f, .4f, .2f));
                            GetHud()?.ShowDialogue("Edelzio", "☕ Você bebe o café e devolve a xícara vazia à mesa.\n+1 CORAÇÃO DE SAÚDE.");
                        });
                        return;
                    }
                    edelzio.RestoreOneHeart();
                    message = "☕ Você bebe o café quente. (+1 CORAÇÃO DE SAÚDE)";
                    GetHud()?.ShowDialogue("Edelzio", message);
                    break;

                case PropType.FuscaVehicle:
                    _hasInteracted = false; // A locked car must be retryable after collecting its prerequisites.
                    var fusca = GetComponent<FuscaLevelExit>();
                    if (fusca != null)
                    {
                        fusca.TryEscape(edelzio);
                        return;
                    }
                    break;

                case PropType.OldDocument:
                    edelzio.HasHistoricalDocument = true;
                    edelzio.TryConsumeInventoryItem(4);
                    HideCollectedWorldObject();
                    message = "📜 Documento Antigo de 1898:\n'Relatório de Zé Gomes: Encontramos algo nas cavernas que não deveria ter sido acordado.'";
                    GetHud()?.ShowDialogue("Pista Histórica", message);
                    break;

                case PropType.FuseBox:
                    message = "⚡ Caixa de Fusíveis: A fiação está sobrecarregada por pulsos eletromagnéticos impossíveis.";
                    GetHud()?.ShowDialogue("Investigação", message);
                    break;

                case PropType.PadreFabio:
                    message = inspectMessage;
                    GetHud()?.ShowDialogue("Padre Fábio", message);
                    break;

                case PropType.SecretTome:
                    message = inspectMessage;
                    GetHud()?.ShowDialogue("Livro do Tombo Secreto", message);
                    break;
            }

            OnInteracted?.Invoke(edelzio);
        }

        public void CompleteNotebookPuzzle(EdelzioTopDownController edelzio)
        {
            _hasInteracted = true;
            edelzio.TryConsumeInventoryItem(3);
            // O notebook físico sai da mesa porque agora está sob o braço do Edelzio.
            HideCollectedWorldObject();
            OnInteracted?.Invoke(edelzio);
        }

        private void HideCollectedWorldObject()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            foreach (var collider in GetComponents<Collider2D>()) collider.enabled = false;
            var body = GetComponent<Rigidbody2D>();
            if (body != null) { body.linearVelocity = Vector2.zero; body.simulated = false; }
        }
    }
}
