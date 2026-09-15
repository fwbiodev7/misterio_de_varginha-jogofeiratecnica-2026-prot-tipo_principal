# Histórico de atualizações

## 15/09/2026 — mochila, consumíveis, luz suave e porta do Fusca

- Inventário pela mochila da hotbar/clique ou tecla G: abre nos Itens físicos, com aba Alunos separada, descrição de poderes e botão Equipar. Setas navegam, Tab alterna abas, Enter equipa e Esc/G fecha. Layout escuro de três colunas inspirado na referência fornecida.
- Nenhum aluno equipado por padrão; somente o escolhido é exibido no cenário e no HUD. Trocar esconde o anterior, preservando sua recarga.
- Mochila e notebook redesenhados em cinza, com perfil da mochila, aba/fivelas/bolsos, notebook de tela escura e versão fechada. Encaixes reduzidos ao tamanho do torso; xícara acompanha suavemente o gesto de beber.
- O especial usa exclusivamente o aluno equipado. Trocar não reinicia recargas; o mundo fica pausado enquanto a mochila está aberta.
- Consumo dos itens físicos depois do uso efetivo, preservando a investigação. Saída bloqueada do Fusca permite nova tentativa e não consome requisitos. Os itens usados não reaparecem na escola/igreja.
- Luz estática composta nas três fases, transparência contínua, cruzamentos atenuados, sombras suaves por móveis/paredes e filtro Bilinear somente nas luzes/sombras. Origem e largura dos feixes calculadas a partir das dimensões de cada janela.
- Porta do Fusca com silhueta curva e projeção contínua pela dobradiça dianteira, com entrada do personagem alinhada à porta e suporte ao carro espelhado. Tamanho corrigido para a abertura da cabine, desconsiderando a margem transparente do sprite.
- GDD atualizado para 1.2.0; testes automatizados acrescentados para inventário, seleção/recargas, luz e geometria da porta.

## 15/09/2026 — consolidação do GDD

- Consolidado `Docs/GDD.md` na versão documental 1.1.0 a partir do GDD original 1.0.0 enviado pelo grupo e das regras atuais dos commits `ca185c2` e `c331760`.
- Preservados os seis atos, personagens e três finais; identificadas as mecânicas originais implementadas parcialmente e as previstas para a continuação.
- Documentados combo, esquiva, papéis dos ETs, dificuldade, poderes dos nove alunos, comandos separados, recarga individual de 5 s e intervalo de 0,9 s entre comandos da turma.
- Especificados os requisitos efetivos de conclusão das fases, controles remapeáveis, cenografia, viagem de Fusca e limites das integrações futuras.
- Atualizados README e arquitetura para refletir os comportamentos atuais. As seções anteriores abaixo registram etapas históricas, incluindo controles e visuais depois substituídos; o GDD consolida a versão vigente.
- Revisão documental por leitura do código; testes do Unity não executados nesta atualização.

## Acabamento visual — casa, escola e diocese

- A diocese ganhou dez projeções coloridas dos vitrais no piso, com o mesmo padrão dos vidros, transparência e ordenação abaixo dos bancos e personagens. Novos bancos, altar, arandelas, pilastras, mosaico, plantas e luz suave de velas completam o ambiente.
- A escola recebeu parquet menos contrastado, paredes claras com faixa verde, carteiras com material escolar, lousas, murais, estantes, plantas e luz das janelas.
- A Fase 1 ganhou madeira menos ruidosa, iluminação quente dos cômodos, luar nas janelas, quintal noturno, poste redesenhado, folhas, poças e vaga-lumes discretos.
- Os novos sprites são desenhados no tamanho nativo, com filtro Point, sem mipmaps e reutilizados em cache. A vaga do Fusca também usa o tamanho completo da textura, sem escala duplicada.
- O acabamento é aplicado pelos builders e na entrada das cenas existentes, sem novos obstáculos. A inicialização evita duplicatas e recupera a decoração quando uma cena salva perde referências aos sprites procedurais.
- Adicionados testes de arte, camadas dos reflexos, dimensões da vaga, reinicialização e recuperação de sprites. Prévias visuais verificadas em uma cópia isolada do projeto Unity.

## Ajuste — nomes dos aliados e vaga do Fusca

- Os aliados agora exibem somente o nome sobre o sprite; prontidão, recarga e poder continuam concentrados no HUD lateral da Fase 3.
- A vaga do Fusca foi centralizada em uma referência compartilhada e ampliada para caber a carroceria inteira, tanto nas cenas geradas em runtime quanto no builder de teste.

## Ajuste final — vaga, ETs, porta e continuidade da fuga

- A vaga do Fusca passou a ter uma única posição compartilhada pelo marcador do cenário, pelo runtime da Fase 2 e pelo builder de teste; o carro fica dentro da área jogável e não atrás da parede oeste.
- Todos os subordinados e a manifestação ancestral usam a leitura de ET marrom com olhos vermelhos; os papéis continuam diferenciados por comportamento, golpes e telegráficos, não por trocar a cor do corpo.
- A porta foi redesenhada em pixel art compacto e teve a escala física reduzida, com dobradiça e quadros fechado/entreaberto/aberto preservados.
- A saída da Fase 1 percorre somente o trecho curto da rua antes de entregar o restante à viagem cinematográfica pixel art, evitando o carro terminar fora do cenário.
- O painel de combate do HUD agora reduz a fonte quando necessário e calcula cada linha contra a largura disponível, evitando texto de combo/esquiva truncado em janelas menores.

## Continuação — controles, combo, porta e Marcos

- Criada tela `EDITAR CONTROLES DO TECLADO E MOUSE` no menu, com linhas de comando, botões de reset e persistência entre cenas.
- O movimento, interação, ataque, esquiva e comando de aliados passaram a consumir o mapa remapeável; WASD/setas, espaço/enter, Ctrl e J/K continuam compatíveis.
- Edelzio agora usa três sequências visuais de combo: corte inicial, golpe cruzado e finalizador pesado, com dano, alcance e efeitos progressivos.
- Marcos alterna cortada de vôlei, chute voador e cotovelada, apresentando bordões sem censura no impacto.
- A turma aliada ganhou circulação livre, separação entre colegas e desvio simples de paredes. Fases comuns podem liberar três alunos apenas no modo DIFÍCIL; a fase final libera os nove.
- A porta do Fusca passou a girar em um pivô de dobradiça real, com sprites procedurais em cache e easing independente na abertura e no fechamento.

## Continuação — Fusca e identidade visual das fases

- Corrigida a partida do Fusca para manter a faixa e a profundidade exatas, sem rotação da carroceria, órbita da porta ou deslocamento acumulado nos passageiros.
- Mochila e notebook ficam ocultos durante a chegada/entrada no carro e só reaparecem quando Edelzio termina de sair, evitando o retângulo azul atravessando a carroceria.
- A Fase 2 agora recebe cenografia própria de escola: piso parquet, paredes de tijolo, salas divididas, lousas, carteiras, cadeiras, armários, janelas, cortinas, relógio, murais e saída.
- A Fase 3 agora recebe cenografia própria de diocese: pedra escura, paredes de alvenaria, vitrais coloridos, bancos, tapete, estrado, altar, velas, leitor e porta da sacristia.
- A arte das duas áreas é procedural em `VarginhaEnvironmentArt` e usa sprites pontilhados específicos em `VarginhaPixelArtSprites`, com construção idempotente para cenas antigas.

## Continuação — HUD e viagem cinematográfica

- Corrigido o HUD de combate: o painel agora fica acima da hotbar, respeita as dimensões da tela e usa os comandos remapeados, sem cortar a linha de combo ou esquiva.
- A escola ganhou uma vaga interna demarcada para o Fusca, afastada da parede e com margem para a chegada e a fila dos alunos.
- A saída de cada fase agora percorre apenas o trecho curto até a vaga; o restante do trajeto é entregue à cinematics pixel art 2D, evitando que o carro desapareça pela borda do mapa.
- A transição entre fases mantém carregamento assíncrono, fade em pixels, estrada noturna, Fusca animado, tomada interna com a turma e barra de progresso, com duração mínima reduzida para não alongar a espera sem necessidade.

## Protótipo atual — Fase 3, aliados invocáveis e Ato III

### Fase 3 — O Guardião

- Criada a cena `Assets/Scenes/Fase3_Igreja_Guardiao.unity`, registrada no Build Settings.
- Implementado o Ato III do GDD com igreja/diocese, altar, símbolo do selo, Padre Fábio, Livro do Tombo Secreto e cinco subordinados ETs.
- O clique esquerdo dispara Edelzio e invoca o próximo aluno pronto para um único golpe; cada aliado mantém cooldown individual de 5 segundos.
- Adicionado painel de HUD com os nove alunos e o tempo restante de cada golpe.
- Corrigida a apresentação da turma: na Fase 3 os corpos não se sobrepõem; cada aliado aparece como cabeça e nome com indicador de pronto/cooldown.
- O ataque de Edelzio voltou ao clique esquerdo na Fase 3, com cooldown independente de 5 segundos e sem bloquear o golpe do aluno invocado.
- Após derrotar os ETs, Padre Fábio e o livro liberam a conclusão da fase e a revelação sobre o selo de 1898.

## Protótipo anterior — Fase 2, combate e apresentação

### Correções recentes

- Corrigido o dano das rajadas dos ETs: o projétil agora aplica vida e sanidade mesmo durante o lock curto da animação de Edelzio, sem depender da sanidade ainda estar acima de zero.
- A entrada da Fase 2 agora repara cenas parcialmente salvas: garante quatro ETs, os nove reféns, o Fusca e a câmera antes de iniciar o encontro.
- Os nove alunos recebem sprite, nome, jaula e ordem de desenho válidos; o resgate não começa prematuramente se a população estiver incompleta.
- Corrigida a formação pós-resgate: a turma acompanha Edelzio em uma grade espaçada e entra no Fusca em fila, sem sobreposição de sprites.
- Corrigida a exceção na criação da jaula que interrompia a geração depois da Yasmin; agora todos os nove reféns são criados e as jaulas são recriadas quando uma referência antiga foi destruída.
- Corrigido o cache de sprites procedurais entre cenas e a hotbar agora recupera ícones destruídos sem lançar `MissingReferenceException`.
- Corrigido o encaixe do notebook, mochila e copo nas poses direcionais, com sorting order e offsets separados para cima, baixo e laterais.
- Subordinados ETs ganharam silhueta mais ameaçadora, olhos vermelhos e boca dentada; o ataque passou para 18 de dano de vida e 14 de sanidade, com rajada maior.
- Criado o sistema de aliados para fases futuras: os nove alunos podem seguir Edelzio e atacar automaticamente com perfis próprios, incluindo jiujitsu, ping-pong, guitarra, arte, microfone, piano da Yasmin, katana do Fabio e apoio.
- Props da Fase 1 passam a restaurar automaticamente sprites pixel art, colliders e sorting order quando a cena contém referências antigas ou sprites invisíveis, preservando itens já coletados.

### Gameplay

- Criado o combate direcional com arma para Edelzio.
- Adicionados antecipação, impacto, recuperação, hitbox dinâmica, dano, knockback, hitstop e tremor de câmera.
- Subordinados ETs passaram a perseguir Edelzio, reagir aos golpes e desaparecer ao serem derrotados.
- ETs receberam visual verde de olhos pretos inspirado na referência e uma rajada de energia que causa dano de vida e sanidade no jogador.
- A entidade ancestral permanece protegida contra o ataque comum.
- Criada a Fase 2 da escola com resgate dos nove alunos do terceiro sistema.
- Cada aluno começa dentro de uma jaula pixel art com barras verdes pulsantes; a jaula desaparece no resgate.
- A última derrota liberta a turma; os alunos acompanham Edelzio e entram no Fusca.
- A saída da Fase 1 carrega automaticamente a Fase 2.

### Animação e personagem

- Edelzio passou a usar a folha V3 inspirada na referência visual fornecida.
- Corrigidas as ações de café e de sentar/usar notebook para não voltarem ao estado antigo.
- Mochila cinza maior, presa às costas, com alças e ordenação por direção.
- Corrigida a partida do Fusca para seguir uma trajetória horizontal estável, sem deriva diagonal.
- Criados sprites de ataque, alunos, subordinados ETs e efeitos de impacto.

### Interface e itens

- Adicionada hotbar de cinco slots para os itens coletados no mapa.
- Hotbar sincronizada com mochila, chave, caderno, dados do notebook e documento histórico.
- HUD passou a exibir instrução de ataque e mensagens de resgate.

### Organização

- Nova cena `Assets/Scenes/Fase2_Escola_Resgate.unity` registrada no Build Settings.
- Novo construtor de cena em `VarginhaPhase2Builder` para reconstruir a escola pelo menu `Tools/Varginha`.
- Adicionados testes de apresentação, saída do Fusca e combate em `Assets/Tests/PlayMode`.
- Documentação técnica e de uso atualizada em `README.md` e `Docs/`.

## Limites conhecidos do protótipo

- Os testes de Play Mode estão preparados, mas ainda precisam ser executados pelo Test Runner do Editor antes de uma build final.
- A cenografia das fases 2 e 3 é procedural e reproduzível; os testes de Play Mode ainda precisam ser executados no Test Runner do Editor antes de uma build final.
- A entidade ancestral continua reservada para o arco narrativo e não recebe dano do ataque comum.
