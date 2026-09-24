# Edelzio, Fabio e pausa — 24/09/2026

As folhas foram refeitas com o gerador de imagens integrado (`image_gen`), usando Matias como referência de proporção e os dois desenhos enviados como referência de identidade e roupa. Fontes com transparência em `Assets/ArtSource/Allies/Edelzio.png` e `Assets/ArtSource/Allies/Fabio.png`. O importador existente gera os arquivos de produção de mesmo nome em `Assets/Resources/Varginha/Allies/`.

Cada folha tem 4 × 4 quadros: frente, esquerda, direita e costas; primeira coluna em descanso. A produção usa células de 64 px, altura máxima de 50 px, pés na mesma linha e filtro Point. Edelzio usa a mesma densidade de pixels dos alunos. Sua animação de descanso não alterna quadros de caminhada e respeita a pausa.

A pausa compartilha `PixelHUDFrame`, cores e textura da caixa do Rodrigo. Textos próprios medidos com a fonte do projeto evitam o corte causado pelo estilo de botão anterior. A tela de controles usa a mesma moldura e rolagem em janelas baixas.

## Verificação

- Compilação no Unity aberto sem erros.
- Invocadas as asserções existentes de `VarginhaStudentSpriteTests` para os nove alunos e Edelzio: 16 quadros distintos, transparência/margens, pés alinhados, filtro Point e retrato na linha frontal.
- Invocada a verificação existente `EdelzioAndFabioSheetsGenerateValidDirectionalFrames`.
- Verificados os estados de pausa/retomada e `Time.timeScale`.
- Capturas reais em `Logs/CharacterPauseQA/`: pausa em 1918 × 879 e 544 × 504, controles em 544 × 504 e retratos no inventário em 960 × 720.
- Resultado das asserções em `Logs/CharacterPauseQA/validation.txt`; execução via `scratch/verify_character_pause.cs`, sem reiniciar a partida aberta.

## Prompts finais

### Fabio

Use case: style-transfer. Create a production game sprite sheet of FABIO, matching precisely the chibi proportions and stylization of the first reference (Matias ally sheet), using second reference ONLY for Fabio's identity and costume. Square image, EXACTLY 4 rows x 4 columns equal cells, 16 whole-body figures, centered with ample transparent space in each cell, feet aligned. Transparent alpha background, no shadows or ground or labels or borders. Row 1 ALL face FRONT toward viewer; row 2 ALL face LEFT profile; row 3 ALL face RIGHT profile; row 4 ALL face BACK away. In each row column 1 idle with both feet planted, column 2 step left, column 3 passing stride, column 4 step right. Consistent character size across all 16 frames. Big expressive head about 45 percent of total height, compact short body and legs exactly like Matias; NOT the realistic tall proportions of second reference. Fabio: warm skin, young clean-shaven face, NO glasses, dark charcoal-brown medium long wavy hair framing cheeks and asymmetric fringe, black Metallica-style band T-shirt with subtle electric blue lightning graphic, silver necklace, black wristbands with silver studs, charcoal cargo pants and side chain, dark sneakers. Clear eyes and visible face in front portrait. Chunky clean pixel-art clusters, crisp dark outline, limited palette, game-ready readable shapes at 50-pixel character height. Match first reference's warm highlights, outline weight, head size, width and body proportions. Do not copy Matias hair or uniform.

### Edelzio

Use case: style-transfer. Create a production game sprite sheet of EDELZIO, matching precisely the chibi proportions and stylization of first reference (Matias ally sheet), using second reference ONLY for Edelzio identity and costume. Square image, EXACTLY 4 rows x 4 columns equal cells, 16 whole-body figures, centered with ample transparent margin inside each cell. Transparent alpha background, no ground shadows, labels or borders. Row 1 ALL face FRONT toward viewer; row 2 ALL face LEFT profile; row 3 ALL face RIGHT profile; row 4 ALL face BACK away. Each row column 1 idle with both feet planted, column 2 left step, column 3 passing stride, column 4 right step. Equal size and feet baseline across figures. Big head 45 percent total body height, short compact torso and legs EXACTLY like Matias, NOT tall realistic human anatomy from second ref. Edelzio: man with warm tan skin, short dark brown slightly spiky hair, thick black rectangular glasses with visible friendly eyes, small dark goatee and moustache, mustard golden yellow short-sleeve plain T-shirt, dark charcoal jeans, black watch on wrist, black sneakers with light sole. Arms relaxed or walking swing, NO thumbs up. Rounded expressive friendly chibi face. Chunky clean pixel-art clusters, crisp dark outline, limited warm highlights and shadows, readable shapes at 50-pixel character height. Match Matias width, head proportions and outline weight. No uniform, no curly hair.
