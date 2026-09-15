# Diário de QA — 15/09/2026

## Entrega

- **Mochila:** clique no primeiro slot da hotbar ou pressione **G**. Abre na aba **Itens físicos**. A aba **Alunos** tem nove colegas, descrição do poder e botão **Equipar aluno**. Setas navegam; Tab alterna abas; Enter equipa; Esc/G/Voltar fecha.
- **Visuais:** mochila e notebook cinza, versões de perfil/fechada para carregar e encaixe no torso; xícara com movimento suave até a boca. Nenhum aluno aparece como acompanhante antes de equipar; ao trocar, o anterior some.
- **Especiais:** somente o aluno equipado ataca. Recarga individual de 5 s e intervalo global de 0,9 s preservados. A mochila pausa o mundo, inclusive recargas. Na casa/escola, poderes ainda indisponíveis aparecem bloqueados.
- **Consumíveis:** chave e caderno desaparecem após confirmar a saída do Fusca; notebook após decodificar; documento após ler. A mochila permanece. O conhecimento obtido não é perdido. Itens usados não são repostos na escola/igreja.
- **Iluminação:** luz e sombra com transparência contínua nas três fases. Reflexos compostos em uma camada do piso para suavizar cruzamentos, com bloqueio por móveis/paredes e penumbra. Os sprites do jogo permanecem nítidos.
- **Fusca:** porta curva, vidro superior e quebra-vento; abertura/fechamento contínuos pela dobradiça dianteira e deslocamento do personagem alinhado ao veículo espelhado ou normal.

## Roteiro de aprovação

| Caso | Ação | Resultado esperado |
|---|---|---|
| Mochila antes da coleta | Clicar no slot 1 na casa | Não abrir sem possuir a mochila. |
| Abertura | Coletar mochila, encerrar diálogo, clicar no slot ou apertar G | Abrir em Itens físicos, sem golpe involuntário; pausar o mundo. |
| Abas | Alternar Itens físicos/Alunos por clique e Tab; fechar/reabrir | Conteúdo separado; reabrir sempre nos itens, mantendo o aluno equipado. |
| Seleção | Na igreja, abrir aba Alunos, inspecionar e clicar Equipar | Nenhum acompanhante antes da seleção; somente o equipado aparece e recebe o comando especial. |
| Recarga | Usar especial e abrir mochila | Não reduzir recarga enquanto a tela estiver aberta. Trocar e voltar não reinicia a recarga. |
| Aluno indisponível | Examinar aluno bloqueado em fase anterior | Mostrar explicação; não permitir equipar. |
| Fechamento | Esc, G e Voltar; repetir abrir/fechar | Retomar tempo normal, sem mover/atacar por causa do clique de interface. |
| Requisito incompleto | Tentar sair com chave, sem caderno | Manter chave; permitir tentar novamente após pegar o caderno. |
| Uso confirmado | Sair com os requisitos, concluir quiz e ler documento | Remover os respectivos ícones; preservar andamento da história. |
| Fases seguintes | Chegar/iniciar escola e igreja | Não reaparecerem chave, caderno e itens já utilizados. |
| Luz e sombra | Conferir janelas pequenas/grandes, cruzamentos e bancos nas três fases | Feixes alinhados à abertura de cada janela; bordas difusas, interseções suaves, móveis não tingidos pela projeção do piso; penumbra atrás deles. |
| Itens carregados | Andar nas quatro direções, usar notebook e beber café | Mochila e notebook cinza e proporcionais, sem cobrir rosto/pés; xícara acompanha a mão. |
| Porta do Fusca | Completar saída da casa | Sem salto entre quadros ou órbita da folha; personagem entra pela porta e carro sai com ela fechada. |
| Resoluções | 1920×1080, 1280×720 e 800×600 | Todos os alunos, descrição, Equipar e Voltar visíveis e utilizáveis. |

## Limites

A iluminação é estática e preparada ao entrar na fase; não adiciona sombras dinâmicas aos personagens. Capacidade expansível, combinação de itens e salvamento geral entre sessões continuam fora desta entrega. Não foi criado um puzzle de porta novo: o consumo foi integrado aos usos existentes.

## Registro do grupo

- Responsável:
- Build/commit:
- Fase e resolução:
- Caso testado:
- Resultado: aprovado / reprovado
- Passos para reproduzir e evidência:

## Verificação técnica

Compilação e resultados automatizados registrados ao concluir a implementação. A passagem dos testes não substitui a aprovação visual do grupo.
