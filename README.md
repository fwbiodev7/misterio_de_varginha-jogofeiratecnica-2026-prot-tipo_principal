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
| Andar | WASD ou setas |
| Correr | Shift |
| Interagir / examinar | E, Espaço ou Enter |
| Atacar na Fase 2 | J ou K |
| Invocar aliado na Fase 3 | Clique esquerdo |
| Selecionar item da hotbar | 1 a 5 |

## Fluxo do jogo

### Fase 1 — investigação

Edelzio explora a casa, encontra a mochila cinza, a chave do Fusca, o caderno de pesquisas, o notebook e documentos históricos. O notebook abre o quiz de decodificação; ao concluir, Edelzio pode sair pelo Fusca.

Ao entrar no carro com os requisitos, a animação de partida termina carregando automaticamente `Fase2_Escola_Resgate`.

### Fase 2 — escola e resgate

A cena começa com uma transição cinematográfica: o Fusca aparece, Edelzio sai já equipado com os itens da Fase 1 e pronto para o combate. Quatro subordinados ETs guardam os alunos do terceiro sistema:

**Yasmin, Pedro, Matias, Fabio, Marcos, Anna Sabia, Ana Tavares, Luis Miguel Messias e Luis Martins.**

O jogador derrota os subordinados usando o novo ataque. Os ETs também lançam rajadas verdes que drenam vida e sanidade. Depois da última derrota, as jaulas pixel art desaparecem, os alunos são libertados, acompanham Edelzio e formam uma fila no Fusca. Quando a turma chega ao carro, a saída final conclui a fase.

### Fase 3 — O Guardião

Edelzio chega à área secreta da diocese seguindo as coordenadas decodificadas. Os ETs guardam a passagem para Padre Fábio e para o Livro do Tombo Secreto. Cada clique esquerdo dispara o ataque de Edelzio e também invoca o próximo aluno pronto para executar um único golpe contra o ET mais próximo. Edelzio e cada aluno têm cooldowns independentes de cinco segundos.

Depois que os ETs são derrotados, Edelzio conversa com Padre Fábio e examina o livro. A fase termina revelando que o nome de Edelzio aparece nos registros do selo de 1898, conduzindo a investigação para a mata e para Ouzana.

## Sistemas implementados

- **Ataque direcional:** atlas para baixo, cima, esquerda e direita, com antecipação, impacto e recuperação.
- **Dano e hitbox:** a área de dano fica ativa somente no frame de impacto.
- **Game feel:** hitstop de impacto, knockback, reação visual do inimigo e tremor de câmera.
- **Subordinados ETs:** visual verde de olhos pretos inspirado na referência e rajada de energia com dano de vida e sanidade.
- **Entidade ancestral:** continua invulnerável ao ataque comum, preservando a função narrativa do GDD.
- **Hotbar:** cinco slots para mochila, chave, caderno, dados do notebook e documento histórico.
- **Mochila:** sprite cinza separado, maior, preso às costas e com ordenação ajustada por direção.
- **Animações de Edelzio:** caminhada, café, agachar, alcançar, sentar, usar notebook e ataque.
- **Fusca:** animação de partida e deslocamento horizontal estável, sem a antiga deriva diagonal.
- **Reféns:** nomes, jaulas verdes pulsantes, estado de liberdade, acompanhamento de Edelzio e entrada no Fusca.
- **Aliados para fases futuras:** `VarginhaStudentAllySquad.BuildForFuturePhase(...)` transforma os nove alunos resgatados em companheiros ativos. Cada um usa um golpe próprio: jiujitsu do Matias, raquetadas das Annas, guitarra do Pedro, arte do Luis Martins, microfone do Luis Miguel Messias, piano que cai sobre o alvo da Yasmin, katana do Fabio e apoio do Marcos.
- **Aliados invocáveis da Fase 3:** `VarginhaStudentAllySquad.ActivateManualAllies(...)` desliga o ataque automático e libera um golpe por clique esquerdo, com cooldown individual de 5 segundos e painel de disponibilidade no HUD. O mesmo clique mantém o ataque de Edelzio ativo, com cooldown próprio de 5 segundos.

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

- [Visão técnica e arquitetura](Docs/ARCHITECTURE.md)
- [Atualizações desta versão](Docs/CHANGELOG.md)
- [Arte e animações do Edelzio V3](Docs/EdelzioV3.md)

## Validação

A compilação de scripts do Unity 6 foi concluída com sucesso após a implementação do combate, da Fase 2 e da Fase 3. Os testes de Play Mode estão em `Assets/Tests/PlayMode`; eles devem ser executados pelo Test Runner do Unity antes de uma build de distribuição.

Este repositório é um protótipo em desenvolvimento. Os assets gerados e os sistemas descritos aqui fazem parte da versão atual da `main`.
