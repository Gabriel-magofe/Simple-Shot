import mysql.connector 
from mysql.connector import Error

# pode excluir essa função se for fazer uma pra isso, só coloquei aqui pra não dar erro 
# no servidor, mas se quiser pode excluir e fazer uma nova, 
# só não esquece de importar ela no servidor
def verificar_login(nome, senha):
    conexao = None
    cursor = None
    try:
        conexao = mysql.connector.connect(
            host='localhost',
            database='jogo_tiro_db',
            user='root',
            password=''
        )
        cursor = conexao.cursor()
        cursor.execute(
            "SELECT id FROM dados_cadastro WHERE nome = %s AND senha = %s",
            (nome, senha)
        )
        return cursor.fetchone() is not None
    except Error as e:
        print(f" Erro ao verificar login no banco de dados: {e}")
        raise
    finally:
        if cursor is not None:
            cursor.close()
        if conexao is not None and conexao.is_connected():
            conexao.close()

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
        
    
        