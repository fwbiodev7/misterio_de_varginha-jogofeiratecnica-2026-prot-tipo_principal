# Histórico de atualizações

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
- A cena da escola é deliberadamente simples: o foco desta versão é validar o loop de combate, resgate e transição.
- A entidade ancestral continua reservada para o arco narrativo e não recebe dano do ataque comum.
