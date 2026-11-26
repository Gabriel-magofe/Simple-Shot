import os
import sys
import socket
import secrets
import threading
from mysql.connector import Error
PATH = os.path.dirname(os.path.abspath(__file__))
sys.path.append(os.path.dirname(PATH))
from Banco import Banco

HOST = '0.0.0.0'
PORT_UDP = 5005
PORT_TCP = 5006
s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
s.bind((HOST, PORT_UDP))
print(f"Rodando na porta {PORT_UDP}...")  
     
gamers = {}
tokens_ativos = {}
tokens_pendentes = {}
tokens_lock = threading.Lock()
LIMITE= 50

def verify_new_player(addr_cli, token):
    if addr_cli in tokens_ativos:
        print(f"Cliente {addr_cli} já está conectado.")
        status = 200 if tokens_ativos[addr_cli] == token else 401
        s.sendto(str(status).encode('utf-8'), addr_cli)
        return status
    if len(gamers) >= LIMITE:
        print(f"Máximo de jogadores atingido. Conexão de {addr_cli} recusada.")
        s.sendto(b"503", addr_cli)
        return 503
    with tokens_lock:
        nome = tokens_pendentes.pop(token, None)
        if nome is None:
            s.sendto(b"401", addr_cli)
            return 401

        gamers[addr_cli] = nome
        tokens_ativos[addr_cli] = token
    s.sendto(b"200", addr_cli)
    print(f"Conexão aceita de {addr_cli} com jogador {nome}.")
    return 200

def player_out(addr_cli, token):
    if addr_cli in gamers:
        if tokens_ativos.get(addr_cli) != token:
            s.sendto(b"401", addr_cli)
            return 401
        print(f"Jogador {gamers[addr_cli]} desconectado.")
        del gamers[addr_cli]
        del tokens_ativos[addr_cli]
        s.sendto(b"200", addr_cli)
        return 200
    else:
        print(f"Cliente {addr_cli} não encontrado entre os jogadores conectados.")
        s.sendto(b"404", addr_cli)
        return 404    

def servidor_login():
    s_tcp = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s_tcp.bind((HOST, PORT_TCP))
    s_tcp.listen(5)
    print(f"Login rodando na porta {PORT_TCP}...")
    while True:
        conn, addr = s_tcp.accept()
        print(f"Conexão de login recebida de {addr}")
        with conn:
            data = conn.recv(1024).decode('utf-8').strip()
            comando, separador, credenciais = data.partition(":")
            nome, separador_nome, senha = credenciais.partition(":")
            credenciais_validas = (
                separador
                and separador_nome
                and nome
                and senha
                and len(nome) <= 20
                and ":" not in nome
                and len(senha.encode('utf-8')) <= 72
            )
            if comando not in ("LOGIN", "REGISTER") or not credenciais_validas:
                resposta = "REGISTER_INVALIDO" if comando == "REGISTER" else "LOGIN_INVALIDO"
                conn.sendall(resposta.encode('utf-8'))
                continue
            try:
                if comando == "REGISTER":
                    if Banco.buscar_usuario(nome):
                        conn.sendall(b"REGISTER_EXISTE")
                    elif Banco.cadastrar_usuario(nome, senha):
                        conn.sendall(b"REGISTER_OK")
                    else:
                        conn.sendall(b"REGISTER_ERRO")
                    continue

                login_valido = Banco.verificar_login(nome, senha)
            except Error as e:
                print(f"Erro ao verificar login: {e}")
                conn.sendall(b"REGISTER_ERRO" if comando == "REGISTER" else b"LOGIN_ERRO")
                continue
            if not login_valido:
                conn.sendall(b"LOGIN_NEGADO")
                continue
            token = secrets.token_urlsafe(24)
            with tokens_lock:
                tokens_pendentes[token] = nome
            conn.sendall(f"LOGIN_OK:{token}".encode('utf-8'))
thread_login = threading.Thread(target=servidor_login, daemon=True)
thread_login.start()


while True: 
    try:
        
        dados, addr_cli = s.recvfrom(1024)
        d_traduzido = dados.decode('utf-8')
        
        if d_traduzido.startswith("CONNECT:"):
            verify_new_player(addr_cli, d_traduzido.removeprefix("CONNECT:"))
            continue
        if d_traduzido.startswith("DISCONNECT:"):
            player_out(addr_cli, d_traduzido.removeprefix("DISCONNECT:"))
            continue

        if addr_cli not in gamers:
            s.sendto(b"401", addr_cli)
            continue

        tipo, separador, conteudo = d_traduzido.partition(":")
        if tipo not in ("STATE", "SHOT", "HIT", "KILL") or not separador:
            s.sendto(b"400", addr_cli)
            continue

        mensagem = f"{tipo}:{gamers[addr_cli]}:{conteudo}".encode('utf-8')
        for endereco in gamers:
            if endereco != addr_cli:
                s.sendto(mensagem, endereco)
    except KeyboardInterrupt:
        print("\nDesligando...")
        break

s.close()