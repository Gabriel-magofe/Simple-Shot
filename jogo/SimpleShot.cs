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
        Raylib.DrawCubeV
        (
            onde, 
            tamanho, 
            tipo==1?Color.Gray:tipo==2?new(105,100,83,255):tipo==3?new(170,170,170,255):new(39,41,48,225)
        );
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
    public override BoundingBox[] hitbox => [new(onde - (tamanho /2), onde + (tamanho / 2)), new(new Vector3(onde.X,onde.Y+0.07f,onde.Z) - (tamanho /2), onde + (tamanho / 2)),new(new Vector3(onde.X,onde.Y-0.075f,onde.Z) - (tamanho /2), onde + (tamanho / 2))];
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
    public bool act_shield = false;
    public float shield= 3;
    public int vida;
    public bool pulando = false;
    public bool saut_carregando = false;
    public float queda=0f;
    public bool pisando = false;
    public float saut_carga = 45f;
    public float vel = 10f;
    public bool acl = false;
    
    public Player(Vector3 pos,int v, Vector3? size = null) : base(pos, size ?? new Vector3(1.2f, 2.5f, 1.2f))
    {
        vida = v;
    }
    public void Pulo(float dt)
    {
        onde.Y += saut_carga*dt;
    }
    public void cair(float dt)
    {
        if (pisando && !pulando) {queda=0f;return;}
        queda += 190f*dt;
        // if (queda > 12f*dt) queda = 12f*dt; 
        onde.Y -= queda*dt;
        if (onde.Y < -2f) {queda=0;onde.Y=10f;}
    }
    public void pisar(List<Cubo> a)
    {
        pisando = false;
        foreach (var i in a)
        {
            if (Distancia(hitbox[0], i.hitbox[0]) > 0f) continue;
            
            if (Raylib.CheckCollisionBoxes(hitbox[2],i.hitbox[0]))
            {
                if (onde.Y  > i.onde.Y)
                {
                    if (!pulando) onde.Y = i.hitbox[0].Max.Y + (tamanho.Y / 2.0f);
                    queda = 0f;
                    pulando = false;
                    pisando=true;
                    if (!saut_carregando) saut_carga =45f;
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

        onde += direcao * vel * dt;

        onde.X = Math.Clamp(onde.X, -319f, 319f);
        foreach (var i in a)
        {
            if (Distancia(hitbox[0], i.hitbox[0]) > 8f || i.tamanho.Y < 1.25f) continue;

            if (Raylib.CheckCollisionBoxes(hitbox[1], i.hitbox[0]))
            {
                onde.X = X_antes;
            }
        }
        onde.Z = Math.Clamp(onde.Z, -319f, 319f);
        foreach (var i in a)
        {
            if (Distancia(hitbox[0], i.hitbox[0]) > 8f || i.tamanho.Y < 1.25f) continue;

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

            cord_de_ini = onde + (direita * 0.9f) + (frente * 0.8f);

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
        if (act_shield)
        {
            Raylib.DrawCapsule(start,end,radius,16,8, new Color(150,210,230,120));
            Raylib.DrawCapsuleWires(start,end,radius,8,8,Color.DarkBlue);
        }
        else
        {
            Raylib.DrawCapsule(start,end,radius,16,8, new Color(190,190,190,120));
            Raylib.DrawCapsuleWires(start,end,radius,8,8,Color.DarkGray);
        }          
        
    }
}
class SimpleShot
{
    static Player jojo = new(new Vector3(10.0f, 2f, 10.0f), 3);

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
            //centro\torre
            new(new(4f, 7f, 4f),new(7.0f, 1.8f, 7.0f),3),
            new(new(-4f, 14f, 4f),new(7.0f, 1.8f, 7.0f),3),
            new(new(-4f, 21f, -4f),new(7.0f, 1.8f, 7.0f),3),
            new(new(4f, 28f, -4f),new(7.0f, 1.8f, 7.0f),3),
            new(new(4f, 35f, 4f),new(7.0f, 1.8f, 7.0f),3),
            new(new(-4f, 42f, 4f),new(7.0f, 1.8f, 7.0f),3),
            
            new(new(30.25f, 49f, 0f),new(59.5f, 1.8f, 120.0f)),
            new(new(-34.25f, 49f, 0f),new(51.5f, 1.8f, 120.0f)),
            new(new(-4f, 49f, 34.25f),new(9.0f, 1.8f, 51.5f)),
            new(new(-4f, 49f, -30.25f),new(9.0f, 1.8f, 59.5f)),

            new(new(60.5f, 52.9f, 0f), new(1f, 6f, 122f), 2),
            new(new(-60.5f, 52.9f, 0f), new(1f, 6f, 122f), 2),
            new(new(0f, 52.9f, 60.5f), new(122f, 6f, 1f), 2),
            new(new(0f, 52.9f, -60.5f), new(122f, 6f, 1f), 2),

            new(new(-68.0f, 61.7f, 68.0f),new(2.3f, 5.2f, 2.3f),3),
            new(new(-68.0f, 59f, 68.0f),new(7.0f, 1.8f, 7.0f),3),

            new(new(45f, 51.4f, 27.5f), new(1f, 3.5f, 65f), 2),
            new(new(30f, 51.4f, -27.5f), new(1f, 3.5f, 65f), 2),
            new(new(15f, 51.4f, 27.5f), new(1f, 3.5f, 65f), 2),
            new(new(-15f, 51.4f, -27.5f), new(1f, 3.5f, 65f), 2),
            new(new(-30f, 51.4f, -27.5f), new(1f, 3.5f, 65f), 2),
            new(new(-45f, 51.4f, -27.5f), new(1f, 3.5f, 65f), 2),

            new(new(45f, 52.4f, -32.5f), new(6f, 5f, 6.5f), 2),
            new(new(30f, 52.4f, 32.5f), new(6f, 5f, 6.5f), 2),
            new(new(15f, 52.4f, -32.5f), new(6f, 5f, 6.5f), 2),
            new(new(-15f, 52.4f, 32.5f), new(6f, 5f, 6.5f), 2),
            new(new(-30f, 52.4f, 32.5f), new(6f, 5f, 6.5f), 2),
            new(new(-45f, 52.4f, 32.5f), new(6f, 5f, 6.5f), 2),

            new(new(-9f,24.4f,0),new(1,47.8f,16.5f)),
            new(new(9f,24.4f,0),new(1,47.8f,16.5f)),
            new(new(0f,24.4f,-9),new(16.8f,47.5f,1f)),
            new(new(0f,28f,9),new(16.8f,41f,1f)),

            //divisores
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

            // centro - -
            new(new(-160.0f, 2.7f, -160.0f),new(2.3f, 5.2f, 2.3f),3),
            new(new(-160.0f, 1f, -160.0f),new(7.0f, 1.8f, 7.0f),3),

            // centro ++
            new(new(140.0f, 32.7f, 180.0f),new(2.3f, 5.2f, 2.3f),3),
            new(new(140.0f, 31f, 180.0f),new(7.0f, 1.8f, 7.0f),3),

            new(new(170,4,170),new(1.5f,1.5f,1.5f),2),
            new(new(165,8.5f,170),new(1.5f,1.5f,1.5f),2),
            new(new(165,13f,165),new(1.5f,1.5f,1.5f),2),
            new(new(160,17.5f,164),new(1.5f,1.5f,9.5f),2),
            new(new(160,22f,150),new(1.5f,1.5f,1.5f),2),
            new(new(155,26.5f,145),new(9.5f,1.5f,1.5f),2),
            new(new(140,30.5f,145),new(1.5f,1.5f,1.5f),2),
            new(new(140,30.5f,151.5f),new(1.5f,1.5f,1.5f),2),
            new(new(135,30.5f,158),new(1.5f,1.5f,1.5f),2),
            new(new(140,30.5f,164.5f),new(1.5f,1.5f,1.5f),2),
            new(new(145,30.5f,171),new(1.5f,1.5f,1.5f),2),

            // centro -+
            new(new(-160.0f, 2.7f, 160.0f),new(2.3f, 5.2f, 2.3f),3),
            new(new(-160.0f, 1f, 160.0f),new(7.0f, 1.8f, 7.0f),3),

            new(new(-160f, 15.65f, 248.75f),new(35f, 1.8f, 142.5f),4),
            new(new(-160f, 15.65f, 71.25f),new(35f, 1.8f, 142.5f),4),
            new(new(-90.25f, 15.65f, 19f),new(104.5f, 1.8f, 38f),4),
            new(new(-248.75f, 15.65f, 160f),new(142.5f, 1.8f, 320f),4),
            new(new(-71.25f, 15.65f, 179f),new(142.5f, 1.8f, 282f),4),
        ];

        // ++
        for (int x = 300; x >= 20; x -= 20)
        {
            for (int z = 300; z >= 20; z -= 20)
            {
                if (x<=180 && z<=180 && x>=140 && z>=140 || x <=40 && z<=40) continue;
                plat.Add(new(new(x,3.35f,z),new(7f, 6.5f, 7f), 2));
            }
        }
        
        // --
        for (int x = -300; x <= -20; x += 30)
        {
            for (int z = -300; z <= -20; z += 30)
            {
                if (x>=-180 && z>=-180 && x<=-140 && z<=-140 || x >=-40 && z>=-40) continue;
                plat.Add(new(new(x,2.7f,z),new(2.3f, 5.2f, 2.3f), 3));
                plat.Add(new(new(x, 6.1f, z),new(8f, 1.8f, 8f),3));
            }
        }
        for (int x = -285; x <= -20; x += 30)
        {
            for (int z = -285; z <= -20; z += 30)
            {
                if (x>=-180 && z>=-180 && x<=-140 && z<=-140 || x >=-40 && z>=-40) continue;
                plat.Add(new(new(x, 10.6f, z),new(11f, 1.8f, 11f),3));
            }
        }

        // - +
        for (float x = -317.5f; x <= -2.5f; x += 35f)
        {
            for (float z = 317.5f; z >= 2.5f; z -= 35f)
            {
                if (x >= -40f && z <= 40f) continue;
                if (x==-177.5 && z==2.5 || z==177.5 && x ==-2.5 ) continue;

                if (x == -2.5)
                {
                    plat.Add(new(new(x, 5.35f, z), new(28f, 10.5f, 1f), 2));
                    plat.Add(new(new(x, 7.6f, z), new(1f, 15f, 28f), 2));
                    continue;
                }
                if (z == 2.5)
                {
                    plat.Add(new(new(x, 7.6f, z), new(28f, 15f, 1f), 2));
                    plat.Add(new(new(x, 5.35f, z), new(1f, 10.5f, 28f), 2));
                    continue;
                }

                plat.Add(new(new(x, 7.6f, z), new(28f, 15.4f, 1f), 2));
                plat.Add(new(new(x, 7.6f, z), new(1f, 15.4f, 28f), 2));
            }
        }
        for (int x = -300; x <= -20; x += 35)
        {
            for (int z = 300; z >= 20; z -= 35)
            {
                // if (x>=-180 && z<=180 && x<=-140 && z>=140 || x >=-40 && z<=40) continue;
                if (x >=-40 && z<=40 || x==-160 && z==160) continue;
                plat.Add(new(new(x,7.6f,z),new(1.3f, 15.4f, 1.3f), 4));
            }
        }

        float time_tiro = 0;
        var teste = true;
        // float dano = 0;
        Raylib.SetTargetFPS(60);
        while (!Raylib.WindowShouldClose())
        {
            while (!Raylib.WindowShouldClose() && jojo.vida > 0 && teste)
            {
                float dt = Raylib.GetFrameTime();
                // dano += 1 * dt;
                // if (dano> 1){jojo.vida -= 1;dano = 0;}
                jojo.cair(dt);
                jojo.pisar(plat);
                
                
                if (Raylib.IsKeyPressed(KeyboardKey.Left)) jojo.olhando = jojo.olhando - 1 >= 0?jojo.olhando - 1:Direcao.FrenteE;
                if (Raylib.IsKeyPressed(KeyboardKey.Right)) jojo.olhando = jojo.olhando + 1 <= Direcao.FrenteE?jojo.olhando + 1:Direcao.Frente;
                if (Raylib.IsKeyPressed(KeyboardKey.Space) && !jojo.atirando && jojo.munição >= 1) { jojo.atirando = true; jojo.munição -= 1;jojo.ataque(plat);}
                if (Raylib.IsKeyPressed(KeyboardKey.V) && !jojo.act_shield && jojo.shield >= 3) jojo.act_shield = true;
                
                if (Raylib.IsKeyDown(KeyboardKey.W)) {jojo.mover(dt, 0,plat); jojo.acl = true;}
                if (Raylib.IsKeyDown(KeyboardKey.S)) {jojo.mover(dt, 1, plat); jojo.acl = true;}
                if (Raylib.IsKeyDown(KeyboardKey.A)) {jojo.mover(dt, 2,plat); jojo.acl = true;}
                if (Raylib.IsKeyDown(KeyboardKey.D)) {jojo.mover(dt, 3,plat); jojo.acl = true;}
                if (Raylib.IsKeyDown(KeyboardKey.Up) && !jojo.pulando)
                {
                    jojo.saut_carregando = true;
                    jojo.saut_carga += 20f*dt;
                    if (jojo.saut_carga > 66f) jojo.saut_carga = 66f;
                    
                }
                else if (Raylib.IsKeyReleased(KeyboardKey.Up) && jojo.saut_carregando) {jojo.pulando = true; jojo.saut_carregando = false;}
                if (jojo.pulando) {jojo.Pulo(dt); jojo.ai_minha_cabeca(plat);}

                if (jojo.acl)
                {
                    jojo.acl = false;
                    jojo.vel += 4.5f * dt;
                    if (jojo.vel > 26.5f) jojo.vel = 26.5f;
                    
                } else jojo.vel = 10f;

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

                Vector3 origemCam = jojo.onde + new Vector3(0f, 0.8f, 0f);
                Vector3 vetorCam = camera.Position - origemCam;
                float distCam = vetorCam.Length();
                Vector3 dirCam = Vector3.Normalize(vetorCam);

                Ray rayCamera = new(origemCam, dirCam);

                foreach (var i in plat)
                {
                    if (Cubo.Distancia(jojo.hitbox[0], i.hitbox[0]) > 15f) continue;

                    RayCollision colisao_cam = Raylib.GetRayCollisionBox(rayCamera, i.hitbox[0]);
                    if (colisao_cam.Hit && colisao_cam.Distance < distCam)
                    {
                        distCam = colisao_cam.Distance;
                        camera.Position = colisao_cam.Point - (dirCam * 0.4f);
                    }
                }

                // Raylib.UpdateCamera(ref camera, CameraMode.Orbital);

                if (jojo.munição < 3) jojo.munição += jojo.munição>= 1?0.85f * dt:0.48f * dt;

                if (jojo.onde.Y < -40) jojo.vida -= 1;

                if (jojo.act_shield)
                {
                    jojo.shield -= 1.5f*dt;
                    if (jojo.shield <= 0)
                    {
                        jojo.shield = 0;
                        jojo.act_shield = false;
                    }
                } else if (jojo.shield <= 3)
                {
                    jojo.shield += 0.45f*dt;
                    if (jojo.shield >= 3) jojo.shield = 3;
                }

                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(48, 50, 164,255));
                Raylib.BeginMode3D(camera);

                foreach (var i in plat)
                {
                    if (Cubo.Distancia(jojo.hitbox[0], i.hitbox[0])> 35f || i.tipo == 4) continue;
                    i.aparecer();
                }

                if (jojo.atirando)
                {
                    time_tiro += dt;
                    Raylib.DrawLine3D(jojo.cord_de_ini,jojo.cord_de_fim,Color.Orange);
                    Raylib.DrawSphere(jojo.cord_de_fim,0.09f,Color.Orange);
                    if (time_tiro > 0.25f) {jojo.atirando = false;time_tiro=0;}
                }

                Raylib.DrawSphere(jojo.onde, -33.0f, new Color(48, 50, 164,255)); 
                Raylib.DrawSphere(jojo.onde, -30.0f, new Color(48, 50, 164, 160)); 
                Raylib.DrawSphere(jojo.onde, -26.0f, new Color(48, 50, 164, 80));

                foreach (var i in plat)
                {
                    if (Cubo.Distancia(jojo.hitbox[0], i.hitbox[0])> 31f || i.tipo != 4) continue;
                    i.aparecer();
                }

                jojo.aparecer();

                Raylib.EndMode3D();
                
                Raylib.DrawRectangleV(new Vector2(848, 58 ),new Vector2(24, 94), Color.Black);
                Raylib.DrawRectangleV(new Vector2(850, 60 + (30*3 - 30*jojo.vida)>=60?60 + (30*3 - 30*jojo.vida):60),new Vector2(20, 30*jojo.vida<=90?30*jojo.vida:90f), jojo.vida>1 ? Color.Lime: Color.Red);
                
                Raylib.DrawRectangleV(new Vector2(48, 58 ),new Vector2(24, 94), Color.Black);
                Raylib.DrawRectangleV(new Vector2(50, 60 + (30*3 - 30*jojo.shield)>=60?60 + (30*3 - 30*jojo.shield):60),new Vector2(20, 30*jojo.shield<=90?30*jojo.shield:90f), jojo.act_shield? new(193,249,255): new(126,236,255));
                Raylib.DrawText($"Shield", 32, 155, 20, Color.DarkGray);

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
        jojo = new(new Vector3(0.0f, 30f, 0.0f), 3)
        {
            olhando = Direcao.Frente
        };
    }
}