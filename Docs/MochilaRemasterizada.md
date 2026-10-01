# Mochila remasterizada

## Versão manual do usuário

### Edição manual dos socos de 01/10/2026

`Docs/Previews/MochilaUsuarioSocos.png` foi preservado sem alterações e copiado integralmente para `Assets/ArtSource/EdelzioBackpackUserPunchV3.png`. O recurso `Assets/Resources/Varginha/Equipment/EdelzioBackpackUserPunchV3.png` guarda as 16 poses em células nativas de 64 pixels, importadas com filtro Point e sem compressão. A ordem é sul, oeste, leste, norte; as colunas são parado e quadros 3, 9 e 15 dos três socos.

`EdelzioBackpackPunchBaselineV3.png` preserva o preview anterior à edição para identificar somente os pixels alterados manualmente, incluindo retoques em cores da camisa e dos braços. Essa referência deve permanecer intacta ao regenerar as animações. Cada sequência de seis quadros usa o retoque do soco correspondente durante o golpe e o retoque parado no início e no retorno. Na caminhada, os retoques parados acompanham o tronco; nas interações, permanece a composição das oito direções. Um pixel de contorno que descia sobre a calça na caminhada sudoeste foi limitado à região da mochila.

As folhas equipadas e o catálogo de 304 quadros foram regenerados. `Docs/Previews/MochilaUsuarioSocosAplicada.png` é uma conferência separada, extraída dos sprites empacotados usados no jogo. O teste `ManuallyEditedPunchPreviewMatchesThePackagedGameFrames` compara as 16 poses da arte manual com a composição e com os recursos finais, tolerando apenas até três níveis de cor decorrentes do round trip PNG. Cabeça, pés, escala e pivôs continuam cobertos pelos testes existentes.

O desenho manual enviado em 01/10/2026 substitui a apresentação anterior das oito direções. Fonte preservada em `Assets/ArtSource/EdelzioBackpackUserDirectionsV2.png`; recurso nativo em `Assets/Resources/Varginha/Equipment/EdelzioBackpackUserDirectionsV2.png`. As alças curvas laterais e as vistas de frente/costas das diagonais vêm desse arquivo, sem redesenho por IA.

A composição identifica os pixels da mochila e das alças na área do tronco, mantendo cabeça, mãos, pés, pivôs e escala dos sprites atuais. A mochila fica ancorada ao tronco; o braço em movimento passa diante da alça durante as ações. Os 304 quadros equipados são regenerados a partir dessa versão. A prévia editada em `Docs/Previews/MochilaRemasterizada8Direcoes.png` é preservada; as conferências do resultado usam arquivos separados.

Referência: modelo cinza fornecido pelo usuário, com alça superior, dois bolsos, zíperes e oito vistas. Arte criada por edição com ImageGen integrada, preservando fundo transparente. Atlas montado e importado pelo Unity com pixels nítidos e sem compressão.

- Fonte: `Assets/ArtSource/EdelzioBackpackRemasterSourceV1.png`.
- Recurso do jogo: `Assets/Resources/Varginha/Equipment/BackpackRemasterAtlasV1.png`.
- Prévia no Edelzio: `Docs/Previews/MochilaRemasterizada8Direcoes.png`.
- Prévia dos socos: `Docs/Previews/MochilaSocos.png`.
- Prévia das interações: `Docs/Previews/MochilaInteracoes.png`.
- Animações completas: `Assets/Resources/Varginha/Equipment/EdelzioEquippedWalkV1.png` (32 quadros), `EdelzioEquippedPunchV1.png` (144 quadros), `EdelzioEquippedActionsV1.png` (128 quadros).
- Catálogo dos 304 sprites, com recorte, pivô e escala de cada pose original: `Assets/Resources/Varginha/Equipment/EdelzioEquippedV1.json`.
- Ordem na prévia: sul, oeste, leste, norte / sudoeste, sudeste, noroeste, nordeste.

As folhas equipadas usam os sprites atuais do Edelzio e já contêm a mochila desenhada em cada quadro. O jogo seleciona o sprite completo com mochila ao equipar, e o sprite original ao escondê-la ou removê-la. As camadas antigas separadas ficam desativadas. O recorte, o pivô, a escala, a cabeça, os pés e os colisores do Edelzio são preservados.

O volume aparece atrás do corpo nas vistas laterais e de frente; quando Edelzio mostra as costas, aparece sobre o tronco. As alças acompanham a camisa. A ligação do ombro ao volume lateral forma uma silhueta contínua sob o corpo, sem laço solto. Os socos usam a ancoragem do tronco parado para impedir que a mochila acompanhe a extensão dos braços. As diagonais da mochila são selecionadas pela direção real do movimento.

O caminho de criação das folhas também corrige a `MissingComponentException`: verifica componentes pela igualdade de objetos do Unity, inclusive quando um filho já existe sem componente. Durante o jogo, as animações equipadas usam um único renderer e os recursos empacotados, sem depender de arquivos de origem no computador.

Corrigido o desaparecimento ao equipar: referências gerenciadas a sprites destruídos pelo Unity permaneciam no cache. O cache agora carrega os quadros conforme são utilizados, verifica o sprite nativo e sua textura antes de reutilizá-los e recria quadros destruídos. A seleção também verifica a igualdade de objetos do Unity antes de recorrer à composição ou ao corpo original, impedindo que uma referência destruída chegue ao renderer.

## Verificação

- Conferência dos recursos, dos 304 quadros completos, das alças sobre a camisa, da ligação lateral contínua, da estabilidade durante o soco e da preservação da cabeça e da geometria.
- Verificação de equipar, esconder no Fusca, remover, socar e executar poses de interação no jogo.
- Prévias renderizadas a partir dos mesmos sprites completos selecionados no jogo.
- 8 testes de mochila em EditMode aprovados, incluindo recriação de sprites destruídos, conferência dos 304 quadros e correspondência com os 16 quadros do preview manual de socos.
- 6 testes de apresentação visual em EditMode aprovados, incluindo a cabeça e os pés durante os socos equipados.
- 7 testes em PlayMode aprovados, incluindo equipar com um sprite do cache destruído, trocar as oito direções, esconder a mochila no Fusca, remover, socar e executar as interações. Total: 21 testes aprovados, sem falhas. No editor, os testes usam a opção de cena atual uma vez para que o menu inicial não substitua a cena gerada pelo Test Runner.

## Prompt final

Use case: precise-object-edit. Asset type: production pixel-art equipment turn-around atlas for a 2D RPG. Input image1 is the USER'S EXACT BACKPACK MODEL to remaster. Preserve charcoal grey nylon, rounded rectangular silhouette, black outline, top handle, small upper zipper and wide lower front pocket, silver grey zipper highlights. Refine readable pixel clusters and consistent shading without changing model identity. Generate ONLY eight isolated backpacks in a strict 4-column x 2-row evenly spaced grid on TRUE TRANSPARENT background, NO labels or grid or characters. Every backpack has identical height, grounded same baseline inside its cell, ample empty margins. Top row left-to-right: outer FRONT with two pockets; inner BACK with two padded shoulder straps; LEFT SIDE profile; RIGHT SIDE profile. Bottom row left-to-right: diagonal OUTER FRONT LEFT; diagonal OUTER FRONT RIGHT; diagonal INNER BACK LEFT; diagonal INNER BACK RIGHT. For SIDE profiles keep the body compact, show pocket edge and seam, omit any giant empty shoulder-strap loop protruding out in front: shoulder straps will be a separate fitted on-character layer in game. Do not draw any floating cords. Shoulder straps on inner-back views are slim padded bands attached at top and bottom. Diagonal views must rotate the SAME backpack, consistent volume and pocket sizes, not different designs. Crisp retro pixel art logical24x32 per backpack, each pixel enlarged nearest-neighbor, no blur, gradients, glow, brown leather, text, watermark or extra items. Output landscape 4:3.
