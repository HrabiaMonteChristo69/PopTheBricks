# 🎮 PopTheBricks — Arkanoid / Brick Breaker (Unity 6, URP 2D)

---

## 🎯 Specyfikacja projektu

**PopTheBricks** to gra typu **Arkanoid / Brick Breaker**.  
Sterujesz paletką, odbijasz piłkę i rozbijasz klocki. Celem jest **zniszczenie wszystkich klocków** bez utraty piłki.

---

## ✅ Najważniejsze funkcje (MVP)

- Menu startowe (Start / About / Quit)
- Sceny: **Menu → Gra → End**
- Ruch paletki + ograniczenia do ścian
- Start piłki z opóźnieniem + odliczanie **3…2…1…START**
- Podstawowa kolizja piłka ↔ paletka ↔ ściany ↔ klocki
- Panel **About Us** z opisem i autorami

---

## 🎮 Sterowanie

- **A / D** lub **← / →** — ruch paletki  
- (opcjonalnie później) **ESC** — pauza

---

## 🗺️ Sceny i przepływ gry

- **MenuScene** → Start gry / About / Quit  
- **GameScene** → właściwa rozgrywka  
- **EndGameScene** → ekran końcowy (Win / Lose + powrót)

---

## ⚙️ Jak uruchomić projekt

1. Otwórz projekt w **Unity Hub**
2. W `Assets/Scenes` otwórz: **MenuScene**
3. Kliknij **Play**

> Jeśli UI jest rozmyte/pikselowe: na Canvas ustaw  
> **Canvas Scaler → Scale With Screen Size**  
> oraz Reference Resolution np. `1080 x 1920`.

---

## 🧾 Status prac (co mamy / co jeszcze)

Legenda: ✅ zrobione / 🟡 w trakcie / ⬜ do zrobienia

| Obszar | Status | Co mamy | Co robimy dalej |
|---|---|---|---|
| Sterowanie paletką | ✅ | ruch + clamp do ścian | smoothing/feeling (opcjonalnie) |
| Start piłki | ✅ | opóźnienie + start w stronę paletki | odbicie zależne od miejsca na paletce |
| Odliczanie | ✅ | 3…2…1…START | animacja (fade/scale) |
| Menu | ✅ | Start / About / Quit | Settings/Credits (opcjonalnie) |
| About panel | ✅ | panel + tekst | przycisk Back (jeśli trzeba dopracować) |
| Sceny w Build | ✅ | Menu/Game/End dodane | spiąć logikę Win/Lose z End |
| Win / Lose | ⬜ | częściowo | WIN: brak klocków, LOSE: piłka spada |
| HUD | ✅ | — | Score / Lives / Target |
| Poziomy | ✅ | jeden układ | kilka leveli + rosnąca trudność |
| Pauza | ⬜ | — | PausePanel + Resume/Menu |
| Audio | ⬜ | — | odbicia / destroy / win / lose |
| VFX | ⬜ | — | glow / particles / camera shake |
| Porządek projektu | 🟡 | działa | prefaby, foldery, nazewnictwo |

---

## 🧠 Najbliższe kroki (priorytet)

1. **Win/Lose + EndGameScene** (pełna pętla gry)
2. **HUD** (Score/Lives/Target)
3. **Kąt odbicia od paletki** (zależny od miejsca trafienia)
4. **Pauza** + restart
5. **Audio + proste VFX** (mega podnosi jakość)

---

## 🧱 Struktura projektu (pliki i odpowiedzialności)

Poniżej szybka mapa: **gdzie co jest** i **za co odpowiada**.

### 📁 Sceny (`Assets/Scenes/`)

| Plik | Opis |
|---|---|
| `MenuScene.unity` | Menu główne (Start / About / Quit) |
| `GameScene.unity` | Rozgrywka (paletka, piłka, klocki, tło, ściany/kolizje) |
| `EndGameScene.unity` | Ekran końcowy (Win/Lose + powrót do menu) |

---

### 📁 Skrypty (`Assets/Scripts/`)

| Skrypt | Gdzie używany | Co robi |
|---|---|---|
| `Paddle.cs` | GameScene → obiekt `Paddle` | Sterowanie paletką (A/D lub ←/→), ograniczenie ruchu do ścian / colliderów |
| `BallMove.cs` (lub `Ball.cs`) | GameScene → obiekt `Ball` | Start piłki po czasie, ruch piłki, kierunek startu w stronę paletki, (opcjonalnie) logika przegranej |
| `MenuUI.cs` | MenuScene → `MenuManager` | Przełączanie widoków UI: Menu ↔ AboutPanel (pokazywanie/ukrywanie) |
| `SceneLoader.cs` | MenuScene → `MenuManager` | Ładowanie scen: Start → GameScene, Quit, powrót do Menu itd. |

> Jeśli nazwy plików różnią się u Was (np. `BallMove` vs `Ball`), zostaw w README tę nazwę, którą faktycznie macie w projekcie.

---

### 🎨 Grafiki / UI (`Assets/Sprites/`, `Canvas`)

| Element | Gdzie | Opis |
|---|---|---|
| `background.png/jpg` | GameScene → obiekt `Background` | Tło gry (wizualne). Do niego można dodać collider’y ścian/sufitu jako osobne childy |
| `paddle.png` | GameScene → obiekt `Paddle` | Sprite paletki (wizualnie). Collider (BoxCollider2D) odpowiada za odbicie piłki |
| Canvas (HUD) | GameScene | UI: score/lives/target (jeśli dodane) |
| Canvas (Menu) | MenuScene | UI: tytuł, przyciski Start/Quit/About + AboutPanel |

---

### 🧩 Najważniejsze obiekty w scenach (co warto wiedzieć)

#### `MenuScene`
- `Canvas`
  - `MenuRoot` (Start / About / Quit)
  - `AboutPanel` (opis gry + autorzy, ukryty domyślnie)
- `MenuManager` (obiekt z `MenuUI.cs` i `SceneLoader.cs`)
- `EventSystem` (wymagany do kliknięć UI)

#### `GameScene`
- `Background` (sprite tła)
- `Walls` / collidery (ściany + sufit) — najlepiej jako childy `Background`
- `Paddle` (`Paddle.cs` + BoxCollider2D)
- `Ball` (`BallMove.cs` + Rigidbody2D + Collider2D)
- `Bricks` (klocki)

---

## 🔧 Konwencje i szybkie tipy (żeby się nie zgubić)

- UI powinno być na **Canvas** (TextMeshPro / Buttons)
- Obiekty gry (piłka/paletka/tło/klocki) jako **SpriteRenderer** + collidery
- Sceny muszą być dodane w **Build Profiles → Scene List**:
  0. `MenuScene`
  1. `GameScene`
  2. `EndGameScene`


---

## 👥 Autorzy

- **Jakub Żurawski**
- **Jarosław Żukowski**

---

## 🧩 Użyte technologie

- Unity **6.3 LTS**
- **URP 2D**
- TextMeshPro (UI)

---

## 📌 Uwagi

Projekt może wykorzystywać zewnętrzne assety (tło/paletka).  
Przed publikacją warto dopisać źródło/licencję lub zastąpić własnymi.
