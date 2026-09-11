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
        FuseBox         // Caixa de força / eletricidade
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
            if (!CanInteract) return;
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
                        var action = edelzio.GetComponent<VarginhaPlayerActionAnimation>();
                        if (action != null)
                            action.PlayNotebookSession(transform, () => GetQuiz()?.Open(edelzio, this));
                        else
                            quiz.Open(edelzio, this);
                        return;
                    }

                    // Proteção para cenas antigas que ainda não receberam o gerenciador do quiz.
                    edelzio.HasDecodedData = true;
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
                    var fusca = GetComponent<FuscaLevelExit>();
                    if (fusca != null)
                    {
                        fusca.TryEscape(edelzio);
                        return;
                    }
                    break;

                case PropType.OldDocument:
                    message = "📜 Documento Antigo de 1898:\n'Relatório de Zé Gomes: Encontramos algo nas cavernas que não deveria ter sido acordado.'";
                    GetHud()?.ShowDialogue("Pista Histórica", message);
                    break;

                case PropType.FuseBox:
                    message = "⚡ Caixa de Fusíveis: A fiação está sobrecarregada por pulsos eletromagnéticos impossíveis.";
                    GetHud()?.ShowDialogue("Investigação", message);
                    break;
            }

            OnInteracted?.Invoke(edelzio);
        }

        public void CompleteNotebookPuzzle(EdelzioTopDownController edelzio)
        {
            _hasInteracted = true;
            // O notebook físico sai da mesa porque agora está sob o braço do Edelzio.
            HideCollectedWorldObject();
            OnInteracted?.Invoke(edelzio);
        }

        private void HideCollectedWorldObject()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            var collider = GetComponent<Collider2D>();
            if (collider != null) collider.enabled = false;
        }
    }
}
