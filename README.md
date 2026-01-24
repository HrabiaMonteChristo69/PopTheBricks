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
| Win / Lose | 🟡 | częściowo | WIN: brak klocków, LOSE: piłka spada |
| HUD | ⬜ | — | Score / Lives / Target |
| Poziomy | ⬜ | jeden układ | kilka leveli + rosnąca trudność |
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
