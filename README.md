````md
<div align="center">

# 🤖 DOAIBU

### Greedy-Based Robocode Tank Royale Bot

**Tugas Besar IF25-21013 Strategi Algoritma**

<img src="https://img.shields.io/badge/C%23-.NET%2010-purple?style=for-the-badge">
<img src="https://img.shields.io/badge/Robocode-Tank%20Royale-red?style=for-the-badge">
<img src="https://img.shields.io/badge/Algorithm-Greedy-blue?style=for-the-badge">
<img src="https://img.shields.io/badge/Status-Completed-success?style=for-the-badge">

</div>

---

## 🚀 Overview

**DOAIBU** adalah bot utama Robocode Tank Royale yang menerapkan strategi **Greedy Algorithm** untuk mengambil keputusan secara cepat pada setiap tick pertandingan.

Bot ini berfokus pada keseimbangan antara:

`survival` · `targeting` · `movement` · `radar lock` · `firepower` · `risk minimization`

---

## 🧩 Bot Lineup

| Bot | Tipe Strategi | Fokus Utama |
|---|---|---|
| 🤖 **DOAIBU** | Adaptive Greedy | Survival + scoring seimbang |
| 🔥 **Nicegang** | Aggressive Greedy | Serangan agresif dan finishing |
| 🎯 **Tripanca** | Balanced Greedy | Firepower efisien dan jarak ideal |
| 🛡️ **ProtokolKesehatan** | Risk-Minimizing Greedy | Posisi aman dan survival |

---

## ✨ Core Features

| Area | Implementasi |
|---|---|
| Movement | Anti-Gravity, Stop and Go |
| Targeting | Play It Forward Prediction |
| Radar | Radar Lock System |
| Defense | Virtual Bullet Simulation |
| Fire Control | Dynamic Firepower |
| Decision Making | Greedy Heuristic Evaluation |

---

## 🧠 Greedy Flow

```text
Enemy Scan
    ↓
Risk Evaluation
    ↓
Best Local Decision
    ↓
Movement / Targeting / Firepower
    ↓
Action Execution
````

---

## 👨‍💻 Team

| NIM       | Nama                 |
| --------- | -------------------- |
| 124140096 | Syahrul Afwan        |
| 124140144 | Muhammad Faiz Ashfaq |
| 124140210 | Farid Rizky Fauzan   |

**Dosen Pengampu:** Winda Yulita, M.Cs.

---

## 📂 Project Structure

```text
TubesStima_DOAIBU
├── src
│   ├── DOAIBU
│   └── alternative-bots
│       ├── Nicegang
│       ├── Tripanca
│       └── ProtokolKesehatan
├── doc
│   └── Laporan TUBES STIMA.pdf
└── README.md
```

---

## ⚙️ Requirements

* Java
* .NET SDK
* Robocode Tank Royale GUI

---

## ▶️ How to Run

Clone repository:

```bash
git clone https://github.com/arull969/TubesStima_DOAIBU.git
cd TubesStima_DOAIBU
```

Jalankan Robocode GUI:

```bash
java -jar robocode-tankroyale-gui-0.30.0.jar
```

Tambahkan bot directory:

```text
src/DOAIBU
src/alternative-bots/Nicegang
src/alternative-bots/Tripanca
src/alternative-bots/ProtokolKesehatan
```

Lalu buka:

```text
Battle → Start Battle → Boot Bot → Add Bot → Start
```

---

## 📊 Testing Result

| Rank | Bot               | Avg Score |
| ---- | ----------------- | --------: |
| 🥇   | DOAIBU            |   2158.67 |
| 🥈   | ProtokolKesehatan |   1958.00 |
| 🥉   | Tripanca          |   1065.00 |
| ⭐    | Nicegang          |    694.67 |

---

## 🔗 Links

| Component   | Link                                          |
| ----------- | --------------------------------------------- |
| Repository  | https://github.com/arull969/TubesStima_DOAIBU |
| Video Bonus | https://youtu.be/5gjnGg4iJfQ                  |

---

<div align="center">

### Made with ☕, C#, and Greedy Strategy

</div>
```
