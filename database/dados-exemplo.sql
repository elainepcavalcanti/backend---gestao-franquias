USE franquiasDB;

-- Franqueadora de exemplo
INSERT INTO Franqueadora
    (NomeFantasia, RazaoSocial, CNPJ, Email, Ativa, DataCadastro)
SELECT
    'Rede Exemplo',
    'Rede Exemplo LTDA',
    '99999999000199',
    'contato@redeexemplo.local',
    1,
    UTC_TIMESTAMP(6)
WHERE NOT EXISTS (
    SELECT 1
    FROM Franqueadora
    WHERE CNPJ = '99999999000199'
);

SET @franqueadoraId = (
    SELECT Id
    FROM Franqueadora
    WHERE CNPJ = '99999999000199'
    LIMIT 1
);

-- Unidade de exemplo
INSERT INTO UnidadeFranqueada
    (
        NomeUnidade,
        CNPJ,
        Endereco,
        Cidade,
        Estado,
        Telefone,
        PercentualRoyalty,
        Ativa,
        DataInicio,
        FranqueadoraId
    )
SELECT
    'Unidade Exemplo',
    '88888888000188',
    'Rua de Exemplo, 100',
    'Gravata',
    'PE',
    '81999999999',
    5.00,
    1,
    UTC_TIMESTAMP(6),
    @franqueadoraId
WHERE NOT EXISTS (
    SELECT 1
    FROM UnidadeFranqueada
    WHERE CNPJ = '88888888000188'
);

SET @unidadeId = (
    SELECT Id
    FROM UnidadeFranqueada
    WHERE CNPJ = '88888888000188'
    LIMIT 1
);

-- Categoria de exemplo
INSERT INTO Categoria (Nome, Ativa)
SELECT 'Bebidas', 1
WHERE NOT EXISTS (
    SELECT 1
    FROM Categoria
    WHERE Nome = 'Bebidas'
);

SET @categoriaId = (
    SELECT Id
    FROM Categoria
    WHERE Nome = 'Bebidas'
    LIMIT 1
);

-- Fornecedor de exemplo
INSERT INTO Fornecedor
    (RazaoSocial, CNPJ, Email, Telefone, Ativo)
SELECT
    'Fornecedor Exemplo LTDA',
    '77777777000177',
    'fornecedor@exemplo.local',
    '81988888888',
    1
WHERE NOT EXISTS (
    SELECT 1
    FROM Fornecedor
    WHERE CNPJ = '77777777000177'
);

SET @fornecedorId = (
    SELECT Id
    FROM Fornecedor
    WHERE CNPJ = '77777777000177'
    LIMIT 1
);

-- Produto de exemplo
INSERT INTO Produto
    (
        Nome,
        Descricao,
        PrecoBase,
        EstoqueMinimo,
        CategoriaId,
        FornecedorId,
        Ativo
    )
SELECT
    'Agua Mineral',
    'Garrafa de agua mineral',
    4.50,
    5,
    @categoriaId,
    @fornecedorId,
    1
WHERE NOT EXISTS (
    SELECT 1
    FROM Produto
    WHERE Nome = 'Agua Mineral'
      AND CategoriaId = @categoriaId
);

SET @produtoId = (
    SELECT Id
    FROM Produto
    WHERE Nome = 'Agua Mineral'
      AND CategoriaId = @categoriaId
    LIMIT 1
);

-- Estoque inicial
INSERT INTO EstoqueUnidade
    (ProdutoId, UnidadeId, QuantidadeSaldo)
SELECT
    @produtoId,
    @unidadeId,
    20
WHERE NOT EXISTS (
    SELECT 1
    FROM EstoqueUnidade
    WHERE ProdutoId = @produtoId
      AND UnidadeId = @unidadeId
);

-- Responsável da unidade
INSERT INTO Responsavel
    (Nome, CPF, Telefone, Email, Ativo, UnidadeId)
SELECT
    'Responsavel Exemplo',
    '12345678901',
    '81977777777',
    'responsavel@exemplo.local',
    1,
    @unidadeId
WHERE NOT EXISTS (
    SELECT 1
    FROM Responsavel
    WHERE CPF = '12345678901'
      AND UnidadeId = @unidadeId
);

-- Usuário administrador de demonstração
-- E-mail: admin.exemplo@franquias.local
-- Senha de teste: Admin123!
INSERT INTO usuarios
    (Nome, Email, SenhaHash, perfilId, unidadeId, ativo)
SELECT
    'Administrador Exemplo',
    'admin.exemplo@franquias.local',
    '3EB3FE66B31E3B4D10FA70B5CAD49C7112294AF6AE4E476A1C405155D45AA121',
    0,
    NULL,
    1
WHERE NOT EXISTS (
    SELECT 1
    FROM usuarios
    WHERE Email = 'admin.exemplo@franquias.local'
);