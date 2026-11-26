import mysql.connector 
from mysql.connector import Error
import bcrypt  

def cadastrar_usuario(nome, senha_pura, pontos, vitorias, derrotas, kills):
    try:
        
        senha_bytes = senha_pura.encode('utf-8')
        senha_hash = bcrypt.hashpw(senha_bytes, bcrypt.gensalt()).decode('utf-8')
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
                VALUES (%s, %s, %s, %s, %s, %s)
            """
            valores = (nome, senha_hash, pontos, vitorias, derrotas, kills)
            
            cursor.execute(comando_sql, valores)
            conexao.commit()

            print("\n Usuário cadastrado com sucesso e senha protegida com hash!")

    except Error as e:
        print(f"\nErro ao conectar ou inserir no banco de dados: {e}")

    finally:
        if 'conexao' in locals() and conexao.is_connected():
            cursor.close()
            conexao.close()

if __name__ == "__main__":
    print("--- Cadastro de Novo Jogador ---")
    nome_usuario = input("Digite o nome do jogador: ")
    senha_usuario = input("Digite a senha: ")

    cadastrar_usuario(nome_usuario, senha_usuario, pontos=0, vitorias=0, derrotas=0, kills=0)