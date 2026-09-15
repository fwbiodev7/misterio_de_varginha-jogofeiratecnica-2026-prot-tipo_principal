# Arquitetura do protótipo

Regras de design, valores de balanceamento e limites do conteúdo jogável estão no [GDD consolidado](GDD.md), atualizado em 15/09/2026.

## Visão geral

O projeto usa Unity 6, C# e uma cena 2D top-down com sprites pixel art. A lógica específica de Varginha fica em `Assets/Scripts/Game/Varginha`, enquanto os gerenciadores e componentes genéricos continuam em `Assets/Scripts/Game/Managers`, `Player`, `Level` e `UI`.

## Entrada e cenas

`VarginhaMainMenu` inicia a investigação. A cena da casa usa `VarginhaLevelBuilderTool` para manter o mapa organizado no Editor. A saída é conduzida por `FuscaLevelExit`; depois da animação de partida, o script carrega `Fase2_Escola_Resgate` se a cena estiver no Build Settings.

A cena da escola tem um objeto mínimo com `VarginhaPhase2Controller`. Em runtime, a fábrica interna desse controlador monta o ambiente, a câmera, Edelzio, o Fusca, os inimigos e os reféns. `VarginhaEnvironmentArt` constrói a identidade da escola com parquet, paredes de tijolo, salas, lousas, carteiras, armários, janelas e elementos de sinalização; o marcador visual torna a construção idempotente em cenas antigas. A cena `Fase3_Igreja_Guardiao` usa o mesmo padrão com `VarginhaPhase3Controller`, montando a diocese com pedra escura, vitrais, bancos, tapete, estrado, altar, velas e sacristia, além do padre, livro, cinco ETs e turma aliada em runtime. Isso deixa as cenas leves e mantém os fluxos reproduzíveis em builds.

## Combate

`VarginhaPlayerAttack` executa três golpes encadeáveis, cada um com uma sequência própria de seis frames:

1. corte inicial;
2. golpe cruzado;
3. finalizador pesado.

Cada golpe tem antecipação, extensão, impacto com hitbox única, clarão, hitstop e recuperação. O combo expira depois da janela curta sem novo comando.

`VarginhaCombatTarget` conecta o alvo ao `HealthSystem`. `VarginhaCombatEnemy` implementa atirador, investidor e sentinela, com perseguição, preparação sinalizada, projétil/investida/área, recuperação, stagger e knockback. Todos usam sprites marrons com olhos vermelhos; as cores dos papéis identificam os avisos e efeitos. O dano efetivo à vida também drena sanidade, e a esquiva bloqueia esses acertos. `VarginhaCombatRuntimeBootstrap` instala o ataque e a vida em cenas antigas e marca a entidade principal como `AncestralEntity`, impedindo que o golpe comum a derrote.

## Estado da Fase 2

`VarginhaPhase2Controller` controla quatro estados práticos:

- chegada cinematográfica;
- combate enquanto houver subordinados ETs vivos;
- resgate, no qual os alunos acompanham Edelzio e depois formam a fila do carro;
- partida, com os passageiros ocultados dentro do Fusca e `VarginhaTravelCinematic.Begin(true)` carregando a Fase 3.

`VarginhaStudentHostage` mantém o nome de cada aluno, a filha visual `Jaula_ET` com barras verdes pulsantes, o estado de libertação, o seguimento do líder e a confirmação de chegada ao Fusca. `ReleaseTo` desativa a jaula quando os subordinados são derrotados.

## Aliados e extensões para outras fases

`VarginhaStudentAllySquad` é o ponto de entrada para fases posteriores. `BuildForFuturePhase(parent, leader, true)` cria a turma como esquadrão e ativa `VarginhaStudentAlly` em cada aluno. A movimentação usa circulação, separação entre colegas e desvio simples de paredes, em vez de prender os alunos a uma grade. O componente busca o alvo menor mais próximo, usa `VarginhaCombatTarget.ReceiveHit` e cria um efeito de ataque correspondente ao perfil:

- Matias: jiujitsu.
- Anna Sabia e Ana Tavares: raquetada de ping-pong.
- Pedro: golpe de guitarra.
- Luis Martins: golpe de arte.
- Luis Miguel Messias: golpe de microfone.
- Yasmin: piano que surge acima do alvo e cai sobre ele.
- Fabio: golpe de katana.
- Marcos: ciclo de vôlei, chute voador e cotovelada, com bordões sem censura no impacto.

O controlador da Fase 2 não ativa o combate dos alunos. `ActivateForPhase` oferece a regra reutilizável de três aliados em fases comuns a partir da Fase 2 no DIFÍCIL; a Fase 3 usa `ActivateManualAllies(..., true)` e libera os nove. `TryInvokeAttack(aim)` pontua combinações de aluno pronto e alvo válido, priorizando a mira, a situação de combate e os perfis. `TryInvokeNextAttack()` chama essa seleção sem mira explícita. O cooldown individual é de 5 segundos e o comando da turma tem um intervalo adicional de 0,9 segundo. Alcance, linha de visão e caminhos entre alvos limitam impactos, ricochetes e efeitos em área.

## Estado da Fase 3

`VarginhaPhase3Controller` implementa o Ato III — O Guardião do GDD:

- chegada à área secreta da diocese e instrução dos comandos independentes de ataque e aliado;
- combate contra cinco subordinados ETs, com combo de Edelzio e uma habilidade de aluno por comando aceito;
- encontro com Padre Fábio depois da última derrota;
- interação com o Livro do Tombo Secreto e revelação do selo de 1898;
- tela de vitória com gancho narrativo para a mata e Ouzana; a continuação dessa rota ainda não tem cena jogável registrada.

`VarginhaPhase3RuntimeFactory` cria piso de pedra, altar, símbolo do selo, padre, livro, inimigos, câmera, HUD e turma. O painel lateral da fase mostra o estado de cada cooldown e deixa explícita a regra de um golpe por clique.

## Apresentação e HUD

`VarginhaPlayerSpriteAnimation` seleciona os quadros de Edelzio e `VarginhaPlayerActionAnimation` mantém as ações de interação. A mochila e o notebook são objetos filhos separados para evitar que equipamentos desapareçam ou sejam deformados nos sprites de caminhada.

`VarginhaGameHUD` desenha saúde, diálogos, dicas, prompts de interação, hotbar e telas de vitória. Os cinco slots usam o inventário do `EdelzioTopDownController` como fonte única de estado. `VarginhaMainMenu` mantém a tela de controles em OnGUI e `VarginhaInputBindings` persiste a tecla/botão escolhido com PlayerPrefs, consumido por movimento, interação, ataque, esquiva e comando de aliados.

O painel de combate do HUD é calculado acima da hotbar, com dimensões responsivas para não cortar o combo nem a esquiva em janelas menores. `VarginhaTravelCinematic` bloqueia a ativação da próxima cena enquanto carrega e renderiza a viagem em uma composição inteira de 384x216 com filtro Point; a saída física do Fusca usa apenas um deslocamento curto até a vaga, e a estrada pixel art assume o restante do percurso.

## Assets

- `EdelzioTopDownV3.png`: caminhada e ações de Edelzio.
- `EdelzioAttackV1.png`: quadros de ataque com arma curta.
- `fusca-sprite-sheet.png`: quadros do Fusca usados na entrada e na saída.
- `VarginhaPixelArtSprites.cs`: sprites auxiliares procedurais para mochila, itens, ETs, jaulas, alunos, VFX e cenário.
- `VarginhaEnvironmentArt.cs`: montagem compartilhada e idempotente da cenografia visual da escola e da diocese.
- `VarginhaEnvironmentPolish.cs`: acabamento das três fases, aplicado também a cenas existentes; recupera decoração procedural perdida, mantém reflexos no piso e não cria colisores.
- `VarginhaSceneryArt.cs`: sprites decorativos no tamanho nativo de 32 pixels por unidade, com cache e filtro Point; vitrais e projeções compartilham o padrão colorido.
- `VarginhaAmbientPixelEffect.cs`: variação suave de opacidade das luzes e movimento de partículas alinhado aos pixels, sem gerar texturas por quadro.
- `VarginhaPixelPresentation.cs`: nitidez das câmeras 2D, filtro sem antialiasing e bloqueio de HDR/MSAA/dynamic resolution nas câmeras procedurais.

## Extensão

Para adicionar uma nova fase, crie uma cena em `Assets/Scenes`, registre-a em `ProjectSettings/EditorBuildSettings.asset` e prefira um controlador de fluxo próprio. Para novos inimigos, use `VarginhaCombatTarget` e defina um `EnemyKind` que preserve a regra de invulnerabilidade da entidade ancestral.
