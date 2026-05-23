````md
<h1 align="center">🤖 DOAIBU</h1>

<p align="center">
  <b>Greedy-Based Robocode Tank Royale Bot</b>
</p>

<p align="center">
  Tugas Besar IF25-21013 Strategi Algoritma
</p>

<p align="center">
  <img src="https://img.shields.io/badge/C%23-.NET%2010-purple?style=for-the-badge">
  <img src="https://img.shields.io/badge/Robocode-Tank%20Royale-red?style=for-the-badge">
  <img src="https://img.shields.io/badge/Algorithm-Greedy-blue?style=for-the-badge">
  <img src="https://img.shields.io/badge/Status-Completed-success?style=for-the-badge">
</p>

---

# 📖 About Project

Repository ini berisi implementasi bot **Robocode Tank Royale** berbasis **Algoritma Greedy** yang dikembangkan untuk memenuhi Tugas Besar mata kuliah Strategi Algoritma.

Bot utama yang dikembangkan adalah **DOAIBU**, dengan beberapa bot alternatif sebagai pembanding strategi:

- ⚔️ DOAIBU
- 🔥 Nicegang
- 🎯 Tripanca
- 🛡️ ProtokolKesehatan

Fokus utama proyek ini adalah bagaimana bot mengambil keputusan terbaik secara lokal pada setiap tick pertandingan menggunakan pendekatan greedy.

---

# ✨ Features

## 🤖 DOAIBU
- Anti-Gravity Movement
- Radar Lock System
- Play It Forward Targeting
- Virtual Bullet Simulation
- Stop and Go Movement
- Adaptive Greedy Strategy

## 🔥 Nicegang
- Aggressive Greedy Behavior
- Sniper Orbit
- Bullet Dodge
- Ram Mode
- Patrol Movement

## 🎯 Tripanca
- Balanced Greedy Strategy
- Distance Management
- Efficient Firepower Control
- Predator Mode

## 🛡️ ProtokolKesehatan
- Safe Area Evaluation
- Risk-Minimizing Positioning
- Candidate Position Scoring
- Adaptive Firepower

---

# 🧠 Greedy Concept

Bot melakukan pengambilan keputusan berdasarkan kondisi lokal terbaik pada saat itu.

```text
Enemy Detection
      ↓
Evaluate Position
      ↓
Calculate Risk
      ↓
Predict Enemy Movement
      ↓
Choose Best Action
      ↓
Execute
````

Pendekatan greedy digunakan karena pertandingan Robocode berlangsung secara real-time dan membutuhkan keputusan cepat pada setiap tick.

---

# ⚙️ Tech Stack

| Technology           | Usage                     |
| -------------------- | ------------------------- |
| C#                   | Main Programming Language |
| .NET 10              | Runtime Environment       |
| Robocode Tank Royale | Battle Engine             |
| Greedy Algorithm     | Decision Making           |
| OOP                  | Program Structure         |

---

# 👨‍💻 Team Members

| NIM       | Nama                 |
| --------- | -------------------- |
| 124140096 | Syahrul Afwan        |
| 124140144 | Muhammad Faiz Ashfaq |
| 124140210 | Farid Rizky Fauzan   |

### Dosen Pengampu

**Winda Yulita, M.Cs.**

---

# 📂 Project Structure

```text
├── src/
│   ├── DOAIBU/
│   └── alternative-bots/
│       ├── Nicegang/
│       ├── Tripanca/
│       └── ProtokolKesehatan/
│
├── doc/
│   └── Laporan TUBES STIMA.pdf
│
└── README.md
```

---

# 🚀 Installation

Clone repository:

```bash
git clone https://github.com/arull969/TubesStima_DOAIBU.git
cd TubesStima_DOAIBU
```

---

# ▶️ Running The Program

## 1. Jalankan GUI Robocode Tank Royale

```bash
java -jar robocode-tankroyale-gui-0.30.0.jar
```

---

## 2. Tambahkan Bot Directory

Masuk ke:

```text
Config → Bot Root Directories
```

Tambahkan path berikut:

```text
src/DOAIBU
src/alternative-bots/Nicegang
src/alternative-bots/Tripanca
src/alternative-bots/ProtokolKesehatan
```

---

## 3. Start Battle

* Klik Battle
* Klik Start Battle
* Boot bot
* Add bot
* Start battle

---

# 📊 Testing Result

| Bot               | Avg Score | Rank |
| ----------------- | --------: | ---: |
| DOAIBU            |   2158.67 |   🥇 |
| ProtokolKesehatan |   1958.00 |   🥈 |
| Tripanca          |   1065.00 |   🥉 |
| Nicegang          |    694.67 |    ⭐ |

DOAIBU menjadi bot paling stabil karena mampu menyeimbangkan:

* Risk Minimization
* Positioning
* Survival
* Prediction Accuracy
* Score Maximization

---

# 📚 References

* Rinaldi Munir — Algoritma Greedy
* Robocode Tank Royale Documentation
* Microsoft C# Documentation
* Microsoft .NET Documentation

---

# 🔗 Links

| Component   | Link                                          |
| ----------- | --------------------------------------------- |
| Repository  | https://github.com/arull969/TubesStima_DOAIBU |
| Video Bonus | https://youtu.be/5gjnGg4iJfQ                  |

---

<p align="center">
  Made with ☕ and Greedy Strategy
</p>
```
