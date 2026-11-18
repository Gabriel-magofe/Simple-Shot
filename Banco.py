import mysql.connector 
from mysql.connector import Error

def cadastrar_usuario(nome, senha, pontos):
    try:
        
        conexao = mysql.connector.connect(
            host='localhost',
            database='jogo_tiro_db',
            user='root',
            password=''
        )

        if conexao.is_connected():
            cursor = conexao.cursor()

        comando_sql = """
            INSERT INTO dados_cadastro (nome, senha, pontos, vitorias, derrotas, kills) 
            VALUES (%s, %s, %s)
        """
        valores = (nome, senha, pontos)
        cursor.execute(comando_sql, valores)
        conexao.commit()

        print(" Usuário cadastrado com sucesso no banco de dados!")

    except Error as e:
        print(f" Erro ao conectar ou inserir no banco de dados: {e}")

    finally:
        if 'conexao' in locals() and conexao.is_connected():
            cursor.close()
            conexao.close()

if __name__ == "__main__":
    print("--- Cadastro de Novo Jogador ---")
    nome_usuario = input("Digite o nome do jogador: ")
    senha_usuario = input("Digite a senha: ")

    cadastrar_usuario(nome_usuario, senha_usuario, pontos=0)
        
    
        