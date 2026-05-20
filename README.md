# Vertigo Wheel Demo

## Proje Bilgileri

- Unity sürümü: `2021.3.45f2 LTS`
- Hedef platform: Android
- Test Cihaz Modeli: HUAWEI AQM-LX1
- Ana sahne: `Assets/_Project/Scenes/GameScene.unity`
- APK sürümü: https://github.com/ceyhunCFC/VertigoWheelDemo/releases
- Oynanış Videosu: https://drive.google.com/file/d/1nWO7u8rMvttLjtw-hMh5gDk6SoizVTjV/view?usp=sharing
- Drawio Diagram: https://drive.google.com/file/d/1eJceboPRJdb2-GctmZXTYUhw9ufJB1AK/view?usp=sharing

## Özellikler

- Sabit üst göstergeli 8 slotlu çark çevirme sistemi.
- Normal, Güvenli ve Süper bölge çark varyantları.
- Mevcut bölge odağı ve kilometre taşı sayaçları içeren yatay bölge çubuğu.
- Ödül türüne göre yığınlama yapan ödül envanteri.
- Çark sonucundan ödül listesine uçuş animasyonu.
- Toplanabilir bölgeler için çıkış onay akışı.
- Ölüm sonuçları için oyun sonu akışı.
- Geç-atla özellikli ödül açılış animasyonuna sahip ödülleri topla ekranı.
- Yaygın yatay en-boy oranlarında test edilmiş duyarlı Unity UI: 20:9, 16:9 ve 4:3.

## Mimari

Proje, küçük bir oynanış koordinatörü ve ayrılmış UI/veri sorumlulukları etrafında düzenlenmiştir.

```text
Assets/_Project/
├── Scenes/              GameScene
├── Scripts/
│   ├── Data/            ScriptableObject tabanlı veri modelleri
│   ├── Gameplay/        Oyun akışı, bölge mantığı, ödül envanteri
│   └── UI/              Çark, bölge çubuğu, ödül ve modal görünümler
├── ScriptableObjects/   Çark, ödül ve bölge görsel verileri
├── Prefabs/             Yeniden kullanılabilir UI prefabları
└── Sprites/             UI, çark, bölge ve ödül sprite'ları
```

Temel sorumluluklar:

- `ZoneDebugController`: Çark, bölgeler, ödüller, çıkış, toplama ve oyun sonu ekranları arasındaki çalışma zamanı akışını koordine eder.
- `WheelView`: Çark verisini uygular ve slot görsellerini günceller.
- `WheelSpinner`: Çevirme girdisini, DOTween dönüşünü ve sonuç çözümlemesini yönetir.
- `ZoneBarView`: Bölge ilerleme çubuğunu render eder ve animasyonlar.
- `RewardInventory`: Toplanan ödülleri saklar ve tekrarlanan ödül türlerini yığınlar.
- `RewardPanelView` ve `CollectRewardsPanelView`: Ödül sunum akışlarını yönetir.
- `WheelDataSO`, `WheelDataSetSO`, `RewardDataSO` ve `ZoneVisualDataSO`: Oyun/UI verilerini Unity'den düzenlenebilir tutar.

## Bağımlılıklar

- DOTween
- TextMeshPro
- Unity UI

DOTween, `Assets/Plugins/Demigiant/DOTween` altında dahil edilmiştir.

## Araçlar

- Git versiyon kontrolü [Sourcetree] kullanılmıştır.

## Nasıl Çalıştırılır

1. Depoyu klonlayın.
2. Projeyi Unity `2021.3.45f2 LTS` ile açın.
3. `Assets/_Project/Scenes/GameScene.unity` sahnesini açın.
4. Oynat'a basın.

## Notlar

- UI referansları, Unity `OnClick` callback'lerine güvenmek yerine mümkün olduğunca koddan bağlanmaktadır.
- Değiştirilebilir UI metin alanları `_value` adlandırmasını kullanır.
- Gereksiz UI raycast hedefleri devre dışı bırakılmıştır.
- Çark, ödül ve bölge içeriği, daha kolay iterasyon için ScriptableObject'ler tarafından yönetilmektedir.

## Oyun User IDs

- Critical Strike : User ID = `661855836FA6C7E2` , Nickname = `Ceyhuncfc`
- Polygun Arena : User ID = `4C74CB5422885F4E` , Nickname = `CeyhunCFC`


