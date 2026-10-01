# Cenário, colisões e entrada da Industrial

## Regra comum dos mapas

Casa/quintal, escola e igreja passam por `VarginhaWorldGeometry` ao carregar a cena e após as fábricas de runtime criarem a população. A migração é idempotente. `VarginhaWorldDepth` ordena objetos pelo ponto de contato com o chão, usando `SortingGroup` para manter personagens, mochila, mãos, cabeça, armas e acessórios na mesma profundidade. As ordens ficam abaixo dos efeitos de iluminação existentes.

Os postes permanecem atrás dos personagens e recebem uma base sólida estreita. Árvores bloqueiam pelo tronco. Plantas em vasos, arbustos, móveis, bancos, estantes e equipamentos grandes recebem colisão quando ela estava ausente. Colisores específicos de carteiras, cadeiras, paredes e móveis já configurados são preservados. Pisos, tapetes, asfalto, marcas da rua, luzes e passagens continuam livres. Objetos sobre mesas acompanham a profundidade da mesa.

## Fachada da fase 2

Referência: fotografia enviada pelo usuário da Escola Estadual Deputado Domingos de Figueiredo, Industrial. Arte criada com a ferramenta integrada ImageGen, com fundo transparente, muro branco desgastado, base verde, painéis dos cursos, identificação e portão aberto.

- Fonte: `Assets/ArtSource/SchoolIndustrialFacadeV1.png`.
- Recurso empacotado: `Assets/Resources/Varginha/SchoolIndustrialFacadeV1.png`.
- Importação reutilizável: `VarginhaIndustrialFacadeImporter.Configure()`.
- Integração: `VarginhaIndustrialSchoolFacade.Ensure()`, chamada por `VarginhaSchoolExterior`.

A arte foi importada com filtro Point, sem compressão e sem mipmaps, em dois painéis persistentes. Os painéis se encaixam nas paredes existentes. A passagem central de 2,5 unidades mantém a soleira, os colisores laterais e as rotas de resgate. Ao entrar, a fachada alta é recortada da apresentação para não esconder a sala de informática; o contorno original da parede permanece visível.

## Prompt da arte

Método: ImageGen integrado, fotografia como referência, fundo transparente.

```text
Use case: stylized-concept. Asset type: production transparent pixel-art school entrance sprite for a Brazilian top-down 2D RPG at 32 pixels per world unit. Input image is a reference photograph of Escola Estadual Deputado Domingos de Figueiredo (Industrial), Varginha. Create a beautiful, carefully crafted game facade matching this real school: long low weathered WHITE stucco perimeter wall, dark GREEN painted plinth, beige vertically slatted metal school gate near the middle, taller white rectangular wall mass on the RIGHT, turquoise and pale green geometric chevron emblem on the right white mass, painted blue word 'Industrial', small dark words 'E.E. DEP. DOMINGOS DE FIGUEIREDO' underneath. Left wall has six small classroom-course poster panels in two rows, black heading 'CURSOS TÉCNICOS'; tiny panel text may be simplified into convincing pixel strokes. Frame the ENTIRE front wall straight-on, with slight top-down RPG view of the wall cap and shallow roof thickness. Wide horizontal sprite, logical approximately 576x160 pixel-art at 2x or 3x nearest-neighbor enlargement, uniform crisp pixel clusters, restrained warm worn plaster, carefully shaded but no smoothing, dark restrained outlines matching rustic detailed RPG scenery. Essential gameplay constraint: place a clearly OPEN pedestrian doorway EXACTLY at the horizontal CENTER, width 14 percent of the sprite, reaching from the BOTTOM edge up to 70 percent of wall height. Draw beige metal gate leaves swung aside tightly against the two doorway jambs; the passage itself is TRUE TRANSPARENT, so player can walk through and into the school map. Leave matching empty margins; facade has flat horizontal baseline. Only the isolated school facade and short attached wire loops along wall top, no people, cars, utility poles, sky, street, sidewalk, background buildings or trees, no cast shadow on ground. Entire exterior background TRUE TRANSPARENT, no checkerboard illustration. Preserve recognizable school's identity and lettering, no invented towers, no isometric rotation, no perspective street scene.
```
