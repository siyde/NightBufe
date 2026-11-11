# Gece Büfesi (NightBufe)

Gece vardiyasında bir büfeyi işlettiğin, beş gece süren bir korku/anlatı oyunu. Her gece yeni bir yaratık, yeni bir kural ve yeni bir anomali gelir. Oyuncuya gösterilmeyen dört gizli sayaç (Anlaşma, Merak, Gerçeklik, Çocukla bağ) yaptığın seçimlere göre değişir ve finalde hangi sonların açılacağını belirler.

## Teknik
- **Motor:** Unity 6 (6000.3.21f1), URP
- **Mimari:** Ayrı assembly'ler: `Core`, `Data`, `Gameplay`, `Editor`
- Özel shader'lar ve render ayarları (`Assets/Rendering`)
- EditMode testleri (`Assets/Tests`)

## Doküman
- [docs/DESIGN.md](docs/DESIGN.md): Gece gece akış, seçimler ve sonlar

## Açma
Projeyi Unity Hub'dan **Unity 6000.3.21f1** ile aç. Görseller Git LFS ile tutulduğu için klonlamadan önce `git lfs install` çalıştır.
