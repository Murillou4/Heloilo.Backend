# 💕 Heloilo

<div align="center">

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp)
![License](https://img.shields.io/badge/license-MIT-blue?style=for-the-badge)

**Um aplicativo íntimo e privado para casais celebrarem seu amor** 💑

[✨ Funcionalidades](#-funcionalidades) • [🚀 Começando](#-começando) • [🏗️ Arquitetura](#️-arquitetura) • [📚 Documentação](#-documentação)

</div>

---

## 📖 Sobre o Projeto

**Heloilo** é uma plataforma digital desenvolvida especialmente para casais que desejam manter viva a chama do relacionamento. Um espaço privado, seguro e romântico onde vocês podem:

- 🎂 Acompanhar marcos e aniversários do relacionamento
- 💭 Compartilhar desejos e presentes
- 🖼️ Criar memórias através de fotos e histórias
- 😊 Registrar humores diários e acompanhar o bem-estar emocional
- 📅 Compartilhar agendas e status em tempo real
- 💬 Conversar em um chat privado só de vocês

> _"Porque todo amor merece ser celebrado todos os dias"_ ✨

---

## ✨ Funcionalidades

### 🧑‍❤️‍👩 Perfis Personalizados

- Cadastro individual para cada membro do casal
- Sistema de vinculação com aprovação mútua
- Perfis personalizáveis com fotos e temas de cores
- Definição de datas especiais (primeiro encontro, namoro, etc.)

### 💝 Linha do Tempo do Relacionamento

- Contador de dias juntos
- Notificações de aniversário e mêsversário
- Tela especial em datas comemorativas
- Criação de história ilustrada em formato de livro

### 💭 Lista de Desejos

- Criação e gerenciamento de desejos
- Níveis de importância personalizáveis
- Links diretos para produtos online
- Comentários e notas em cada desejo
- Sincronização em tempo real

### 🖼️ Memórias Especiais

- Upload e organização de fotos
- Carrossel com transições suaves
- Música ambiente romântica
- Metadados (data, título, descrição)

### 😊 Registro de Humor Diário

- 13 categorias de humor (positivos, negativos e neutros)
- Dashboard com gráficos e estatísticas
- Filtros por período customizável
- Visualização do humor do parceiro

### 📅 Agenda e Status

- Lista de tarefas/atividades do dia
- Status em tempo real ("o que estou fazendo agora")
- Visualização da agenda do parceiro
- Acompanhamento de rotinas

### 💬 Chat Privado

- Mensagens em tempo real
- Emojis e stickers românticos
- Histórico completo de conversas
- Pesquisa de mensagens
- Notificações instantâneas

### 🔐 Segurança e Privacidade

- Criptografia de dados sensíveis
- Autenticação JWT
- Acesso restrito apenas ao casal
- Sem anúncios ou integração com redes sociais
- 100% privado e seguro

---

## 🚀 Começando

### Pré-requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) ou [Visual Studio Code](https://code.visualstudio.com/)
- SQL Server (LocalDB ou instância completa)

### Instalação

1. **Clone o repositório**

   ```bash
   git clone https://github.com/seu-usuario/heloilo-backend.git
   cd heloilo-backend
   ```

2. **Restaure as dependências**

   ```bash
   dotnet restore
   ```

3. **Configure a string de conexão**

   Edite o arquivo `appsettings.json` com suas configurações de banco de dados:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=HeloiloDB;Trusted_Connection=True;"
     }
   }
   ```

4. **Execute as migrations**

   ```bash
   dotnet ef database update
   ```

5. **Execute a aplicação**

   ```bash
   dotnet run
   ```

6. **Acesse a API**

   A aplicação estará disponível em: `https://localhost:5001`

   Documentação Swagger: `https://localhost:5001/swagger`

---

## 🏗️ Arquitetura

O projeto segue os princípios de **Clean Architecture**, garantindo manutenibilidade, testabilidade e escalabilidade.

```
Heloilo.Backend/
│
├── 📁 Heloilo.Domain/              # Entidades e regras de negócio
│   └── Models/                      # Modelos de domínio
│
├── 📁 Heloilo.Application/          # Casos de uso e lógica de aplicação
│   ├── DTOs/                        # Data Transfer Objects
│   ├── Interfaces/                  # Contratos de serviços
│   ├── Services/                    # Implementação de serviços
│   └── UseCases/                    # Casos de uso específicos
│
├── 📁 Heloilo.Infrastructure/       # Implementações de infraestrutura
│   ├── Data/                        # Contexto do banco de dados
│   ├── Repositories/                # Repositórios
│   └── External/                    # Serviços externos
│
└── 📁 Heloilo.WebAPI/               # API REST
    ├── Controllers/                 # Controladores da API
    ├── Middlewares/                 # Middlewares personalizados
    └── Payloads/                    # Request/Response models
```

### Camadas

- **Domain**: Núcleo da aplicação com regras de negócio puras
- **Application**: Orquestração de casos de uso e lógica de aplicação
- **Infrastructure**: Implementação de persistência e serviços externos
- **WebAPI**: Camada de apresentação (API REST)

---

## 🛠️ Tecnologias

### Backend

- **.NET 9.0** - Framework principal
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **SQL Server** - Banco de dados relacional
- **JWT** - Autenticação e autorização
- **SignalR** - Comunicação em tempo real
- **AutoMapper** - Mapeamento de objetos

### Boas Práticas

- Clean Architecture
- SOLID Principles
- Repository Pattern
- Dependency Injection
- Unit of Work

---

## 📚 Documentação

Para informações detalhadas sobre os requisitos do sistema, consulte:

- 📋 [Requisitos Funcionais e Não Funcionais](requirements.md)

---

## 🗺️ Roadmap

- [x] Definição de requisitos
- [x] Estrutura do projeto
- [ ] Implementação do módulo de autenticação
- [ ] Sistema de vinculação de casais
- [ ] Módulo de perfis e configurações
- [ ] Lista de desejos
- [ ] Galeria de memórias
- [ ] Registro de humor diário
- [ ] Agenda e status
- [ ] Chat em tempo real
- [ ] Sistema de notificações
- [ ] Dashboard e relatórios
- [ ] Testes unitários e de integração
- [ ] Deploy e CI/CD

---

## 🤝 Contribuindo

Contribuições são sempre bem-vindas! Para contribuir:

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/MinhaFeature`)
3. Commit suas mudanças (`git commit -m 'Adiciona MinhaFeature'`)
4. Push para a branch (`git push origin feature/MinhaFeature`)
5. Abra um Pull Request

---

## 📝 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

## 💖 Feito com Amor

Desenvolvido com 💕 para casais que acreditam que o amor deve ser celebrado todos os dias.

---

<div align="center">

**[⬆ Voltar ao topo](#-heloilo)**

</div>
