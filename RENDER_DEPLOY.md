# Guia de Deploy no Render

Este guia explica como fazer deploy da aplicação Heloilo no Render usando Docker.

## Pré-requisitos

- Conta no [Render](https://render.com)
- Repositório Git configurado (GitHub, GitLab ou Bitbucket)
- Código da aplicação pushado para o repositório

## Passo a Passo

### 1. Criar um novo Web Service no Render

1. Acesse o [Dashboard do Render](https://dashboard.render.com)
2. Clique em "New +" e selecione "Web Service"
3. Conecte seu repositório Git
4. Configure as seguintes opções:
   - **Name**: `heloilo-backend` (ou o nome que preferir)
   - **Environment**: `Docker`
   - **Region**: Escolha a região mais próxima dos seus usuários
   - **Branch**: `main` (ou a branch que você usa)
   - **Root Directory**: Deixe em branco (raiz do projeto)
   - **Dockerfile Path**: `Dockerfile` (já está na raiz)
   - **Docker Context**: Deixe em branco

### 2. Configurar Variáveis de Ambiente

No painel do serviço, vá em "Environment" e adicione as seguintes variáveis:

#### Obrigatórias:

```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Data Source=/app/data/heloilo.db
Jwt__SecretKey=<sua-chave-secreta-minimo-32-caracteres>
Jwt__Issuer=Heloilo
Jwt__Audience=Heloilo
```

#### Opcionais (mas recomendadas):

```
Cors__AllowedOrigins__0=https://seu-frontend.onrender.com
Cors__AllowedOrigins__1=https://seu-dominio.com
RateLimiting__MaxRequests=100
RateLimiting__WindowMinutes=1
RequestLimits__MaxJsonSize=10485760
RequestLimits__MaxMultipartSize=52428800
```

**Importante sobre a chave JWT:**
- Use uma chave secreta forte com pelo menos 32 caracteres
- Gere uma chave segura usando: `openssl rand -base64 32`
- Nunca compartilhe ou commite a chave secreta no Git

**Importante sobre CORS:**
- Adicione a URL do seu frontend na lista de origens permitidas
- Use o formato de array do .NET Core: `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, etc.

### 3. Configurar Volume Persistente para SQLite

**IMPORTANTE**: Como estamos usando SQLite, é essencial configurar um volume persistente para não perder dados quando o container reiniciar.

1. No painel do serviço, vá em "Volumes"
2. Clique em "Connect Volume"
3. Configure:
   - **Name**: `heloilo-data`
   - **Mount Path**: `/app/data`
   - **Size**: Escolha o tamanho necessário (1GB é suficiente para começar)

Isso garantirá que o arquivo `heloilo.db` seja persistido entre reinicializações do serviço.

### 4. Configurações Adicionais

#### Health Check (Recomendado)

No painel do serviço, vá em "Settings" → "Health Check Path" e configure:
- **Health Check Path**: `/health` (ou o endpoint que você usa)
- **Timeout**: `300` segundos

#### Auto-Deploy

Certifique-se de que "Auto-Deploy" está habilitado para que novos commits na branch principal acionem automaticamente o deploy.

#### Build Command e Start Command

Como estamos usando Docker, não é necessário configurar build/start commands. O Render usará o Dockerfile automaticamente.

### 5. Deploy

1. Após configurar tudo, clique em "Create Web Service"
2. O Render irá:
   - Fazer build da imagem Docker
   - Iniciar o container
   - Aplicar as migrations do banco de dados automaticamente (via Program.cs)

### 6. Verificar Deploy

1. Aguarde o build completar (pode levar alguns minutos na primeira vez)
2. Verifique os logs em "Logs" para garantir que não há erros
3. Acesse a URL fornecida pelo Render (ex: `https://heloilo-backend.onrender.com`)
4. Teste o endpoint de health: `https://seu-servico.onrender.com/health`
5. Acesse o Swagger (se habilitado em produção): `https://seu-servico.onrender.com/swagger`

## Variáveis de Ambiente Detalhadas

### ConnectionStrings__DefaultConnection

```
Data Source=/app/data/heloilo.db
```

O caminho `/app/data/heloilo.db` é onde o arquivo SQLite será armazenado. Este diretório deve estar montado em um volume persistente.

### JWT Configuration

```
Jwt__SecretKey=your-super-secret-key-minimum-32-characters-long
Jwt__Issuer=Heloilo
Jwt__Audience=Heloilo
```

- `SecretKey`: Chave secreta usada para assinar tokens JWT (obrigatório, mínimo 32 caracteres)
- `Issuer`: Emissor do token (opcional, padrão: Heloilo)
- `Audience`: Audiência do token (opcional, padrão: Heloilo)

### CORS Configuration

Para permitir requisições do frontend:

```
Cors__AllowedOrigins__0=https://seu-frontend.onrender.com
Cors__AllowedOrigins__1=https://www.seudominio.com
```

Use o formato de array do .NET Core Configuration Provider com índices numéricos.

### Rate Limiting

```
RateLimiting__MaxRequests=100
RateLimiting__WindowMinutes=1
```

- `MaxRequests`: Número máximo de requisições por janela de tempo
- `WindowMinutes`: Janela de tempo em minutos

### Request Limits

```
RequestLimits__MaxJsonSize=10485760
RequestLimits__MaxMultipartSize=52428800
```

- `MaxJsonSize`: Tamanho máximo de requisições JSON em bytes (padrão: 10MB)
- `MaxMultipartSize`: Tamanho máximo de requisições multipart (upload de arquivos) em bytes (padrão: 50MB)

## Troubleshooting

### Erro: "Database is locked"

Isso pode acontecer se houver múltiplas instâncias tentando acessar o mesmo arquivo SQLite. Soluções:
1. Certifique-se de que está usando apenas 1 instância do serviço
2. Considere migrar para PostgreSQL (Render oferece PostgreSQL gerenciado)

### Erro: "Port already in use"

O Render define automaticamente a variável `PORT`. Não defina `ASPNETCORE_URLS` manualmente, deixe o ASP.NET Core usar a variável `PORT` automaticamente.

### Erro: "JWT SecretKey não configurado"

Certifique-se de que a variável `Jwt__SecretKey` está configurada com pelo menos 32 caracteres.

### Migrations não são aplicadas

O Program.cs aplica migrations automaticamente na inicialização. Se isso não estiver funcionando:
1. Verifique os logs do container
2. Certifique-se de que o diretório `/app/data` existe e tem permissões de escrita
3. Verifique se o volume está montado corretamente

### Logs não aparecem

Verifique a seção "Logs" no painel do Render. Se não aparecer nada:
1. Aguarde alguns segundos (logs podem ter delay)
2. Verifique se o serviço está rodando (status deve ser "Live")
3. Tente fazer uma requisição para forçar logs

## Custos e Limites

### Free Tier do Render

- **750 horas/mês** de tempo de execução grátis
- Sleep automático após 15 minutos de inatividade (free tier)
- Volume persistente de 1GB grátis
- Sem SSL customizado no free tier

### Recomendações para Produção

1. **Upgrade para plano pago** para evitar sleep automático
2. **Use PostgreSQL** ao invés de SQLite para produção (melhor para múltiplas instâncias)
3. **Configure domínio customizado** com SSL
4. **Configure backups** do banco de dados
5. **Monitore logs** regularmente

## Migração para PostgreSQL (Opcional)

Para ambientes de produção com múltiplas instâncias, considere migrar para PostgreSQL:

1. Crie um serviço PostgreSQL no Render
2. Atualize a connection string:
   ```
   ConnectionStrings__DefaultConnection=Host=<host>;Port=<port>;Database=<db>;Username=<user>;Password=<password>;SSL Mode=Require;
   ```
3. Atualize o `Program.cs` para usar `UseNpgsql` ao invés de `UseSqlite`
4. Aplique as migrations

## Suporte

Para mais informações:
- [Documentação do Render](https://render.com/docs)
- [Documentação do ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Documentação do Entity Framework Core](https://docs.microsoft.com/ef/core)

