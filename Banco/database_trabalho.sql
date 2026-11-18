USE jogo_tiro_db;
SHOW Tables;
SELECT * FROM dados_cadastro;
ALTER TABLE dados_cadastro ADD COLUMN vitorias int NOT NULL;
ALTER TABLE dados_cadastro ADD COLUMN derrotas int NOT NULL;
ALTER TABLE dados_cadastro ADD COLUMN killls int NOT NULL;
ALTER TABLE dados_cadastro ADD COLUMN Patentes 
