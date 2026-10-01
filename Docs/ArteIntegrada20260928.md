# Arte integrada — 28/09/2026

Edelzio: pele bege/castanha menos saturada, cabelo curto com entradas discretas, óculos e barba baseados na foto fornecida. Caminhada e combate compartilham a direção de cor. Removida a conversão que voltava a deixar a pele alaranjada ao reconstruir o atlas.

A recuperação do soco termina em guarda. O próximo comando guardado é consumido imediatamente ao concluir o golpe, sem limpar a pose entre corrotinas. O combo mantém três golpes e um único comando pendente.

Mobiliário: conjunto de 12 peças em madeira castanha, tecidos azul-petróleo e detalhes creme: escrivaninha, cadeira, sofá, mesa de centro, estante, criado-mudo, cômoda, gabinete de cozinha, cama, carteira escolar, banco de igreja e altar. Os pisos de casa/escola/igreja receberam cores mais discretas para combinar com o conjunto. Os colliders existentes são preservados.

## Arquivos e reprodução

- Fontes: `Assets/ArtSource/EdelzioCleanSourceV2.png`, `EdelzioPunchSourceV2.png`, `FurnitureSourceV1.png`.
- Assets em uso: `Assets/Resources/Varginha/Allies/Edelzio.png`, `EdelzioPunchV2.png`, `FurnitureV1.png`.
- Reconstrução: menu `Varginha/Art/Rebuild Edelzio Punch Atlas`. Recria caminhada, padre, combate e mobiliário a partir das fontes.
- Personagens: células 64 × 64; mobiliário: atlas 256 × 192. Filtro Point, alpha transparente, sem mipmaps.
- As divisões horizontais da fonte de móveis são irregulares e estão documentadas no construtor do atlas.

## Composição dos ambientes

- Casa: móveis organizados por cômodo, acessórios apoiados nas mesas, cozinha alinhada e sombras de contato. As seis interações principais permanecem acessíveis a partir da posição inicial.
- Escola: carteiras afastadas da divisória, cadeiras alinhadas, lousa inferior visível sobre a parede e estantes com a madeira do novo conjunto.
- Igreja: bancos e altar do conjunto compartilhado; estante da sacristia atualizada.
- Fase 2: estacionamento externo, acesso de veículos, calçada e porta aberta. A câmera acompanha o jogador na área ampliada e os alunos procuram rotas livres de paredes até o carro.

## Validação

- Compilação no Unity: sem erros ou avisos.
- Edit Mode: execução mais recente com 93/93 testes aprovados, incluindo composição, estacionamento fora da escola, passagem para o jogador e rotas dos nove alunos.
- Capturas e relatórios locais em `Logs/`; revisão final de Play Mode registrada ao concluir a retomada.

## Retomada das interações — 29/09/2026

Base atualizada para `2001d7e` de `origin/main`, preservando as melhorias de resgate, iluminação e a arte atual da cabine do Fusca.

- Integrada `Assets/Resources/Varginha/EdelzioInteractionsV1.png`: a última folha gerada na tarefa anterior, corrigida conforme a referência definitiva `EdelzioCleanSourceV2.png`. Contém quatro quadros de café, quatro de cadeira e quatro de webcam; importação legível, filtro Point, sem mipmaps e sem compressão.
- Os recortes encontram os espaços transparentes entre as linhas da folha, evitando cortar os pés e misturar partes de ações diferentes. A pose de descanso sentado usa o quadro de joelhos dobrados.
- Interromper o café restaura a xícara e permite tentar de novo. O consumo e o evento de interação acontecem apenas ao terminar a animação.
- Fechar o notebook restaura o tempo, a posição e a colisão da cadeira. Uma abertura recusada ou a desativação do personagem também encerram a sessão sem prender o controle.
- Regressões em `VarginhaInteractionTests`: recursos e recortes, café interrompido e repetido, saída e interrupção do notebook, pose sentada e retratos atuais.
- Resgate: os alunos planejam o trajeto a partir da posição física, mantêm folga nas quinas e verificam o próximo passo antes de mover. Se a vaga estiver obstruída, podem concluir o embarque no ponto livre próximo ao destino.
- Câmera: o zoom é recalculado depois de reservar a faixa do inventário, usando o formato efetivo da área de jogo já no primeiro quadro.

A folha foi recuperada da geração integrada de imagens da tarefa anterior, sem nova geração. A instrução final pediu a manutenção da grade de café/cadeira/webcam, com o rosto, cabelo castanho, óculos com olhos visíveis, pequeno bigode/cavanhaque, camiseta mostarda e proporções do Edelzio da referência definitiva.

Validação no Unity 6000.6.0f1: 40 testes distintos aprovados após as correções (10 de Edit Mode e 30 de Play Mode). Os quatro testes de resgate foram reexecutados depois do ajuste final de parada dos alunos; os demais 26 testes de Play Mode já haviam passado. Relatórios locais em `Logs/interaction-edit-regression.xml`, `Logs/interaction-play-regression.xml` e `Logs/interaction-rescue-final.xml`, com consolidação em `Logs/interaction-validation-summary.txt`. A compilação e `git diff --check` também passaram.

