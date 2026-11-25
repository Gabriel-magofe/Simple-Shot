# esse código é 1000% IA eé só pra eu testar enquanto o jogo não envia dados:

import argparse
import getpass
import secrets
import socket
import sys


class TesteServidor:
    def __init__(self, host, porta_tcp, porta_udp, timeout):
        self.endereco_tcp = (host, porta_tcp)
        self.endereco_udp = (host, porta_udp)
        self.timeout = timeout
        self.jogadores = []
        self.testes = 0

    def verificar(self, condicao, descricao):
        if not condicao:
            raise AssertionError(descricao)
        self.testes += 1
        print(f"OK - {descricao}")

    def tcp(self, mensagem):
        with socket.create_connection(self.endereco_tcp, timeout=self.timeout) as conexao:
            conexao.settimeout(self.timeout)
            conexao.sendall(mensagem.encode("utf-8"))
            conexao.shutdown(socket.SHUT_WR)
            resposta = bytearray()
            while True:
                trecho = conexao.recv(1024)
                if not trecho:
                    return resposta.decode("utf-8")
                resposta.extend(trecho)

    def login(self, nome, senha):
        resposta = self.tcp(f"LOGIN:{nome}:{senha}")
        if not resposta.startswith("LOGIN_OK:"):
            raise AssertionError(f"Login esperado com sucesso; servidor respondeu {resposta!r}")
        token = resposta.partition(":")[2]
        if not token:
            raise AssertionError("O servidor respondeu LOGIN_OK sem token.")
        return token

    def novo_udp(self):
        cliente = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        cliente.settimeout(self.timeout)
        return cliente

    def receber(self, cliente):
        dados, _ = cliente.recvfrom(1024)
        return dados.decode("utf-8")

    def esperar_resposta(self, cliente, esperada, descricao):
        recebida = self.receber(cliente)
        self.verificar(recebida == esperada, f"{descricao} (esperado {esperada!r}, recebido {recebida!r})")

    def conectar_jogador(self, token, esperada="200"):
        cliente = self.novo_udp()
        cliente.sendto(f"CONNECT:{token}".encode("utf-8"), self.endereco_udp)
        try:
            self.esperar_resposta(cliente, esperada, f"CONNECT retorna {esperada}")
        except (AssertionError, OSError, UnicodeError):
            cliente.close()
            raise
        if esperada == "200":
            self.jogadores.append((cliente, token))
        else:
            cliente.close()
        return cliente

    def enviar_e_esperar(self, cliente, mensagem, esperada, descricao):
        cliente.sendto(mensagem.encode("utf-8"), self.endereco_udp)
        self.esperar_resposta(cliente, esperada, descricao)

    def sem_mensagem(self, cliente, descricao):
        timeout_original = cliente.gettimeout()
        cliente.settimeout(0.25)
        try:
            dados, _ = cliente.recvfrom(1024)
        except socket.timeout:
            self.verificar(True, descricao)
        else:
            self.verificar(False, f"{descricao}; chegou {dados!r}")
        finally:
            cliente.settimeout(timeout_original)

    def desconectar(self, cliente, token):
        self.enviar_e_esperar(
            cliente,
            f"DISCONNECT:{token}",
            "200",
            "desconexão autorizada"
        )
        self.jogadores = [
            jogador for jogador in self.jogadores
            if jogador[0] is not cliente
        ]

    def testar_login(self, nome, senha):
        resposta = self.tcp("LOGIN")
        self.verificar(resposta == "LOGIN_INVALIDO", "login TCP com formato inválido é recusado")

        senha_incorreta = secrets.token_urlsafe(24)
        resposta = self.tcp(f"LOGIN:{nome}:{senha_incorreta}")
        self.verificar(resposta == "LOGIN_NEGADO", "senha incorreta é recusada")

        token = self.login(nome, senha)
        self.verificar(bool(token), "login válido retorna token UDP")
        return token

    def testar_udp_e_eventos(self, nome, senha, token_um):
        desconhecido = self.novo_udp()
        self.enviar_e_esperar(
            desconhecido,
            "CONNECT:token-invalido",
            "401",
            "token UDP desconhecido é recusado"
        )
        desconhecido.close()

        jogador_um = self.conectar_jogador(token_um)

        repetido = self.novo_udp()
        self.enviar_e_esperar(
            repetido,
            f"CONNECT:{token_um}",
            "401",
            "token já utilizado não pode ser reutilizado por outro cliente"
        )
        repetido.close()

        resposta = self.login(nome, senha)
        jogador_dois = self.conectar_jogador(resposta)

        self.enviar_e_esperar(
            jogador_um,
            "CONNECT:token-invalido",
            "401",
            "cliente conectado não pode trocar seu token"
        )
        self.enviar_e_esperar(
            jogador_um,
            "DISCONNECT:token-invalido",
            "401",
            "token incorreto não desconecta o jogador"
        )
        self.enviar_e_esperar(
            jogador_um,
            "COMANDO:teste",
            "400",
            "mensagem UDP desconhecida é recusada"
        )
        self.enviar_e_esperar(
            jogador_um,
            "STATE",
            "400",
            "mensagem UDP sem conteúdo é recusada"
        )

        eventos = (
            ("STATE", "posicao=1,2,3", f"STATE:{nome}:posicao=1,2,3"),
            ("SHOT", "inicio=0,0,0;fim=4,0,0", f"SHOT:{nome}:inicio=0,0,0;fim=4,0,0"),
            ("HIT", "alvo=jogador;dano=1", f"HIT:{nome}:alvo=jogador;dano=1"),
            ("KILL", "alvo=jogador", f"KILL:{nome}:alvo=jogador"),
        )
        for tipo, conteudo, esperado in eventos:
            jogador_um.sendto(f"{tipo}:{conteudo}".encode("utf-8"), self.endereco_udp)
            self.esperar_resposta(
                jogador_dois,
                esperado,
                f"{tipo} é retransmitido com o nome do remetente"
            )
        self.sem_mensagem(jogador_um, "eventos não voltam ao remetente")

        nao_conectado = self.novo_udp()
        self.enviar_e_esperar(
            nao_conectado,
            "STATE:posicao=9,9,9",
            "401",
            "cliente não conectado não pode enviar estado"
        )
        nao_conectado.close()

        self.desconectar(jogador_um, token_um)
        self.enviar_e_esperar(
            jogador_um,
            "STATE:posicao=9,9,9",
            "401",
            "jogador desconectado não pode continuar enviando"
        )
        self.sem_mensagem(jogador_dois, "mensagem de jogador desconectado não é retransmitida")
        jogador_um.close()

        token_reutilizado = self.novo_udp()
        self.enviar_e_esperar(
            token_reutilizado,
            f"CONNECT:{token_um}",
            "401",
            "token continua inválido depois da desconexão"
        )
        token_reutilizado.close()

        self.desconectar(jogador_dois, resposta)
        jogador_dois.close()

    def testar_limite(self, nome, senha, limite):
        for indice in range(limite):
            token = self.login(nome, senha)
            self.conectar_jogador(token)

        token_extra = self.login(nome, senha)
        self.conectar_jogador(token_extra, "503")
        self.verificar(
            len(self.jogadores) == limite,
            f"servidor aceita {limite} jogadores e recusa o próximo"
        )
        cliente, token = self.jogadores[0]
        self.desconectar(cliente, token)
        cliente.close()

        cliente_extra = self.conectar_jogador(token_extra)
        self.desconectar(cliente_extra, token_extra)
        cliente_extra.close()
        self.verificar(
            len(self.jogadores) == limite - 1,
            "vaga liberada pode ser ocupada e o token pendente é consumido"
        )

    def executar(self, nome, senha, limite):
        token = self.testar_login(nome, senha)
        self.testar_udp_e_eventos(nome, senha, token)
        self.testar_limite(nome, senha, limite)
        print(f"\nTodos os {self.testes} testes passaram.")

    def limpar(self):
        for cliente, token in self.jogadores:
            try:
                cliente.sendto(f"DISCONNECT:{token}".encode("utf-8"), self.endereco_udp)
                cliente.settimeout(0.5)
                cliente.recvfrom(1024)
            except OSError:
                pass
            finally:
                cliente.close()
        self.jogadores.clear()


def main():
    parser = argparse.ArgumentParser(
        description="Testa login TCP e o protocolo UDP do servidor Simple Shot."
    )
    parser.add_argument("--host", default="127.0.0.1", help="Endereço do servidor (padrão: 127.0.0.1)")
    parser.add_argument("--tcp-port", type=int, default=5006)
    parser.add_argument("--udp-port", type=int, default=5005)
    parser.add_argument("--user", required=True, help="Nome de uma conta já cadastrada no banco")
    parser.add_argument("--limit", type=int, default=50, help="Limite configurado no servidor (padrão: 50)")
    parser.add_argument("--timeout", type=float, default=3.0, help="Timeout de rede em segundos")
    args = parser.parse_args()

    if args.limit < 1:
        parser.error("--limit precisa ser pelo menos 1")

    senha = getpass.getpass("Senha da conta: ")
    teste = TesteServidor(args.host, args.tcp_port, args.udp_port, args.timeout)
    try:
        teste.executar(args.user, senha, args.limit)
    except (AssertionError, OSError, UnicodeError) as erro:
        print(f"\nFALHA: {erro}", file=sys.stderr)
        return 1
    finally:
        teste.limpar()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
