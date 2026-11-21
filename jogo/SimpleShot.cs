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

class Cubo(Vector3 a, Vector3 b, int tipo=1)
{
    public int tipo = tipo;
    public Vector3 onde = a;
    public Vector3 tamanho 
    { 
        get; 
        private set
        {
            if (value.X >= 0 && value.Y >= 0 && value.Z >= 0) field = value;
        }
    } = b;
    public virtual BoundingBox[] hitbox => [new(onde - (tamanho /2), onde + (tamanho / 2))];


    public virtual void aparecer()
    {
        Raylib.DrawCubeV(onde, tamanho, tipo==1?Color.Gray:tipo==2?new(105,100,83,255):new(170,170,170,255));
        Raylib.DrawCubeWiresV(onde, tamanho, Color.Black);
    }
    public static float Distancia(BoundingBox a, BoundingBox b)
    {
        float dx = MathF.Max(0.0f, MathF.Max(a.Min.X - b.Max.X, b.Min.X - a.Max.X));
        float dz = MathF.Max(0.0f, MathF.Max(a.Min.Z - b.Max.Z, b.Min.Z - a.Max.Z));

        
        return MathF.Sqrt(dx * dx + dz * dz);
    }
    // public static float DistanciaY(BoundingBox a, BoundingBox b)
    // {
    //     float dy = MathF.Max(0.0f, MathF.Max(a.Min.Y - b.Max.Y, b.Min.Y - a.Max.Y));
        
    //     return MathF.Sqrt(dy * dy);
    // }
    
}
class Player : Cubo
{
    public override BoundingBox[] hitbox => [new(onde - (tamanho /2), onde + (tamanho / 2)), new(new Vector3(onde.X,onde.Y+0.07f,onde.Z) - (tamanho /2), onde + (tamanho / 2))];
    public Direcao olhando = Direcao.Frente;
    Vector3 frente => olhando switch
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

    Vector3 direita => new(-frente.Z, 0f, frente.X);
   public int arm = 1;
    public Vector3 cord_de_ini;
    public Vector3 cord_de_fim;

    public bool atirando = false;
    public float munição = 3;
    public float golpear = 1;
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
        queda += 190f*dt;
        // if (queda > 12f*dt) queda = 12f*dt; 
        onde.Y -= queda*dt;
        if (onde.Y < -2f) {queda=0;onde.Y=10f;}
    }
    public void nochao(List<Cubo> a)
    {
        foreach (var i in a)
        {
            if (Distancia(hitbox[0], i.hitbox[0]) > 0f) continue;
            
            if (Raylib.CheckCollisionBoxes(hitbox[0],i.hitbox[0]))
            {
                if (onde.Y  > i.onde.Y)
                {
                    if (!pulando) onde.Y = i.hitbox[0].Max.Y + (tamanho.Y / 2.0f);
                    queda = 0f;
                    pulando = false;
                    if (!sautCarregando) saut_carga =45f;
                }
                
            }
        }

    }
    public void ai_minha_cabeca(List<Cubo> a)
    {
        
        if (!pulando || (saut_carga <= queda)) return;

        foreach (var i in a)
        {
            if (Distancia(hitbox[0], i.hitbox[0]) > 8f) continue;

            if (Raylib.CheckCollisionBoxes(hitbox[0], i.hitbox[0]))
            {
                
                if (onde.Y < i.onde.Y)
                {
                    onde.Y = i.hitbox[0].Min.Y - (tamanho.Y / 2.0f);
                    pulando = false;
                    saut_carga = 45f;
                }
            }
        }
    }
    public void mover(float dt, int dir, List<Cubo> a)
    {

        Vector3 direcao = dir switch
        {
            0 => frente,
            1 => -frente,
            2 => -direita,
            3 => direita,
            _ => Vector3.Zero
        };

        float X_antes = onde.X;
        float Z_antes = onde.Z;

        onde += direcao * 22f * dt;

        onde.X = Math.Clamp(onde.X, -319f, 319f);
        foreach (var i in a)
        {
            if (Distancia(hitbox[0], i.hitbox[0]) > 8f) continue;

            if (Raylib.CheckCollisionBoxes(hitbox[1], i.hitbox[0]))
            {
                onde.X = X_antes;
            }
        }
        onde.Z = Math.Clamp(onde.Z, -319f, 319f);
        foreach (var i in a)
        {
            if (Distancia(hitbox[0], i.hitbox[0]) > 8f) continue;

            if (Raylib.CheckCollisionBoxes(hitbox[1], i.hitbox[0]))
            {
                onde.Z = Z_antes;
            }
        }
    }
    
    public void ataque(List<Cubo> a)
    {
        if (arm == 1)
        {

            cord_de_ini = onde + (direita * 0.9f) + (frente * 1.7f);

            float alc = 32.0f;
            cord_de_fim = cord_de_ini + (frente * alc);

            Ray ray = new(cord_de_ini, frente);

            foreach (var i in a)
            {
                if (Distancia(hitbox[0], i.hitbox[0]) > 32f) continue;

                RayCollision colisao = Raylib.GetRayCollisionBox(ray, i.hitbox[0]);

                if (colisao.Hit && colisao.Distance < alc)
                {
                    alc = colisao.Distance;
                    cord_de_fim = colisao.Point;
                }
            }
        }
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
                        
        Raylib.DrawCapsule(start,end,radius,16,8, new Color(190,190,190,120));
        Raylib.DrawCapsuleWires(start,end,radius,8,8,Color.DarkGray);
    }
}
class SimpleShot
{
    static Player jojo = new(new Vector3(0.0f, 60f, 0.0f),new Vector3(1.2f, 2.5f, 1.2f), 3);

    static void Main()
    {

        Raylib.InitWindow(900, 750, "SimpleShot");

        Camera3D camera = new()
        {
            Up = new Vector3(0.0f, 1.0f, 0.0f),
            // Position = jojo.onde + new Vector3(0f, 16f, 10f),
            Target = new Vector3(0.0f, 0.0f, 0.0f),
            Projection = CameraProjection.Perspective,
            FovY = 50.0f
        };

        Vector3 cameraOffset;
        Vector3 joOffset;
        
        Cubo chao = new(new(0.0f, 0.0f, 0.0f),new(640.0f, 0.2f, 640.0f),1);

        List<Cubo> plat = [
            chao, 
            new(new(4f, 7f, 4f),new(7.0f, 1.8f, 7.0f),3),
            new(new(-4f, 14f, 4f),new(7.0f, 1.8f, 7.0f),3),
            new(new(-4f, 21f, -4f),new(7.0f, 1.8f, 7.0f),3),
            new(new(4f, 28f, -4f),new(7.0f, 1.8f, 7.0f),3),
            new(new(4f, 35f, 4f),new(7.0f, 1.8f, 7.0f),3),
            new(new(-4f, 42f, 4f),new(7.0f, 1.8f, 7.0f),3),
            
            new(new(23f, 49f, 0f),new(46.0f, 1.8f, 100.0f)),
            new(new(-31f, 49f, 0f),new(46.0f, 1.8f, 100.0f)),
            new(new(-4f, 49f, 30f),new(8.0f, 1.8f, 40.0f)),
            new(new(-4f, 49f, -24.5f),new(8.0f, 1.8f, 51.0f)),

            new(new(38f,5.2f,0),new(1f, 10.2f,48f),2),
            new(new(-38f,5.2f,0),new(1f, 10.2f,48f),2),
            new(new(0f,5.2f,38f),new(48f, 10.2f,1f),2),
            new(new(-0f,5.2f,-38f),new(48f, 10.2f,1f),2),

            new(new(105f,10.3f,0),new(135.75f,20.4f,1f),2),
            new(new(252f,10.3f,0),new(135.75f,20.4f,1f),2),
            new(new(-105f,10.3f,0),new(135.75f,20.4f,1f),2),
            new(new(-252f,10.3f,0),new(135.75f,20.4f,1f),2),
            new(new(0,10.3f,105f),new(1f,20.4f,135.75f),2),
            new(new(0f,10.3f,252f),new(1f,20.4f,135.75f),2),
            new(new(0,10.3f,-105f),new(1f,20.4f,135.75f),2),
            new(new(0f,10.3f,-252f),new(1f,20.4f,135.75f),2),

            new(new(160.0f, 2.7f, 160.0f),new(2.3f, 5.2f, 2.3f),3),
            new(new(160.0f, 1f, 160.0f),new(7.0f, 1.8f, 7.0f),3),

            

        ];

        for (int x = 300; x >= 20; x -= 20)
        {
            for (int z = 300; z >= 20; z -= 20)
            {
                if (x<=180 && z<=180 && x>=140 && z>=140 || x <=40 && z<=40) continue;
                plat.Add(new(new(x,3.35f,z),new(7f, 6.5f, 7f), 2));
            }
        }
        float time_tiro = 0;
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
                jojo.nochao(plat);
                
                
                if (Raylib.IsKeyPressed(KeyboardKey.Left)) jojo.olhando = jojo.olhando - 1 >= 0?jojo.olhando - 1:Direcao.FrenteE;
                if (Raylib.IsKeyPressed(KeyboardKey.Right)) jojo.olhando = jojo.olhando + 1 <= Direcao.FrenteE?jojo.olhando + 1:Direcao.Frente;
                if (Raylib.IsKeyPressed(KeyboardKey.V) && !jojo.atirando && jojo.munição >= 1) { jojo.atirando = true; jojo.munição -= 1;jojo.ataque(plat);}
                
                if (Raylib.IsKeyDown(KeyboardKey.W)) jojo.mover(dt, 0,plat);
                if (Raylib.IsKeyDown(KeyboardKey.S)) jojo.mover(dt, 1, plat);
                if (Raylib.IsKeyDown(KeyboardKey.A)) jojo.mover(dt, 2,plat);
                if (Raylib.IsKeyDown(KeyboardKey.D)) jojo.mover(dt, 3,plat);
                if (Raylib.IsKeyDown(KeyboardKey.Up) && !jojo.pulando)
                {
                    jojo.sautCarregando = true;
                    jojo.saut_carga += 20f*dt;
                    if (jojo.saut_carga > 60f) jojo.saut_carga = 60f;
                    
                }
                else if (Raylib.IsKeyReleased(KeyboardKey.Up)) {jojo.pulando = true; jojo.sautCarregando = false;}
                if (jojo.pulando) {jojo.Pulo(dt); jojo.ai_minha_cabeca(plat);}

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

                // cameraOffset = new(0f, 16f, 10f);
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

                if (jojo.munição < 3) jojo.munição += jojo.munição>= 1?0.85f * dt:0.48f * dt;

                if (jojo.onde.Y < -40) jojo.vida -= 1;

                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(48, 50, 164,255));
                Raylib.BeginMode3D(camera);

                foreach (var i in plat)
                {
                    if (Cubo.Distancia(jojo.hitbox[0], i.hitbox[0])> 35f) continue;
                    i.aparecer();
                }

                if (jojo.atirando)
                {
                    time_tiro += dt;
                    Raylib.DrawLine3D(jojo.cord_de_ini,jojo.cord_de_fim,Color.Orange);
                    Raylib.DrawSphere(jojo.cord_de_fim,0.08f,Color.Orange);
                    Raylib.DrawSphereWires(jojo.cord_de_fim,0.1f,4,4,Color.Yellow);
                    if (time_tiro > 0.25f) {jojo.atirando = false;time_tiro=0;}
                }

                Raylib.DrawSphere(jojo.onde, -33.0f, new Color(48, 50, 164,255)); 
                Raylib.DrawSphere(jojo.onde, -30.0f, new Color(48, 50, 164, 160)); 
                Raylib.DrawSphere(jojo.onde, -26.0f, new Color(48, 50, 164, 80));  

                jojo.aparecer();

                Raylib.EndMode3D();
                
                Raylib.DrawRectangleV(new Vector2(848, 58 ),new Vector2(24, 94), Color.Black);
                Raylib.DrawRectangleV(new Vector2(850, 60 + (30*3 - 30*jojo.vida)>=60?60 + (30*3 - 30*jojo.vida):60),new Vector2(20, 30*jojo.vida<=90?30*jojo.vida:90f), jojo.vida>1 ? Color.Lime: Color.Red);
                // Raylib.DrawText($"vida ", 10, 10, 20, Color.DarkGray);

                Raylib.DrawText($"X: {(int)jojo.onde.X};Y: {(int)jojo.onde.Y};Z: {(int)jojo.onde.Z}", 10, 10, 20, Color.DarkGray);
                Raylib.DrawText($"tiros: {(int)jojo.munição}/3", 10, 30, 20, Color.DarkGray);
                Raylib.DrawText($"{jojo.vida}/3", 835, 155, 20, Color.DarkGray);

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
        jojo = new(new Vector3(0.0f, 30f, 0.0f), new Vector3(1.2f, 2.5f, 1.2f), 3)
        {
            olhando = Direcao.Frente
        };
    }
}