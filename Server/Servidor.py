import os
import sys
import socket
import math
import time as t
import threading
PATH = os.path.dirname(os.path.abspath(__file__))
sys.path.append(os.path.dirname(PATH))
from Banco import Banco


HOST = '0.0.0.0'
PORT_UDP = 5005
PORT_TCP = 5006
s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
s.bind((HOST, PORT_UDP))
        
def servidor_login():
    s_tcp = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s_tcp.bind((HOST, PORT_TCP))
    s_tcp.listen(5)
    print(f"Login rodando na porta {PORT_TCP}...")
    while True:
        conn, addr = s_tcp.accept()
        print(f"Conexão de login recebida de {addr}")
        data = conn.recv(1024).decode('utf-8')
        if data:
            response = Banco.verificar_login(data)
            conn.sendall(response.encode('utf-8'))
        conn.close()
thread_login = threading.Thread(target=servidor_login, daemon=True)
thread_login.start()

gamers = {}
partida = {}
tokens_ativos = {}
intervalo = 0.05
MAXJGDRS = 50
VDMAX = 1000
DNTIRO = 50
FREQMSGPS = 20
TMNHAREN = 1000
nE= 0

print(f"Rodando na porta {PORT_UDP}...")

def verify_new_player(addr_cli, d_traduzido):
    token = d_traduzido.removeprefix("CONNECT:")
    if addr_cli in tokens_ativos:
        print(f"Cliente {addr_cli} já está conectado.")
        s.sendto("200", addr_cli)
        return 200
    if len(gamers) >= MAXJGDRS:
        print(f"Máximo de jogadores atingido. Conexão de {addr_cli} recusada.")
        s.sendto("503", addr_cli)
        return 503
    if token in tokens_ativos.values():
        print(f"Token/Nome de jogador '{token}' já em uso. Conexão de {addr_cli} recusada.")
        s.sendto("409", addr_cli)
        return 409
    s.sendto("200", addr_cli)
    print(f"Conexão aceita de {addr_cli} com token {token}.")
    
    gamers[addr_cli] = f"Jogador_{token}"
    tokens_ativos[addr_cli] = token
    Banco.cadastrar_usuario(token)
    return 200

def player_out(addr_cli):
    if addr_cli in gamers:
        print(f"Jogador {gamers[addr_cli]} desconectado.")
        del gamers[addr_cli]
        del tokens_ativos[addr_cli]
        return 200
    else:
        print(f"Cliente {addr_cli} não encontrado entre os jogadores conectados.")
        return 404
# def verify_shot(partida):
    

while True: 
    try:
        
        dados, addr_cli = s.recvfrom(1024)
        d_traduzido = dados.decode('utf-8')
        
        if d_traduzido.startswith("CONNECT:"):
            status = verify_new_player(addr_cli, d_traduzido)
            if status != 200:
                continue
        if d_traduzido.startswith("DISCONNECT:"):
            player_out(addr_cli)
        
        estado = d_traduzido
        partida[f'estado{nE}'] = estado
        nE += 1
        t.sleep(intervalo)
        continue
    except KeyboardInterrupt:
        print("\nDesligando...")
        break

s.close()
