// ============================================================
//  Nicegang.cs  —  Robocode Tank Royale  (C# / .NET 6)
//
//  FIX: Bot tidak crash/keluar lagi.
//  - ArenaWidth/Height hanya dibaca di dalam Run() SETELAH loop mulai
//  - PickPatrolTarget() tidak dipanggil sebelum arena size siap
//  - Semua properti bot (X, Y, dll) hanya dibaca di dalam loop
//  - Tidak ada NaN / division-by-zero
//
//  Bot selalu bergerak, tidak pernah TargetSpeed=0
//  API v0.30.0
// ============================================================
/*
 __  __                     ____                                 
/\ \/\ \  __               /\  _`\                               
\ \ `\\ \/\_\    ___     __\ \ \L\_\     __      ___      __     
 \ \ , ` \/\ \  /'___\ /'__`\ \ \L_L   /'__`\  /' _ `\  /'_ `\   
  \ \ \`\ \ \ \/\ \__//\  __/\ \ \/, \/\ \L\.\_/\ \/\ \/\ \L\ \  
   \ \_\ \_\ \_\ \____\ \____\\ \____/\ \__/.\_\ \_\ \_\ \____ \ 
    \/_/\/_/\/_/\/____/\/____/ \/___/  \/__/\/_/\/_/\/_/\/___L\ \
                                                          /\____/
                                                          \_/__/ 
*/

using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

namespace Nicegang;

public static class Program
{
    public static void Main(string[] args) => new NicegangBot().Start();
}

public sealed class NicegangBot : Bot
{
    // ── Konstanta ─────────────────────────────────────────────
    private const double BotRadius    = 18.0;
    private const double WallMargin   = 60.0;
    private const double WallHugBand  = 90.0;
    private const double GunAimThresh = 3.0;
    private const double RadarMult    = 2.0;
    private const double OrbitDist    = 250.0;
    private const double MinSpd       = 3.0;
    private const int    MaxVB        = 60;
    private const int    StrafeMinT   = 10;
    private const int    StrafeMaxT   = 22;

    // ── Warna 1v1 — Olive ─────────────────────────────────────
    private static readonly Color C1Body   = Color.FromArgb(0x4b, 0x52, 0x20);
    private static readonly Color C1Turret = Color.FromArgb(0x5a, 0x62, 0x28);
    private static readonly Color C1Gun    = Color.FromArgb(0x2e, 0x32, 0x12);
    private static readonly Color C1Radar  = Color.FromArgb(0x8a, 0x9a, 0x3c);
    private static readonly Color C1Track  = Color.FromArgb(0x1e, 0x1e, 0x10);
    private static readonly Color C1Bullet = Color.FromArgb(0xd4, 0xc8, 0x6a);
    private static readonly Color C1Scan   = Color.FromArgb(0xa8, 0xb8, 0x50);

    // ── Warna Melee — Amber ───────────────────────────────────
    private static readonly Color CMBody   = Color.FromArgb(0x7a, 0x3a, 0x0a);
    private static readonly Color CMTurret = Color.FromArgb(0x9c, 0x4e, 0x10);
    private static readonly Color CMGun    = Color.FromArgb(0x5a, 0x2b, 0x08);
    private static readonly Color CMRadar  = Color.FromArgb(0xef, 0x9f, 0x27);
    private static readonly Color CMTrack  = Color.FromArgb(0x3d, 0x1d, 0x06);
    private static readonly Color CMBullet = Color.FromArgb(0xfa, 0xc7, 0x75);
    private static readonly Color CMScan   = Color.FromArgb(0xba, 0x75, 0x17);

    // ── State ─────────────────────────────────────────────────
    private readonly Dictionary<int, Enemy> _en  = new();
    private readonly LinkedList<VBullet>    _vbs = new();
    private readonly Random                 _rng = new();

    // Arena size — HANYA diisi di dalam Run() setelah siap
    private double _aw = 800;   // default fallback, akan diisi ulang
    private double _ah = 600;

    // Strafe
    private int    _strafeDir   = 1;
    private int    _strafeTimer = 0;
    private double _strafeSpd   = 6.0;

    // Patrol
    private double _patrolX    = 90;
    private double _patrolY    = 90;
    private int    _patrolTimer = 0;

    // Orbit 1v1
    private int _orbitDir      = 1;
    private int _orbitCooldown = 0;

    // ── Sin/Cos LUT ───────────────────────────────────────────
    private static readonly double[] SinT = new double[3600];
    private static readonly double[] CosT = new double[3600];
    static NicegangBot()
    {
        for (int i = 0; i < 3600; i++)
        {
            double r = i * Math.PI / 1800.0;
            SinT[i] = Math.Sin(r);
            CosT[i] = Math.Cos(r);
        }
    }
    private static double FS(double d) => SinT[((int)(d * 10) % 3600 + 3600) % 3600];
    private static double FC(double d) => CosT[((int)(d * 10) % 3600 + 3600) % 3600];

    public NicegangBot() : base(BotInfo.FromFile("Nicegang.json")) { }

    // ─────────────────────────────────────────────────────────
    //  MAIN LOOP
    //  Semua akses ke properti bot (X, Y, ArenaWidth, dll)
    //  HANYA boleh di dalam Run() setelah Go() pertama
    // ─────────────────────────────────────────────────────────
    public override void Run()
    {
        // Baca arena size SEKALI di sini — sudah pasti valid
        _aw = ArenaWidth;
        _ah = ArenaHeight;

        AdjustGunForBodyTurn   = true;
        AdjustRadarForGunTurn  = true;
        AdjustRadarForBodyTurn = true;

        // Set patrol awal ke pojok terdekat dari posisi spawn
        _patrolX    = WallHugBand;
        _patrolY    = WallHugBand;
        _patrolTimer = 0;

        ApplyColors(isMelee: true);

        while (IsRunning)
        {
            // Tick timers
            _strafeTimer--;
            _patrolTimer--;
            _orbitCooldown--;

            // Strafe: ganti arah/kecepatan berkala
            if (_strafeTimer <= 0)
            {
                _strafeTimer = StrafeMinT + _rng.Next(StrafeMaxT - StrafeMinT);
                _strafeSpd   = MinSpd + _rng.NextDouble() * (MaxSpeed - MinSpd);
                if (_rng.NextDouble() < 0.30)
                    _strafeDir = -_strafeDir;
            }

            Enemy? target = BestTarget();
            ApplyColors(isMelee: EnemyCount != 1);

            // Gun & radar independen dari body
            DoGun(target);
            DoRadar(target);

            // Movement — tidak pernah diam
            DoMovement(target);

            TickVBullets();
            Go();
        }
    }

    // ─────────────────────────────────────────────────────────
    //  GUN — aim & fire, independen dari movement
    // ─────────────────────────────────────────────────────────
    private void DoGun(Enemy? t)
    {
        if (t == null) return;

        double fp    = CalcFp(t.Dist, t.Energy);
        double spd   = CalcBulletSpeed(fp);
        // Guard: spd tidak boleh 0
        if (spd < 0.1) return;

        double ticks = t.Dist / spd;
        double px    = Math.Clamp(t.X + FC(t.Dir) * t.Speed * ticks, BotRadius, _aw - BotRadius);
        double py    = Math.Clamp(t.Y + FS(t.Dir) * t.Speed * ticks, BotRadius, _ah - BotRadius);

        double gt   = NormalizeRelativeAngle(DirectionTo(px, py) - GunDirection);
        GunTurnRate = gt;

        if (Math.Abs(gt) < GunAimThresh && GunHeat <= 0)
        {
            SetFire(fp);
            if (_en.TryGetValue(t.Id, out Enemy? e))
                e.DmgDealt += BulletDmg(fp) * 0.5;
        }
    }

    // ─────────────────────────────────────────────────────────
    //  RADAR
    // ─────────────────────────────────────────────────────────
    private void DoRadar(Enemy? t)
    {
        if (EnemyCount == 1 && t != null)
        {
            double delta  = NormalizeRelativeAngle(DirectionTo(t.X, t.Y) - RadarDirection);
            RadarTurnRate = delta * RadarMult;
        }
        else
        {
            RadarTurnRate = MaxRadarTurnRate;
        }
    }

    // ─────────────────────────────────────────────────────────
    //  MOVEMENT — Selalu Bergerak
    //  Prioritas: 1) Escape tembok  2) Dodge peluru  3) Mode
    // ─────────────────────────────────────────────────────────
    private void DoMovement(Enemy? t)
    {
        // 1) WALL ESCAPE — paling prioritas
        if (IsNearWall())
        {
            EscapeWall();
            return;
        }

        // 2) BULLET DODGE
        VBullet? threat = FindThreat();
        if (threat != null)
        {
            DodgeBullet(threat);
            return;
        }

        // 3) MODE
        if (EnemyCount == 1)
            Move1v1(t);
        else
            MoveMelee();
    }

    // ─────────────────────────────────────────────────────────
    //  WALL ESCAPE
    // ─────────────────────────────────────────────────────────
    private bool IsNearWall()
        => X < WallMargin || X > _aw - WallMargin
        || Y < WallMargin || Y > _ah - WallMargin;

    private void EscapeWall()
    {
        // Clamp ke safe zone dalam arena
        double tx = Math.Clamp(X, WallHugBand, _aw - WallHugBand);
        double ty = Math.Clamp(Y, WallHugBand, _ah - WallHugBand);
        // Kalau sudah di safe zone, arah ke tengah
        if (Math.Abs(tx - X) < 5 && Math.Abs(ty - Y) < 5)
        {
            tx = _aw / 2;
            ty = _ah / 2;
        }
        TurnRate    = NormalizeRelativeAngle(DirectionTo(tx, ty) - Direction);
        TargetSpeed = MaxSpeed;
    }

    // ─────────────────────────────────────────────────────────
    //  BULLET DODGE
    // ─────────────────────────────────────────────────────────
    private VBullet? FindThreat()
    {
        VBullet? best = null;
        double   bd   = double.MaxValue;
        foreach (VBullet vb in _vbs)
        {
            double fx = vb.X + vb.Vx * 6;
            double fy = vb.Y + vb.Vy * 6;
            double d  = DistToSeg(X, Y, vb.X, vb.Y, fx, fy);
            if (d < 60.0 && d < bd) { bd = d; best = vb; }
        }
        return best;
    }

    private void DodgeBullet(VBullet vb)
    {
        double p1 = vb.Heading + 90.0;
        double p2 = vb.Heading - 90.0;
        // Pilih arah dodge yang tidak menuju tembok
        double s1 = ZoneScore(X + FC(p1) * 80, Y + FS(p1) * 80);
        double s2 = ZoneScore(X + FC(p2) * 80, Y + FS(p2) * 80);
        double dodge = s1 >= s2 ? p1 : p2;
        TurnRate    = NormalizeRelativeAngle(dodge - Direction);
        TargetSpeed = MaxSpeed;
    }

    // Skor zona: ideal = di WallHugBand, buruk = terlalu dekat/jauh tembok
    private double ZoneScore(double cx, double cy)
    {
        if (cx < BotRadius || cx > _aw - BotRadius ||
            cy < BotRadius || cy > _ah - BotRadius)
            return -9999;
        double mw = Math.Min(Math.Min(cx, _aw - cx), Math.Min(cy, _ah - cy));
        if (mw < WallMargin) return -200 + mw;
        return -Math.Abs(mw - WallHugBand);
    }

    // ─────────────────────────────────────────────────────────
    //  1v1 MOVEMENT — Orbit erratic di pinggir peta
    // ─────────────────────────────────────────────────────────
    private void Move1v1(Enemy? t)
    {
        if (t == null)
        {
            // Tidak ada target: patrol ke pojok
            MovePatrol();
            return;
        }

        // Random reversal arah orbit
        if (_orbitCooldown <= 0 && _rng.NextDouble() < 0.018)
        {
            _orbitDir      = -_orbitDir;
            _orbitCooldown = 25;
        }

        // Sudut perpendicular + sedikit blend ke wall-hug
        double perp  = DirectionTo(t.X, t.Y) + 90.0 * _orbitDir;
        double wh    = WallHugDirection();
        double diff  = NormalizeRelativeAngle(wh - perp);
        double angle = perp + diff * 0.18;

        TurnRate = NormalizeRelativeAngle(angle - Direction);

        // Kecepatan: jaga jarak orbit + strafe jitter
        double spd;
        if (t.Dist < OrbitDist - 70)      spd = -MaxSpeed;
        else if (t.Dist > OrbitDist + 70) spd =  MaxSpeed;
        else                               spd =  _strafeDir * _strafeSpd;

        TargetSpeed = Math.Sign(spd) * Math.Max(MinSpd, Math.Abs(spd));
    }

    // Arah menuju zona wall-hug terdekat
    private double WallHugDirection()
    {
        double m  = WallHugBand;
        double tx = X, ty = Y;

        double dL = X - m;
        double dR = (_aw - m) - X;
        double dB = Y - m;
        double dT = (_ah - m) - Y;

        // Sumbu horizontal
        if (dL > dR + 20) tx = _aw - m;
        else if (dR > dL + 20) tx = m;

        // Sumbu vertikal
        if (dB > dT + 20) ty = _ah - m;
        else if (dT > dB + 20) ty = m;

        if (Math.Abs(tx - X) < 10 && Math.Abs(ty - Y) < 10) return Direction;
        return DirectionTo(tx, ty);
    }

    // ─────────────────────────────────────────────────────────
    //  MELEE MOVEMENT — Patrol pojok ke pojok di pinggir
    // ─────────────────────────────────────────────────────────
    private void MoveMelee()
    {
        if (_patrolTimer <= 0 || IsNear(_patrolX, _patrolY))
            RefreshPatrol();

        TurnRate    = NormalizeRelativeAngle(DirectionTo(_patrolX, _patrolY) - Direction);
        TargetSpeed = Math.Max(MinSpd, _strafeSpd);
    }

    private void MovePatrol()
    {
        if (_patrolTimer <= 0 || IsNear(_patrolX, _patrolY))
            RefreshPatrol();
        TurnRate    = NormalizeRelativeAngle(DirectionTo(_patrolX, _patrolY) - Direction);
        TargetSpeed = MaxSpeed;
    }

    // ─────────────────────────────────────────────────────────
    //  REFRESH PATROL — Pilih titik pinggir/pojok terbaik
    //  AMAN: hanya dipanggil dari dalam Run() loop
    // ─────────────────────────────────────────────────────────
    private void RefreshPatrol()
    {
        double m = WallHugBand;

        // 8 kandidat di pinggir — semua pakai _aw/_ah yang sudah valid
        (double px, double py)[] pts =
        {
            (m,        m),       (_aw - m, m),
            (m,        _ah - m), (_aw - m, _ah - m),
            (_aw / 2,  m),       (_aw / 2, _ah - m),
            (m,        _ah / 2), (_aw - m, _ah / 2),
        };

        double best = double.NegativeInfinity;

        foreach (var (px, py) in pts)
        {
            // Skip titik yang sudah dekat
            if (IsNear(px, py)) continue;

            // Jarak minimum ke semua musuh yang terdeteksi
            double minED = 999.0;
            foreach (Enemy e in _en.Values)
            {
                double d = Dist(px, py, e.X, e.Y);
                if (d < minED) minED = d;
            }

            double myDist = Dist(X, Y, px, py);
            double score  = minED * 0.8 - myDist * 0.15;

            if (score > best)
            {
                best     = score;
                _patrolX = px;
                _patrolY = py;
            }
        }

        _patrolTimer = 60 + _rng.Next(40);
    }

    // ─────────────────────────────────────────────────────────
    //  TARGET SELECTION
    // ─────────────────────────────────────────────────────────
    private Enemy? BestTarget()
    {
        Enemy? best = null;
        double bs   = double.NegativeInfinity;
        bool   duel = EnemyCount == 1;

        foreach (Enemy e in _en.Values)
        {
            if (TurnNumber - e.Seen > 15) continue;

            double sc = duel
                ? (100 - e.Energy) * 2.0 + e.DmgDealt * 1.5 - e.Dist * 0.05
                : (100 - e.Energy) * 2.0 + e.DmgDealt * 1.5 - Math.Max(0, e.Dist - 480) * 0.4;

            if (sc > bs) { bs = sc; best = e; }
        }
        return best;
    }

    // ─────────────────────────────────────────────────────────
    //  VIRTUAL BULLET TICK
    // ─────────────────────────────────────────────────────────
    private void TickVBullets()
    {
        var node = _vbs.First;
        while (node != null)
        {
            var next = node.Next;
            node.Value.Tick();
            if (node.Value.OOB(_aw, _ah)) _vbs.Remove(node);
            node = next;
        }
    }

    // ─────────────────────────────────────────────────────────
    //  PEWARNAAN
    // ─────────────────────────────────────────────────────────
    private bool _lastMelee = false;

    private void ApplyColors(bool isMelee)
    {
        if (isMelee == _lastMelee) return;
        _lastMelee = isMelee;

        if (isMelee)
        {
            BodyColor = CMBody; TurretColor = CMTurret; GunColor = CMGun;
            RadarColor = CMRadar; TracksColor = CMTrack;
            BulletColor = CMBullet; ScanColor = CMScan;
        }
        else
        {
            BodyColor = C1Body; TurretColor = C1Turret; GunColor = C1Gun;
            RadarColor = C1Radar; TracksColor = C1Track;
            BulletColor = C1Bullet; ScanColor = C1Scan;
        }
    }

    // ─────────────────────────────────────────────────────────
    //  EVENT HANDLERS
    //  PENTING: Jangan akses X, Y, ArenaWidth di sini —
    //  event bisa dipanggil sebelum Run() siap
    // ─────────────────────────────────────────────────────────
    public override void OnScannedBot(ScannedBotEvent e)
    {
        _en.TryGetValue(e.ScannedBotId, out Enemy? prev);

        if (prev != null)
        {
            double drop = prev.Energy - e.Energy;
            if (drop >= 0.1 && drop <= 3.0)
                AddVB(prev.X, prev.Y, prev.Dir, drop);
        }

        _en[e.ScannedBotId] = new Enemy
        {
            Id       = e.ScannedBotId,
            X        = e.X,
            Y        = e.Y,
            Energy   = e.Energy,
            Dir      = e.Direction,
            Speed    = e.Speed,
            Dist     = DistanceTo(e.X, e.Y),
            Seen     = TurnNumber,
            DmgDealt = prev?.DmgDealt ?? 0.0
        };
    }

    public override void OnHitWall(HitWallEvent e)
    {
        _strafeDir     = -_strafeDir;
        _orbitDir      = -_orbitDir;
        _orbitCooldown = 20;
        _strafeTimer   = 0; // reset supaya langsung re-evaluate
    }

    public override void OnHitBot(HitBotEvent e)
    {
        _strafeDir     = -_strafeDir;
        _orbitDir      = -_orbitDir;
        _orbitCooldown = 15;
        _strafeTimer   = 0;
    }

    public override void OnBulletHit(BulletHitBotEvent e)
    {
        if (_en.TryGetValue(e.VictimId, out Enemy? en))
            en.DmgDealt += BulletDmg(e.Bullet.Power);
    }

    public override void OnBotDeath(BotDeathEvent e)
        => _en.Remove(e.VictimId);

    public override void OnDeath(DeathEvent e) { }

    // OnRoundStarted: HANYA reset flag/state sederhana
    // JANGAN panggil fungsi yang butuh arena size di sini
    public override void OnRoundStarted(RoundStartedEvent e)
    {
        _en.Clear();
        _vbs.Clear();
        _strafeDir     = _rng.NextDouble() < 0.5 ? 1 : -1;
        _strafeTimer   = 0;
        _strafeSpd     = 6.0;
        _orbitDir      = _rng.NextDouble() < 0.5 ? 1 : -1;
        _orbitCooldown = 0;
        _patrolTimer   = 0;
        // TIDAK memanggil PickPatrolTarget() atau RefreshPatrol() di sini
        // karena _aw/_ah belum tentu valid — diisi di Run()
    }

    // ─────────────────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────────────────
    private void AddVB(double x, double y, double h, double fp)
    {
        if (_vbs.Count >= MaxVB) return;
        _vbs.AddLast(new VBullet(x, y, h, CalcBulletSpeed(fp)));
    }

    private double CalcFp(double dist, double ene)
    {
        double fp = dist switch { < 150 => 3.0, < 300 => 2.0, < 500 => 1.5, _ => 0.8 };
        if (ene < fp * 4) fp = ene / 4;
        return Math.Clamp(fp, Constants.MinFirepower,
                              Math.Min(Constants.MaxFirepower, Energy / 4.0));
    }

    private static double BulletDmg(double fp)
    {
        double d = 4 * fp;
        if (fp > 1) d += 2 * (fp - 1);
        return d;
    }

    private static double Dist(double ax, double ay, double bx, double by)
    {
        double dx = ax - bx, dy = ay - by;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private bool IsNear(double tx, double ty)
        => Dist(X, Y, tx, ty) < 55;

    // Jarak titik ke segmen garis
    private static double DistToSeg(double px, double py,
                                    double ax, double ay,
                                    double bx, double by)
    {
        double dx = bx - ax, dy = by - ay;
        double lenSq = dx * dx + dy * dy;
        if (lenSq < 1e-9) return Dist(px, py, ax, ay);
        double t = Math.Clamp(((px - ax) * dx + (py - ay) * dy) / lenSq, 0, 1);
        return Dist(px, py, ax + t * dx, ay + t * dy);
    }
}

// ─────────────────────────────────────────────────────────────
//  DATA CLASSES
// ─────────────────────────────────────────────────────────────
internal sealed class Enemy
{
    public int    Id       { get; init; }
    public double X        { get; init; }
    public double Y        { get; init; }
    public double Energy   { get; init; }
    public double Dir      { get; init; }
    public double Speed    { get; init; }
    public double Dist     { get; set;  }
    public int    Seen     { get; init; }
    public double DmgDealt { get; set;  }
}

internal sealed class VBullet
{
    public double X       { get; private set; }
    public double Y       { get; private set; }
    public double Heading { get; }
    public double Vx      { get; }
    public double Vy      { get; }

    public VBullet(double x, double y, double h, double spd)
    {
        X = x; Y = y; Heading = h;
        Vx = Math.Cos(h * Math.PI / 180.0) * spd;
        Vy = Math.Sin(h * Math.PI / 180.0) * spd;
    }

    public void Tick()              { X += Vx; Y += Vy; }
    public bool OOB(double w, double h) => X < 0 || X > w || Y < 0 || Y > h;
}