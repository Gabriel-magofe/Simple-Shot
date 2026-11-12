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
    public float saut_carga = 0;
    public virtual void aparecer()
    {
        Raylib.DrawCubeV(onde, tamanho, new Color(245,245,245,255));
        Raylib.DrawCubeWiresV(onde, tamanho, new Color(205,205,205,205));
    }
    public void cair(float dt)
    {
        queda += 3f*dt;
        if (queda > 12f*dt) queda = 12f*dt; 
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
    public void Pulo(float dt)
    {
        onde.Y += 4f*dt;
    }
    public override void aparecer()
    {
        float halfWidth = tamanho.X / 2.0f;
        float halfDepth = tamanho.Z / 2.0f;
        float halfHeight = tamanho.Y / 2.0f;
        float radius = MathF.Sqrt(halfWidth * halfWidth + halfDepth * halfDepth);

        Vector3 start = new Vector3(onde.X, onde.Y - halfHeight, onde.Z);
        Vector3 end = new Vector3(onde.X, onde.Y + halfHeight, onde.Z);

        // Raylib.DrawCubeV(onde, tamanho, new Color(245,245,245,255));
        // Raylib.DrawCubeWiresV(onde, tamanho, new Color(205,205,205,205));
        Raylib.DrawCapsule(start,end,radius,16,8,Color.Gray);
        Raylib.DrawCapsuleWires(start,end,radius,16,8,Color.DarkGray);
    }
}
class TimeBad
{
    static Player jojo = new(new Vector3(0.0f, 2.8f, 0.0f),new Vector3(1.2f, 2.5f, 1.2f), 3, 1);

    static Direcao yo = Direcao.Frente;
    static void Main()
    {

        Raylib.InitWindow(685, 560, "SimpleShot");

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
                float dt = Raylib.GetFrameTime();
                if (Raylib.IsKeyDown(KeyboardKey.W)) {jojo.onde.Z -= 9.5f*dt; yo = Raylib.IsKeyDown(KeyboardKey.D)? Direcao.FrenteD:Raylib.IsKeyDown(KeyboardKey.A)?Direcao.FrenteE:Direcao.Frente;}
                if (Raylib.IsKeyDown(KeyboardKey.S)) {jojo.onde.Z += 9.5f*dt; yo = Raylib.IsKeyDown(KeyboardKey.D)? Direcao.TrasD:Raylib.IsKeyDown(KeyboardKey.A)?Direcao.TrasE:Direcao.Tras;}
                if (Raylib.IsKeyDown(KeyboardKey.A)) {jojo.onde.X -= 9.5f*dt; cameraOffset.X += 0.04f; yo = Raylib.IsKeyDown(KeyboardKey.W)? Direcao.FrenteE:Raylib.IsKeyDown(KeyboardKey.S)?Direcao.TrasE:Direcao.Esquerda;}
                if (Raylib.IsKeyDown(KeyboardKey.D)) {jojo.onde.X += 9.5f*dt; cameraOffset.X -= 0.04f; yo = Raylib.IsKeyDown(KeyboardKey.W)? Direcao.FrenteD:Raylib.IsKeyDown(KeyboardKey.S)?Direcao.TrasD:Direcao.Direita;}
                if (Raylib.IsKeyDown(KeyboardKey.Space))
                {
                    
                }

                BoundingBox chao_box = new BoundingBox(chao_pos - (chao_size / 2), chao_pos + (chao_size / 2));
                
                jojo.cair(dt);
                if (Raylib.CheckCollisionBoxes(jojo.hitbox, chao_box) && jojo.queda >= 0)
                {
                    jojo.onde.Y = chao_box.Max.Y + (jojo.tamanho.Y / 2.0f);
                    jojo.queda = 0f;
                    jojo.pulando = false;
                }

                camera.Target = jojo.onde;
                
                camera.Position = jojo.onde + cameraOffset;

                if (jojo.onde.Y < -20) jojo.vida -= 1;

                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(48, 50, 164,255));
                Raylib.BeginMode3D(camera);

                Raylib.DrawCubeV(chao_pos, chao_size, Color.White);
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
    public static void Reset()
    {
        jojo = new(new Vector3(0.0f, 2.8f, 0.0f),new Vector3(1.2f, 2.5f, 1.2f), 3, 1);
        yo = Direcao.Frente;
    }
}