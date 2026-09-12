# Edelzio V3 — arte e interações

Arte final: `Assets/Resources/Varginha/EdelzioTopDownV3.png`.
PNG RGBA 256 × 384, 24 células de 64 × 64, fundo transparente, pés alinhados.
Linhas: caminhada baixo/esquerda/direita/cima, café, agachar/alcançar/sentar/digitar.
O desenho é exibido com fator visual 1,45, sem ampliar o collider.

Gerado com a ferramenta integrada ImageGen, usando a foto fornecida como referência de aparência e a folha V2 como referência de estilo. Remoção de fundo e alinhamento por script local autorizados pelo usuário. A mochila é um sprite separado, cinza, com posição e ordem de desenho por direção.

## Prompt da geração

Create a production game pixel art sprite atlas PNG with true transparent background. Reference 1 is the real man's likeness: short dark brown hair with receding temples, rectangular thin dark glasses, warm medium tan skin, modest short beard and mustache, mustard yellow plain short sleeve T shirt. Reference 2 is only game pixel art style reference. New character Edelzio should resemble reference 1, blue jeans and dark shoes. NO backpack, NO cup, NO laptop baked into sprites. Exactly 4 columns and 6 rows, equal size cells, all characters centered identically in each cell with consistent scale, feet baseline and head sizes. No grid or labels or text. Whole image portrait ratio 2:3, preferred 1024x1536. Every cell has one entire character, transparent padding. Top four rows form walking loops: row1 facing down/front four steps; row2 facing left four steps; row3 facing right four steps; row4 facing up/back four steps. Fifth row: four front-facing coffee drinking poses with right hand from chest to mouth then down (no cup: cup added in engine). Sixth row: cell1 crouch front, cell2 reach front, cell3 sitting facing right hands resting in lap, cell4 sitting facing right typing with arms extended toward imaginary desk (no furniture). Match body proportions and clothing across ALL 24 cells; crisp pixel edges, restrained palette, recognizable adult man with glasses and beard. No ground shadows outside sprites, no checkerboard background.

## Prompt da revisão final

Edit this sprite sheet keeping all 24 sprites, 1024x1536 dimensions, 4 columns 6 rows, and exact placement. Change ONLY the bottom row's last TWO sprites: the sitting and typing poses must FACE FRONT (toward bottom of screen, directly toward viewer), not right. Same Edelzio short brown hair, thin rectangular glasses, short beard, plain mustard T-shirt blue jeans. In bottom row cell3 seated frontal with bent knees, hands at lap. Cell4 seated frontal with both forearms slightly extended forward/down toward keyboard in front of him (do not draw keyboard or desk). Keep same head and body size as other frames; seated shorter due bent knees. All remaining 22 sprites must remain unchanged. Use a uniform pure white background, no checkerboard and no ground shadow. Hard crisp pixel outline so background can be removed.

## Verificação

- Compilação de Game, GamePlayModeTests e Assembly-CSharp-Editor: zero erros e avisos.
- Verificação visual da folha e de seu canal alpha concluída.
- Testes adicionados para parada ao bloquear entrada, posição/ordem da mochila e consistência de arte/collider nas ações. Compilados, mas ainda não executados em Play Mode.
- Validação dentro da fase pendente: ferramenta de conexão indisponível; instalação remota bloqueada pela revisão automática.
- As alterações das duas cenas já estavam presentes antes desta tarefa; não foram editadas por este trabalho.

Ao testar no Unity, conferir mochila em quatro direções, café completo, saída/cancelamento e conclusão do quiz, caminhada após levantar e aproximação das interações junto a móveis e paredes.
