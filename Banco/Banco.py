import mysql.connector 
from mysql.connector import Error
import bcrypt

def criar_conexao():
    try:
        conexao = mysql.connector.connect(
            host='localhost',
            database='jogo_tiro_db',
            user='root',
            password=''
        )
        if conexao.is_connected():
            return conexao
    except Error as e:
        print(f"Erro ao conectar ao banco de dados: {e}")
    return None


def buscar_usuario(nome):
    conexao = criar_conexao()
    if conexao:
        try:
            cursor = conexao.cursor()
            comando_sql = "SELECT id, nome, senha, pontos, vitorias, derrotas, kills FROM dados_cadastro WHERE nome = %s"
            cursor.execute(comando_sql, (nome,))
            usuario = cursor.fetchone()
            return usuario
        except Error as e:
            print(f"Erro ao buscar usuário: {e}")
        finally:
            cursor.close()
            conexao.close()
    return None


def cadastrar_usuario(nome, senha, pontos=0, vitorias=0, derrotas=0, kills=0):
    if buscar_usuario(nome):
        print(f"O usuário '{nome}' já existe no banco de dados!")
        return

    conexao = criar_conexao()
    if conexao:
        try:
            
            senha_bytes = senha.encode('utf-8')
            salt = bcrypt.gensalt()
            senha_criptografada = bcrypt.hashpw(senha_bytes, salt)

            cursor = conexao.cursor()
            comando_sql = """
                INSERT INTO dados_cadastro (nome, senha, pontos, vitorias, derrotas, kills) 
                VALUES (%s, %s, %s, %s, %s, %s)
            """
            valores = (nome, senha_criptografada.decode('utf-8'), pontos, vitorias, derrotas, kills)
            cursor.execute(comando_sql, valores)
            conexao.commit()
            print("Usuário cadastrado com sucesso e senha criptografada!")
        except Error as e:
            print(f"Erro ao cadastrar usuário: {e}")
        finally:
            cursor.close()
            conexao.close()


def verificar_login(nome, senha):
    usuario = buscar_usuario(nome)
    if not usuario:
        print("Usuário não encontrado!")
        return False

    senha_armazenada = usuario[2].encode('utf-8')
    senha_digitada = senha.encode('utf-8')
    
    if bcrypt.checkpw(senha_digitada, senha_armazenada):
        print(f"Login bem-sucedido! Bem-vindo de volta, {nome}!")
        return True
    else:
        print("Senha incorreta!")
        return False


def adicionar_pontos(nome, pontos_ganhos):
    conexao = criar_conexao()
    if conexao:
        try:
            cursor = conexao.cursor()
            comando_sql = "UPDATE dados_cadastro SET pontos = pontos + %s WHERE nome = %s"
            cursor.execute(comando_sql, (pontos_ganhos, nome))
            conexao.commit()
            print(f" +{pontos_ganhos} pontos adicionados para {nome}!")
        except Error as e:
            print(f"Erro ao adicionar pontos: {e}")
        finally:
            cursor.close()
            conexao.close()


def registrar_kill(nome):
    conexao = criar_conexao()
    if conexao:
        try:
            cursor = conexao.cursor()
            comando_sql = "UPDATE dados_cadastro SET kills = kills + 1 WHERE nome = %s"
            cursor.execute(comando_sql, (nome,))
            conexao.commit()
            print(f"Kill registrada para {nome}!")
        except Error as e:
            print(f"Erro ao registrar kill: {e}")
        finally:
            cursor.close()
            conexao.close()


def registrar_vitoria(nome):
    conexao = criar_conexao()
    if conexao:
        try:
            cursor = conexao.cursor()
            comando_sql = "UPDATE dados_cadastro SET vitorias = vitorias + 1 WHERE nome = %s"
            cursor.execute(comando_sql, (nome,))
            conexao.commit()
            print(f"Vitória registrada para {nome}!")
        except Error as e:
            print(f"Erro ao registrar vitória: {e}")
        finally:
            cursor.close()
            conexao.close()


def registrar_derrota(nome):
    conexao = criar_conexao()
    if conexao:
        try:
            cursor = conexao.cursor()
            comando_sql = "UPDATE dados_cadastro SET derrotas = derrotas + 1 WHERE nome = %s"
            cursor.execute(comando_sql, (nome,))
            conexao.commit()
            print(f"Derrota registrada para {nome}.")
        except Error as e:
            print(f"Erro ao registrar derrota: {e}")
        finally:
            cursor.close()
            conexao.close()


if __name__ == "__main__":
    print("--- Testando Sistema do Jogo ---")
    escolha = input("Deseja (1) Cadastrar ou (2) Logar? Digite 1 ou 2: ")
    
    if escolha == "1":
        n = input("Nome: ")
        s = input("Senha: ")
        cadastrar_usuario(n, s)
    elif escolha == "2":
        n = input("Nome: ")
        s = input("Senha: ")
        if verificar_login(n, s):
            registrar_kill(n)
            adicionar_pontos(n, 100)