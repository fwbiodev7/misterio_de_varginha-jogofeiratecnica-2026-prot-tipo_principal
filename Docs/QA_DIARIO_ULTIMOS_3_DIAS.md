# Diário de QA — novas features

## Período e referência

- **Período analisado:** 12 a 14/09/2026.
- **Base atual:** commit `c331760` — 14/09/2026.
- **Entregas consideradas:** `ca185c2` (12/09) e `c331760` (14/09).
- **13/09:** não há commit registrado no histórico do projeto.
- **Editor de referência:** Unity `6000.6.0f1`.

Este documento resume as mudanças visíveis ao jogador e serve como roteiro inicial para o diário de validação do grupo de QA.

## Resumo das entregas

### 12/09 — Fase 2, Fase 3 e base de gameplay

- **Fase 2 — Escola e Resgate:** nova cena registrada no Build Settings, com escola montada em runtime, quatro subordinados ETs, nove alunos/reféns, jaulas pixel art, resgate, acompanhamento de Edelzio e entrada sequencial no Fusca.
- **Combate de Edelzio:** ataque direcional com combo de três golpes, antecipação, impacto, recuperação, hitbox ativa no impacto, dano, knockback, hitstop e tremor de câmera.
- **ETs:** perseguição, telegráfico de ataque, rajada que afeta vida e sanidade, reação ao golpe e derrota. A entidade ancestral permanece invulnerável ao ataque comum por regra narrativa.
- **Fase 3 — O Guardião:** nova cena com diocese/igreja, cinco ETs, Padre Fábio, Livro do Tombo Secreto e conclusão do Ato III.
- **Aliados:** os nove alunos possuem perfis de ataque próprios. Na Fase 3, um aliado é invocado por comando, executa um golpe e entra em recarga individual de 5 segundos.
- **Interface e fluxo:** hotbar de cinco itens, HUD de combate/diálogo/vitória, transição pixel art de viagem e saída do Fusca.
- **Suporte de validação:** construtores de cena, assets pixel art e testes Play Mode de combate, Fusca, Fase 3, dificuldade e aliados.

### 14/09 — controles, apresentação e polimento

- **Remapeamento de controles:** tela de edição no menu para movimento, corrida, interação, ataque, comando de aliado e esquiva. Cada ação aceita tecla ou botão do mouse, pode ser restaurada ao padrão e permanece salva entre cenas.
- **Apresentação pixel art:** câmeras ortográficas passam a desabilitar MSAA, HDR e resolução dinâmica quando aplicável, preservando a nitidez dos sprites.
- **Cenografia procedural:** acabamento visual para casa, escola e diocese, incluindo pisos, paredes, salas, bancos, vitrais, altar, iluminação, plantas, murais, janelas, poças, folhas e vaga-lumes.
- **Robustez da cenografia:** construção idempotente, cache de sprites, filtro Point e recuperação de sprites ausentes sem criar duplicatas ou novos colisores decorativos.
- **Fusca e transições:** vaga compartilhada e dimensionada para a carroceria completa; deslocamento físico curto até a vaga; restante da saída apresentado na cinematics de estrada noturna.
- **Polimento de gameplay:** ajustes de combo, esquiva, bloqueios durante diálogo/pausa, feedback visual dos ETs, circulação/separação dos aliados e apresentação dos nomes/cooldowns no HUD.
- **Suporte de validação:** testes Edit Mode de cenografia e testes Play Mode de combo, paredes, dano único, esquiva, pausa, diálogo e hitstop.

## Roteiro de smoke test

Marcar cada item como **PASS**, **FAIL** ou **BLOCKED** e anexar vídeo/screenshot quando houver falha.

1. Abrir `Menu_MisterioDeVarginha` e acessar **TUTORIAL / CONTROLES**.
2. Alterar ataque e esquiva para outras teclas; alterar o comando de aliado para um botão do mouse; sair e voltar ao menu para confirmar a persistência.
3. Abrir `Fase2_Escola_Resgate` diretamente. Aguardar a chegada e confirmar escola visível, quatro ETs e nove alunos enjaulados.
4. Testar movimento, corrida, ataque, combo 1–2–3, esquiva e seleção da hotbar.
5. Confirmar que o golpe não atravessa paredes, não aplica dano duplicado em múltiplos colliders e que a rajada do ET reduz vida e sanidade.
6. Derrotar os quatro ETs. Confirmar desaparecimento das jaulas, liberação dos nove alunos, formação sem sobreposição e entrada em fila no Fusca.
7. Confirmar que a saída mantém o carro dentro da vaga/faixa e que a viagem pixel art mostra estrada noturna, carro, turma e progresso sem travamento.
8. Abrir `Fase3_Igreja_Guardiao` diretamente. Confirmar igreja/diocese, altar, vitrais, cinco ETs, Padre Fábio, livro e painel lateral dos aliados.
9. Usar o comando de ataque para Edelzio e o comando de aliado mirando em um ET. Confirmar um único golpe por invocação, alternância dos alunos e cooldown individual de 5 segundos.
10. Derrotar os cinco ETs; falar com Padre Fábio; examinar o Livro do Tombo Secreto; confirmar a tela de vitória e a revelação do selo de 1898.
11. Recarregar cada cena ao menos uma vez e verificar ausência de decoração duplicada, sprites invisíveis, textos cortados e colisores indevidos no cenário.

## Cenários prioritários para o diário

| ID | Área | Resultado esperado | Prioridade |
| --- | --- | --- | --- |
| QA-01 | Controles | Remapeamentos funcionam imediatamente e persistem entre menu e cenas; reset restaura o padrão. | Alta |
| QA-02 | Fase 2 / população | Sempre existem 4 ETs, 9 alunos e jaulas; uma cena parcialmente salva é reparada sem duplicar objetos. | Alta |
| QA-03 | Combo | Os três golpes seguem a ordem correta, atingem somente no impacto e o combo expira após a janela de continuidade. | Alta |
| QA-04 | Combate | Parede bloqueia o golpe; múltiplos colliders do mesmo alvo geram apenas um dano; ET aplica vida e sanidade. | Alta |
| QA-05 | Esquiva e estados | Esquiva tem duração/recarga limitadas e ataque/esquiva ficam bloqueados durante pausa, diálogo e vitória. | Alta |
| QA-06 | Resgate e Fusca | Ao derrotar o último ET, os 9 alunos são libertados, seguem Edelzio e entram no carro sem sobreposição. | Alta |
| QA-07 | Viagem | O Fusca mantém faixa e posição esperadas; a cinematics termina sem tela travada ou carro fora do quadro. | Média |
| QA-08 | Aliados da Fase 3 | Cada comando usa um aluno pronto, respeita cooldown individual de 5 s, mira o ET correto e não gasta recarga sem alvo válido. | Alta |
| QA-09 | Conclusão da Fase 3 | Padre Fábio e o livro só concluem a fase depois da derrota dos 5 ETs; a tela de vitória é exibida corretamente. | Alta |
| QA-10 | Pixel art/cenário | Sprites estão nítidos, sem blur; cenário não duplica ao reinicializar e a decoração não cria colisão. | Média |

## Controles padrão para referência

| Ação | Padrão |
| --- | --- |
| Mover | `WASD` ou setas compatíveis |
| Correr | `Shift` |
| Interagir / examinar | `E`, `Espaço` ou `Enter` compatíveis |
| Atacar / combo | Clique esquerdo ou `J` |
| Comandar aluno | Clique direito ou `L` |
| Esquivar | `Ctrl` |
| Hotbar | Teclas `1` a `5` |

## Pontos de atenção e pendências

- Os testes automatizados foram adicionados ao projeto, mas ainda precisam ser executados no **Test Runner** do Unity antes de uma build final.
- A cenografia das fases 2 e 3 é criada proceduralmente; validar tanto a abertura direta quanto o fluxo vindo da fase anterior.
- A entidade ancestral não deve receber dano do ataque comum; isso é comportamento esperado, não bug.
- Ao registrar uma falha, informar cena, commit/build, resolução da janela, controles usados, passo de reprodução e evidência.

## Modelo para cada entrada diária

```text
Data/hora:
Build/commit:
Testador:
Cenário/ID:
Passos executados:
Resultado: PASS / FAIL / BLOCKED
Comportamento observado:
Comportamento esperado:
Evidência: screenshot/vídeo/log
Severidade ou chamado:
```
