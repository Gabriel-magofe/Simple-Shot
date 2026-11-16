using System.Numerics;
// using System.Runtime.Intrinsics.Arm;

// using System.Runtime.Serialization;
using Raylib_cs;
using Color = Raylib_cs.Color;

public enum Direcao
{
    Frente,
    FrenteD,
    Direita,
    TrasD,
    Tras,
    TrasE,
    Esquerda,
    FrenteE
}

class Cubo(Vector3 a, Vector3 b)
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


    public virtual void aparecer()
    {
        Raylib.DrawCubeV(onde, tamanho, Color.Gray);
        Raylib.DrawCubeWiresV(onde, tamanho, Color.DarkGray);
    }
    
}
class Player : Cubo
{
    public Direcao olhando = Direcao.Frente;
    public int arm = 1;
    public int vida;
    public bool pulando = false;
    public bool sautCarregando = false;
    public float queda=0f;
    public float saut_carga = 45f;
    
    public Player(Vector3 pos, Vector3 size, int v) : base(pos, size)
    {
        vida = v;
    }
    public void Pulo(float dt)
    {
        onde.Y += saut_carga*dt;
    }
    public void cair(float dt)
    {
        queda += 3f*dt;
        // if (queda > 12f*dt) queda = 12f*dt; 
        onde.Y -= queda;
    }
    public void mover(float dt, int dir)
    {

        Vector3 frente = olhando switch
        {
            Direcao.Frente => new(0f, 0f, -1f),
            Direcao.FrenteD => Vector3.Normalize(new(1f, 0f, -1f)),
            Direcao.Direita => new(1f, 0f, 0f),
            Direcao.TrasD => Vector3.Normalize(new(1f, 0f, 1f)),
            Direcao.Tras => new(0f, 0f, 1f),
            Direcao.TrasE => Vector3.Normalize(new(-1f, 0f, 1f)),
            Direcao.Esquerda => new(-1f, 0f, 0f),
            Direcao.FrenteE => Vector3.Normalize(new(-1f, 0f, -1f)),
            _ => new(0f, 0f, -1f)
        };

        Vector3 direita = new(-frente.Z, 0f, frente.X);

        Vector3 direcao = dir switch
        {
            0 => frente,
            1 => -frente,
            2 => -direita,
            3 => direita,
            _ => Vector3.Zero
        };

        onde += direcao * 10f * dt;
    }
    public override void aparecer()
    {
        float halfWidth = tamanho.X / 2.0f;
        float halfDepth = tamanho.Z / 2.0f;
        float halfHeight = tamanho.Y / 2.0f;
        float radius = MathF.Sqrt(halfWidth * halfWidth + halfDepth * halfDepth);

        Vector3 start = new Vector3(onde.X, onde.Y - halfHeight, onde.Z);
        Vector3 end = new Vector3(onde.X, onde.Y + halfHeight, onde.Z);

        
        if (arm == 1)
        {
            float angulo = olhando switch
            {
                Direcao.Frente => 0f,
                Direcao.FrenteD => 315f,
                Direcao.Direita => 270f,
                Direcao.TrasD => 225f,
                Direcao.Tras => 180f,
                Direcao.TrasE => 135f,
                Direcao.Esquerda => 90f,
                Direcao.FrenteE => 45f,
                _ => 0f
            };

            Rlgl.PushMatrix();

            Rlgl.Translatef(onde.X, onde.Y, onde.Z);

            Rlgl.Rotatef(angulo, 0.0f, 1.0f, 0.0f);

            Raylib.DrawCubeV(new Vector3(0.9f, 0f, -0.8f),new Vector3(0.3f, 0.3f, 1.8f), Color.Black);
            Raylib.DrawCubeWiresV(new Vector3(0.9f, 0f, -0.8f), new Vector3(0.3f, 0.3f, 1.8f), Color.DarkGray);

            Rlgl.PopMatrix();
        }
                        
        Raylib.DrawCapsule(start,end,radius,16,8, new Color(190,190,190,125));
        Raylib.DrawCapsuleWires(start,end,radius,8,8,Color.DarkGray);
    }
}
class SimpleShot
{
    static Player jojo = new(new Vector3(0.0f, 2.8f, 0.0f),new Vector3(1.2f, 2.5f, 1.2f), 3);

    static void Main()
    {

        Raylib.InitWindow(900, 750, "SimpleShot");

        Camera3D camera = new()
        {
            Up = new Vector3(0.0f, 1.0f, 0.0f),
            
            Target = new Vector3(0.0f, 0.0f, 0.0f),
            Projection = CameraProjection.Perspective,
            FovY = 50.0f
        };

        Vector3 cameraOffset;
        Vector3 joOffset;
        
        Cubo chao = new(new(0.0f, 0.0f, 0.0f),new(25.0f, 0.2f, 25.0f));
        // float dano = 0;
        Raylib.SetTargetFPS(60);
        while (!Raylib.WindowShouldClose())
        {
            while (!Raylib.WindowShouldClose() && jojo.vida > 0)
            {
                float dt = Raylib.GetFrameTime();
                // dano += 1 * dt;
                // if (dano> 1){jojo.vida -= 1;dano = 0;}
                jojo.cair(dt);
                if (Raylib.CheckCollisionBoxes(jojo.hitbox, chao.hitbox) && jojo.queda >= 0 )
                {
                    if (!jojo.pulando) jojo.onde.Y = chao.hitbox.Max.Y + (jojo.tamanho.Y / 2.0f);
                    jojo.queda = 0f;
                    jojo.pulando = false;
                    if (!jojo.sautCarregando) jojo.saut_carga =45f;
                }
                
                if (Raylib.IsKeyPressed(KeyboardKey.Left)) jojo.olhando = jojo.olhando - 1 >= 0?jojo.olhando - 1:Direcao.FrenteE;
                if (Raylib.IsKeyPressed(KeyboardKey.Right)) jojo.olhando = jojo.olhando + 1 <= Direcao.FrenteE?jojo.olhando + 1:Direcao.Frente;

                if (Raylib.IsKeyDown(KeyboardKey.W)) jojo.mover(dt, 0);
                if (Raylib.IsKeyDown(KeyboardKey.S)) jojo.mover(dt, 1);
                if (Raylib.IsKeyDown(KeyboardKey.A)) jojo.mover(dt, 2);
                if (Raylib.IsKeyDown(KeyboardKey.D)) jojo.mover(dt, 3);
                if (Raylib.IsKeyDown(KeyboardKey.Space) && !jojo.pulando)
                {
                    jojo.sautCarregando = true;
                    jojo.saut_carga += 16f*dt;
                    if (jojo.saut_carga > 80f) jojo.saut_carga = 80f;
                    
                }
                else if (Raylib.IsKeyReleased(KeyboardKey.Space)) {jojo.pulando = true; jojo.sautCarregando = false;}
                if (jojo.pulando) jojo.Pulo(dt);

                joOffset = jojo.olhando switch
                {
                    Direcao.Frente => new(0f, 0f, -7f),
                    Direcao.FrenteD => new(5f, 0f, -5.0f),
                    Direcao.Direita => new(7f, 0f, 0.0f),
                    Direcao.TrasD => new(5f, 0f, 5.0f),
                    Direcao.Tras => new(0f, 0f, 7.0f),
                    Direcao.TrasE => new(-5f, 0f, 5.0f),
                    Direcao.Esquerda => new(-7f, 0f, 0.0f),
                    Direcao.FrenteE => new(-5f, 0f, -5.0f),     
                    _ => new(0f, 0f, -7.0f)
                };

                cameraOffset = jojo.olhando switch
                {
                    Direcao.Frente => new(0f, 3f, 6f),
                    Direcao.FrenteD => new(-4.24f, 3f, 4.24f),
                    Direcao.Direita => new(-6f, 3f, 0.0f),
                    Direcao.TrasD => new(-4.24f, 3f, -4.24f),
                    Direcao.Tras => new(0f, 3f, -6.0f),
                    Direcao.TrasE => new(4.24f, 3f, -4.24f),
                    Direcao.Esquerda => new(6f, 3f, 0.0f),
                    Direcao.FrenteE => new(4.24f, 3f, 4.24f),     
                    _ => new(0f, 3f, 4.24f)
                };

                camera.Target = jojo.onde + joOffset;
                
                camera.Position = jojo.onde + cameraOffset;

                // Raylib.UpdateCamera(ref camera, CameraMode.Orbital);

                if (jojo.onde.Y < -40) jojo.vida -= 1;

                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(48, 50, 164,255));
                Raylib.BeginMode3D(camera);

                chao.aparecer();

                jojo.aparecer();

                Raylib.EndMode3D();
                Raylib.DrawRectangleV(new Vector2(848, 58 ),new Vector2(24, 94), Color.Black);
                Raylib.DrawRectangleV(new Vector2(850, 60 + (30*3 - 30*jojo.vida)),new Vector2(20, 30*jojo.vida), jojo.vida>1 ? Color.Lime: Color.Red);
                Raylib.DrawText($"X: {(int)jojo.onde.X};Y: {(int)jojo.onde.Y};Z: {(int)jojo.onde.Z}", 10, 10, 20, Color.DarkGray);
                // Raylib.DrawText($"X: {jojo.olhando}", 10, 10, 20, Color.DarkGray);

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
        jojo = new(new Vector3(0.0f, 2.8f, 0.0f), new Vector3(1.2f, 2.5f, 1.2f), 3)
        {
            olhando = Direcao.Frente
        };
    }
}