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

## Prompts ImageGen

### Caminhada (edição preservando identidade)
undefined

### Combate (edição preservando identidade)
undefined

### Mobiliário (novo conjunto)
undefined

## Validação

- Compilação no Unity: sem erros ou avisos.
- Edit Mode: 106/106 testes aprovados, incluindo atlas, transparência, dimensões, atualização dos móveis e colisores.
- Play Mode e inspeção visual: em conclusão.

