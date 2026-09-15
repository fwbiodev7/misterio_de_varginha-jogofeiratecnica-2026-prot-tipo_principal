# O Mistério de Varginha — protótipo jogável

Protótipo 2D top-down de investigação sobrenatural, inspirado no GDD de **O Mistério de Varginha**. O jogador controla Edelzio, explora a casa, coleta pistas, resolve o notebook, enfrenta manifestações menores e segue de Fusca até a investigação da escola.

Repositório: [github.com/fwbiodev7/misterio_de_varginha-jogofeiratecnica-2026-prot-tipo_principal](https://github.com/fwbiodev7/misterio_de_varginha-jogofeiratecnica-2026-prot-tipo_principal)

## Requisitos

- Unity **6000.6.0f1**
- Windows ou outra plataforma suportada pelo Unity 6
- Pacotes instalados pelo `Packages/manifest.json`, incluindo URP, Input System, 2D Sprite/Tilemap e Test Framework

## Como abrir

1. Clone o repositório.
2. Abra a pasta no Unity Hub usando o Editor `6000.6.0f1`.
3. Aguarde a importação dos assets.
4. Pressione Play. O menu principal inicia a investigação pela cena `FaseTopView_Varginha`.

As cenas também podem ser abertas diretamente em `Assets/Scenes`:

- `Menu_MisterioDeVarginha.unity`: menu principal.
- `FaseTopView_Varginha.unity`: investigação da casa, pistas, notebook e Fusca.
- `Fase2_Escola_Resgate.unity`: chegada à escola, combate e resgate da turma.
- `Fase3_Igreja_Guardiao.unity`: Ato III do GDD, igreja/diocese, ETs, aliados invocáveis, Padre Fábio e Livro do Tombo Secreto.

## Controles

| Ação | Teclado / mouse |
| --- | --- |
| Andar | WASD ou setas (remapeável no menu) |
| Correr | Shift |
| Interagir / examinar | E, Espaço ou Enter |
| Atacar / combo | Mouse esquerdo ou J (remapeável no menu) |
| Comandar aluno | Mouse direito ou L na fase final (remapeável no menu) |
| Esquivar | Ctrl |
| Selecionar item da hotbar | 1 a 5 |

No menu `TUTORIAL / CONTROLES`, o botão `EDITAR CONTROLES DO TECLADO E MOUSE` abre a tela de remapeamento inspirada na referência. Cada comando pode receber uma tecla ou botão do mouse e fica salvo entre as cenas.

## Fluxo do jogo

### Fase 1 — investigação

Edelzio explora a casa, encontra a mochila cinza, a chave do Fusca, o caderno de pesquisas, o notebook e documentos históricos. O notebook abre o quiz de decodificação. A saída pelo Fusca exige a chave e o caderno; concluir o quiz faz parte do fluxo narrativo, mas ainda não é uma condição obrigatória no código.

Ao entrar no carro com os requisitos, a animação de partida termina carregando automaticamente `Fase2_Escola_Resgate`.

### Fase 2 — escola e resgate

A cena começa com uma transição cinematográfica: o Fusca aparece, Edelzio sai já equipado com os itens da Fase 1 e pronto para o combate. Quatro subordinados ETs guardam os alunos do terceiro sistema:

**Yasmin, Pedro, Matias, Fabio, Marcos, Anna Sabia, Ana Tavares, Luis Miguel Messias e Luis Martins.**

O jogador derrota os subordinados usando o combo de três golpes: corte inicial, golpe cruzado e finalizador pesado. Os ETs alternam papéis de atirador, investidor e sentinela, com ataques sinalizados que afetam vida e sanidade. Depois da última derrota, as jaulas pixel art desaparecem, os alunos são libertados, acompanham Edelzio e formam uma fila no Fusca. Quando toda a turma chega ao carro e Edelzio se aproxima, a partida inicia a viagem para a Fase 3.

### Fase 3 — O Guardião

Edelzio chega à área secreta da diocese seguindo as coordenadas decodificadas. Os ETs guardam a passagem para Padre Fábio e para o Livro do Tombo Secreto. O comando de ataque dispara o combo de Edelzio, sem recarga adicional após a animação; o comando de aliado escolhe um aluno pronto e prioriza o ET na mira, considerando o contexto de combate quando necessário. Os nove alunos estão disponíveis, com recarga individual de cinco segundos e intervalo de 0,9 segundo entre comandos da turma.

Depois que os ETs são derrotados, Padre Fábio orienta Edelzio a examinar o livro. A leitura conclui a fase e revela que o nome de Edelzio aparece nos registros do selo de 1898. A mata e Ouzana são o gancho narrativo para a continuação; essa rota termina na tela de vitória da Fase 3 no protótipo atual.

## Sistemas implementados

- **Ataque direcional:** atlas para baixo, cima, esquerda e direita, com antecipação, impacto e recuperação.
- **Dano e hitbox:** a área de dano fica ativa somente no frame de impacto.
- **Game feel:** hitstop de impacto, knockback, reação visual do inimigo e tremor de câmera.
- **Subordinados ETs:** visual marrom com olhos vermelhos; atirador, investidor e sentinela com avisos de ataque e dano de vida e sanidade.
- **Entidade ancestral:** continua invulnerável ao ataque comum, preservando a função narrativa do GDD.
- **Hotbar:** cinco slots para mochila, chave, caderno, dados do notebook e documento histórico.
- **Mochila:** sprite cinza separado, maior, preso às costas e com ordenação ajustada por direção.
- **Animações de Edelzio:** caminhada, café, agachar, alcançar, sentar, usar notebook e ataque.
- **Fusca:** porta com pivô de dobradiça, sprites em cache, abertura/fechamento com easing e deslocamento horizontal estável, sem a antiga deriva diagonal.
- **Reféns:** nomes, jaulas verdes pulsantes, estado de liberdade, acompanhamento de Edelzio e entrada no Fusca.
- **Poderes da turma:** jiujitsu do Matias, ping-pong das Annas, guitarra do Pedro, tinta do Luis Martins, microfone do Luis Miguel Messias, piano da Yasmin, katana do Fabio e ciclo de vôlei, chute voador e cotovelada do Marcos. Incluem controle de inimigos, efeitos em área, ricochetes e recuperação de sanidade; detalhes no GDD.
- **Aliados invocáveis:** os alunos circulam em torno de Edelzio, com separação e desvio simples de paredes. Na Fase 3, todos os nove são liberados em qualquer dificuldade. A regra reutilizável de três alunos em fases comuns no **DIFÍCIL** existe no código, mas ainda não é ativada pelo controlador da escola. Marcos apresenta bordões como “to doido com vc então uai!”, “o Exu!!” e “o cu!!!” ao acertar.

## Estrutura principal

```text
Assets/
  Scenes/                         Cenas jogáveis
  Resources/Varginha/             Folhas de sprites e arte pixelada
  Scripts/Game/Varginha/          Sistemas de Edelzio, combate, HUD e fases
  Scripts/Editor/Testing/         Construtores e importadores de apoio
  Tests/PlayMode/                 Testes automatizados do protótipo
Docs/                             Documentação de design e histórico
ProjectSettings/                  Configuração do projeto e Build Settings
Packages/                         Dependências UPM
```

## Documentação adicional

- [GDD consolidado — regras e features atuais](Docs/GDD.md)
- [Guia do prólogo de 1996 com Timeline e exemplos C#](Docs/GUIA_PROLOGO_TIMELINE.md)
- [Visão técnica e arquitetura](Docs/ARCHITECTURE.md)
- [Atualizações desta versão](Docs/CHANGELOG.md)
- [Arte e animações do Edelzio V3](Docs/EdelzioV3.md)

## Validação

A compilação de scripts do Unity 6 foi concluída com sucesso após a implementação do combate, da Fase 2 e da Fase 3. Os testes de Play Mode estão em `Assets/Tests/PlayMode`; eles devem ser executados pelo Test Runner do Unity antes de uma build de distribuição.

Este repositório é um protótipo em desenvolvimento. Os assets gerados e os sistemas descritos aqui fazem parte da versão atual da `main`.
