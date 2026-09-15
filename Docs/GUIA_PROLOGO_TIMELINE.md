# Prólogo de 1996 — guia de cutscene com Timeline

Guia para o roteiro de Edelzio criança: exploração da casa, televisão com noticiário, falha elétrica, brilho na janela, perda gradual de controle, feixe de luz, figura indistinta, corte e despertar na cama.

**Entrega:** instruções de montagem e três scripts de exemplo. Os arquivos estão em `Docs/Exemplos/PrologoTimeline`; cenas, animações e assets de Timeline ainda precisam ser criados e conectados no Editor. O prólogo continua como conteúdo planejado no [GDD](GDD.md).

## 1. Estrutura narrativa e duração

O início é jogável e não tem cronômetro obrigatório. O jogador pode ler o jornal e examinar a TV e o computador. Como proposta de encenação, examinar o computador provoca a anomalia e habilita um gatilho próximo da janela. Ao caminhar até esse brilho, começa a perda de controle.

| Etapa | Duração proposta | O que acontece |
| --- | --- | --- |
| Exploração | Livre | Edelzio anda, corre e interage; o noticiário toca ao fundo. |
| Primeiro sinal | Até se aproximar da janela | TV perde o sinal, surge estática e o brilho aparece. O jogador ainda controla o garoto. |
| Perda de controle | 1,5 s | Os comandos ficam progressivamente mais fracos; luzes oscilam. |
| Timeline: 0–5 s | 5 s | Edelzio é conduzido da janela ao quintal, usando o ciclo Run em ritmo de caminhada. |
| Timeline: 5–8 s | 3 s | O garoto para em Idle e o feixe cresce sobre ele. |
| Timeline: 8–11 s | 3 s | A visão fica encoberta; uma silhueta aparece por instantes. |
| Timeline: 11 s | Corte | Tela preta instantânea e corte do áudio. |
| Timeline: 11,3–15 s | 3,7 s | Plano da cama: Edelzio criança desperta, ainda em 1996. |
| Timeline: 15–20 s | 5 s | Escurecimento e título “30 anos depois”; término do prólogo. |
| Depois da Timeline | Conforme carregamento | Abre `FaseTopView_Varginha`, já com Edelzio adulto. |

Esses tempos são uma proposta de direção para o roteiro fornecido. A mudança de cena acompanha o encerramento real da Timeline; aumentar a duração do asset não exige alterar uma espera fixa no script.

## 2. Preparação no projeto

O projeto já declara Unity **6000.6.0f1**, Timeline **6.6.0**, Input System **1.20.0** e uGUI **2.6.0**. Há assemblies de TextMeshPro no ambiente local. Confira esses pacotes no Package Manager da instalação usada pelo grupo.

1. Crie uma cena `Prologo_1996` em `Assets/Scenes`.
2. Prepare uma versão da casa em 1996: TV, jornal, computador antigo, janela, quintal e cama.
3. Crie sprites específicos de Edelzio criança: Idle, Run e despertar. O atlas adulto atual pode servir de placeholder identificado durante os testes.
4. Crie uma pasta `Assets/Timelines/Prologo1996` para os assets e clips.
5. Copie os três exemplos para `Assets/Scripts/Game/Varginha/Prologue` quando for implementar. Eles usam o mapa remapeável `VarginhaInputBindings` já presente no projeto.
6. Selecione `Assets/Scripts/Game/Game.asmdef` e acrescente **Unity.TextMeshPro** em Assembly Definition References para compilar `PrologueInteractable`. Preserve as referências existentes; Unity.InputSystem já está listado. Os scripts usam PlayableDirector do módulo de engine, sem tipos C# do pacote Timeline.

### Hierarquia proposta

```text
Prologo_1996
├── Casa_1996
│   ├── TV_Noticiario / TV_Estatica / Jornal / Computador
│   ├── Brilho_Janela
│   └── Quintal / Cama
├── CaptureOrigin                 Animator; escala 1, rotação 0
│   └── MotionRig                 Transform; escala 1, rotação 0
│       └── EdelzioCrianca        Rigidbody2D + Collider2D + PrologueChildController
│           └── Visual            SpriteRenderer + Animator
├── CameraRig                     Animator
│   └── Main Camera               Ortográfica + AudioListener
├── Despertar1996                 Grupo visual da cama/criança; inicialmente inativo
├── Feixe / Silhueta              Objetos visuais para a abdução
├── Timeline_Anomalia             PlayableDirector: TL_Anomalia, 1,5 s
├── Timeline_Prologo              PlayableDirector + SignalReceiver + PrologueSequence
│                                BoxCollider2D trigger próximo da janela
└── Canvas_Prologo                Screen Space - Overlay
    ├── LegendaExploracao         TextMeshProUGUI
    ├── Overlay_Foco              Animator + CanvasGroup + Image translúcida
    ├── CortePreto                Animator + CanvasGroup + Image preta
    ├── Titulo                    Animator + CanvasGroup
    │   └── Texto                 TextMeshProUGUI: “30 anos depois”
    └── LoadingCover              CanvasGroup + Image preta; sem Animator
```

Deixe `Timeline_Prologo` inicialmente **inativo** e habilite-o após a interação que inicia os fenômenos. Seus objetos de personagem e Canvas ficam fora dele, como na árvore. Assim ativar/desativar o gatilho não desativa o personagem nem a UI.

`CaptureOrigin` fornece a origem da trajetória cinematográfica. O script reposiciona essa origem para a posição real de Edelzio antes de reproduzir a Timeline. O clip anima somente `MotionRig.localPosition`; não grava a posição de `CaptureOrigin` nem a do Rigidbody. Isso evita um salto ao assumir o personagem.

## 3. Personagem jogável e transição Idle/Run

Use [PrologueChildController.cs](Exemplos/PrologoTimeline/PrologueChildController.cs).

1. No Rigidbody2D, configure **Dynamic**, Gravity Scale = 0, Freeze Rotation Z e Interpolation = Interpolate. Use um único colisor corporal e colisores sólidos nas paredes.
2. Crie `Child1996.controller` no Animator do objeto `Visual`.
3. Adicione os estados `Idle` e `Run`, ambos com clips de sprites em loop. Os clips devem animar apenas `SpriteRenderer.sprite`.
4. Crie um parâmetro float `Speed`, iniciando em 0.
5. Transição Idle → Run: `Speed > 0.05`. Run → Idle: `Speed < 0.05`. Desative Has Exit Time e use duração 0 para a troca de sprites.
6. Associe o Animator de `Visual` ao campo `Visual Animator` do controlador.

O exemplo usa um ciclo simples Idle/Run. Sprites para quatro direções podem ser acrescentados depois com um Blend Tree e parâmetros de direção. Mantenha os clips de sprites separados do movimento no mundo.

Trechos centrais:

```csharp
// Movimento de gameplay, somente enquanto o script possui o controle.
private void FixedUpdate()
{
    if (!_cinematic)
        _body.linearVelocity = _input * (_speed * Mathf.Clamp01(InputStrength));
}

// Chamado antes de a Timeline animar a trajetoria.
public void BeginCinematic()
{
    if (_cinematic) return;
    _cinematic = true;
    _input = Vector2.zero;
    _wasSimulated = _body.simulated;
    _body.linearVelocity = Vector2.zero;
    _body.simulated = false;
}
```

O controlador deixa também de escrever `Speed` durante a cutscene. A Timeline passa a comandar os clips de `Visual`, e a física deixa de disputar o movimento. Durante esse trecho sem simulação, a trajetória deve ser autorada passando por portas e áreas livres; ela não desvia de paredes automaticamente.

O controlador infantil é próprio desta cena. O bootstrap atual instala combate em objetos com `EdelzioTopDownController`; por isso, montar a criança com o controlador de exemplo mantém a exploração do prólogo sem as ações de combate do adulto.

## 4. Objetos interativos e início da anomalia

Use [PrologueInteractable.cs](Exemplos/PrologoTimeline/PrologueInteractable.cs) em TV, jornal e computador. Cada objeto recebe um Collider2D com **Is Trigger**, uma área de aproximação e referência à `LegendaExploracao`.

Sugestões de texto ficcional:

- Jornal: “Outra notícia sobre a criatura... será que alguém tirou uma foto?”
- TV: “Moradores relatam ter visto uma criatura em Varginha.”
- Computador: “O arquivo travou. Eu nem abri esse programa...”

No `On Interacted` do computador, configure:

1. `AudioSource.Stop()` na fonte do noticiário.
2. `TV_Noticiario.SetActive(false)` e `TV_Estatica.SetActive(true)`.
3. `AudioSource.Play()` na fonte de estática, com Loop ligado.
4. `Brilho_Janela.SetActive(true)`.
5. `Timeline_Prologo.SetActive(true)` para habilitar o gatilho junto à janela.

Mantenha o noticiário em uma fonte própria e reserve outra para áudio da Timeline. Evite triggers de interação sobrepostos; o exemplo trata cada objeto separadamente e usa a tecla configurada para interação. Não há tutorial modal: a curiosidade sobre os objetos conduz a sequência.

Ao entrar no gatilho de `Timeline_Prologo`, `PrologueSequence.Begin()` é chamado. Ele reduz `InputStrength` de 1 para 0 durante 1,5 s, enquanto `TL_Anomalia` apresenta oscilações de luz. Ao terminar, desliga a simulação do Rigidbody e inicia a Timeline principal.

## 5. Criar e configurar as Timelines

Abra **Window → Sequencing → Timeline**. Selecione `Timeline_Prologo` e use Create para salvar `TL_Abducao_1996.playable`. Faça o mesmo para `Timeline_Anomalia` e o asset de 1,5 s.

No **Playable Director principal**:

- Play On Awake: desmarcado também no Inspector.
- Update Method: Game Time.
- Wrap Mode: None.
- Initial Time: 0.
- Duração do asset: Fixed Length, 20 s, em 30 fps como proposta inicial.

Game Time acompanha a pausa do jogo; o script não zera `Time.timeScale` para bloquear comandos. A autoridade sobre o personagem é retirada no controlador. `None` permite encerrar a reprodução; `Hold` mantém a última imagem e `Loop` repete a sequência. [Referência do Playable Director](https://docs.unity3d.com/Packages/com.unity.timeline@1.8/manual/playable-director.html).

Configure `TL_Anomalia` também com Play On Awake desmarcado e Game Time. Ela anima apenas a oscilação inicial da iluminação, por exemplo a opacidade de uma máscara escura; seu término é comandado pela passagem para a abdução. Não vincule a ela o script de mudança de cena.

### Trilhas da Timeline principal

| Trilha | Binding | O que anima |
| --- | --- | --- |
| Animation — Caminho | Animator de `CaptureOrigin` | Propriedade do filho `MotionRig`: Local Position. |
| Animation — Personagem | Animator de `Visual` | Clips Run de 0–5 s e Idle de 5–11 s. |
| Animation — Câmera | Animator de `CameraRig` | Posição da câmera filha; aproximação e mudança para o plano da cama. |
| Animation — Feixe | Animator do grupo visual do feixe | Opacidade e escala do sprite. |
| Activation — Silhueta | GameObject da silhueta | Presença entre 8,2 e 10,5 s. |
| Activation — Visual jogável | GameObject `Visual` | Ativo de 0–11 s; oculto no plano do despertar. |
| Activation — Despertar | Grupo `Despertar1996` | Ativo de 11,2–15,5 s, com clip próprio de despertar. |
| Animation — Título | Animator de `Titulo` | CanvasGroup.alpha e RectTransform.localScale. |
| Animation — Percepção | Animator de `Overlay_Foco` | Visão encoberta durante feixe/silhueta. |
| Animation — Cortes | Animator de `CortePreto` | Preto instantâneo, revelação da cama e preto final. |
| Audio — Abdução | AudioSource dedicado | Ambiente, crescendo do feixe e silêncio no corte. |
| Signal — Eventos | SignalReceiver de `Timeline_Prologo` | Parar estática e marcar conclusão narrativa. |

Adicione Animator aos objetos de trilhas de animação quando o Editor solicitar. Animators de UI e cenário podem ficar sem Runtime Animator Controller. Deixe Apply Root Motion desligado: a trajetória está gravada como propriedade do filho `MotionRig`, não como deslocamento da raiz do Animator.

Nas Activation Tracks, confira **Post-playback State**: `Revert` para retornar ao estado anterior em um cancelamento. Mantenha todos os clips de UI/posição cobrindo os intervalos em que precisam conservar valores. Não use Activation Track para desligar `Timeline_Prologo`, `CaptureOrigin`, o controlador ou `LoadingCover`.

## 6. Gravar o caminho e ajustar tangentes

1. Antes da gravação, posicione `CaptureOrigin` na referência de captura ao lado da janela. Deixe `MotionRig.localPosition = (0,0,0)`, rotação 0 e escala 1.
2. Crie um Animation Clip `CaminhoQuintal.anim` na trilha vinculada a `CaptureOrigin`.
3. Ative Record e selecione o filho `MotionRig` para gravar **Local Position**, verificando o caminho da propriedade na janela Animation.
4. Grave o ponto inicial em 0 s e pontos de passagem até o quintal em 5 s. Por exemplo, se o quintal estiver à direita, use X de 0 até 3,5 unidades. Adapte Y e os pontos às portas reais da cena.
5. Mantenha a posição final até 20 s. Não grave scale/rotation da origem nem a posição do Rigidbody.
6. Nas curvas de posição, selecione as chaves e use **Both Tangents → Linear** nos trechos de velocidade constante.
7. Reserve curvas suaves para movimentos intencionais da câmera. Nos apagões e no corte seco, use tangentes **Constant** ou chaves de ativação.

Linear conecta as chaves em linha reta; Constant mantém o valor até a próxima chave. [Referência das tangentes](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AnimationUtility.TangentMode.html).

O script centraliza a trajetória na posição real do garoto. O gatilho deve ser pequeno e ficar junto à passagem para que as variações de posição durante os 1,5 s de perda de controle continuem dentro de uma rota livre. Prefira conferir a entrada pelo lado esquerdo, direito e correndo.

## 7. Fade de UI com TextMeshPro

Use Canvas **Screen Space - Overlay** para os textos. Em Canvas Scaler, escolha Scale With Screen Size; **384 × 216** é uma referência inicial coerente com a composição de viagem do projeto. Ajuste tamanhos de fonte e margens na resolução de jogo.

No grupo `Titulo`:

- CanvasGroup: Alpha = 0, Interactable = false e Blocks Raycasts = false.
- Filho `Texto`: TextMeshProUGUI, texto “30 anos depois”, alinhado ao centro.
- Fonte com os caracteres usados em português, tamanho definido e Auto Size desativado após acertar o layout.
- Retire Raycast Target dos textos e imagens decorativas.

Deixe também `CortePreto`, `Overlay_Foco` e `LoadingCover` com **CanvasGroup Alpha = 0 no Inspector**, para a exploração começar visível. As imagens preta/branca em si têm alpha 1. Em `LoadingCover`, use uma imagem preta em Stretch, Raycast Target ligado e Blocks Raycasts inicialmente desligado; mantenha esse grupo fora de qualquer animação.

Grave no clip `Titulo30Anos.anim`:

| Tempo | CanvasGroup.alpha | Escala do grupo |
| --- | --- | --- |
| 0 s | 0 | 0,96 |
| 15,5 s | 0 | 0,96 |
| 16,2 s | 1 | 1,00 |
| 18,8 s | 1 | 1,00 |
| 19,5 s | 0 | 1,00 |
| 20 s | 0 | 1,00 |

`CanvasGroup.alpha` aplica opacidade ao grupo de UI, incluindo os textos filhos. Não é preciso reconstruir o texto a cada quadro ou criar uma coroutine de fade para competir com a Timeline. [Referência do CanvasGroup](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/CanvasGroup.html).

Para `CortePreto`, use uma `UnityEngine.UI.Image` preta com anchors em Stretch e offsets 0. Anime o alpha do grupo: 0 até imediatamente antes de 11 s, 1 em 11 s, 1 até 11,25 s, 0 em 11,3 s; volte a 1 entre 15 e 15,5 s. Mantenha preto até 20 s. **Na hierarquia, coloque `Titulo` depois de `CortePreto`** para o texto aparecer sobre o fundo final, e deixe `LoadingCover` como último filho de todos, conforme a árvore da seção 2.

Se desejar o título “Varginha, 1996” durante a exploração, use outro grupo e uma Timeline curta de entrada, exclusiva de UI. Ela pode usar Play On Awake e terminar sem disparar a transição de cena.

### Perda de foco em pixel art

A proposta visual usa `Overlay_Foco`: máscara clara translúcida e vinheta sobre o mundo, crescendo entre 7,5–10,5 s, com a silhueta pouco definida dentro do feixe. Isso representa a percepção comprometida sem alterar permanentemente o filtro dos sprites. Um blur óptico real seria uma extensão de renderização, não faz parte destes exemplos.

O pacote URP está instalado, e o projeto já possui `VarginhaPixelPresentation.DetectPipeline()` e `Configure(camera)`. Use a detecção em runtime para identificar o pipeline ativo antes de escolher recursos específicos. Preserve o enquadramento e os PPUs existentes: o próprio helper do projeto alerta que os sprites usam escalas diferentes. O Canvas Overlay mantém o texto fora de uma eventual renderização de baixa resolução da câmera.

## 8. Término da Timeline e carregamento da fase

Use [PrologueSequence.cs](Exemplos/PrologoTimeline/PrologueSequence.cs). Ele reúne a passagem de controle e a função de Scene Changer solicitada.

No componente, associe:

| Campo | Referência |
| --- | --- |
| Child | PrologueChildController de EdelzioCrianca. |
| Capture Origin | CaptureOrigin, pai direto de MotionRig. |
| Motion Rig | MotionRig, pai direto de EdelzioCrianca. |
| Loading Cover | CanvasGroup da imagem preta final, sem Animator. |
| Anomaly Director | PlayableDirector de TL_Anomalia; opcional. |
| Takeover Seconds | 1,5. |
| Next Scene | FaseTopView_Varginha. |

No `On Anomaly Started`, oculte a legenda de exploração e intensifique os efeitos desejados. No `On Cancelled`, restaure esses objetos se quiser testar cancelamento e repetir a sequência.

### Configurar o sinal de conclusão

1. Adicione Signal Receiver ao GameObject `Timeline_Prologo`.
2. Crie um Signal Asset `PrologoNarrativaConcluida`.
3. Na Signal Track vinculada ao Receiver, insira um Signal Emitter em **19,9 s**, dentro da duração de 20 s.
4. Associe o Signal Asset. Ative Emit Once e Retroactive.
5. Na reação correspondente do Receiver, associe `PrologueSequence.MarkNarrativeComplete()`.
6. Em 11 s, outro sinal pode chamar `AudioSource.Stop()` na estática para produzir o silêncio do corte.

O sinal final apenas marca que a narrativa chegou ao encerramento. O carregamento é iniciado quando o Director realmente emite `stopped`. Para abortar por script ou por botão, chame `Cancel()`; chamar `Stop()` diretamente depois do sinal final pode ser interpretado como uma conclusão. Não desative o objeto do sequenciador durante a transição.

Trecho do carregamento:

```csharp
private void OnStopped(PlayableDirector director)
{
    if (director != _director || !_running || !_timelineStarted || _loading) return;
    if (_endingMarked)
    {
        _loading = true;
        loadingCover.alpha = 1f;
        loadingCover.blocksRaycasts = true;
    }
    StartCoroutine(FinishAfterGraphStops());
}

// Dentro da coroutine, depois das validacoes e de yield return null:
operation = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Single);
```

O exemplo completo espera um quadro antes do carregamento porque `stopped` é emitido antes dos callbacks finais do grafo. A cobertura preta independente evita que a restauração das propriedades animadas revele o cenário anterior por um quadro. [Evento stopped](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Playables.PlayableDirector-stopped.html). O carregamento usa a API assíncrona e valida previamente a presença da cena. [LoadSceneAsync](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/SceneManagement.SceneManager.LoadSceneAsync.html).

## 9. Ligação com o menu e o despertar

1. Registre `Prologo_1996` e `FaseTopView_Varginha` na Scene List do Build Profile utilizado.
2. Quando o prólogo estiver montado, selecione `VarginhaMainMenu` na cena do menu e altere seu campo serializado **Gameplay Scene Name** para `Prologo_1996`. `StartInvestigation()` já copia esse campo para `_pendingScene`; a alteração no Inspector aproveita o fluxo existente.
3. Preserve a seleção de dificuldade que já ocorre antes do carregamento.
4. O plano do despertar faz parte da própria cena do prólogo: `Despertar1996` mostra a criança na cama. Anime a câmera até esse plano durante o intervalo de preto e reproduza o clip de despertar.
5. Ao encerrar o asset, a próxima cena é a casa com o Edelzio adulto. O título “30 anos depois” explica essa passagem temporal.

O caminho de integração fica: **menu → prólogo interativo de 1996 → Timeline de abdução/despertar → casa em 2026**. Os exemplos de documentação não alteram automaticamente o menu nem a Scene List.

## 10. Conferência no Editor

- Exploração inicial: criança anda, corre e usa TV, jornal e computador; animação alterna Idle/Run.
- Computador: anomalia e gatilho aparecem uma vez; o jogador ainda consegue caminhar até a janela.
- Captura: entrada por posições diferentes não provoca salto; movimento enfraquece durante 1,5 s.
- Cutscene: Rigidbody fica sem simulação, o caminho passa pelas aberturas e a Timeline controla os sprites.
- UI: título faz fade e escala, fica acima do preto e os acentos aparecem corretamente.
- Corte: em 11 s, o preto é instantâneo, o ruído para e não aparecem dois Edelzios no mesmo plano.
- Conclusão: sinal final e stopped resultam em um único carregamento da casa adulta.
- Cancelamento: chamar Cancel antes da conclusão restaura a posição e o controle; não carrega outra cena.
- Sinal ausente ou Stop antecipado: registra o problema e devolve o controle após o grafo parar.
- Cena ausente no Build Profile: Begin informa o erro antes de retirar o controle.
- Pausa: Game Time e perda de controle param juntos quando Time.timeScale é zero.
- Duração alterada: mova também o sinal e os clips finais; o script aguarda o novo término.

### Observações de implementação

Os scripts guardam referências e hashes de parâmetros; não procuram objetos globalmente a cada frame. O texto muda somente nas interações. Coroutines são usadas durante captura e transição; o término depende de evento, e a física possui um único responsável por etapa.

O layout segue uGUI/TMP em Canvas Overlay e os cuidados de pixel art do projeto. A validação visual, os bindings, os assets infantis e a reprodução integral precisam ser conferidos no Unity após a montagem.

**Verificação realizada em 15/09/2026:** os três exemplos e o `VarginhaInputBindings` existente compilaram com Roslyn/C# 9 e referências do Unity 6000.6.0f1, Input System e TextMeshPro. Essa verificação de compilação foi isolada; não executou cenas nem validou os bindings no Editor.

## Arquivos de exemplo

- [PrologueChildController.cs](Exemplos/PrologoTimeline/PrologueChildController.cs): movimento, Idle/Run e passagem para controle cinematográfico.
- [PrologueInteractable.cs](Exemplos/PrologoTimeline/PrologueInteractable.cs): interação contextual com texto e eventos.
- [PrologueSequence.cs](Exemplos/PrologoTimeline/PrologueSequence.cs): perda de controle, reprodução, sinal de conclusão, cancelamento e transição de cena.

Referência de configuração específica da versão instalada: documentação do pacote Timeline 6.6.0 em `Library/PackageCache/com.unity.timeline@281d5f7e7d7d/Documentation~`. Os links públicos acima descrevem as APIs e os controles utilizados.
