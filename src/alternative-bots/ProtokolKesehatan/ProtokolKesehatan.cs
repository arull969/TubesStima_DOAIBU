using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;
using System;
using System.Drawing;
using System.Collections;
using SvgNet.Types;
using System.Runtime;

// ------------------------------------------------------------------
// ProtokolKesehatan
// ------------------------------------------------------------------
// ProtokolKesehatan adalah bot yang dirancang untuk Robocode Tank Royale dengan strategi berbasis algoritma greedy 
// yang fokus pada penjagaan jarak dari musuh. Bot ini selalu bergerak menuju posisi di arena yang memiliki risiko 
// minimum, mencerminkan prinsip menjaga jarak dari keramaian—seperti protokol kesehatan di dunia nyata yang 
// menekankan social distancing.

// Pada setiap langkah, ProtokolKesehatan mengevaluasi 200 posisi di sekitar lokasinya saat ini. 
// Bot ini menghitung risiko setiap posisi berdasarkan kedekatan dan jumlah bot lain di sekitarnya, 
// lalu memilih untuk bergerak ke posisi dengan risiko paling rendah.
// ------------------------------------------------------------------
/*
 ____               __           __              ___          __  __                         __                __                       
/\  _`\            /\ \__       /\ \            /\_ \        /\ \/\ \                       /\ \              /\ \__                    
\ \ \L\ \_ __   ___\ \ ,_\   ___\ \ \/'\     ___\//\ \       \ \ \/'/'     __    ____     __\ \ \___      __  \ \ ,_\    __      ___    
 \ \ ,__/\`'__\/ __`\ \ \/  / __`\ \ , <    / __`\\ \ \       \ \ , <    /'__`\ /',__\  /'__`\ \  _ `\  /'__`\ \ \ \/  /'__`\  /' _ `\  
  \ \ \/\ \ \//\ \L\ \ \ \_/\ \L\ \ \ \\`\ /\ \L\ \\_\ \_      \ \ \\`\ /\  __//\__, `\/\  __/\ \ \ \ \/\ \L\.\_\ \ \_/\ \L\.\_/\ \/\ \ 
   \ \_\ \ \_\\ \____/\ \__\ \____/\ \_\ \_\ \____//\____\      \ \_\ \_\ \____\/\____/\ \____\\ \_\ \_\ \__/.\_\\ \__\ \__/.\_\ \_\ \_\
    \/_/  \/_/ \/___/  \/__/\/___/  \/_/\/_/\/___/ \/____/       \/_/\/_/\/____/\/___/  \/____/ \/_/\/_/\/__/\/_/ \/__/\/__/\/_/\/_/\/_/
                                                                                                                                        
                                                                                                                                        
*/
public class ProtokolKesehatan : Bot
{
    // Inisialisasi variabel
    private Hashtable enemies = new Hashtable();
    private EnemyData target;
    private Vector2D nextDestination;
    private Vector2D lastPosition;
    private Vector2D currentPosition;
    private double myEnergy;
    private Random random = new Random();
    private double delta = 0;
    
    // Constructor
    public ProtokolKesehatan() : base(BotInfo.FromFile("ProtokolKesehatan.json")) { }

    // Main loop
    public override void Run()
    {
        // Tank Color
        BodyColor = Color.FromArgb(0x80, 0x80, 0x80);    // Black
        TurretColor = Color.FromArgb(0x80, 0x80, 0x80);  // Black
        RadarColor = Color.FromArgb(0xFF, 0xFF, 0xFF);   // White
        BulletColor = Color.FromArgb(0x80, 0x80, 0x80);  // Black
        ScanColor = Color.FromArgb(0x80, 0x80, 0x80);    // Black
        TracksColor = Color.FromArgb(0x80, 0x80, 0x80);  // Black
        GunColor = Color.FromArgb(0x80, 0x80, 0x80);     // Black

        // Set radar to continuously scan the arena
        RadarTurnRate = MaxRadarTurnRate;
        nextDestination = lastPosition = currentPosition = new Vector2D(X, Y);
        target = new EnemyData();

        Console.WriteLine("Run");
        // main bot activity
        while (IsRunning)
        {
            currentPosition = new Vector2D(X, Y);
            myEnergy = Energy;

            // fokus mendaftarkan data musuh
            if (target.live && TurnNumber > 10) {
                try {
                    DoMovement();
                    DoGun();
                } catch (Exception ex) {
                    Console.WriteLine("Exception in DoGun and DoMovement: " + ex.Message);
                }
            }

            Go();
        }
    }
    
    // minor events
    public override void OnBotDeath(BotDeathEvent e) {
        ((EnemyData)enemies[e.VictimId]).live = false;
    }

    // scan event
    public override void OnScannedBot(ScannedBotEvent e) {
        EnemyData en = (EnemyData)enemies[e.ScannedBotId];
        if (en == null) {
            en = new EnemyData();
            enemies[e.ScannedBotId] = en;
        }
        
        en.energy = e.Energy;
        en.live = true;

        double e_distance = currentPosition.Distance(new Vector2D(e.X, e.Y));
        en.pos = new Vector2D(e.X, e.Y);
        
        // update target jika salah satu keadaan benar.
        if (target == null || !target.live || e_distance < currentPosition.Distance(target.pos)) {
            target = en;
        }
    }

    // Algoritma Penembakan
    public void DoGun() {
        double distanceToTarget = DistanceTo(target.pos.X, target.pos.Y);

        // Gun (HeadOnTargeting)
        // tembak saat gun aligned, cooled, dan energy > 1
        delta = NormalizeRelativeAngle(calcAngleDegrees(target.pos, currentPosition) - GunDirection);
        if(Math.Abs(delta) < 5 && GunHeat==0 && myEnergy > 1) {
            int aliveEnemies = 0;
            foreach (DictionaryEntry entry in enemies)
            {
                EnemyData enemy = (EnemyData)entry.Value;
                if (enemy.live)
                {
                    aliveEnemies++;
                }
            }

            // Gunakan firepower maksimal saat musuh ada banyak dan firepower dinamis untuk jumlah musuh <= 3
            if (aliveEnemies > 3) {
                SetFire(3);
            }
            else {
                SetFire(Math.Min(myEnergy/6d, 1300d/distanceToTarget));
            }
        }

        // sesuaikan posisi gun (mengarah target)
        SetTurnGunLeft(delta);
    }

    // Algoritma Pergerakan
    public void DoMovement() {
        // Bergerak ke Destinasi aman
        double distanceToNextDestination = DistanceTo(nextDestination.X, nextDestination.Y);
        // Mencari destinasi baru jika destinasi saat ini telah tercapai
        if(distanceToNextDestination < 15) {
            int aliveEnemies = 0;
            foreach (DictionaryEntry entry in enemies)
            {
                EnemyData enemy = (EnemyData)entry.Value;
                if (enemy.live)
                {
                    aliveEnemies++;
                }
            }

            // oneLeftWeight digunakan untuk meningkatkan performa Bot di 1v1 (bot akan sedikit mengurangi kecenderungan untuk menjauh dari posisi awal)
            double oneLeftWeight = 1 - Math.Round(Math.Pow(random.NextDouble(), aliveEnemies));

            Rectangle2D safeArea = new Rectangle2D(30, 30, ArenaWidth - 60, ArenaHeight - 60);
            Vector2D testPoint;
            // menghasilkan dan mengevaluasi 200 testPoint
            for (int i = 0; i<200; i++) {
                testPoint = calcPoint(currentPosition, Math.Min(DistanceTo(target.pos.X, target.pos.Y)*0.8, 100 + 200*random.NextDouble()), 360*random.NextDouble());

                if(safeArea.Contains(testPoint.X, testPoint.Y) && EvaluatePosition(testPoint, oneLeftWeight) < EvaluatePosition(nextDestination, oneLeftWeight)) {
                    nextDestination = testPoint;
                }
            }

            lastPosition = currentPosition;

        } else {
            double angle = calcAngleDegrees(nextDestination, currentPosition) - Direction;     
            double moveDirection = 1;

            // untuk efisiensi pergerakan, bot dapat bergerak mundur
            if (Math.Cos(angle * Math.PI / 180) < 0) {
                angle += 180;
                moveDirection = -1;
            }
            
            angle = NormalizeRelativeAngle(angle);
            SetForward(distanceToNextDestination * moveDirection);
            SetTurnLeft(angle);
            
            // mengatur kecepatan berdasarkan rotasi yang harus dilakukan bot
            MaxSpeed = Math.Abs(angle) > 57 ? 0 : 8;
        }

    }

    // Evaluasi Posisi
    // Evaluasi resiko untuk kandidat "p"
    public double EvaluatePosition(Vector2D p, double oneLeftWeight){
        // Basis resiko: supaya merugikan kandidat posisi yang terlalu dekat dengan posisi terakhir (lastPosition)
        double eval = oneLeftWeight*0.08/p.DistanceSquared(lastPosition);
        
        // menghitung resiko berdasarkan enemy yang masih hidup (dari energi dan posisi [sudut dan jarak])
        foreach (DictionaryEntry entry in enemies) {
            EnemyData en = (EnemyData)entry.Value;
            double angleDiff = calcAngleDegrees(currentPosition, p) - calcAngleDegrees(en.pos, p);
            if (en.live) {
                eval += Math.Min(en.energy/myEnergy,2) * (1 + Math.Abs(Math.Cos(angleDiff * Math.PI / 180))) / p.DistanceSquared(en.pos);
            }
        }
        return eval;
    }

    // Class untuk membantu menyimpan data enemy atau target
    public class EnemyData {
        public Vector2D pos;
        public double energy;
        public bool live;
    }

    // HELPER FUNCTIONS
    private Vector2D calcPoint(Vector2D p, double dist, double angDegrees) {
        double angRadians = angDegrees * Math.PI / 180;
        return new Vector2D(p.X + dist * Math.Sin(angRadians), p.Y + dist * Math.Cos(angRadians));
    }

    private double calcAngleDegrees(Vector2D p2, Vector2D p1) {
        return Math.Atan2(p2.Y - p1.Y, p2.X - p1.X) * 180 / Math.PI;
    }

    // HELPER LIGHTWEIGHT DATA STRUCTURE
    public struct Vector2D
    {
        public double X { get; }
        public double Y { get; }

        public Vector2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Vector2D operator +(Vector2D a, Vector2D b) => new Vector2D(a.X + b.X, a.Y + b.Y);
        public static Vector2D operator -(Vector2D a, Vector2D b) => new Vector2D(a.X - b.X, a.Y - b.Y);
        public static Vector2D operator *(double scalar, Vector2D v) => new Vector2D(scalar * v.X, scalar * v.Y);

        // Mengkalkulasi jarak antar dua titik
        public double DistanceSquared(Vector2D other)
        {
            double deltaX = this.X - other.X;
            double deltaY = this.Y - other.Y;
            return deltaX * deltaX + deltaY * deltaY;
        }

        public double Distance(Vector2D other)
        {
            return Math.Sqrt(DistanceSquared(other));
        }
    }

    public struct Rectangle2D
    {
        public double X;
        public double Y;
        public double Width;
        public double Height;

        public Rectangle2D(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        // Mengecek apakah poin (x,y) ada di dalam rectangle
        public bool Contains(double x, double y)
        {
            return x >= X && x <= X + Width && y >= Y && y <= Y + Height;
        }
    }

    static void Main(string[] args)
    {
        new ProtokolKesehatan().Start();
    }
}