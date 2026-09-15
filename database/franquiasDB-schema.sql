-- MySQL dump 10.13  Distrib 8.0.43, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: franquiasDB
-- ------------------------------------------------------
-- Server version	8.0.43

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Current Database: `franquiasDB`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `franquiasDB` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `franquiasDB`;

--
-- Table structure for table `categoria`
--

DROP TABLE IF EXISTS `categoria`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `categoria` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Nome` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Ativa` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `chamadosuporte`
--

DROP TABLE IF EXISTS `chamadosuporte`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `chamadosuporte` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Titulo` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Descricao` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Categoria` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Prioridade` int NOT NULL,
  `Status` int NOT NULL,
  `UnidadeId` int NOT NULL,
  `UsuarioAberturaId` int NOT NULL,
  `DataAbertura` datetime(6) NOT NULL,
  `DataEncerramento` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ChamadoSuporte_UnidadeId` (`UnidadeId`),
  KEY `IX_ChamadoSuporte_UsuarioAberturaId` (`UsuarioAberturaId`),
  CONSTRAINT `FK_ChamadoSuporte_UnidadeFranqueada_UnidadeId` FOREIGN KEY (`UnidadeId`) REFERENCES `unidadefranqueada` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_ChamadoSuporte_usuarios_UsuarioAberturaId` FOREIGN KEY (`UsuarioAberturaId`) REFERENCES `usuarios` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cobrancaroyalty`
--

DROP TABLE IF EXISTS `cobrancaroyalty`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cobrancaroyalty` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UnidadeId` int NOT NULL,
  `MesReferencia` int NOT NULL,
  `AnoReferencia` int NOT NULL,
  `FaturamentoTotalPeriodo` decimal(65,30) NOT NULL,
  `PercentualAplicado` decimal(65,30) NOT NULL,
  `ValorDevido` decimal(65,30) NOT NULL,
  `StatusPagamento` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_CobrancaRoyalty_UnidadeId` (`UnidadeId`),
  CONSTRAINT `FK_CobrancaRoyalty_UnidadeFranqueada_UnidadeId` FOREIGN KEY (`UnidadeId`) REFERENCES `unidadefranqueada` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `estoqueunidade`
--

DROP TABLE IF EXISTS `estoqueunidade`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `estoqueunidade` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ProdutoId` int NOT NULL,
  `UnidadeId` int NOT NULL,
  `QuantidadeSaldo` int NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_EstoqueUnidade_ProdutoId_UnidadeId` (`ProdutoId`,`UnidadeId`),
  KEY `IX_EstoqueUnidade_UnidadeId` (`UnidadeId`),
  CONSTRAINT `FK_EstoqueUnidade_Produto_ProdutoId` FOREIGN KEY (`ProdutoId`) REFERENCES `produto` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_EstoqueUnidade_UnidadeFranqueada_UnidadeId` FOREIGN KEY (`UnidadeId`) REFERENCES `unidadefranqueada` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fornecedor`
--

DROP TABLE IF EXISTS `fornecedor`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fornecedor` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `RazaoSocial` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CNPJ` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Email` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Telefone` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Ativo` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Fornecedor_CNPJ` (`CNPJ`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `franqueadora`
--

DROP TABLE IF EXISTS `franqueadora`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `franqueadora` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `NomeFantasia` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `RazaoSocial` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CNPJ` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Email` varchar(100) NOT NULL DEFAULT '',
  `Ativa` tinyint(1) NOT NULL DEFAULT '1',
  `DataCadastro` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Franqueadora_CNPJ` (`CNPJ`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `itemvenda`
--

DROP TABLE IF EXISTS `itemvenda`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `itemvenda` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `VendaId` int NOT NULL,
  `ProdutoId` int NOT NULL,
  `Quantidade` int NOT NULL,
  `PrecoUnitario` decimal(65,30) NOT NULL,
  `Subtotal` decimal(65,30) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ItemVenda_ProdutoId` (`ProdutoId`),
  KEY `IX_ItemVenda_VendaId` (`VendaId`),
  CONSTRAINT `FK_ItemVenda_Produto_ProdutoId` FOREIGN KEY (`ProdutoId`) REFERENCES `produto` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_ItemVenda_Venda_VendaId` FOREIGN KEY (`VendaId`) REFERENCES `venda` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `movimentacaoestoque`
--

DROP TABLE IF EXISTS `movimentacaoestoque`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `movimentacaoestoque` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `EstoqueUnidadeId` int NOT NULL,
  `Tipo` int NOT NULL,
  `Quantidade` int NOT NULL,
  `Motivo` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `DataMovimentacao` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_MovimentacaoEstoque_EstoqueUnidadeId` (`EstoqueUnidadeId`),
  CONSTRAINT `FK_MovimentacaoEstoque_EstoqueUnidade_EstoqueUnidadeId` FOREIGN KEY (`EstoqueUnidadeId`) REFERENCES `estoqueunidade` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `produto`
--

DROP TABLE IF EXISTS `produto`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `produto` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Nome` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Descricao` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `PrecoBase` decimal(65,30) NOT NULL,
  `EstoqueMinimo` int NOT NULL,
  `CategoriaId` int NOT NULL,
  `FornecedorId` int DEFAULT NULL,
  `Ativo` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Produto_CategoriaId` (`CategoriaId`),
  KEY `IX_Produto_FornecedorId` (`FornecedorId`),
  CONSTRAINT `FK_Produto_Categoria_CategoriaId` FOREIGN KEY (`CategoriaId`) REFERENCES `categoria` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Produto_Fornecedor_FornecedorId` FOREIGN KEY (`FornecedorId`) REFERENCES `fornecedor` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `responsavel`
--

DROP TABLE IF EXISTS `responsavel`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `responsavel` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Nome` varchar(100) NOT NULL,
  `CPF` varchar(14) NOT NULL,
  `Telefone` varchar(20) NOT NULL DEFAULT '',
  `Email` varchar(100) NOT NULL DEFAULT '',
  `Ativo` tinyint(1) NOT NULL DEFAULT '1',
  `UnidadeId` int NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Responsavel_UnidadeId_CPF` (`UnidadeId`,`CPF`),
  CONSTRAINT `FK_Responsavel_UnidadeFranqueada_UnidadeId` FOREIGN KEY (`UnidadeId`) REFERENCES `unidadefranqueada` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `unidadefranqueada`
--

DROP TABLE IF EXISTS `unidadefranqueada`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `unidadefranqueada` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `NomeUnidade` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CNPJ` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Endereco` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Cidade` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Estado` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Telefone` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `PercentualRoyalty` decimal(65,30) NOT NULL,
  `Ativa` tinyint(1) NOT NULL,
  `DataInicio` datetime(6) DEFAULT NULL,
  `FranqueadoraId` int NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_UnidadeFranqueada_CNPJ` (`CNPJ`),
  KEY `IX_UnidadeFranqueada_FranqueadoraId` (`FranqueadoraId`),
  CONSTRAINT `FK_UnidadeFranqueada_Franqueadora_FranqueadoraId` FOREIGN KEY (`FranqueadoraId`) REFERENCES `franqueadora` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `usuarios`
--

DROP TABLE IF EXISTS `usuarios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuarios` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Nome` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Email` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `SenhaHash` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `perfilId` int NOT NULL,
  `unidadeId` int DEFAULT NULL,
  `ativo` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_usuarios_Email` (`Email`),
  KEY `IX_usuarios_unidadeId` (`unidadeId`),
  CONSTRAINT `FK_usuarios_UnidadeFranqueada_unidadeId` FOREIGN KEY (`unidadeId`) REFERENCES `unidadefranqueada` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `venda`
--

DROP TABLE IF EXISTS `venda`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `venda` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UnidadeId` int NOT NULL,
  `UsuarioId` int NOT NULL,
  `DataVenda` datetime(6) NOT NULL,
  `ValorTotal` decimal(65,30) NOT NULL,
  `Status` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Venda_UnidadeId` (`UnidadeId`),
  KEY `IX_Venda_UsuarioId` (`UsuarioId`),
  CONSTRAINT `FK_Venda_UnidadeFranqueada_UnidadeId` FOREIGN KEY (`UnidadeId`) REFERENCES `unidadefranqueada` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Venda_usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping routines for database 'franquiasDB'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-09-11 20:13:38
