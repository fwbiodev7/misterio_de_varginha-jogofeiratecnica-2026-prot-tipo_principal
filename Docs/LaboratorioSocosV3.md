# Laboratório, cadeiras e Edelzio — 1 de outubro de 2026

- Colisão das mesas: 1,55 × 0,40 unidades, independente da escala do sprite. A atualização global dos móveis preserva o laboratório, inclusive depois de recarregar a cena.
- Doze cadeiras azuis de costas para o jogador, voltadas às mesas. Uma pessoa por cadeira. As vazias permitem `[E] Sentar / levantar`.
- Os nove alunos começam sentados diante dos computadores. Ao serem libertados, levantam no corredor e seguem até o Fusca com física e navegação normais.
- Postes junto à fachada afastados da placa; proporção original preservada e halo alinhado à lâmpada.
- Socos V3: 72 quadros, quatro direções, mesma escala, pivô, cabeça e pés do Edelzio parado. Guarda e retorno reproduzem exatamente o quadro parado; braços e tronco mantêm as poses de soco. A reconstrução do atlas não substitui a arte de caminhada.

## Arte

Criada com a ferramenta integrada ImageGen, depois recortada e alinhada pelo pipeline C# do Unity.

- Fonte dos socos: `Assets/ArtSource/EdelzioPunchSourceV3.png`.
- Atlas de jogo: `Assets/Resources/Varginha/EdelzioPunchV3.png`.
- Cadeira de costas: `Assets/Resources/Varginha/SchoolChairRearV1.png`.
- Comparação parado/socos: `Docs/Previews/EdelzioSocosMesmoCorpo.png`.
- Comparação ampliada com mochila: `Docs/Previews/EdelzioCabecaComparacao.png`; colunas parado, preparação, contato e retorno.

### Prompt de socos

Use case: identity-preserve. Production game pixel-art combat sprite sheet. Image 1 is the exact current idle/walk Edelzio and is the identity, design and proportions authority. Image 2 is the punch sheet to edit: preserve its 9 columns, 4 rows and exact pose sequence/directions, but change EVERY head, body, arms and legs to look IDENTICAL to Image 1, with same short brown hair, small rectangular glasses, short beard, slim mustard yellow shirt, thin limbs, charcoal trousers and white soles. The previous punch figure is too wide and has an oversized head: narrow its torso and use precisely the head/body size ratio of image1. Same height and body width as image1 idle. Crisp tiny pixel art, logical 64x64 cells at 4x nearest-neighbor enlarged, 2304x1024 image, 9 equal columns 4 equal rows. Row1 faces DOWN/viewer, row2 LEFT, row3 RIGHT, row4 UP/back. Nine poses per row: guard, windup, jab extending, jab contact, retract, opposite-arm cross contact, retract/coil, finisher contact, return. Maintain consistent head and feet size and position. No backpack, no shadows, no motion effects, no text, no grid. Full true transparent background. Transparent padding in each equal cell. Head height ~12 logical pixels, complete figure ~42 logical pixels high, centered x32 feet baseline y58. Keep clean connected boxing arms, body mass stable; hands extend beyond original body width only during strikes.

### Prompt de cadeira

Use case: precise-object-edit. Reference image is current classroom furniture atlas, reference only the blue school chair. Generate ONE isolated matching BLUE SCHOOL CHAIR seen from BEHIND, facing AWAY from viewer / toward TOP of image toward computer desk. Same royal-blue rectangular backrest with four silver rivets, black and gray steel frame, same pixel art palette, clean black outlines and slight overhead 3/4 RPG game perspective. Backrest must be closest to viewer at bottom/front, seat cushion visible BEHIND backrest toward top. Clearly rear view, four metal feet grounded, no desk/computer/student in asset. One complete chair centered, generous transparent padding, portrait 2:3 ratio. Crisp pixel art logically ~48x72 pixels enlarged 8x nearest-neighbor. True transparent background, no checkerboard, no shadow, no labels/text.

## Verificação

Compilação do Unity sem erros. Testes Edit Mode: 5/5 do laboratório e 5/5 da apresentação visual, incluindo a cabeça inteira nos 72 quadros com e sem mochila. Play Mode: 1/1 de sentar, levantar, liberar movimento e restaurar as colisões da cadeira e da mesa.

Correção após as novas capturas: o atlas foi reconstruído, reimportado e o cache limpo; a cabeça usa exatamente os pixels da pose parada da respectiva direção. A mochila dos socos usa as dimensões do tronco parado, evitando aumentar quando o braço estende. Importação do V3 incluída no processador de arte, sem compressão nem redimensionamento.

Conferência direta na cena Fase2: nove alunos inicialmente sentados; após ReleaseTo, todos levantaram e os nove chegaram ao Fusca externo. Doze colisores de mesas com largura de 1,55 e postes completos ao lado da placa. A suíte antiga VarginhaSchoolRescueTests foi descoberta pelo editor, mas o executor encerrou com zero casos; esse resultado não foi contado como aprovação. O trajeto foi conferido diretamente em runtime com scratch/verify_seated_rescue_runtime.cs.
