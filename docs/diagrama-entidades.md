# Diagrama de Entidades

O diagrama abaixo apresenta os principais relacionamentos do sistema de gestão de franquias.

```mermaid
erDiagram
    FRANQUEADORA ||--o{ UNIDADE : possui
    UNIDADE ||--o{ RESPONSAVEL : possui
    UNIDADE ||--o{ USUARIO : vincula

    CATEGORIA ||--o{ PRODUTO : classifica
    FORNECEDOR o|--o{ PRODUTO : fornece

    UNIDADE ||--o{ ESTOQUE : possui
    PRODUTO ||--o{ ESTOQUE : armazenado
    ESTOQUE ||--o{ MOVIMENTACAO_ESTOQUE : registra

    UNIDADE ||--o{ VENDA : realiza
    USUARIO ||--o{ VENDA : registra
    VENDA ||--|{ ITEM_VENDA : possui
    PRODUTO ||--o{ ITEM_VENDA : vendido

    UNIDADE ||--o{ ROYALTY : gera
    UNIDADE ||--o{ CHAMADO : abre
    USUARIO ||--o{ CHAMADO : registra

    FRANQUEADORA {
        int Id PK
        string NomeFantasia
        string RazaoSocial
        string CNPJ UK
        string Email
        bool Ativa
    }

    UNIDADE {
        int Id PK
        int FranqueadoraId FK
        string Nome
        string CNPJ UK
        bool Ativa
        decimal PercentualRoyalty
    }

    RESPONSAVEL {
        int Id PK
        int UnidadeId FK
        string Nome
        string CPF
        bool Ativo
    }

    USUARIO {
        int Id PK
        int UnidadeId FK
        string Nome
        string Email UK
        int Perfil
        bool Ativo
    }

    CATEGORIA {
        int Id PK
        string Nome
        bool Ativa
    }

    FORNECEDOR {
        int Id PK
        string RazaoSocial
        string CNPJ UK
        bool Ativo
    }

    PRODUTO {
        int Id PK
        int CategoriaId FK
        int FornecedorId FK
        string Nome
        decimal Preco
        int EstoqueMinimo
        bool Ativo
    }

    ESTOQUE {
        int Id PK
        int UnidadeId FK
        int ProdutoId FK
        int Quantidade
    }

    MOVIMENTACAO_ESTOQUE {
        int Id PK
        int EstoqueId FK
        int Tipo
        int Quantidade
        datetime DataMovimentacao
    }

    VENDA {
        int Id PK
        int UnidadeId FK
        int UsuarioId FK
        datetime DataVenda
        decimal Total
    }

    ITEM_VENDA {
        int Id PK
        int VendaId FK
        int ProdutoId FK
        int Quantidade
        decimal PrecoUnitario
        decimal Subtotal
    }

    ROYALTY {
        int Id PK
        int UnidadeId FK
        int MesReferencia
        int AnoReferencia
        decimal ValorDevido
        string StatusPagamento
    }

    CHAMADO {
        int Id PK
        int UnidadeId FK
        int UsuarioId FK
        string Titulo
        int Prioridade
        int Status
        datetime DataAbertura
        datetime DataFechamento
    }
    