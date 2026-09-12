# Arquitetura do protótipo

## Visão geral

O projeto usa Unity 6, C# e uma cena 2D top-down com sprites pixel art. A lógica específica de Varginha fica em `Assets/Scripts/Game/Varginha`, enquanto os gerenciadores e componentes genéricos continuam em `Assets/Scripts/Game/Managers`, `Player`, `Level` e `UI`.

## Entrada e cenas

`VarginhaMainMenu` inicia a investigação. A cena da casa usa `VarginhaLevelBuilderTool` para manter o mapa organizado no Editor. A saída é conduzida por `FuscaLevelExit`; depois da animação de partida, o script carrega `Fase2_Escola_Resgate` se a cena estiver no Build Settings.

A cena da escola tem um objeto mínimo com `VarginhaPhase2Controller`. Em runtime, a fábrica interna desse controlador monta o ambiente, a câmera, Edelzio, o Fusca, os inimigos e os reféns. A cena `Fase3_Igreja_Guardiao` usa o mesmo padrão com `VarginhaPhase3Controller`, montando a igreja, o altar, o padre, o livro, cinco ETs e a turma aliada em runtime. Isso deixa as cenas leves e mantém os fluxos reproduzíveis em builds.

## Combate

`VarginhaPlayerAttack` executa a sequência de seis frames:

1. antecipação;
2. extensão do golpe;
3. impacto e ativação da hitbox;
4. clarão de impacto;
5. recuperação;
6. retorno ao idle.

`VarginhaCombatTarget` conecta o alvo ao `HealthSystem`. `VarginhaCombatEnemy` fornece perseguição, rajada de energia, stagger e knockback. A rajada cria um projétil pixel art verde, aplica dano à vida e à sanidade de Edelzio e usa `VarginhaCameraShake` para reforçar o impacto. `VarginhaCombatRuntimeBootstrap` instala o ataque e a vida em cenas antigas e marca a entidade principal como `AncestralEntity`, impedindo que o golpe comum a derrote.

## Estado da Fase 2

`VarginhaPhase2Controller` controla quatro estados práticos:

- chegada cinematográfica;
- combate enquanto houver subordinados ETs vivos;
- resgate, no qual os alunos acompanham Edelzio e depois formam a fila do carro;
- partida, com os passageiros ocultados dentro do Fusca e tela de vitória.

`VarginhaStudentHostage` mantém o nome de cada aluno, a filha visual `Jaula_ET` com barras verdes pulsantes, o estado de libertação, o seguimento do líder e a confirmação de chegada ao Fusca. `ReleaseTo` desativa a jaula quando os subordinados são derrotados.

## Aliados das fases futuras

`VarginhaStudentAllySquad` é o ponto de entrada para fases posteriores. `BuildForFuturePhase(parent, leader, true)` cria a turma como esquadrão e ativa `VarginhaStudentAlly` em cada aluno. O componente busca o alvo menor mais próximo, usa `VarginhaCombatTarget.ReceiveHit` e cria um efeito de ataque correspondente ao perfil:

- Matias: jiujitsu.
- Anna Sabia e Ana Tavares: raquetada de ping-pong.
- Pedro: golpe de guitarra.
- Luis Martins: golpe de arte.
- Luis Miguel Messias: golpe de microfone.
- Yasmin: piano que surge acima do alvo e cai sobre ele.
- Fabio: golpe de katana.
- Marcos: golpe de apoio.

O ataque automático fica desligado na Fase 2 atual. Na Fase 3, `ActivateManualAllies` coloca os nove alunos em modo manual: `TryInvokeNextAttack` percorre a lista em ordem, escolhe um aluno pronto, encontra o ET mais próximo e dispara um golpe único. O cooldown manual é centralizado em `VarginhaStudentAlly.ManualCooldownSeconds` (5 segundos), então cada aluno recupera seu próprio golpe sem bloquear os demais. `VarginhaPlayerAttack.Configure(5f, true)` mantém o clique esquerdo para Edelzio com um timer separado.

## Estado da Fase 3

`VarginhaPhase3Controller` implementa o Ato III — O Guardião do GDD:

- chegada à área secreta da diocese e instrução do clique esquerdo compartilhado por Edelzio e aliados;
- combate contra cinco subordinados ETs usando um aliado por golpe;
- encontro com Padre Fábio depois da última derrota;
- interação com o Livro do Tombo Secreto e revelação do selo de 1898;
- tela de vitória que encaminha a investigação para a mata e Ouzana.

`VarginhaPhase3RuntimeFactory` cria piso de pedra, altar, símbolo do selo, padre, livro, inimigos, câmera, HUD e turma. O painel lateral da fase mostra o estado de cada cooldown e deixa explícita a regra de um golpe por clique.

## Apresentação e HUD

`VarginhaPlayerSpriteAnimation` seleciona os quadros de Edelzio e `VarginhaPlayerActionAnimation` mantém as ações de interação. A mochila e o notebook são objetos filhos separados para evitar que equipamentos desapareçam ou sejam deformados nos sprites de caminhada.

`VarginhaGameHUD` desenha saúde, diálogos, dicas, prompts de interação, hotbar e telas de vitória. Os cinco slots usam o inventário do `EdelzioTopDownController` como fonte única de estado.

## Assets

- `EdelzioTopDownV3.png`: caminhada e ações de Edelzio.
- `EdelzioAttackV1.png`: quadros de ataque com arma curta.
- `fusca-sprite-sheet.png`: quadros do Fusca usados na entrada e na saída.
- `VarginhaPixelArtSprites.cs`: sprites auxiliares procedurais para mochila, itens, ETs, jaulas, alunos, VFX e cenário.

## Extensão

Para adicionar uma nova fase, crie uma cena em `Assets/Scenes`, registre-a em `ProjectSettings/EditorBuildSettings.asset` e prefira um controlador de fluxo próprio. Para novos inimigos, use `VarginhaCombatTarget` e defina um `EnemyKind` que preserve a regra de invulnerabilidade da entidade ancestral.
