# O Segredo de Varginha — Game Design Document

**Equipe:** 3º Sistemas. **Versão documental:** 1.2.0. **Atualização:** 15/09/2026.

Base narrativa: GDD 1.0.0 fornecido pelo grupo nesta revisão. Base de implementação: `c331760` (14/09/2026), incluindo as entregas de `ca185c2` (12/09/2026). O repositório e o menu também usam o título **O Mistério de Varginha**; esta revisão preserva o título do GDD original, sem renomear o jogo nos arquivos de implementação.

Este GDD mantém os seis atos, os personagens, o mistério central e os três finais previstos no original. As features recentes foram incorporadas à proposta de gameplay. **Implementado** significa presente no código do protótipo, **parcial** indica uma parte disponível e **planejado** identifica conteúdo ainda sem integração jogável na sequência principal. Esses estados não equivalem à aprovação de QA.

## 1. Visão geral

### Tema

Terror sobrenatural, investigação e mistério inspirados no Caso do ET de Varginha e no folclore regional. A história é uma interpretação fictícia que relaciona acontecimentos de 1898, as memórias de 1996 e o presente de Edelzio, em 2026.

### Mecânicas básicas

O jogador explora ambientes 2D com visão de cima, interage com objetos e personagens, reúne pistas e usa o notebook para decodificar informações. O protótipo acrescenta combate direcional, combo, esquiva e o resgate de nove alunos que passam a oferecer poderes de apoio na igreja. A entidade ancestral continua sendo uma ameaça da qual Edelzio precisa fugir.

O ciclo disponível combina exploração, pistas, combate contra manifestações menores, resgate e viagem de Fusca. A campanha completa prevê ainda memórias interativas, uso do reagente de Ouzana e decisões que determinam os finais.

### Plataforma e distribuição

- **Plataforma-alvo:** PC, com teclado e mouse.
- **Distribuição pretendida no original:** itch.io e Steam como meta. Não há publicação confirmada nesta revisão.
- **Monetização:** o original lista canais de distribuição, mas não define preço ou modelo comercial; permanece a definir pelo grupo.
- **Referência técnica atual:** Unity 6000.6.0f1 e C#.

## 2. Escopo do projeto

O prazo proposto no GDD original é de **um mês**, sem datas de início e término preenchidas. A equipe é o 3º Sistemas; nomes, responsabilidades individuais e custos continuam a definir. Os valores exemplificativos do modelo não constituem orçamento aprovado.

Recursos citados no planejamento: Unity, ChatGPT Plus e possíveis assets da Asset Store. A necessidade de licenças, compras e hardware deve ser registrada pelo grupo conforme os recursos realmente utilizados.

| Recorte | Conteúdo | Situação |
| --- | --- | --- |
| Protótipo disponível | Menu, casa, escola/resgate, diocese, combate, aliados, interface e viagens. | Implementado; validação de QA pendente. |
| Campanha completa | Prólogo de 1996, mata, Ouzana, casarão, subsolo, selo e finais. | Planejado, além do trecho já representado no protótipo. |
| Sistemas de investigação completos | Inventário com capacidade, combinação de itens, análise avançada no notebook, memórias e reagente. | Parcial ou planejado, conforme a seção 6. |

## 3. Referências

- **Stranger Things:** mistério sobrenatural conectado ao passado e descoberta gradual de uma ameaça desconhecida.
- **Silent Hill:** atmosfera sombria, exploração e dúvida sobre a percepção do protagonista.
- **Outlast:** tensão, perseguição e necessidade de fugir da ameaça principal. Essa referência permanece na relação com a entidade ancestral, mesmo com o combate contra subordinados.
- **Resident Evil:** exploração, itens, enigmas e documentos que revelam a história.

## 4. Pitch e descrição do projeto

### Pitch de elevador

O Segredo de Varginha é um jogo 2D de terror e investigação no qual Edelzio, professor de Desenvolvimento de Sistemas, descobre que suas memórias de 1996 escondem a ligação com uma entidade presa sob a cidade. Entre pistas, enigmas e viagens de Fusca, ele enfrenta manifestações menores, resgata sua turma e usa os poderes dos alunos para alcançar os registros secretos da diocese. A campanha planejada amplia a investigação até a origem do selo de 1898 e uma decisão sobre o destino da entidade.

### Descrição breve

Edelzio encontra um antigo caderno de infância e passa a investigar fenômenos que afetam sua casa, seus equipamentos e seu Fusca. A exploração conecta documentos históricos, tecnologia e terror sobrenatural. O protótipo permite percorrer a casa, libertar nove alunos na escola e enfrentar os guardas do Livro do Tombo Secreto com ajuda da turma.

### Descrição detalhada

Edelzio tem 36 anos, trabalha como professor de Desenvolvimento de Sistemas e vive em Varginha. Um feixe de luz visto na infância deixou suas memórias fragmentadas. Trinta anos depois, interferências elétricas, anotações que ele não lembra de escrever e comportamentos impossíveis do Fusca o levam de volta àquela noite.

A investigação passa pela casa, escola, diocese e, na campanha prevista, por áreas rurais, pelo casarão de Zé Gomes e por um complexo subterrâneo. O notebook representa os conhecimentos tecnológicos de Edelzio. Padre Fábio guarda registros históricos; Ouzana investiga as alterações biológicas da região e fornece um reagente para revelar marcas ocultas.

A atualização de gameplay acrescenta uma turma resgatável e confrontos contra manifestações menores. Edelzio possui meios de combate e esquiva, enquanto os alunos oferecem ataques e efeitos próprios. A ameaça ancestral conserva sua invulnerabilidade ao ataque comum e o conflito final permanece ligado à investigação e ao selo. Memórias reconstruídas, decisões e finais alternativos continuam previstos para a campanha completa.

## 5. Diferenciais

1. Mistério regional brasileiro reinterpretado como terror sobrenatural, com personagens e locais ligados a Varginha.
2. Tecnologia como ferramenta de investigação: Edelzio relaciona documentos, códigos e eventos do passado.
3. Narrativa de memórias fragmentadas, com revelações sobre 1898, 1996 e a participação do protagonista no selo.
4. Nove alunos com identidade própria, resgatados na escola e convertidos em aliados na diocese.
5. Combate com preparação visível dos inimigos, combo, esquiva e poderes complementares da turma.
6. Cenários em pixel art e viagens cinematográficas de Fusca conectando as etapas da investigação.

## 6. Mecânicas de jogabilidade — proposta e estado atual

As doze mecânicas do original permanecem como referência da campanha. A implementação disponível adapta parte delas; as novas mecânicas estão listadas em seguida.

| Nº | Mecânica original | Proposta preservada | Estado no protótipo |
| --- | --- | --- | --- |
| 1 | Investigação e coleta de pistas | Encontrar documentos, fotografias, jornais e símbolos; relacionar informações para compreender 1898 e 1996. | **Parcial:** pistas interativas, caderno, documento histórico e quiz. Organização livre e cruzamento de evidências no caderno/notebook permanecem planejados. |
| 2 | Exploração | Interagir com ambientes, descobrir áreas e encontrar pistas opcionais. | **Parcial:** casa, escola e diocese disponíveis. Mata, casarão e percurso subterrâneo permanecem planejados. |
| 3 | Inventário | Começar com um espaço, ampliar com a mochila, escolher itens e combinar objetos. | **Parcial/adaptado:** hotbar de cinco posições, mochila com seleção de alunos e consumo dos itens após uso. Limite inicial de um espaço, expansão de capacidade e combinações ainda não integram o fluxo atual. |
| 4 | Puzzles | Resolver códigos, símbolos, coordenadas e mecanismos ligados à história. | **Parcial:** quiz de três perguntas no notebook da casa. O puzzle investigativo completo da escola, incluindo horário 23:17, permanece previsto. |
| 5 | Notebook e tecnologia | Analisar imagens, arquivos, mapas, anotações e criptografia. | **Parcial:** interface de quiz, dados decodificados e notebook equipado. As demais ferramentas de análise permanecem planejadas. |
| 6 | Memórias de 1996 | Observar ou controlar fragmentos do passado e compará-los com evidências. | **Planejado:** há referências narrativas a 1996, mas o prólogo e a reconstrução interativa de memórias não integram a rota atual. |
| 7 | Reagente de Ouzana | Revelar marcas, fluidos e símbolos ocultos, com possível quantidade limitada. | **Planejado:** encontro com Ouzana, aplicação do reagente e gestão de suas cargas. |
| 8 | Anomalias sobrenaturais | Interferir em luzes, eletrônicos, objetos e ambientes para gerar tensão e pistas. | **Parcial:** despertar da entidade e efeitos de manifestação. Os eventos e alterações de cenário de toda a campanha permanecem previstos. |
| 9 | Furtividade e fuga | Correr, esconder-se, apagar luzes e usar rotas para escapar da entidade. | **Parcial:** perseguição, corrida e fuga de Fusca. Sistema completo de esconderijos e furtividade permanece planejado. |
| 10 | Fusca | Transporte, direção em trechos específicos e falhas sobrenaturais no motor, rádio e faróis. | **Parcial/adaptado:** chegada, embarque, partida e viagem cinematográfica. Não há direção manual integrada a essas viagens; o surto completo do veículo continua previsto. |
| 11 | Sanidade/percepção | Exposição à entidade altera a percepção, provocando vozes, sombras e dúvidas sobre a realidade. | **Parcial:** sanidade numérica, drenagem, recuperação e colapso. A camada completa de distorções e pistas perceptivas permanece planejada. |
| 12 | Decisões e finais | Escolher em quem confiar e o que fazer com o selo; pistas e decisões abrem finais. | **Planejado:** três finais preservados na seção 7. A vitória na igreja encerra somente o protótipo. |

### Novas mecânicas incorporadas

- **13 — Combate direcional:** três golpes encadeados, impacto único por alvo, paredes bloqueando o dano e feedback de acerto. Detalhes na seção 9.
- **14 — Esquiva e leitura de ataques:** deslocamento curto com recarga, proteção contra ataques dos subordinados e avisos de trajetória/área. Seções 9 e 10.
- **15 — Resgate e turma aliada:** nove reféns, libertação coletiva, embarque em fila e poderes comandados na igreja. Seções 8 e 11.
- **16 — Dificuldade:** fácil, médio e difícil alteram os atributos e o tempo de preparação dos ETs. Seção 10.
- **17 — Controles configuráveis e HUD:** remapeamento persistente, hotbar, indicadores de combo/esquiva e painel dos aliados. Seção 12.
- **18 — Apresentação dos ambientes e viagens:** cenografia procedural, acabamento em pixels e transições entre fases com carregamento. Seção 13.

## 7. História e jogabilidade da campanha

### História breve e ideia central

Em 1996, Edelzio criança vê um feixe de luz sobre sua casa durante os acontecimentos que dão origem ao mistério local. Suas lembranças se fragmentam. Aos 36 anos, ele encontra o antigo caderno de pesquisas e percebe que o passado voltou a interferir em sua rotina.

Documentos de 1898 revelam que Zé Gomes abriu uma ruptura subterrânea. Uma linhagem de guardiões tentou contê-la, mas o selo precisa de uma pessoa viva. Com ajuda de Padre Fábio e Ouzana, Edelzio descobre que faz parte desse mecanismo. O caso de 1996 é um ciclo de uma história mais antiga, e as revelações finais questionam tanto as intenções humanas quanto a natureza da entidade.

### História detalhada — seis atos

#### Ato I — A Lembrança

**Planejado — prólogo de 1996.** Edelzio criança explora sua casa em uma noite comum, com a televisão noticiando a criatura avistada em Varginha. Interage com objetos, jornais e um computador antigo. A energia oscila, a TV perde o sinal e um brilho o atrai ao quintal. O controle dos movimentos diminui, um feixe de luz revela uma figura indistinta e um corte brusco mostra Edelzio acordando na cama.

#### Ato II — O Chamado

**Parcial — casa e viagem representadas no protótipo.** Trinta anos depois, o professor segue uma rotina de banho, comida, mochila, notebook e preparação para sair. A chave desaparece; arranhões sob a cama o levam à caixa de brinquedos. Dentro estão a chave e o caderno de 1996, com a anotação infantil “Eu sei que vi alguma coisa.” e outra, recente, que ele não lembra de escrever: “ELA AINDA ESTÁ AQUI.”

No roteiro original, o caminho para a escola inclui o surto do Fusca: rádio, faróis e painel falham, a voz infantil diz “Não deixa ela sair.”, o motor para e volta a funcionar sozinho. A versão jogável já inclui caixa/caderno, manifestação, coleta, quiz e saída cinematográfica. A encenação completa da rotina, do desaparecimento da chave e das falhas do carro permanece a integrar.

#### Ato III — O Guardião

**Parcial — escola e diocese jogáveis, com novas features.** A ida à escola passa a incluir o resgate de nove alunos presos por quatro ETs. Após libertá-los e reuni-los no Fusca, Edelzio segue com a turma para a diocese.

O roteiro preserva a análise de uma fotografia com números, letras e símbolos, cuja decodificação aponta o horário **23:17** e uma área subterrânea da igreja. No protótipo, essa análise está representada pelo quiz simplificado na casa e pelas falas sobre coordenadas; o puzzle completo na escola ainda não está integrado.

Na diocese, cinco ETs bloqueiam o acesso aos registros. Edelzio enfrenta os inimigos com seu combo e os poderes da turma. Padre Fábio revela que seu nome consta no Livro do Tombo Secreto. O documento liga Zé Gomes, a ruptura de 1898 e Edelzio ao selo. A revelação completa de que o selo é uma pessoa viva permanece como direção narrativa do ato; a versão jogável termina com a leitura do livro e o gancho para a mata.

#### Ato IV — Ouzana

**Planejado.** Padre Fábio explica que a criatura vista em 1996 era uma manifestação incompleta tentando atravessar a fenda. O feixe de luz fazia parte da transferência que transformou Edelzio em âncora viva. A investigação segue para o casarão em ruínas de Zé Gomes, coberto pela floresta.

Na mata, árvores marcadas, animais mortos sem ferimentos, padrões geométricos na vegetação e uma substância brilhante ampliam o terror ambiental. Edelzio encontra Ouzana, bióloga que pesquisa as anomalias. Seu reagente revela símbolos na vegetação e marcas no Fusca: cicatrizes do tecido da realidade. O recurso permitirá revisitar locais em busca de novas pistas.

#### Ato V — O Subsolo

**Planejado.** As pistas do reagente revelam a entrada do complexo subterrâneo de Zé Gomes. Ferramentas, registros e objetos religiosos dão lugar a cavernas orgânicas que parecem vivas. Sons abafados e a voz de Edelzio criança acompanham a descida.

Sombras, reflexos e distorções conduzem a uma câmara com uma ferida na realidade. Edelzio compreende que a criatura de 1996 tentava voltar para casa. A descoberta muda a interpretação da ameaça sem resolver ainda o conflito do selo.

#### Ato VI — O Selo

**Planejado.** Padre Fábio e Ouzana chegam ao subsolo. As falhas elétricas são relacionadas à tentativa de romper o selo. A entidade projeta a memória completa de 1996: ela propôs um acordo, e Edelzio criança escolheu participar. O trauma apagou essa decisão durante trinta anos. A investigação culmina na escolha sobre o selo e suas consequências.

### Clímax e finais previstos

1. **Sacrifício:** Edelzio permanece no subsolo como selo definitivo; a entidade é contida, Padre Fábio e Ouzana escapam e ele desaparece. Anos depois, outra criança encontra seu caderno: “Se você está lendo isso, significa que ela ainda está dormindo.”
2. **Libertação — final ruim:** Edelzio rompe o selo para se libertar. A entidade atravessa a fenda, Varginha sofre um colapso sobrenatural, as luzes se apagam e o Fusca é encontrado abandonado. O rádio transmite: “Não estamos sozinhos.”
3. **O Verdadeiro Segredo — final verdadeiro:** exige coletar **todas as pistas opcionais**, conforme a condição mais específica do original. Edelzio descobre que Padre Fábio omitiu parte da verdade, Zé Gomes não agiu sozinho e a entidade foi aprisionada por interesses humanos. Ele encontra uma resolução alternativa, ainda a detalhar pelo grupo, com possibilidade de continuação.

Os três finais e o sistema de decisões são planejados. A tela de vitória da Fase 3 não corresponde a nenhum desses desfechos.

### Gameplay breve

Explorar, reunir pistas e resolver enigmas abre caminho pela investigação. No trecho atual, o jogador sobrevive à entidade, enfrenta subordinados, resgata a turma e usa seus poderes na igreja. A continuação planejada acrescenta reagente, revisitação de locais, memórias interativas e decisões finais.

### Mapeamento entre atos e fases

| Campanha do GDD | Recorte jogável atual |
| --- | --- |
| Ato I — infância em 1996 | Prólogo planejado. |
| Ato II — rotina, caderno e chamado | Fase 1: casa, investigação e fuga. |
| Ato III — investigação e guardião | Fase 2: escola/resgate; Fase 3: diocese/livro. |
| Atos IV, V e VI | Continuação planejada após a vitória da igreja. |

Os números das fases não substituem a numeração dos atos. A antiga mensagem “ATO I: O CHAMADO” da saída da casa é uma divergência de texto em relação ao GDD, no qual O Chamado é o Ato II.

## 8. Gameplay detalhado — fases disponíveis

### Fase 1 — Casa de Edelzio

- Explorar a casa e o quintal, recolher a mochila e examinar objetos e documentos.
- Abrir a caixa debaixo da cama entrega a chave do Fusca e o caderno de 1996, além de despertar a entidade ancestral.
- Usar o notebook abre um quiz de três perguntas. Respostas incorretas mantêm a pergunta; completar o quiz libera os dados decodificados e equipa o notebook.
- O documento de 1898 apresenta a pista de Zé Gomes. Café recupera um terço da sanidade máxima, equivalente a um dos três corações do HUD.
- A entidade drena sanidade por proximidade e permanece invulnerável a ataques comuns.
- Interagir com o Fusca inicia a entrada no carro, a partida e a viagem para a escola.

**Condição efetiva de saída:** possuir a chave e o caderno de pesquisas. O fluxo narrativo incentiva concluir o notebook, mas `FuscaLevelExit` atualmente não exige dados decodificados, mochila ou documento histórico.

Cena: `FaseTopView_Varginha`.

### Fase 2 — Escola e Resgate

- Chegada cinematográfica de Fusca. Edelzio começa equipado com os cinco itens de investigação.
- Quatro ETs guardam nove alunos: Yasmin, Pedro, Matias, Fabio, Marcos, Anna Sabia, Ana Tavares, Luis Miguel Messias e Luis Martins.
- Os alunos começam em jaulas com barras verdes pulsantes e nomes visíveis.
- Derrotar todos os ETs inicia o resgate. As jaulas desaparecem e a turma acompanha Edelzio em formação espaçada até o Fusca.
- A partida aguarda a chegada de todos os alunos e a aproximação de Edelzio ao carro, dentro de 3,4 unidades do mundo.
- Os passageiros e os equipamentos ficam ocultos dentro do veículo. A viagem seguinte leva automaticamente à diocese.

**Condição de conclusão:** eliminar os quatro ETs, reunir os nove alunos no Fusca e aproximar Edelzio. Os alunos atuam como reféns/acompanhantes nesta cena; seus comandos de combate estão integrados à Fase 3.

Cena: `Fase2_Escola_Resgate`.

### Fase 3 — O Guardião / Ato III

- Chegada à área secreta da diocese, com cinco ETs, altar, símbolo do selo, Padre Fábio e Livro do Tombo Secreto.
- Edelzio usa seu próprio combo e comanda os nove aliados, disponíveis em qualquer dificuldade nesta fase.
- Enquanto houver ETs vivos, as interações com o padre e o livro informam que a passagem está bloqueada.
- Após o combate, Padre Fábio explica a relação de Edelzio com o selo. Examinar o livro conclui a fase e revela referências a Zé Gomes, 1898, à mata e a Ouzana.
- A conclusão desativa os aliados, bloqueia novas ações e apresenta a tela de vitória com a opção de jogar a cena novamente.

**Condição efetiva de conclusão:** eliminar os cinco ETs e interagir com o livro. Conversar com o padre faz parte da sequência narrativa, mas não existe uma verificação que obrigue essa conversa antes da leitura.

Cena: `Fase3_Igreja_Guardiao`. É a última fase jogável da sequência atual; mata e Ouzana aparecem como gancho narrativo.

## 9. Edelzio: movimento, combate e sobrevivência

### Movimento e esquiva

O movimento aceita oito direções, com caminhada e corrida. Edelzio pode se deslocar durante os ataques. A esquiva usa a direção de movimento ou a direção em que ele está olhando quando parado, dura **0,18 s** e tem recarga de **1,1 s**, contada desde sua ativação.

Durante a esquiva, novos ataques ficam bloqueados e os ataques dos subordinados ETs não aplicam dano. Essa proteção é verificada pelo combate dos ETs; a drenagem por proximidade da entidade ancestral é um sistema separado. Pausa, diálogo, vitória, ausência de sanidade, morte ou bloqueio de entrada impedem iniciar ataques/esquivas.

### Combo direcional de três golpes

O ataque usa quatro direções visuais: cima, baixo, esquerda e direita. Com mouse, a mira determina a direção cardinal; com teclado, utiliza a direção de Edelzio. Segurar o comando continua o combo. Um comando próximo ao fim do golpe pode ficar em espera por **0,18 s**.

| Etapa | Golpe | Dano base atual | Característica |
| --- | --- | --- | --- |
| 1 | Corte inicial | 34 | Contato frontal. |
| 2 | Golpe cruzado | 37,4 | Dano multiplicado por 1,10 e sequência visual própria. |
| 3 | Finalizador pesado | 49,3 | Dano multiplicado por 1,45, área maior e interrupção do inimigo. |

Valores derivados do dano padrão de 34 do componente; ajustes no Inspector podem alterar essa base. Cada golpe tem preparação, impacto e recuperação. Edelzio pode atacar novamente após terminar a animação, sem recarga adicional. A sequência volta ao primeiro golpe após o terceiro ou após **0,75 s** sem continuação, contados do término do golpe.

O dano ocorre no momento do impacto, uma vez por alvo em cada golpe, mesmo que ele tenha vários colisores. Paredes bloqueiam o contato. Clarões, recuo do inimigo, breve desaceleração do jogo no impacto e tremor de câmera reforçam a leitura; o finalizador intensifica esses efeitos. Uma pausa iniciada durante o impacto deve permanecer ativa.

### Vida, sanidade e inventário

- Vida de combate e sanidade são estados separados. Um ataque de ET que efetivamente tira vida também drena sanidade, conforme a dificuldade.
- Sanidade chega até 100 por padrão. O painel chamado **SAÚDE** mostra três corações e um percentual baseados na sanidade. Chegar a zero provoca o colapso sobrenatural.
- A hotbar mantém cinco posições. Clicar na mochila ou apertar **G** abre o inventário na aba **Itens físicos**. A aba **Alunos** reúne os nove especiais e suas descrições. É necessário ter coletado a mochila; os números 1–5 apenas selecionam slots.
- **Equipar aluno** define quem executará o próximo especial. Nenhum começa equipado; somente o escolhido aparece junto de Edelzio e no painel de aliado. Setas navegam, Tab troca de aba, Enter equipa e Esc/G fecha. Alunos indisponíveis na fase aparecem bloqueados na aba Alunos; os especiais continuam sendo liberados na igreja.
- Mochila e notebook são **cinza**. A mochila tem aba, duas fivelas, bolsos laterais e versão de perfil; o notebook tem tela escura, teclado prateado e uma versão fechada sob o braço. Os itens usam escala e ordem de desenho próprias para não cobrir o rosto ou os pés. O café acompanha o gesto da mão até a boca.
- O inventário pausa o mundo e as recargas. Abrir, inspecionar ou trocar de aluno não usa poderes nem reinicia recargas. O HUD identifica o aluno equipado com `>`.
- Itens físicos saem da hotbar após uso bem-sucedido: chave e caderno ao confirmar a saída no Fusca; notebook após concluir a decodificação; documento após sua leitura. A mochila permanece como equipamento. Tentativas incompletas não consomem os requisitos.
- Pistas e estados de investigação permanecem registrados mesmo sem o item físico. Escola e igreja não repõem os itens já utilizados, inclusive ao iniciar essas fases diretamente. Isso não constitui um sistema geral de salvamento entre sessões.

## 10. ETs e dificuldade

Os subordinados e a manifestação ancestral usam a identidade visual de **ET marrom com olhos vermelhos**. Os subordinados apresentam três comportamentos; a cor dos avisos e efeitos ajuda a identificar o golpe.

| Tipo | Comportamento | Aviso visual | Recuperação após atacar |
| --- | --- | --- | --- |
| Atirador | Dispara um projétil direcional, bloqueado por obstáculos. | Ciano, indicando trajetória. | 0,42 s |
| Investidor | Avança em linha reta até o destino sinalizado ou uma obstrução. | Laranja, indicando o avanço. | 0,72 s |
| Sentinela | Golpe em área ao redor do próprio corpo, com raio de 1,65 unidade. | Roxo, indicando a área. | 0,95 s |

Os inimigos perseguem, preparam o golpe, atacam e se recuperam. Os avisos mostram onde o ataque ocorrerá antes do impacto. Golpes de Edelzio e dos aliados podem interromper ataques e aplicar recuo/atordoamento; paredes também limitam os efeitos ofensivos. A entidade ancestral conserva seu papel de ameaça narrativa invulnerável ao ataque comum.

A dificuldade é escolhida no menu antes da investigação e mantida entre cenas e tentativas da mesma sessão. O padrão de uma nova sessão é **MÉDIO**.

| Parâmetro | Fácil | Médio | Difícil |
| --- | --- | --- | --- |
| Multiplicador de vida dos ETs | 1,00 | 1,55 | 2,20 |
| Multiplicador de velocidade | 0,85 | 1,10 | 1,40 |
| Multiplicador de dano | 0,65 | 1,00 | 1,35 |
| Multiplicador do intervalo entre ataques | 1,60 | 1,15 | 0,80 |
| Preparação base do aviso | 0,85 s | 0,60 s | 0,38 s |
| Multiplicador da duração de controle recebido | 1,00 | 0,70 | 0,45 |

O papel do ET aplica ajustes adicionais: atirador soma 0,06 s à preparação, investidor soma 0,18 s e sentinela soma 0,30 s. Vida, velocidade e dano também podem receber fatores do papel. No difícil, os inimigos antecipam um pouco o movimento do jogador ao mirar. Os valores são referência de balanceamento do código atual.

## 11. Turma aliada e poderes

### Regras de comando na Fase 3

- O comando de aliado é independente do ataque de Edelzio. Cada comando aceito inicia a habilidade do aluno equipado na mochila; essa habilidade pode atingir outros ETs conforme seu perfil.
- Cada aluno tem **5 s** de recarga individual, iniciada ao ativar o poder. A turma tem também um intervalo de **0,9 s** entre comandos aceitos.
- Com mouse, um ET próximo à mira recebe prioridade. Sem alvo na mira, o sistema escolhe um ET alcançável usando proximidade e adequação do poder equipado. O aluno nunca muda automaticamente: se estiver em recarga, o comando é recusado sem gastar outra habilidade. Trocar pela mochila mantém tanto a recarga individual quanto o intervalo global.
- O alvo precisa estar vivo, ao alcance de **7 unidades de Edelzio** e sem parede bloqueando o caminho. A entidade ancestral não é um alvo válido.
- Sem alvo válido, o comando não inicia a recarga. Um golpe já iniciado pode errar se o alvo sair da região prevista antes do impacto.
- Dano em área, perfuração e ricochetes também respeitam as paredes entre os alvos.
- Os aliados circulam ao redor de Edelzio, com separação entre colegas e desvio simples de paredes. O modo manual usa apresentação compacta com cabeça e nome; prontidão, poder e recarga ficam no painel lateral.
- Pausa e bloqueios de interação suspendem a execução dos poderes e a contagem da recarga individual.

### Perfis atuais

| Aluno | Habilidade | Dano principal base | Efeito característico |
| --- | --- | --- | --- |
| Matias | Jiujitsu | 30 | Arremesso com controle e possibilidade de atingir mais um ET próximo. |
| Anna Sabia | Ping-pong / topspin | 23 | Até dois ricochetes para novos alvos, sem repetir dano no mesmo ET. |
| Ana Tavares | Ping-pong / smash | 28 | Um ricochete adicional, com maior dano que o topspin. |
| Pedro | Guitarra | 31 | Acorde em área; recupera 6 de sanidade ao acertar o alvo principal. |
| Luis Martins | Arte / tinta | 24 | Impacto em área que imobiliza os inimigos atingidos. |
| Luis Miguel Messias | Microfone | 26 | Onda sônica em área; recupera 12 de sanidade ao acertar o alvo principal. |
| Yasmin | Piano | 45 | Piano cai sobre o alvo, com dano e controle em área. |
| Fabio | Katana | 38 | Perfura alvos atrás da mira em uma faixa estreita; soma 14 ao dano principal contra alvo com até 35% da vida. |
| Marcos | Vôlei / chute voador / cotovelada | 34 / 30 / 38 | Alterna os três golpes, com impactos secundários e bordões no acerto. |

Os valores da tabela são do impacto principal; alvos secundários recebem os multiplicadores de cada habilidade. A duração do controle é reduzida pela dificuldade. Marcos mantém os bordões presentes na implementação: “to doido com vc então uai!”, “o Exu!!” e “o cu!!!”.

**Suporte para outras fases:** existe uma regra reutilizável que permite três alunos em fases comuns a partir da Fase 2 somente no difícil e nove em fases finais. O controlador atual da escola não ativa essa regra; a integração jogável de invocação está na Fase 3. Também existe suporte a ataque automático dos aliados para futuras integrações.

## 12. Controles e interface

| Ação | Comando padrão |
| --- | --- |
| Mover | WASD, com setas como alternativa. |
| Correr | Shift. |
| Interagir / examinar | E, Espaço ou Enter. |
| Atacar / continuar combo | Mouse esquerdo ou J; K continua aceito por compatibilidade. |
| Comandar aluno | Mouse direito ou L, na Fase 3. |
| Esquivar | Ctrl. |
| Abrir/fechar mochila | Clique na mochila ou G; Esc também fecha. Tab alterna Itens físicos/Alunos. |
| Navegar/equipar aluno | Setas e Enter ou mouse e botão Equipar aluno. |
| Selecionar demais posições da hotbar | 2 a 5. |

Em **TUTORIAL / CONTROLES → EDITAR CONTROLES DO TECLADO E MOUSE**, o jogador remapeia as quatro direções, corrida, interação, ataque, aliado e esquiva. Cada ação aceita uma tecla ou um dos três botões principais do mouse, possui reset individual e salva a escolha entre cenas e novas execuções. Escolher uma tecla substitui o botão de mouse configurado naquela ação, e vice-versa. Alguns atalhos de compatibilidade, como J/K para ataque, continuam ativos após o remapeamento.

O HUD apresenta os itens, dicas de Rodrigo, diálogos, instruções de interação, estágio do combo e estado da esquiva. Na Fase 3, apresenta também os nove aliados, seus poderes e recargas. O painel de combate fica acima da hotbar e ajusta dimensões e fonte à janela. Os painéis de combate usam os comandos remapeados; algumas falas de tutorial ainda citam os controles padrão.

## 13. Direção de arte e transições

- **Casa:** madeira de menor contraste, iluminação quente, luar nas janelas e quintal noturno com poste, folhas, poças e vaga-lumes discretos.
- **Escola:** parquet, paredes claras com faixa verde, salas, carteiras com material escolar, lousas, murais, estantes, armários, plantas e luz de janelas.
- **Diocese:** pedra escura, bancos, altar, vitrais, tapete, velas, arandelas, pilastras, mosaico e sacristia. Dez projeções translúcidas dos vitrais aparecem no piso, abaixo de móveis e personagens.
- **Personagens:** Edelzio usa o atlas V3 e poses de caminhada, interação e combate. Alunos, ETs e poderes têm sprites e efeitos próprios.
- **Nitidez:** a nova decoração usa sprites em tamanho nativo de 32 pixels por unidade, filtro Point, sem mipmaps e com reutilização em cache. Câmeras ortográficas compatíveis desabilitam HDR, MSAA e resolução dinâmica, preservando o enquadramento. Isso não unifica todos os sprites do projeto em uma única grade de pixels.
- **Montagem dos ambientes:** escola e diocese são construídas em runtime. O acabamento também atende cenas existentes, evita duplicatas e recupera sprites ausentes. Os objetos de acabamento não acrescentam colisores; paredes e obstáculos continuam pertencendo ao ambiente base.
- **Luz nas três fases:** projeções e halos com transparência contínua e filtragem suave, compostos em uma camada única no piso. Cada feixe parte da borda interna e acompanha a largura da abertura da respectiva janela. As interseções têm intensidade limitada; móveis e paredes bloqueiam a projeção com penumbra e sombras de contato. A suavização não desfoca os sprites do cenário. A composição é estática, calculada ao preparar o ambiente; não é iluminação dinâmica de personagens.
- **Fusca:** vaga de referência compartilhada e dimensionada para a carroceria inteira; porta arredondada, janela na parte superior e quebra-vento; folha única com perspectiva contínua em torno da dobradiça dianteira. Abertura de 0,72 s e fechamento de 0,58 s, sem troca brusca entre três desenhos. Partida horizontal estável e curta antes da viagem.
- **Viagem entre fases:** estrada noturna, Fusca animado, tomada interna com a turma quando resgatada, transição em pixels e indicador de progresso. A composição usa 384 × 216 pixels e carrega a próxima cena de forma assíncrona. A tomada dura pelo menos 6,4 s antes da ativação da cena, além do encerramento da transição; pode durar mais se o carregamento exigir.

## 14. Assets necessários e personagens

### Personagens e funções preservadas

- **Edelzio — protagonista:** professor de Desenvolvimento de Sistemas, 36 anos; programação, análise de pistas e uso de ferramentas tecnológicas. A atualização acrescenta combo e esquiva contra manifestações menores. Continua sem poderes sobrenaturais e vulnerável à entidade e à perda de sanidade.
- **Padre Fábio — guardião:** protege registros, conhece símbolos e rituais, pertence à linhagem ligada ao selo e depende da investigação de Edelzio. Seu encontro está representado na Fase 3. Informações incompletas e segredos fazem parte da narrativa.
- **Ouzana — bióloga:** pesquisa fauna, flora e substâncias anômalas, desenvolve o reagente e depende de equipamentos e materiais. Não tem papel de combate previsto. Seu encontro jogável permanece planejado.
- **Rodrigo — guia da interface:** apresenta objetivos, instruções e dicas; não possui corpo no mundo nem participação física em confrontos. Está presente no HUD.
- **Turma resgatada — nova adição:** os nove alunos da seção 11 precisam de sprites, nomes, jaulas, seguimento, embarque e apresentação dos poderes, já representados no protótipo.
- **NPCs previstos no original:** Matheus, Luisa, Renan, Diogo, Lipinho, Daiana e Alex. Permanecem no planejamento, com funções e integração a detalhar; são uma lista distinta dos nove aliados já implementados.

### Ameaças e antagonista histórico

- **Entidade ancestral:** ameaça central vinculada ao selo, com interferência elétrica, percepção e memórias como poderes narrativos. Perseguição e drenagem estão presentes na casa; rituais, reagente e demais efeitos da campanha permanecem previstos.
- **Manifestações menores:** no protótipo, os subordinados ETs podem ser derrotados e usam os três papéis de combate. Afastá-los por símbolos de proteção e outros comportamentos do original continuam previstos.
- **Criaturas alteradas:** fauna afetada pela ruptura, agressiva e sensível a sons/movimentos, com possibilidades de distração e uso de substâncias. Conteúdo planejado para a mata e regiões afetadas.
- **Zé Gomes:** antagonista histórico humano relacionado aos experimentos de 1898, apresentado por documentos e relatos. Sua ambição e seus registros ajudam a explicar a ruptura; não é a entidade ancestral.

### Inventário de produção

| Grupo | Disponível no protótipo | Necessidades da campanha |
| --- | --- | --- |
| Cenários 2D | Casa/quintal, escola, diocese e estrada da viagem. | Mata, área rural, casarão, túneis, câmara e fenda; áreas do prólogo. |
| Objetos | Fusca, mochila, chave, caderno, notebook, documentos, jaulas, altar e livro. | Reagente, pistas ocultas, ferramentas e objetos dos puzzles futuros. |
| Animações | Movimento, interações, ataque de Edelzio, poderes dos alunos, ETs, porta/partida e viagem. | Infância, memórias, reagente, criaturas rurais e desfechos do selo. |
| Interface | Hotbar, quiz, dicas, diálogos, sanidade, combate, aliados e remapeamento. | Organização completa das pistas, capacidade/combinação de itens e decisões. |
| Código | Controladores das três fases, combate, dificuldade, resgate, aliados, arte procedural e testes. | Progressão dos demais atos, sistemas ainda planejados e gatilhos dos três finais. |
| Sons | O original não fornece um inventário final de arquivos; áudio não foi auditado nesta revisão. | Catalogar ambientes internos/externos, passos, golpes, dano, motor, rádio, vozes, anomalias e efeitos narrativos. |
| 3D | Nenhum requisito concreto definido para este jogo 2D. | A seção 3D do modelo original continha exemplos sem assets especificados. |

## 15. Cronograma e marcos

O horizonte original de um mês permanece como referência do planejamento. Datas futuras, responsáveis e esforço precisam ser definidos pelo grupo; as datas abaixo de entregas concluídas são registros de commits.

| Marco | Data | Objetivo / resultado | Estado |
| --- | --- | --- | --- |
| Base de gameplay | 12/09/2026 | Combate direcional, escola/resgate, igreja, turma, hotbar, arte de Edelzio e viagem. | Registrado em `ca185c2`. |
| Features e polimento | 14/09/2026 | Combo, esquiva, controles, papéis dos ETs, poderes táticos, cenários, Fusca e HUD. | Registrado em `c331760`. |
| Atualização do GDD | 15/09/2026 | Integrar o GDD original às regras implementadas e identificar conteúdo futuro. | Revisão documental 1.1.0. |
| Validação do protótipo | A definir | Executar testes e roteiro de QA, registrar falhas e avaliar balanceamento. | Próximo marco de validação. |
| Continuação da campanha | A definir | Planejar prólogo, Ouzana/reagente, casarão, subsolo, memórias e finais. | Planejado; ordem de produção a definir. |

## 16. Limites atuais e decisões de continuidade

1. **Campanha:** a mata e Ouzana aparecem no encerramento da Fase 3. Não há continuação jogável dessa rota registrada no Build Settings.
2. **Requisitos narrativos:** tornar o quiz obrigatório para sair da casa ou exigir conversa prévia com Padre Fábio depende de uma decisão de design e de implementação adicional. Os requisitos atuais estão descritos na seção 8.
3. **Aliados fora da igreja:** a regra de três alunos no difícil e o ataque automático existem como suporte de código, ainda sem ativação no fluxo atual da escola.
4. **Persistência:** controles são salvos; dificuldade é mantida na sessão. A inicialização dos itens nas fases seguintes não comprova salvamento de progresso da campanha.
5. **Clareza da interface:** o título SAÚDE representa sanidade; falas com comandos fixos podem divergir de controles remapeados. Esses pontos ficam registrados para revisão de apresentação.
6. **Título e numeração:** o GDD original usa “O Segredo de Varginha”; o projeto também usa “O Mistério de Varginha”. A campanha tem seis atos e o protótipo tem três fases. Padronizar os títulos e corrigir a numeração exibida pela saída da casa permanece uma tarefa de apresentação.
7. **Continuidade do roteiro:** o original descreve tanto reconhecimento imediato quanto desconfiança inicial de Padre Fábio. Mantém-se como fato central que ele guarda o livro e revela a ligação de Edelzio ao selo; a encenação dessa confiança permanece a fechar. O original também traz duas formulações para o final verdadeiro; esta revisão preserva a exigência explícita de todas as pistas opcionais.

## 17. Validação e referências

Esta atualização usa o GDD 1.0.0 fornecido pelo grupo no anexo `pasted-text.txt`, além da leitura do histórico e do código. Os testes do Unity não foram executados nesta revisão documental. A existência de testes não significa aprovação da build.

Prioridades para QA: percorrer casa → escola → diocese; confirmar os quatro ETs/nove resgates e os cinco ETs da igreja; verificar combo/esquiva, ataques bloqueados por paredes, comandos separados, recargas individuais e intervalo da turma; testar controles salvos, poderes secundários, diálogos, reinício e cenários sem duplicação.

- [Diário e roteiro de QA](QA_DIARIO_ULTIMOS_3_DIAS.md).
- [Arquitetura](ARCHITECTURE.md), [histórico de alterações](CHANGELOG.md) e [apresentação de Edelzio V3](EdelzioV3.md).
- Fases: [saída da casa](../Assets/Scripts/Game/Varginha/FuscaLevelExit.cs), [escola](../Assets/Scripts/Game/Varginha/VarginhaPhase2Controller.cs), [diocese](../Assets/Scripts/Game/Varginha/VarginhaPhase3Controller.cs) e [viagem](../Assets/Scripts/Game/Varginha/VarginhaTravelCinematic.cs).
- Regras: [jogador](../Assets/Scripts/Game/Varginha/EdelzioTopDownController.cs), [combo](../Assets/Scripts/Game/Varginha/VarginhaPlayerAttack.cs), [ETs](../Assets/Scripts/Game/Varginha/VarginhaCombatEnemy.cs), [dificuldade](../Assets/Scripts/Game/Varginha/VarginhaDifficulty.cs), [turma](../Assets/Scripts/Game/Varginha/VarginhaStudentAllySquad.cs) e [poderes](../Assets/Scripts/Game/Varginha/VarginhaStudentAlly.cs).
- Interface: [controles](../Assets/Scripts/Game/Varginha/VarginhaInputBindings.cs) e [HUD](../Assets/Scripts/Game/Varginha/VarginhaGameHUD.cs).
- Testes existentes: [Play Mode](../Assets/Tests/PlayMode) e [cenografia em Edit Mode](../Assets/Tests/EditMode/VarginhaSceneryTests.cs).

## 18. Registro de atualização do GDD

- **12/09:** base de combate direcional, cenas da escola e diocese, resgate, turma com poderes próprios, hotbar, apresentação de Edelzio e viagem entre fases.
- **14/09:** combo de três etapas, esquiva, comandos remapeáveis, separação entre ataque e aliado, comportamentos dos ETs, efeitos táticos dos poderes, circulação da turma, cenografia e polimento visual/Fusca/HUD.
- **15/09:** atualização documental 1.1.0 a partir do GDD original 1.0.0, preservando os seis atos, personagens e três finais. Incorporadas as features recentes, o mapa de implementação e referências de uso/arquitetura. As datas anteriores correspondem aos commits que registram as entregas.
