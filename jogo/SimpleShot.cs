using System.Numerics;
// using System.Runtime.Serialization;
using Raylib_cs;
using Color = Raylib_cs.Color;

public enum Direcao
{
    Tras,
    Frente,
    Direita,
    Esquerda,
    FrenteD,
    FrenteE,
    TrasD,
    TrasE
}
class Cubo(Vector3 a, Vector3 b, int p)
{
    public Vector3 onde = a;
    public Vector3 tamanho 
    { 
        get; 
        private set
        {
            if (value.X >= 0 && value.Y >= 0 && value.Z >= 0) field = value;
        }
    } = b;
    public BoundingBox hitbox => new(onde - (tamanho /2), onde + (tamanho / 2));

    private int peso = p;
    public float queda=0f;
    public virtual void aparecer()
    {
        Raylib.DrawCubeV(onde, tamanho, new Color(245,245,245,255));
        Raylib.DrawCubeWiresV(onde, tamanho, new Color(205,205,205,205));
    }
    public void cair(float dt)
    {
        queda += 3f*dt;
        onde.Y -= queda;
    }
}
class Player : Cubo
{
    public int vida;
    public bool pulando = false;
    
    public Player(Vector3 pos, Vector3 size, int v, int p) : base(pos, size, p)
    {
        vida = v;
    }
    public void Pulo()
    {
        onde.Y += 0.2f;
    }
}
class TimeBad
{
    static Player jojo = new(new Vector3(0.0f, 1.1f, 0.0f),new Vector3(1.2f, 1.2f, 1.2f), 3, 1);

    static Direcao yo = Direcao.Frente;
    static void Main()
    {

        Raylib.InitWindow(685, 560, "Tempo Ruim (em mais de um sentido) 3D");

        Camera3D camera = new()
        {
            Up = new Vector3(0.0f, 1.0f, 0.0f),

            Target = new Vector3(0.0f, 0.0f, 0.0f),
            Projection = CameraProjection.Perspective,
            FovY = 50.0f
        };

        Vector3 cameraOffset = new(0f, 9.4f, 15.0f);

        Vector3 chao_pos = new(0.0f, 0.0f, 0.0f);
        Vector3 chao_size = new(25.0f, 0.2f, 25.0f);

        Raylib.SetTargetFPS(60);

        int mortes = 0;
        while (!Raylib.WindowShouldClose())
        {
            while (!Raylib.WindowShouldClose() && jojo.vida > 0)
            {
                if (Raylib.IsKeyDown(KeyboardKey.W)) {jojo.onde.Z -= 0.095f; yo = Raylib.IsKeyDown(KeyboardKey.D)? Direcao.FrenteD:Raylib.IsKeyDown(KeyboardKey.A)?Direcao.FrenteE:Direcao.Frente;}
                if (Raylib.IsKeyDown(KeyboardKey.S)) {jojo.onde.Z += 0.095f; yo = Raylib.IsKeyDown(KeyboardKey.D)? Direcao.TrasD:Raylib.IsKeyDown(KeyboardKey.A)?Direcao.TrasE:Direcao.Tras;}
                if (Raylib.IsKeyDown(KeyboardKey.A)) {jojo.onde.X -= 0.095f; cameraOffset.X += 0.04f; yo = Raylib.IsKeyDown(KeyboardKey.W)? Direcao.FrenteE:Raylib.IsKeyDown(KeyboardKey.S)?Direcao.TrasE:Direcao.Esquerda;}
                if (Raylib.IsKeyDown(KeyboardKey.D)) {jojo.onde.X += 0.095f; cameraOffset.X -= 0.04f; yo = Raylib.IsKeyDown(KeyboardKey.W)? Direcao.FrenteD:Raylib.IsKeyDown(KeyboardKey.S)?Direcao.TrasD:Direcao.Direita;}

                BoundingBox chao_box = new BoundingBox(chao_pos - (chao_size / 2), chao_pos + (chao_size / 2));
                

                if (Raylib.CheckCollisionBoxes(jojo.hitbox, chao_box))
                {
                    jojo.queda = -0.003f;
                    jojo.pulando = false;
                }
    
                jojo.cair();

                camera.Target = jojo.onde;
                
                camera.Position = jojo.onde + cameraOffset;

                if (jojo.onde.Y < -20) jojo.vida -= 1;

                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(48, 50, 164,255));
                Raylib.BeginMode3D(camera);

                Raylib.DrawCubeV(chao_pos, chao_size, Color.Gray);
                Raylib.DrawCubeWiresV(chao_pos, chao_size,Color.DarkGray);

                jojo.aparecer();

                Raylib.EndMode3D();
                Raylib.DrawRectangleV(new Vector2(650, 60),new Vector2(20, 20*jojo.vida), jojo.vida>2 ? Color.Lime: Color.Red);
                Raylib.DrawText($"tentativas: {mortes}", 10, 10, 20, Color.DarkGray);

                Raylib.EndDrawing();
            }
            while (!Raylib.WindowShouldClose() && jojo.vida <= 0)
            {
                if (Raylib.IsKeyDown(KeyboardKey.Space)) Reset(); 


                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                Raylib.DrawText($"Você Perdeu", 250, 220, 40, Color.White);
                Raylib.EndDrawing();
            }
            
        }
        Raylib.CloseWindow();
    }
}