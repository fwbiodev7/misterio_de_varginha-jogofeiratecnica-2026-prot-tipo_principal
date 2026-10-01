# Correção dos braços do soco

O quadro lateral copiava as mãos pendentes da pose parada junto com as pernas, criando uma mão extra perto da cintura. A montagem agora elimina essas mãos do trecho copiado. A fonte de combate também recebeu antebraços mais finos e punhos menores; cabeça e pés continuam derivados da pose parada.

- Arte fonte: `Assets/ArtSource/EdelzioPunchSourceV4.png`.
- Atlas usado pelo jogo: `Assets/Resources/Varginha/EdelzioPunchV3.png` (mantido o nome do recurso para atualizar os consumidores existentes).
- Comparação com mochila: `Docs/Previews/EdelzioCabecaComparacao.png`.
- Ferramenta: ImageGen integrada, edição com fundo transparente; montagem e importação feitas no Unity.

## Prompt

Use case: precise-object-edit. Asset type: production pixel-art punch sprite sheet for Unity. Image 1 is the EDIT TARGET: preserve EXACT 9 equal columns x 4 equal rows, pose order and facing directions. Image 2 is the identity and body-proportions reference. Change ONLY the boxing arms/hands and mustard sleeve contours in Image 1: make forearms slim with consistent 3 logical pixel thickness and small CLOSED FISTS 4x4 logical pixels, matching the thin idle arms in Image 2. Every hand MUST be visibly attached by an unbroken wrist, forearm and upper arm to its shoulder. Fix the straight-side punches (rows2 and3 columns3,4,6,8): natural shoulder-to-elbow-to-wrist anatomy, short upper arm, full thin skin forearm, compact fist, no enormous squared-off glove or thick sausage hand. Opposite hand stays tucked by chest; no dangling detached hand by waist on the contact poses. Draw smooth anticipation, extension, contact and recovery; keep consistent arm length, do not lengthen the torso. Preserve existing head, glasses, hair, legs, feet, identity, palette, centered baseline and spacing exactly; final game will copy idle head and legs. Crisp pixel art logical64x64 per cell, nearest-neighbor enlarged4x. True transparent background, no text, no labels, no grid, no motion trails. Row1 facing DOWN, row2 LEFT, row3 RIGHT, row4 UP/back. Same dimensions/aspect ratio as image1.
