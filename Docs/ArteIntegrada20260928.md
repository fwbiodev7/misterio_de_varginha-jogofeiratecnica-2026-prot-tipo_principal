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

