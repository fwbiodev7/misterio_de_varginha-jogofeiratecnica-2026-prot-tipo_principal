# Edelzio — sequência de socos V2

Arte criada com ImageGen integrado a partir do atlas atual de Edelzio e das duas referências de personagem e movimento enviadas pelo usuário.

- Fonte: `Assets/ArtSource/EdelzioPunchSourceV2.png`, 9 poses × 4 direções.
- Atlas do jogo: `Assets/Resources/Varginha/EdelzioPunchV2.png`, 1152 × 256; células 64 × 64, baixo/esquerda/direita/cima.
- Jab, direto com braço oposto e finalizador; cada golpe usa guarda, preparação, extensão, contato, retorno e guarda.
- São 36 poses de origem distribuídas em 72 entradas, incluindo poses repetidas de guarda e sustentação do impacto.
- Duração nominal: 260 ms para jab/direto, 291 ms para finalizador, mais hitstop em acertos.
- Dano: 34 / 37,4 / 49,3; uma aplicação por alvo/golpe, respeitando paredes e entidade ancestral.
- Um clique durante o ataque guarda somente o próximo soco. Manter pressionado continua o combo. Esquiva/interação/morte/vitória descartam a fila. Pausa congela a animação.

`Varginha/Art/Rebuild Edelzio Punch Atlas` reconstrói a folha com amostragem nearest-neighbor, escala constante por direção e alinhamento pelos pés. Importação Point sem mipmaps/compressão; pivô, pixels por unidade e collider preservados. V1 permanece como fallback.

O acabamento de materiais existente foi estendido a plantas, folhas, livros, murais, lousas, tapetes, colunas e estacionamento. Luzes suaves não recebem textura de superfície.

## Prompt original

Use case: identity-preserve. Asset type: production pixel-art game combat sprite sheet for Unity.
Reference 1 is the exact CURRENT Edelzio game sprite; preserve its identity, tiny pixel scale, mustard yellow t-shirt, charcoal trousers, black sneakers, dark rectangular glasses, short dark hair and small beard. Reference 2 shows the same character's details. Reference 3 supplies boxing animation timing/poses only.
Create a NEW TRANSPARENT PNG sprite sheet with EXACTLY 9 equal columns and 4 equal rows, canvas 2304x1024 (each cell 256x256). Each of the 36 cells contains one complete Edelzio, no overlaps. Pixel art logically drawn on 64x64 cells enlarged 4x with nearest-neighbor crisp pixels. Figure about 42 logical pixels tall, soles at logical y=58, hips centered x=32, consistent head and body size, lots of padding for fists. No ground shadows, no grids, no labels, no text, no weapons, no slash effects, no backpack.
Row1: facing DOWN/front (punching toward viewer with foreshortening); row2: facing LEFT; row3: facing RIGHT; row4: facing UP/back (punching away from viewer). Direction MUST remain correct throughout each row.
Each row has these exact nine sequential poses: 1 guard both clenched fists at chest; 2 anticipation slight backward torso twist bent knees; 3 extending lead jab halfway; 4 full lead jab contact, other fist guards chin; 5 retract jab and coil opposite shoulder; 6 opposite arm cross punch fully extended with torso twist; 7 retract cross and lower body preparing strong punch; 8 strong straight finisher full extension body leans toward hit and front knee bends; 9 follow-through with elbow bending back toward guard. Strong readable boxing poses with anatomical connected arms, two arms only, deliberate leg weight shifts and planted feet. Shoulders and elbows articulate naturally, never just sliding idle sprites. Maintain same identity, clothes and sharp pixel outlines across ALL 36 cells. True transparent background.

## Revisão

Corrigir apenas direções: espelhar cada personagem da segunda linha para esquerda, sem reordenar quadros; primeira linha, colunas 6 e 8, punhos para frente com perspectiva encurtada; quarta linha, golpes para cima/de costas, punhos acima dos ombros. Preservar identidade, grade, roupa, pixel art e transparência.

