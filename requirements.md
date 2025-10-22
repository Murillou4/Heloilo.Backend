# 📋 Requisitos do Sistema — Heloilo

---

## 🧩 1. Requisitos Funcionais (RF)

### 🧑‍❤️‍👩 Módulo: Cadastro e Perfis do Casal

- **RF01** – O sistema deve permitir o cadastro de dois usuários (um casal).

- **RF02** – Após o cadastro individual, um usuário deve enviar uma solicitação de vinculação ao outro parceiro.

- **RF03** – O sistema deve notificar o parceiro sobre a solicitação de vinculação pendente.

- **RF04** – Após ambos os usuários aceitarem a vinculação, o sistema deve liberar a etapa de configuração inicial do relacionamento.

- **RF05** – O sistema deve exibir uma etapa de configuração inicial (opcional) onde o casal pode definir dados históricos e datas especiais. Esta etapa pode ser completada ou pulada por ambos os usuários.

- **RF06** – O sistema deve ser liberado completamente apenas após ambos os usuários completarem ou pularem a etapa de configuração inicial.

- **RF07** – Cada usuário deve ter perfil individual, com:

  - Nome e apelido carinhoso
  - Foto de perfil personalizável
  - Cor/tema visual próprio (a aplicação deve possuir um padrão de cor e o usuário altera se quiser)

- **RF08** – O casal deve poder definir:

  - Data em que se conheceram
  - Local onde se conheceram
  - Data de início do relacionamento
  - Tipo de comemoração (anual ou mensal)

- **RF09** – O sistema deve calcular e exibir quantos dias o casal está junto.

- **RF10** – O sistema deve notificar o casal quando estiver próximo o aniversário de relacionamento.

- **RF11** – O sistema deve exibir uma tela especial ou destaque visual no dia do aniversário do casal.

- **RF12** – O sistema deve permitir login individual, com senha distinta para cada membro.

- **RF13** – O sistema deve permitir logout seguro e rápido.

- **RF14** – O casal deve poder criar sua história com imagens e texto em ordem cronológica. No frontend, cada página deve ser uma página de um livro.

- **RF65** – O sistema deve permitir desvinculação do relacionamento apenas com confirmação de ambos os usuários.

- **RF66** – Após a desvinculação, os dados de cada usuário devem ser mantidos separadamente, permitindo novo vínculo com outro parceiro.

- **RF67** – O sistema não deve permitir alteração de parceiro após vinculação ativa, sendo necessário desvincular primeiro.

---

### 💭 Módulo: Desejos e Presentes

- **RF15** – O usuário deve poder criar desejos, informando:

  - Título
  - Descrição (opcional)
  - Link (ex: loja online)
  - Imagem ilustrativa
  - Nível de importância (de 1 a 5)
  - Data de registro

- **RF16** – O usuário deve poder editar ou excluir seus desejos.

- **RF17** – O sistema deve exibir uma lista cronológica dos desejos.

- **RF18** – O usuário deve poder pesquisar desejos por título.

- **RF19** – O sistema deve diferenciar visualmente os desejos criados por cada pessoa (cores distintas).

- **RF20** – O sistema deve permitir comentários e notas em cada desejo.

- **RF21** – O sistema deve sincronizar em tempo real novos desejos e comentários entre os dois perfis.

- **RF83** – O sistema deve permitir ordenação configurável da lista de desejos por data de criação, nível de importância ou ambos combinados.

- **RF84** – O sistema deve implementar resolução de conflitos quando ambos os usuários editarem o mesmo desejo simultaneamente, aplicando last-write-wins e notificando o outro usuário da alteração.

---

### 🖼️ Módulo: Memórias e Momentos Especiais

- **RF22** – O sistema deve permitir upload de fotos para a página de memórias.

- **RF23** – O sistema deve exibir as fotos em carrossel com transições suaves.

- **RF24** – O usuário deve poder ouvir música ambiente romântica na página de memórias.

- **RF25** – O sistema deve permitir controlar a música (play/pause).

- **RF26** – O sistema deve permitir navegar facilmente entre memórias e tela inicial.

- **RF27** – O sistema deve armazenar metadados da memória (data, título, descrição opcional).

- **RF58** – O sistema deve permitir adicionar tags e categorias personalizadas para organizar memórias e fotos.

- **RF59** – O sistema deve aplicar compressão automática em imagens maiores que 5MB, mantendo qualidade visual adequada.

- **RF60** – O sistema deve definir limite máximo de 10MB por imagem após compressão e 50MB para vídeos curtos.

---

### 🔔 Módulo: Notificações e Sincronização

- **RF28** – O sistema deve enviar notificações em tempo real quando:

  - O parceiro adicionar ou editar um desejo
  - Houver um novo comentário
  - Aproximar-se uma data especial (aniversário ou mêsversário)

- **RF29** – As notificações devem aparecer tanto no app móvel quanto na versão web.

- **RF30** – O sistema deve manter as informações sincronizadas entre os dispositivos.

- **RF73** – O sistema deve permitir que cada usuário configure suas preferências de notificações, podendo ativar ou desativar por tipo (desejos, comentários, datas especiais, chat, humor do parceiro).

- **RF74** – O sistema deve permitir definir horários preferenciais para receber notificações não urgentes (ex: não notificar entre 22h e 7h).

- **RF75** – O sistema deve permitir controle granular de categorias de notificações, incluindo intensidade (silenciosa, normal, prioritária).

---

### 🔐 Módulo: Segurança e Privacidade

- **RF31** – Cada usuário deve possuir senha única e obrigatória.

- **RF32** – O sistema deve impedir acessos não autorizados.

- **RF33** – As informações do casal devem ser acessíveis apenas entre os dois usuários cadastrados.

- **RF34** – O sistema deve criptografar dados sensíveis (senhas, mensagens, links, imagens).

- **RF35** – O sistema não deve permitir integração com redes sociais externas.

- **RF36** – O sistema não deve exibir anúncios ou conteúdo de terceiros.

---

### 📋 Módulo: Validações e Regras de Dados

- **RF61** – O sistema deve aceitar datas no formato DD/MM/AAAA e validar a existência da data (ex: 31/02 é inválida).

- **RF62** – O sistema deve validar que a data de início do relacionamento não seja futura e não seja anterior a 1900.

- **RF63** – O sistema deve aplicar limite de 500 caracteres para títulos, 2000 caracteres para descrições e 5000 caracteres para textos longos (história do casal).

- **RF64** – O sistema deve validar URLs de links de desejos, aceitando apenas protocolos HTTP e HTTPS.

---

### 😊 Módulo: Registro de Humor Diário

- **RF38** – Cada membro do casal deve poder registrar diariamente como está se sentindo.

- **RF39** – O sistema deve oferecer as seguintes opções de humores positivos:

  - Feliz / contente: sensação de leveza ou satisfação com algo do dia
  - Animado / motivado: energia para fazer coisas, associado a conquistas ou expectativas positivas
  - Calmo / relaxado: paz interior, sem estresse, se sente bem consigo mesmo
  - Orgulhoso: quando sente que fez algo legal ou atingiu uma meta
  - Grato / satisfeito: aprecia o que tem ou momentos bons do dia

- **RF40** – O sistema deve oferecer as seguintes opções de humores negativos:

  - Triste / melancólico: desânimo, perda de interesse ou leve sofrimento emocional
  - Irritado / frustrado: raiva ou impaciência por situações externas ou falhas próprias
  - Ansioso / preocupado: sensação de tensão ou medo do que pode acontecer
  - Culpado / arrependido: quando pensa em erros ou decisões que poderia ter feito diferente
  - Cansado / desmotivado: falta de energia ou vontade de fazer coisas

- **RF41** – O sistema deve oferecer as seguintes opções de humores neutros / mistos:

  - Entediado: sem estímulo ou interesse, sensação de "vazio" momentâneo
  - Confuso / indeciso: não sabe bem o que pensar ou fazer
  - Curioso / intrigado: mente ativa, querendo aprender ou descobrir algo

- **RF42** – O sistema deve exibir um dashboard/relatório com gráficos mostrando o histórico de humores registrados.

- **RF43** – O usuário deve poder filtrar o relatório de humores por período customizável (dia, semana, mês, intervalo personalizado).

- **RF44** – O sistema deve permitir visualizar o humor do parceiro para fomentar empatia e comunicação no relacionamento.

- **RF76** – O sistema deve permitir múltiplos registros de humor por dia, mantendo histórico de todas as alterações com horário.

- **RF77** – O sistema deve permitir adicionar um comentário ou nota opcional junto ao registro de humor (ex: "Triste porque meu gato morreu").

- **RF78** – O sistema deve exibir a evolução do humor ao longo do dia em uma timeline visual, mostrando mudanças e comentários.

---

### 📅 Módulo: Agenda e Status Diário

- **RF46** – O usuário deve poder criar uma agenda do dia com uma lista de atividades/tarefas que pretende realizar.

- **RF47** – O usuário deve poder editar, marcar como concluída ou excluir atividades da agenda.

- **RF48** – O usuário deve poder informar o que está fazendo no momento através de um status em tempo real.

- **RF49** – O status atual do usuário deve ser visível para o parceiro.

- **RF50** – O sistema deve exibir a agenda do parceiro para que ambos possam acompanhar a rotina um do outro.

- **RF79** – O sistema deve permitir configurar alertas e lembretes para atividades agendadas, com antecedência configurável (5, 15, 30 minutos ou 1 hora antes).

- **RF80** – O status em tempo real deve ter frequência de atualização máxima de 30 segundos entre os dispositivos.

- **RF81** – O sistema deve expirar automaticamente um status após 4 horas sem atualização, marcando como "status desatualizado".

- **RF82** – O sistema deve manter histórico de status ao longo do dia, permitindo visualizar o que o parceiro fez durante todo o dia.

---

### 💬 Módulo: Chat e Comunicação

- **RF51** – O sistema deve fornecer um chat privado entre o casal.

- **RF52** – O chat deve suportar o envio de mensagens de texto em tempo real.

- **RF53** – O sistema deve notificar o usuário quando receber uma nova mensagem do parceiro.

- **RF54** – O chat deve permitir o envio de emojis e stickers românticos.

- **RF55** – O sistema deve manter o histórico de conversas do casal.

- **RF56** – O usuário deve poder pesquisar mensagens no histórico do chat.

- **RF68** – O chat deve permitir o envio de imagens com limite de 10MB por arquivo.

- **RF69** – O chat deve permitir o envio de mensagens de áudio com limite de 5 minutos e 20MB por arquivo.

- **RF70** – O sistema deve aplicar compressão automática em imagens e áudios enviados no chat para otimizar uso de armazenamento e transferência.

- **RF71** – O chat deve exibir status de mensagens: enviada (um check), entregue (dois checks) e lida (dois checks coloridos).

- **RF72** – O chat deve exibir indicador visual quando o parceiro estiver digitando uma mensagem.

---

### 🌐 Módulo: Multiplataforma

- **RF57** – O sistema deve estar disponível para:
  - Web (versão responsiva)

---

## ⚙️ 2. Requisitos Não Funcionais (RNF)

### 🚀 Desempenho

- **RNF01** – O sistema deve carregar a tela inicial em até 2 segundos.

- **RNF02** – As atualizações em tempo real devem ocorrer com latência inferior a 1 segundo.

- **RNF03** – O app deve suportar sincronização simultânea entre múltiplos dispositivos do casal.

- **RNF26** – O sistema deve implementar lazy loading e paginação para listas longas, carregando inicialmente apenas 20 itens e carregando mais sob demanda.

- **RNF27** – O sistema deve resolver conflitos de sincronização offline aplicando estratégia de mesclagem inteligente: operações locais têm prioridade para criação, last-write-wins para edição com timestamp do servidor.

- **RNF28** – O carregamento inicial da aplicação deve limitar-se aos dados essenciais: perfil do usuário, últimas 5 notificações e status atual do parceiro.

---

### 🔒 Segurança

- **RNF04** – As senhas devem ser armazenadas com hash seguro (ex: bcrypt, Argon2).

- **RNF05** – As requisições devem trafegar sobre HTTPS.

- **RNF06** – As imagens e dados pessoais devem ser armazenados em repositório seguro (ex: S3, Firebase Storage).

- **RNF07** – O sistema deve aplicar autenticação baseada em token (JWT).

- **RNF08** – O acesso ao backend deve exigir verificação de identidade e autorização por sessão.

- **RNF29** – O sistema deve definir claramente rotas públicas (login, cadastro) e rotas privadas (todas as demais, requerendo autenticação).

- **RNF30** – Os tokens JWT devem ter validade de 7 dias, com renovação automática silenciosa 24 horas antes da expiração.

- **RNF31** – O sistema deve implementar bloqueio anti-brute force: após 5 tentativas de login falhas, bloquear conta por 15 minutos.

- **RNF32** – Dados sensíveis no banco (mensagens, comentários, links) devem ser criptografados com AES-256 em repouso.

- **RNF33** – O sistema deve permitir renovação manual de token através de refresh token com validade de 30 dias.

---

### 💡 Usabilidade

- **RNF09** – A interface deve ser romântica, acolhedora e minimalista.

- **RNF10** – A navegação deve ser intuitiva e fluida, com poucas ações para alcançar as principais funções.

- **RNF11** – O design deve refletir a identidade do casal, com cores personalizáveis.

- **RNF12** – As fontes e elementos visuais devem manter consistência e legibilidade em todos os dispositivos.

- **RNF34** – O sistema deve suportar leitores de tela e seguir diretrizes WCAG 2.1 nível AA para acessibilidade.

- **RNF35** – O sistema deve garantir contraste mínimo de 4.5:1 para textos normais e 3:1 para textos grandes, permitindo tema de alto contraste.

- **RNF36** – O sistema deve oferecer modo reduzido de animações para usuários com sensibilidade a movimentos, respeitando preferência do sistema operacional.

- **RNF37** – O sistema deve permitir controle de volume e on/off para música ambiente, mantendo a preferência do usuário entre sessões.

---

### ⚡ Confiabilidade

- **RNF13** – O sistema deve manter disponibilidade mínima de 99%.

- **RNF14** – Deve haver mecanismo de backup automático dos dados do casal.

- **RNF15** – Em caso de falha de conexão, o app deve armazenar alterações localmente e sincronizar quando online.

- **RNF38** – O sistema deve implementar backup incremental diário automático dos dados do casal, com política de retenção de 30 dias.

- **RNF39** – O sistema deve notificar o usuário sobre falha de conexão e exibir indicador visual de operações pendentes na fila de sincronização.

- **RNF40** – O sistema deve implementar logging estruturado de erros, métricas de uso e performance, com monitoramento em tempo real e alertas para falhas críticas.

---

### 🧱 Manutenibilidade

- **RNF16** – O código deve seguir boas práticas de clean architecture.

- **RNF17** – O sistema deve ter camadas separadas (frontend, backend e banco).

- **RNF18** – O backend deve ser modular, permitindo expansão (ex: novos tipos de lembretes, timeline).

- **RNF19** – O banco de dados deve ser relacional e normalizado.

---

### 🌍 Portabilidade

- **RNF20** – O sistema deve funcionar corretamente em diferentes tamanhos de tela.

- **RNF21** – A versão web deve ser responsiva.

- **RNF22** – O app deve permitir login e sincronização entre dispositivos sem perda de dados.

- **RNF41** – O sistema deve implementar layouts adaptativos específicos para mobile (< 768px), tablet (768px - 1024px) e desktop (> 1024px).

- **RNF42** – A aplicação web deve suportar tanto orientação portrait quanto landscape, ajustando o layout automaticamente conforme a orientação do dispositivo.

---

### 🎨 Experiência Sensorial

- **RNF23** – O app deve ter animações suaves (ex: corações flutuando, transições com fade).

- **RNF24** – Deve reproduzir músicas temáticas na página de memórias.

- **RNF25** – O modo de celebração (aniversário/mêsversário) deve ter efeitos visuais especiais.
