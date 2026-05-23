# Tugas Besar IF25-21013 Strategi Algoritma  
## Pemanfaatan Algoritma Greedy dalam Pembuatan Bot Permainan Robocode Tank Royale

Repository ini berisi implementasi bot **Robocode Tank Royale** yang dikembangkan untuk Tugas Besar IF25-21013 Strategi Algoritma. Fokus utama proyek ini adalah menerapkan strategi algoritma **greedy** dalam pengambilan keputusan bot secara real-time, terutama pada aspek movement, targeting, radar scanning, pemilihan target, dan penentuan firepower.

Bot utama yang dikembangkan adalah **DOAIBU**, sedangkan bot alternatif yang digunakan sebagai pembanding strategi adalah **Nicegang**, **Tripanca**, dan **ProtokolKesehatan**.

---

## Anggota Kelompok

| NIM | Nama |
|---|---|
| 124140096 | Syahrul Afwan |
| 124140144 | Muhammad Faiz Ashfaq |
| 124140210 | Farid Rizky Fauzan |

Dosen Pengampu: **Winda Yulita, M.Cs.**

---

## Deskripsi Singkat

Robocode Tank Royale adalah permainan pemrograman yang menempatkan pemain sebagai pengembang logika bot tank virtual. Bot tidak dikendalikan secara langsung, tetapi bergerak berdasarkan program yang dibuat. Setiap bot harus mampu bergerak, memindai musuh, mengarahkan radar, mengarahkan gun, menembak, menghindari peluru, serta bertahan hingga akhir pertandingan.

Permasalahan pada Robocode Tank Royale cocok dimodelkan dengan strategi greedy karena bot harus mengambil keputusan secara cepat pada setiap tick berdasarkan informasi lokal yang tersedia saat itu. Keputusan yang dipilih tidak selalu menjamin solusi global paling optimal, tetapi diharapkan mampu menghasilkan skor tinggi melalui kombinasi **risk minimization** dan **score maximization**.

---

## Bot yang Dikembangkan

### 1. DOAIBU — Main Bot

**DOAIBU** merupakan bot utama dengan pendekatan **adaptive risk-minimizing greedy**. Bot ini menggabungkan beberapa strategi utama:

- Anti-Gravity Movement
- Stop and Go
- Virtual Bullet Simulation
- Radar Lock
- Play It Forward Targeting

Strategi movement DOAIBU menyesuaikan kondisi pertandingan. Pada kondisi satu lawan satu, bot dapat menggunakan Stop and Go untuk menghindari tembakan sederhana. Pada kondisi banyak musuh, DOAIBU menggunakan Anti-Gravity untuk memilih posisi dengan risiko paling rendah.

Targeting DOAIBU menggunakan **Play It Forward**, yaitu prediksi posisi musuh berdasarkan riwayat state seperti angular velocity, speed, dan acceleration. Dengan strategi ini, DOAIBU tidak hanya menembak posisi musuh saat ini, tetapi memperkirakan posisi musuh ketika peluru sampai.

---

### 2. Nicegang — Alternative Bot

**Nicegang** menggunakan pendekatan **aggressive controlled greedy**. Bot ini dirancang lebih agresif, tetapi tetap memiliki mekanisme pengamanan.

Strategi utama Nicegang:

- Escape opening pada awal ronde
- Wall escape ketika terlalu dekat dinding
- Bullet dodge berdasarkan virtual bullet
- Sniper orbit pada kondisi 1v1
- Ram mode ketika musuh memiliki energi rendah
- Patrol movement pada melee battle
- Iterative linear prediction untuk targeting

Nicegang memilih target menggunakan fungsi scoring berdasarkan energi musuh, jarak, dan damage yang sudah diberikan. Bot ini kuat dalam situasi duel dan finishing target lemah, tetapi memiliki risiko tinggi jika terlalu agresif pada kondisi arena yang masih ramai.

---

### 3. Tripanca — Alternative Bot

**Tripanca** merupakan bot dengan pendekatan **balanced greedy**. Bot ini memodifikasi pola DOAIBU dengan penekanan pada efisiensi firepower dan pengaturan jarak ideal.

Strategi utama Tripanca:

- Anti-Gravity Movement
- Play It Forward Targeting
- Fire control greedy
- Pengaturan jarak ideal terhadap musuh
- Predator mode terhadap musuh berenergi rendah
- Penalti terhadap lintasan peluru dan dinding

Tripanca tidak selalu menggunakan firepower maksimum. Bot menghitung daya tembak berdasarkan jarak target, energi musuh, dan energi sendiri agar tembakan tetap efisien serta tidak mengorbankan survival.

---

### 4. ProtokolKesehatan — Alternative Bot

**ProtokolKesehatan** menggunakan strategi **risk-minimizing greedy** yang berfokus pada pemilihan posisi aman.

Strategi utama ProtokolKesehatan:

- Evaluasi 200 kandidat posisi
- Pemilihan posisi dengan risiko minimum
- Safe area checking
- Head-on targeting
- Firepower adaptif berdasarkan jumlah musuh hidup

Bot ini memilih posisi berdasarkan fungsi `EvaluatePosition`, yaitu dengan mempertimbangkan jarak terhadap musuh, energi musuh, sudut posisi, dan posisi sebelumnya. ProtokolKesehatan kuat dalam survival dan menghindari kerumunan, tetapi akurasi tembakannya lebih rendah karena menggunakan head-on targeting.

---

## Penerapan Strategi Greedy

Penerapan greedy pada proyek ini dilakukan dengan memetakan komponen Robocode Tank Royale ke elemen greedy berikut.

| Elemen Greedy | Penerapan pada Robocode Tank Royale |
|---|---|
| Himpunan Kandidat | Aksi movement, target, kandidat posisi, arah radar, metode targeting, dan firepower |
| Himpunan Solusi | Rangkaian aksi yang sudah dieksekusi bot selama pertandingan |
| Fungsi Seleksi | Memilih kandidat terbaik berdasarkan nilai heuristik |
| Fungsi Kelayakan | Memastikan posisi valid, target hidup, gun heat nol, dan firepower legal |
| Fungsi Objektif | Memaksimalkan skor akhir dan meminimalkan kehilangan energi |
| Heuristic Value | Risiko posisi, peluang hit, jarak ideal, energi musuh, lintasan peluru, dan keamanan posisi |

---

## Struktur Program

```text
├── src/
│   ├── DOAIBU/
│   │   └── ...
│   └── alternative-bots/
│       ├── Nicegang/
│       │   └── ...
│       ├── Tripanca/
│       │   └── ...
│       └── ProtokolKesehatan/
│           └── ...
├── doc/
│   └── Laporan TUBES STIMA.pdf
└── README.md
