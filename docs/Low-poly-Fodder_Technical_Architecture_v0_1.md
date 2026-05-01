# LOW-POLY-FODDER (робоча назва)

## ТЕХНІЧНА АРХІТЕКТУРА ПРОТОТИПУ v0.1

**Детальна специфікація для початку розробки MVP**

|  |  |
| :---- | :---- |
| **Дата** | Квітень 2026 |
| **Формат гри** | LANDSCAPE (горизонтальна орієнтація, як Brawl Stars) |
| **Рушій** | Unity 2023 LTS (URP) · C\# · Mobile-first |
| **Цільові платформи** | iOS 14+ / Android 9+ (API 28+) |

---

## Зміст

1. [Загальний огляд архітектури та MVP-скоуп](#1-загальний-огляд-архітектури-та-mvp-скоуп)  
2. [Landscape-орієнтація: технічна реалізація](#2-landscape-орієнтація-технічна-реалізація)  
3. [Структура Unity-проєкту](#3-структура-unity-проєкту)  
4. [Архітектура ігрового клієнта](#4-архітектура-ігрового-клієнта)  
5. [Система введення (Twin-Stick Controls)](#5-система-введення-twin-stick-controls)  
6. [Система загону та Permadeath](#6-система-загону-та-permadeath)  
7. [Бойова система та балістика](#7-бойова-система-та-балістика)  
8. [Roguelike-перки (Runtime Buff System)](#8-roguelike-перки-runtime-buff-system)  
9. [Система ворогів та AI](#9-система-ворогів-та-ai)  
10. [Процедурна генерація рівнів](#10-процедурна-генерація-рівнів)  
11. [Камера та рендеринг (URP Pipeline)](#11-камера-та-рендеринг-urp-pipeline)  
12. [UI/UX архітектура](#12-uiux-архітектура)  
13. [Аудіо-система](#13-аудіо-система)  
14. [Мета-прогресія: Boot Camp та економіка](#14-мета-прогресія-boot-camp-та-економіка)  
15. [Серверна архітектура та бекенд](#15-серверна-архітектура-та-бекенд)  
16. [Аналітика та Live-Ops](#16-аналітика-та-live-ops)  
17. [Оптимізація та цільові пристрої](#17-оптимізація-та-цільові-пристрої)  
18. [Кооперативний режим](#18-кооперативний-режим)  
19. [CI/CD та білд-пайплайн](#19-cicd-та-білд-пайплайн)  
20. [MVP vs Post-MVP: повна карта фіч](#20-mvp-vs-post-mvp-повна-карта-фіч)  
21. [Ризики та мітигація](#21-ризики-та-мітигація)  
22. [Тижневий план запуску розробки](#22-тижневий-план-запуску-розробки)

---

## 1\. Загальний огляд архітектури та MVP-скоуп

Low-poly-Fodder — мобільний roguelike twin-stick shooter у landscape-орієнтації. Архітектура побудована на Unity 2023 LTS із Universal Render Pipeline (URP) для забезпечення оптимальної продуктивності на мобільних пристроях.

### 1.1 Архітектурний принцип

Проєкт використовує **гібридний підхід**: MonoBehaviour для високорівневої логіки (UI, стейт-машини, менеджери) та Data-Oriented підхід для масових обчислень (кулі, частинки, AI натовпу).

**Рішення**: На MVP ми НЕ використовуємо повний DOTS/ECS — це ускладнить розробку без відчутного виграшу при 4 солдатах та 20–40 ворогах на екрані. DOTS розглядається для Post-MVP, якщо знадобиться 100+ сутностей.

### 1.2 Високорівнева діаграма

Клієнт складається з трьох основних шарів:

| Шар | Відповідальність | Ключові класи |
| :---- | :---- | :---- |
| **Presentation Layer** | Рендеринг, UI, анімації, камера, VFX, аудіо | `CameraController`, `UIManager`, `VFXPool`, `AudioManager` |
| **Game Logic Layer** | Стейт-машини місій, бойова система, AI, перки, процедурна генерація | `MissionStateMachine`, `CombatSystem`, `SquadManager`, `PerkEngine`, `EnemyAI`, `LevelGenerator` |
| **Data / Persistence Layer** | Збереження прогресу, конфіг-дані, серверна синхронізація, аналітика | `SaveManager`, `ConfigLoader` (ScriptableObjects), `AnalyticsService`, `BackendClient` |

┌──────────────────────────────────────────────────────────────┐

│                    PRESENTATION LAYER                         │

│  Camera · UI (uGUI) · VFX Pool · Audio · Animations          │

├──────────────────────────────────────────────────────────────┤

│                    GAME LOGIC LAYER                            │

│  ┌───────────┐ ┌──────────┐ ┌────────┐ ┌──────────────────┐ │

│  │ Squad     │ │ Combat   │ │ Enemy  │ │ Perk Engine      │ │

│  │ Manager   │ │ System   │ │ AI     │ │ (Runtime Buffs)  │ │

│  └───────────┘ └──────────┘ └────────┘ └──────────────────┘ │

│  ┌───────────┐ ┌──────────┐ ┌─────────────────────────────┐ │

│  │ Level     │ │ Wave     │ │ Mission State Machine       │ │

│  │ Generator │ │ Spawner  │ │ (Boot→Brief→Play→Result)    │ │

│  └───────────┘ └──────────┘ └─────────────────────────────┘ │

├──────────────────────────────────────────────────────────────┤

│                    DATA / PERSISTENCE LAYER                    │

│  SaveManager · ScriptableObjects · Analytics · EventBus       │

└──────────────────────────────────────────────────────────────┘

### 1.3 MVP-скоуп: що входить, а що ні

MVP — це **вертикальний зріз** (vertical slice), який демонструє core loop за 3–5 хвилин гри. Ціль: довести fun-фактор одного забігу.

#### ✅ MVP — входить

| Система | MVP-скоуп | Критерій готовності |
| :---- | :---- | :---- |
| Twin-stick контроли | Рух загону \+ manual/auto aim у landscape | Грається зручно на iPhone SE та Galaxy A14 |
| Загін (4 солдати) | Формація, індивідуальний HP, permadeath | Солдат гине → надгробок на мапі, загін продовжує |
| Бойова система | Стрільба, перезарядка, гранати (2) | Відчувається «соковито», 4 потоки куль |
| Roguelike-перки | 15 перків (по 3 з кожної категорії), вибір 1 з 3 | Перки комбінуються, впливають на геймплей |
| Вороги (5 типів) | Grunt, Sniper, Rusher, Rocket Soldier, Light Tank | Унікальна поведінка AI для кожного |
| Процедурна генерація | 1 біом (джунглі), 8–12 tiles, спавн хвилями | Кожен забіг відрізняється |
| UI (ігровий) | HUD: HP, ammo, grenades, mini-map, mission objective | Читається на 6" екрані landscape |
| Камера | Top-down 60°, слідування за загоном | Плавна, без motion sickness |
| Аудіо | Placeholder SFX (постріли, смерті, вибухи), 1 BGM loop | Є фідбек на кожну дію |
| Boot Camp (мінімум) | Тільки екран вибору місії та Hill of Fame (список) | Гравець бачить загиблих солдатів між забігами |

#### ⏳ POST-MVP — відкладаємо

| Система | Що відкладаємо | Чому |
| :---- | :---- | :---- |
| Кооператив | Buddy System (local \+ online) | Потребує networking, подвоює QA |
| Battle Pass | 60-денний сезонний пас | Немає сенсу без retention-даних |
| Магазин/IAP | Скіни, косметика, валюти | Спочатку валідуємо core loop |
| Біоми 2–3 | Сніг, пустеля \+ відповідні вороги/боси | Контент масштабується після PMF |
| Veteran Mode шкіра | Pixel-art fallback шейдер | Easter egg, не критично |
| Online Leaderboards | Глобальні таблиці лідерів | Потребує серверну валідацію |
| Розширений Boot Camp | Апгрейди будівель, рекрути | Мета-loop після core loop |
| Локалізація | Мультимовна підтримка | Спочатку EN \+ UA |
| Steam/Switch порт | Десктоп та консолі | Архітектура закладається, порт пізніше |
| Розширені перки | 30–60 перків, рідкості | 15 достатньо для vertical slice |
| Advanced AI | Flanking, cover system, commander AI | Базовий AI достатній для fun-тесту |
| Rewarded Video Ads | Revive, bonus x2 | Немає сенсу до soft-launch |

---

## 2\. Landscape-орієнтація: технічна реалізація

Гра працює **ВИКЛЮЧНО** у горизонтальній (landscape) орієнтації, як Brawl Stars. Це фундаментальне рішення, що впливає на всі системи.

### 2.1 Unity Screen Settings

// ProjectSettings \> Player \> Resolution and Presentation

Default Orientation:        Landscape Left

Allowed Orientations:       Landscape Left \+ Landscape Right

Auto-Rotation:              ON (для обох landscape)

Portrait:                   DISABLED

Portrait Upside Down:       DISABLED

// Цільові розв'язки (aspect ratios):

// 16:9    — стандартні Android (1920x1080, 2560x1440)

// 19.5:9  — сучасні Android (2340x1080)

// 19:9    — iPhone X+ (2436x1125, 2532x1170)

// 4:3     — iPad (2048x1536)

### 2.2 Safe Area та Notch Handling

Сучасні телефони мають notch, Dynamic Island, закруглені кути та системні жести. Усі інтерактивні елементи UI **ПОВИННІ** знаходитись у Safe Area.

// SafeAreaHandler.cs — MonoBehaviour на Canvas

public class SafeAreaHandler : MonoBehaviour {

    RectTransform \_panel;

    Rect \_lastSafeArea;

    void Awake() \=\> \_panel \= GetComponent\<RectTransform\>();

    void Update() {

        if (Screen.safeArea \!= \_lastSafeArea) ApplySafeArea();

    }

    void ApplySafeArea() {

        var sa \= Screen.safeArea;

        var min \= sa.position;

        var max \= sa.position \+ sa.size;

        min.x /= Screen.width;  min.y /= Screen.height;

        max.x /= Screen.width;  max.y /= Screen.height;

        \_panel.anchorMin \= min;

        \_panel.anchorMax \= max;

        \_lastSafeArea \= sa;

    }

}

### 2.3 Layout джойстиків у Landscape

Landscape-режим дає широкий екран, що ідеально підходить для twin-stick. Розташування повторює Brawl Stars:

┌──────────────────────────────────────────────────────────┐

│ \[Mission Obj\]      \[Squad HP x4\]         \[Mini-Map 128²\]│

│                                                          │

│                                                          │

│                    ╔═══════════╗                          │

│                    ║ GAME VIEW ║                          │

│                    ║  (camera) ║                          │

│                    ╚═══════════╝                          │

│                                                          │

│                  \[Ammo 30/120\] \[Gren x2\]                 │

│  ╭─────╮                                   ╭─────╮      │

│  │ MOV │                                   │ AIM │ \[G\]   │

│  │STICK│                                   │STICK│       │

│  ╰─────╯                                   ╰─────╯      │

└──────────────────────────────────────────────────────────┘

| Зона екрану | Елемент | Розмір / Позиція |
| :---- | :---- | :---- |
| Ліва нижня 25% | Джойстик руху (Movement Stick) | Радіус: 120px (@160dpi), Dead Zone: 15px, floating base |
| Права нижня 25% | Джойстик прицілу (Aim Stick) \+ Auto-fire toggle | Радіус: 100px, кнопка гранати поруч (60x60px) |
| Верх ліво | Mission Objective \+ Timer | Макс ширина: 30% екрана |
| Верх право | Mini-map (квадрат) | 128x128px, масштабується DPI |
| Верх центр | Squad HP bars (4 солдати) | Горизонтальна стрічка з 4 портретами |
| Центр | Ігрове поле (viewport) | Камера top-down 60°, aspect-aware |
| Низ центр | Ammo counter (30/120) \+ Grenade counter | Іконка \+ числа, великий шрифт |

### 2.4 Адаптація під різні Aspect Ratio

// CameraAspectAdapter.cs

public class CameraAspectAdapter : MonoBehaviour {

    \[SerializeField\] float \_baseOrthoSize \= 8f;   // для 16:9

    \[SerializeField\] float \_baseFOV \= 50f;          // для perspective

    Camera \_cam;

    float \_targetAspect \= 16f / 9f;

    void Start() {

        \_cam \= GetComponent\<Camera\>();

        AdjustCamera();

    }

    void AdjustCamera() {

        float currentAspect \= (float)Screen.width / Screen.height;

        if (\_cam.orthographic) {

            // Ширші екрани (19.5:9): показуємо більше по горизонталі

            // Вужчі (4:3 iPad): збільшуємо orthoSize щоб вмістити UI

            \_cam.orthographicSize \= \_baseOrthoSize \* (\_targetAspect / currentAspect);

        } else {

            \_cam.fieldOfView \= \_baseFOV \* (\_targetAspect / currentAspect);

        }

    }

}

---

## 3\. Структура Unity-проєкту

Чітка та передбачувана структура папок критична для швидкої розробки. Використовуємо feature-based організацію з розділенням runtime та editor коду.

Assets/

├── \_Project/                        ← Весь наш код і контент

│   ├── Scripts/

│   │   ├── Core/                  ← GameManager, ServiceLocator, EventBus

│   │   ├── Input/                 ← InputActions, TouchStickController

│   │   ├── Squad/                 ← SquadManager, SoldierController, Permadeath

│   │   ├── Combat/                ← WeaponSystem, ProjectilePool, GrenadeSystem

│   │   ├── Enemy/                 ← EnemyBase, AI behaviours, EnemySpawner

│   │   ├── Perks/                 ← PerkDefinition (SO), PerkEngine, PerkUI

│   │   ├── LevelGen/              ← TileDatabase, LevelGenerator, TilePlacer

│   │   ├── Camera/                ← CameraController, CameraShake, AspectAdapter

│   │   ├── UI/                    ← HUDController, PerkSelectionUI, BootCampUI

│   │   ├── Audio/                 ← AudioManager, SFXLibrary (SO), MusicLayer

│   │   ├── Meta/                  ← BootCampManager, HillOfFame, SaveSystem

│   │   ├── Backend/               ← BackendClient, AuthService (POST-MVP)

│   │   ├── Analytics/             ← AnalyticsService, EventDefinitions

│   │   └── Utils/                 ← ObjectPool\<T\>, Extensions, MathHelpers

│   ├── ScriptableObjects/

│   │   ├── Perks/                 ← PerkDefinition assets

│   │   ├── Enemies/               ← EnemyConfig assets

│   │   ├── Weapons/               ← WeaponConfig assets

│   │   ├── Tiles/                 ← TileConfig assets

│   │   └── Audio/                 ← SFXLibrary, MusicPlaylist

│   ├── Prefabs/

│   │   ├── Soldiers/              ← SoldierPrefab з варіантами кольорів

│   │   ├── Enemies/               ← Grunt, Sniper, Rusher, Rocket, Tank

│   │   ├── Projectiles/           ← Bullet, Rocket, GrenadeProjectile

│   │   ├── VFX/                   ← MuzzleFlash, Explosion, HitSpark

│   │   ├── UI/                    ← HUD, PerkCard, DamageNumber

│   │   └── Environment/           ← Trees, Rocks, Buildings, Tombstone

│   ├── Art/

│   │   ├── Models/                ← Low-poly 3D models (.fbx)

│   │   ├── Textures/              ← Atlases, UI sprites

│   │   ├── Materials/             ← URP Lit/Unlit materials

│   │   ├── Animations/            ← Animator controllers \+ clips

│   │   └── Shaders/               ← Custom URP shaders (toon outline)

│   ├── Audio/

│   │   ├── SFX/                   ← .ogg files

│   │   └── Music/                 ← .ogg BGM loops

│   ├── Scenes/

│   │   ├── Boot.unity             ← Завантажувальна сцена (ServiceLocator init)

│   │   ├── BootCamp.unity         ← Мета-хаб (меню, Hill of Fame)

│   │   └── Mission.unity          ← Ігрова сцена (генерується процедурно)

│   └── AddressableAssets/         ← Конфігурація Addressables (POST-MVP)

├── Plugins/                           ← Сторонні SDK

├── Editor/                            ← Кастомні інспектори, тулзи

└── StreamingAssets/                   ← Конфіг JSON, початкові дані

---

## 4\. Архітектура ігрового клієнта

### 4.1 Service Locator \+ Event Bus

Замість Singleton-паттерну використовуємо легкий Service Locator для глобального доступу до менеджерів та Event Bus для комунікації між системами без жорстких залежностей.

// ═══ ServiceLocator.cs — центральний реєстр сервісів ═══

public static class Services {

    static readonly Dictionary\<Type, object\> \_services \= new();

    public static void Register\<T\>(T service) \=\> \_services\[typeof(T)\] \= service;

    public static T Get\<T\>() \=\> (T)\_services\[typeof(T)\];

    public static bool TryGet\<T\>(out T service) {

        if (\_services.TryGetValue(typeof(T), out var obj)) {

            service \= (T)obj;

            return true;

        }

        service \= default;

        return false;

    }

    public static void Clear() \=\> \_services.Clear();

}

// Використання:

Services.Register\<ISquadManager\>(new SquadManager());

var squad \= Services.Get\<ISquadManager\>();

// ═══ EventBus.cs — типізована шина подій ═══

public static class EventBus {

    static readonly Dictionary\<Type, List\<Delegate\>\> \_handlers \= new();

    public static void Subscribe\<T\>(Action\<T\> handler) {

        var type \= typeof(T);

        if (\!\_handlers.ContainsKey(type)) \_handlers\[type\] \= new List\<Delegate\>();

        \_handlers\[type\].Add(handler);

    }

    public static void Unsubscribe\<T\>(Action\<T\> handler) {

        if (\_handlers.TryGetValue(typeof(T), out var list))

            list.Remove(handler);

    }

    public static void Publish\<T\>(T evt) {

        if (\_handlers.TryGetValue(typeof(T), out var list))

            foreach (var handler in list)

                ((Action\<T\>)handler)(evt);

    }

}

// ═══ Визначення подій (structs для zero-alloc) ═══

public struct SoldierDiedEvent {

    public int SoldierId;

    public Vector3 Position;

    public string Name;

    public int KillCount;

    public int MissionsCompleted;

}

public struct EnemyKilledEvent {

    public int EnemyId;

    public EnemyType Type;

    public int KillerSoldierId;

    public float XPReward;

}

public struct PerkSelectedEvent    { public PerkDefinition Perk; }

public struct AmmoChangedEvent     { public int CurrentMag; public int Reserve; }

public struct MissionCompleteEvent { public bool Success; public MissionStats Stats; }

public struct WaveStartedEvent     { public int WaveNumber; public int EnemyCount; }

public struct GrenadeUsedEvent     { public int Remaining; }

public struct LevelUpEvent         { public int NewLevel; }

public struct SoldierDamagedEvent  { public int SoldierId; public float HP; public float MaxHP; }

### 4.2 Game State Machine

Головна стейт-машина керує потоком гри. Переходи чіткі та передбачувані.

// ═══ GameStateMachine — основні стани ═══

public enum GameState {

    Boot,           // Ініціалізація сервісів, завантаження збережень

    BootCamp,       // Мета-хаб: вибір місії, Hill of Fame

    MissionBrief,   // Брифінг перед забігом (3 сек, гумористичний текст)

    MissionPlay,    // Активний геймплей

    PerkSelection,  // Пауза: вибір 1 з 3 перків при level-up

    MissionResult,  // Підсумки: хто вижив, нагороди, нові надгробки

    GameOver        // Весь загін загинув

}

// ═══ Діаграма переходів ═══

//

// Boot ──────────► BootCamp

//                    │

//                    ▼

//               MissionBrief

//                    │

//                    ▼

//              ┌─ MissionPlay ◄──┐

//              │      │          │

//              │      ▼          │

//              │ PerkSelection ──┘

//              │

//         ┌────┴────┐

//         ▼         ▼

//    MissionResult  GameOver

//         │         │

//         └────┬────┘

//              ▼

//          BootCamp

### 4.3 Ігровий цикл (Update Order)

Порядок виконання систем у кожному кадрі критичний для коректності та продуктивності. Використовуємо Unity Script Execution Order.

| Порядок | Система | Що робить |
| :---- | :---- | :---- |
| \-100 | `InputSystem` | Зчитує touch input, обчислює stick vectors |
| \-50 | `SquadMovement` | Рухає загін за input, оновлює формацію |
| 0 | `CombatSystem` | Обробляє стрільбу, влучання, урон |
| 0 | `EnemyAI` | Оновлює AI-стани ворогів, рух, атаки |
| 50 | `ProjectileManager` | Рухає активні кулі, перевіряє колізії |
| 100 | `PerkEngine` | Застосовує пасивні ефекти перків (heal/sec тощо) |
| 150 | `HealthSystem` | Обробляє смерті, тригерить `SoldierDiedEvent` |
| 200 | `CameraController` | Слідує за загоном, застосовує shake |
| 250 | `UIManager` | Оновлює HUD (HP, ammo, minimap) |
| 300 | `VFXManager` | Pool-based VFX lifecycle |

---

## 5\. Система введення (Twin-Stick Controls)

Система введення — найкритичніша частина для feel гри. Використовуємо Unity New Input System для мультиплатформності з кастомним Touch Stick контролером для максимального відгуку.

### 5.1 Input Architecture

// InputActions.inputactions (Unity Input System asset)

// Action Map: Gameplay

//   Move     (Value, Vector2)   — Left Stick / WASD

//   Aim      (Value, Vector2)   — Right Stick / Mouse Position

//   Fire     (Button)           — Auto або Right Stick натиснутий

//   Grenade  (Button)           — Тап по кнопці гранати

//   Special  (Button)           — Кнопка спецздібності (POST-MVP)

//   Pause    (Button)           — Кнопка паузи

// Action Map: UI

//   Navigate, Submit, Cancel    — стандартні UI actions

### 5.2 Touch Stick Controller

Кастомний floating joystick для landscape. Лівий — завжди floating (з'являється де палець торкнувся). Правий — floating з auto-fire toggle.

// TouchStickController.cs

public class TouchStickController : MonoBehaviour {

    \[Header("Configuration")\]

    \[SerializeField\] float \_stickRadius \= 120f;        // Радіус у screen-px

    \[SerializeField\] float \_deadZone \= 0.15f;           // Нормалізована dead zone

    \[SerializeField\] bool \_isFloating \= true;           // Floating vs fixed base

    \[SerializeField\] RectTransform \_stickBase;          // UI елемент бази

    \[SerializeField\] RectTransform \_stickKnob;          // UI елемент шишки

    // Output

    public Vector2 Direction { get; private set; }      // Нормалізований (-1..1)

    public float Magnitude { get; private set; }         // 0..1

    public bool IsPressed { get; private set; }

    int \_touchId \= \-1;

    Vector2 \_basePosition;

    void Update() {

        foreach (var touch in Input.touches) {

            // Початок дотику в зоні стика

            if (touch.phase \== TouchPhase.Began && IsInMyZone(touch.position)) {

                \_touchId \= touch.fingerId;

                if (\_isFloating) {

                    \_basePosition \= touch.position;

                    \_stickBase.position \= touch.position;

                }

                IsPressed \= true;

            }

            // Рух пальця

            if (touch.fingerId \== \_touchId) {

                var delta \= (touch.position \- \_basePosition) / \_stickRadius;

                Magnitude \= Mathf.Clamp01(delta.magnitude);

                Direction \= Magnitude \> \_deadZone

                    ? delta.normalized

                    : Vector2.zero;

                // Візуальний фідбек

                \_stickKnob.position \= \_basePosition

                    \+ Direction \* Magnitude \* \_stickRadius;

            }

            // Кінець дотику

            if ((touch.phase \== TouchPhase.Ended

                || touch.phase \== TouchPhase.Canceled)

                && touch.fingerId \== \_touchId) {

                \_touchId \= \-1;

                IsPressed \= false;

                Direction \= Vector2.zero;

                Magnitude \= 0;

                \_stickKnob.localPosition \= Vector2.zero;

            }

        }

    }

    bool IsInMyZone(Vector2 screenPos) {

        // Ліва половина екрана для Move, права для Aim

        // З урахуванням Safe Area

        return RectTransformUtility.RectangleContainsScreenPoint(

            \_stickBase.parent as RectTransform, screenPos);

    }

}

### 5.3 Auto-Aim System

// AutoAimSystem.cs

// Логіка:

// Якщо правий стик НЕ натиснутий І autoAim включений (default ON):

//   1\. Знаходимо найближчого ворога у зоні видимості

//      \- FOV: 120°, Range: 15 units

//   2\. Пріоритети:

//      \- Найнижчий HP \> Найближчий \> Загрозливий (Rusher)

//   3\. Плавно повертаємо приціл (Lerp, 15°/frame) для natural feel

//   4\. Стріляємо автоматично коли ворог у конусі (±5°)

//

// Якщо правий стик НАТИСНУТИЙ:

//   \- Manual aim override, auto-aim вимикається

//   \- Стрільба поки стик натиснутий

public class AutoAimSystem : MonoBehaviour {

    \[SerializeField\] float \_detectionRange \= 15f;

    \[SerializeField\] float \_detectionFOV \= 120f;

    \[SerializeField\] float \_aimLerpSpeed \= 15f;

    \[SerializeField\] float \_fireAngleThreshold \= 5f;

    Transform \_currentTarget;

    float \_retargetCooldown \= 0.2f; // Не міняємо ціль кожен кадр

    float \_retargetTimer;

    public Vector3 GetAimDirection(Vector3 squadCenter) {

        if (\_currentTarget \== null || \!\_currentTarget.gameObject.activeSelf)

            FindBestTarget(squadCenter);

        if (\_currentTarget \== null) return Vector3.forward;

        var dir \= (\_currentTarget.position \- squadCenter).normalized;

        return Vector3.Lerp(transform.forward, dir,

            \_aimLerpSpeed \* Time.deltaTime);

    }

    void FindBestTarget(Vector3 from) {

        // Physics.OverlapSphere \+ angle check \+ priority scoring

        var colliders \= Physics.OverlapSphere(from, \_detectionRange,

            LayerMask.GetMask("Enemy"));

        Transform best \= null;

        float bestScore \= float.MinValue;

        foreach (var col in colliders) {

            var dir \= col.transform.position \- from;

            if (Vector3.Angle(transform.forward, dir) \> \_detectionFOV \* 0.5f)

                continue;

            var enemy \= col.GetComponent\<EnemyAI\>();

            if (enemy \== null || \!enemy.IsAlive) continue;

            // Score: closer \+ lower HP \+ threat type

            float score \= 0;

            score \+= (1f \- dir.magnitude / \_detectionRange) \* 50f;  // Distance

            score \+= (1f \- enemy.HPPercent) \* 30f;                   // Low HP

            if (enemy.Type \== EnemyType.Rusher) score \+= 40f;        // Threat

            if (enemy.Type \== EnemyType.Sniper) score \+= 20f;

            if (score \> bestScore) {

                bestScore \= score;

                best \= col.transform;

            }

        }

        \_currentTarget \= best;

    }

}

---

## 6\. Система загону та Permadeath

### 6.1 SquadManager

Центральний компонент, що керує всіма 4 солдатами як єдиною бойовою одиницею з можливістю індивідуальних втрат.

// SquadManager.cs

public class SquadManager : MonoBehaviour, ISquadManager {

    \[SerializeField\] SoldierController \_soldierPrefab;

    \[SerializeField\] FormationConfig\[\] \_formations;

    List\<SoldierController\> \_activeSoldiers \= new(4);

    FormationType \_currentFormation \= FormationType.Diamond;

    public int AliveCount \=\> \_activeSoldiers.Count(s \=\> s.IsAlive);

    public bool IsSquadWiped \=\> AliveCount \== 0;

    public Vector3 SquadCenter \=\> GetAliveCentroid();

    // ═══ Формації (зміщення від лідера) ═══

    // Diamond:  \[0,0\], \[-1,-1\], \[1,-1\], \[0,-2\]    ← default

    // Line:     \[0,0\], \[0,-1.5\], \[0,-3\], \[0,-4.5\]

    // Wide:     \[-2,0\], \[-0.7,0\], \[0.7,0\], \[2,0\]

    // Tight:    \[-0.5,-0.5\], \[0.5,-0.5\], \[-0.5,0.5\], \[0.5,0.5\]

    public void MoveTo(Vector2 inputDir) {

        var leader \= GetLeader();

        leader.Move(inputDir);

        for (int i \= 0; i \< \_activeSoldiers.Count; i++) {

            if (\!\_activeSoldiers\[i\].IsAlive) continue;

            if (\_activeSoldiers\[i\] \== leader) continue;

            var offset \= GetFormationOffset(i, \_currentFormation);

            var targetPos \= leader.Position

                \+ RotateOffset(offset, leader.Facing);

            \_activeSoldiers\[i\].MoveTowards(targetPos, followSpeed: 4.5f);

        }

    }

    SoldierController GetLeader() {

        // Лідер \= перший живий солдат

        // Якщо поточний лідер загинув — наступний автоматично стає лідером

        return \_activeSoldiers.First(s \=\> s.IsAlive);

    }

    Vector3 GetAliveCentroid() {

        var alive \= \_activeSoldiers.Where(s \=\> s.IsAlive).ToList();

        if (alive.Count \== 0\) return Vector3.zero;

        return alive.Aggregate(Vector3.zero, (sum, s) \=\> sum \+ s.Position)

            / alive.Count;

    }

    Vector2 RotateOffset(Vector2 offset, Vector2 facing) {

        float angle \= Mathf.Atan2(facing.y, facing.x);

        float cos \= Mathf.Cos(angle), sin \= Mathf.Sin(angle);

        return new Vector2(

            offset.x \* cos \- offset.y \* sin,

            offset.x \* sin \+ offset.y \* cos);

    }

    public void ApplyModifiers(StatModifiers mods) {

        foreach (var s in \_activeSoldiers)

            if (s.IsAlive) s.SetModifiers(mods);

    }

}

### 6.2 SoldierController

// SoldierController.cs

public class SoldierController : MonoBehaviour {

    // ═══ Identity (генерується або з бази) ═══

    public string SoldierName { get; set; }    // «Stoo Cameron»

    public Color HelmetColor { get; set; }      // Унікальний колір

    public int MissionsCompleted { get; set; }  // Для ветеранського статусу

    public int KillCount { get; set; }

    // ═══ Base Stats ═══

    \[SerializeField\] float \_baseHP \= 100f;

    \[SerializeField\] float \_baseMoveSpeed \= 5f;

    \[SerializeField\] float \_baseAccuracy \= 0.85f;

    float \_currentHP;

    bool \_isAlive \= true;

    public bool IsAlive \=\> \_isAlive;

    public Vector3 Position \=\> transform.position;

    public Vector2 Facing { get; private set; }

    // ═══ Modified stats (base \+ perks \+ veteran bonuses) ═══

    StatModifiers \_modifiers;

    public float MaxHP \=\> \_baseHP \* (1f \+ \_modifiers.hpMultiplier);

    public float MoveSpeed \=\> \_baseMoveSpeed \* (1f \+ \_modifiers.speedMultiplier);

    public float Accuracy \=\> Mathf.Clamp01(\_baseAccuracy \+ \_modifiers.accuracyBonus);

    public float HPPercent \=\> \_currentHP / MaxHP;

    public void Init(string name, Color helmetColor) {

        SoldierName \= name;

        HelmetColor \= helmetColor;

        \_currentHP \= MaxHP;

        \_isAlive \= true;

        // Apply helmet color to material

        GetComponentInChildren\<Renderer\>().material

            .SetColor("\_BaseColor", helmetColor);

    }

    public void SetModifiers(StatModifiers mods) {

        float hpPercent \= HPPercent;

        \_modifiers \= mods;

        \_currentHP \= MaxHP \* hpPercent; // Зберігаємо % HP

    }

    public void Move(Vector2 direction) {

        if (\!\_isAlive) return;

        var move \= new Vector3(direction.x, 0, direction.y) \* MoveSpeed \* Time.deltaTime;

        transform.position \+= move;

        if (direction.sqrMagnitude \> 0.01f)

            Facing \= direction.normalized;

    }

    public void MoveTowards(Vector3 target, float followSpeed) {

        if (\!\_isAlive) return;

        transform.position \= Vector3.MoveTowards(

            transform.position, target,

            followSpeed \* Time.deltaTime);

        var dir \= (target \- transform.position);

        if (dir.sqrMagnitude \> 0.01f)

            Facing \= new Vector2(dir.x, dir.z).normalized;

    }

    public void TakeDamage(float damage, string source \= "") {

        if (\!\_isAlive) return;

        \_currentHP \-= damage;

        EventBus.Publish(new SoldierDamagedEvent {

            SoldierId \= GetInstanceID(),

            HP \= \_currentHP,

            MaxHP \= MaxHP

        });

        if (\_currentHP \<= MaxHP \* 0.3f)

            SetWounded(true); // Анімація шкутильгання

        if (\_currentHP \<= 0\)

            Die(source);

    }

    void Die(string causeOfDeath \= "") {

        \_isAlive \= false;

        // 1\. Spawn tombstone

        var tombData \= new TombstoneData {

            soldierName \= SoldierName,

            killCount \= KillCount,

            missionsCompleted \= MissionsCompleted,

            causeOfDeath \= causeOfDeath,

            epitaph \= EpitaphGenerator.Generate(this),

            timestamp \= DateTimeOffset.UtcNow.ToUnixTimeSeconds(),

            missionBiome \= "Jungle" // TODO: current biome

        };

        TombstonePool.Spawn(tombData, transform.position);

        // 2\. Death VFX (cartoon puff \+ «pop\!» SFX)

        VFXManager.Play(VFXType.SoldierDeath, transform.position);

        AudioManager.PlaySFX(SFXType.SoldierDeath);

        // 3\. Camera shake (light, 0.3 sec)

        Services.Get\<CameraController\>().Shake(0.15f, 0.3f);

        // 4\. Broadcast event

        EventBus.Publish(new SoldierDiedEvent {

            SoldierId \= GetInstanceID(),

            Name \= SoldierName,

            Position \= transform.position,

            KillCount \= KillCount,

            MissionsCompleted \= MissionsCompleted

        });

        // 5\. Hide soldier (not destroy — pool reuse)

        gameObject.SetActive(false);

    }

    void SetWounded(bool wounded) {

        // Animator trigger: "Wounded"

        // Зменшена швидкість руху (x0.6)

        // Червона індикація на HUD портреті

        GetComponent\<Animator\>().SetBool("IsWounded", wounded);

    }

}

### 6.3 StatModifiers

// StatModifiers.cs — агреговані модифікатори від перків

\[System.Serializable\]

public struct StatModifiers {

    public float damageMultiplier;     // \+1.0 \= \+100% damage

    public float hpMultiplier;         // \-0.5 \= \-50% HP

    public float speedMultiplier;

    public float accuracyBonus;

    public float fireRateMultiplier;

    public float resistanceBonus;      // 0.2 \= \-20% incoming damage

    public float healPerSecond;

    public float healOnKill;

    public float spreadAngleModifier;

    public PerkSpecial activeSpecials; // Flags для спеціальних ефектів

}

### 6.4 Tombstone та Hill of Fame

// TombstoneData.cs — зберігається в SaveSystem

\[System.Serializable\]

public class TombstoneData {

    public string soldierName;

    public int killCount;

    public int missionsCompleted;

    public string causeOfDeath;     // Генерується: «Гранатою свого командира»

    public string epitaph;          // Процедурна епітафія

    public long timestamp;          // Unix timestamp

    public string missionBiome;

}

// EpitaphGenerator.cs — генерує гумористичні епітафії

public static class EpitaphGenerator {

    static readonly string\[\] \_templates \= {

        "{0}. Влучив у {1} ворогів. {2}",

        "Тут лежить {0}. {2}. Йому було все одно.",

        "R.I.P. {0}. Kills: {1}. {2}",

    };

    static readonly string\[\] \_jokes \= {

        "Помер, бо командир забув кинути гранату",

        "Останні слова: «Тут точно безпечно»",

        "Його мама досі чекає на дзвінок",

        "Пережив 3 війни, не пережив 4-ту",

        "Просто стояв не в тому місці",

        "Вірив, що танки не стріляють по піхоті",

        "Думав, що снайпер промахнеться",

        "Спіймав гранату. Не свою.",

        "Героїчно прикрив відступ. Ніхто не відступав.",

        "Вижив у 5 місіях. Помер у 6-й від дружнього вогню.",

        // ... 20+ варіантів

    };

    public static string Generate(SoldierController soldier) {

        var template \= \_templates\[Random.Range(0, \_templates.Length)\];

        var joke \= \_jokes\[Random.Range(0, \_jokes.Length)\];

        return string.Format(template, soldier.SoldierName,

            soldier.KillCount, joke);

    }

}

---

## 7\. Бойова система та балістика

### 7.1 Weapon System (ScriptableObject-driven)

// WeaponConfig.cs (ScriptableObject)

\[CreateAssetMenu(menuName \= "LPF/Weapon Config")\]

public class WeaponConfig : ScriptableObject {

    \[Header("Stats")\]

    public string weaponName;

    public float damage \= 10f;

    public float fireRate \= 8f;           // Пострілів/сек

    public float bulletSpeed \= 20f;       // Units/sec

    public float range \= 15f;             // Max дальність

    public float spreadAngle \= 5f;        // Кут розкиду (градуси)

    \[Header("Ammo")\]

    public int magSize \= 30;

    public int reserveAmmo \= 120;

    public float reloadTime \= 1.2f;       // Секунди

    \[Header("Feedback")\]

    public AudioClip fireSound;

    public GameObject muzzleFlashPrefab;

    public GameObject bulletTrailPrefab;

    \[Header("Modifiers")\]

    public bool hasFalloff \= false;       // Damage falloff за відстанню

    public float falloffStart \= 10f;

    public DamageType damageType \= DamageType.Bullet; // Bullet, Explosive, Special

}

### 7.2 Projectile Pool

Кулі — найчастіші об'єкти в грі. 4 солдати x 8 пострілів/сек \= 32 кулі/сек. **Обов'язковий Object Pool.**

// ═══ Generic ObjectPool\<T\> ═══

public class ObjectPool\<T\> where T : Component {

    readonly T \_prefab;

    readonly Transform \_parent;

    readonly Queue\<T\> \_pool;

    readonly int \_initialSize;

    public ObjectPool(T prefab, Transform parent, int initialSize \= 32\) {

        \_prefab \= prefab;

        \_parent \= parent;

        \_initialSize \= initialSize;

        \_pool \= new Queue\<T\>(initialSize);

        // Pre-warm

        for (int i \= 0; i \< initialSize; i++) {

            var obj \= Object.Instantiate(prefab, parent);

            obj.gameObject.SetActive(false);

            \_pool.Enqueue(obj);

        }

    }

    public T Get() {

        var obj \= \_pool.Count \> 0

            ? \_pool.Dequeue()

            : Object.Instantiate(\_prefab, \_parent);

        obj.gameObject.SetActive(true);

        return obj;

    }

    public void Return(T obj) {

        obj.gameObject.SetActive(false);

        \_pool.Enqueue(obj);

    }

}

// ═══ Projectile.cs ═══

public class Projectile : MonoBehaviour {

    float \_speed, \_damage, \_maxDistance;

    Vector3 \_startPos, \_direction;

    LayerMask \_targetLayer;

    System.Action\<Projectile\> \_returnToPool;

    public void Init(Vector3 pos, Vector3 dir, float speed,

        float damage, float range, LayerMask targetLayer,

        System.Action\<Projectile\> returnCallback) {

        transform.position \= pos;

        \_startPos \= pos;

        \_direction \= dir.normalized;

        \_speed \= speed;

        \_damage \= damage;

        \_maxDistance \= range;

        \_targetLayer \= targetLayer;

        \_returnToPool \= returnCallback;

    }

    void Update() {

        // Рух

        float step \= \_speed \* Time.deltaTime;

        transform.position \+= \_direction \* step;

        // Перевірка дальності

        if (Vector3.Distance(transform.position, \_startPos) \> \_maxDistance) {

            \_returnToPool(this);

            return;

        }

        // Raycast для влучання (short ray для швидких куль)

        if (Physics.Raycast(transform.position \- \_direction \* step,

            \_direction, out var hit, step \* 1.5f, \_targetLayer)) {

            OnHit(hit);

        }

    }

    void OnHit(RaycastHit hit) {

        // Урон

        var damageable \= hit.collider.GetComponent\<IDamageable\>();

        damageable?.TakeDamage(\_damage);

        // VFX

        VFXManager.Play(VFXType.BulletImpact, hit.point);

        // Повернути в пул

        \_returnToPool(this);

    }

}

### 7.3 Grenade System

// GrenadeSystem.cs

public class GrenadeSystem : MonoBehaviour {

    \[Header("Config")\]

    \[SerializeField\] float \_throwForce \= 12f;

    \[SerializeField\] float \_arcHeight \= 3f;

    \[SerializeField\] float \_flightTime \= 0.8f;

    \[SerializeField\] float \_explosionRadius \= 3f;

    \[SerializeField\] float \_explosionDamage \= 80f;

    \[SerializeField\] LayerMask \_damageLayer; // Enemy \+ Soldier (friendly fire\!)

    int \_currentGrenades \= 2;

    public int GrenadeCount \=\> \_currentGrenades;

    public void ThrowGrenade(Vector3 origin, Vector3 direction) {

        if (\_currentGrenades \<= 0\) return;

        \_currentGrenades--;

        EventBus.Publish(new GrenadeUsedEvent { Remaining \= \_currentGrenades });

        var grenade \= GrenadePool.Get();

        var target \= origin \+ direction \* \_throwForce;

        StartCoroutine(GrenadeArc(grenade, origin, target));

    }

    IEnumerator GrenadeArc(GameObject grenade, Vector3 start, Vector3 end) {

        float elapsed \= 0f;

        while (elapsed \< \_flightTime) {

            elapsed \+= Time.deltaTime;

            float t \= elapsed / \_flightTime;

            // Parabolic arc

            var pos \= Vector3.Lerp(start, end, t);

            pos.y \+= \_arcHeight \* 4f \* t \* (1f \- t); // Peak at midpoint

            grenade.transform.position \= pos;

            yield return null;

        }

        Explode(end);

        GrenadePool.Return(grenade);

    }

    void Explode(Vector3 position) {

        // VFX

        VFXManager.Play(VFXType.GrenadeExplosion, position);

        AudioManager.PlaySFX(SFXType.GrenadeExplosion, position);

        Services.Get\<CameraController\>().Shake(0.25f, 0.4f);

        // AoE Damage (includes friendly fire\!)

        var colliders \= Physics.OverlapSphere(position,

            \_explosionRadius, \_damageLayer);

        foreach (var col in colliders) {

            var damageable \= col.GetComponent\<IDamageable\>();

            if (damageable \== null) continue;

            // Damage falloff від центру вибуху

            float dist \= Vector3.Distance(position, col.transform.position);

            float falloff \= 1f \- (dist / \_explosionRadius);

            damageable.TakeDamage(\_explosionDamage \* falloff, "Grenade");

        }

        // Check for Cluster Bombs perk

        if (Services.Get\<PerkEngine\>()

            .HasSpecial(PerkSpecial.ClusterBombs)) {

            SpawnClusterBombs(position, 3);

        }

    }

}

### 7.4 Damage Pipeline

Увесь урон проходить через єдиний pipeline для консистентності та можливості модифікації перками.

// DamagePipeline.cs

public static class DamagePipeline {

    public static float Calculate(DamageContext ctx) {

        float dmg \= ctx.BaseDamage;

        // 1\. Weapon modifiers (Armor Piercing, Hollow Points)

        dmg \*= ctx.WeaponModifier;

        // 2\. Perk modifiers (Glass Cannon, Last Stand)

        var perks \= Services.Get\<PerkEngine\>();

        dmg \*= (1f \+ perks.GetDamageMultiplier());

        // Last Stand: \+200% коли 1 солдат залишився

        if (perks.HasSpecial(PerkSpecial.LastStand)

            && Services.Get\<ISquadManager\>().AliveCount \== 1\) {

            dmg \*= 3f; // \+200%

        }

        // 3\. Veteran bonus (+5% per mission completed)

        if (ctx.AttackerVeteranRank \> 0\)

            dmg \*= 1f \+ ctx.AttackerVeteranRank \* 0.05f;

        // 4\. Target resistance (Tank armor, Phalanx perk)

        dmg \*= (1f \- ctx.TargetResistance);

        // 5\. Distance falloff (optional, per weapon config)

        if (ctx.HasFalloff) {

            float t \= Mathf.InverseLerp(0, ctx.MaxRange, ctx.Distance);

            dmg \*= Mathf.Lerp(1f, 0.5f, t);

        }

        return Mathf.Max(1f, dmg); // Мінімум 1 урон

    }

}

// DamageContext.cs

public struct DamageContext {

    public float BaseDamage;

    public float WeaponModifier;      // Armor Piercing etc.

    public int AttackerVeteranRank;

    public float TargetResistance;

    public bool HasFalloff;

    public float Distance;

    public float MaxRange;

    public DamageType Type;           // Bullet, Explosive, Special

}

---

## 8\. Roguelike-перки (Runtime Buff System)

### 8.1 PerkDefinition (ScriptableObject)

// PerkDefinition.cs

\[CreateAssetMenu(menuName \= "LPF/Perk")\]

public class PerkDefinition : ScriptableObject {

    \[Header("Identity")\]

    public string perkName;

    public string description;

    public Sprite icon;

    public PerkCategory category;  // HeavyOrdinance, Tactical, Medic, Hardware, Cursed

    public PerkRarity rarity;      // Common (70%), Rare (25%), Epic (5%)

    \[Header("Stacking")\]

    public bool isStackable;

    public int maxStacks \= 1;

    \[Header("Stat Modifiers (fill needed, leave 0 for unused)")\]

    public float damageMultiplier;        // \+100% \= 1.0

    public float hpMultiplier;            // \-50% \= \-0.5

    public float speedMultiplier;

    public float accuracyBonus;

    public float fireRateMultiplier;

    public int extraGrenades;

    public float grenadeCooldownReduction;

    public float healPerSecond;

    public float healOnKill;

    public float spreadAngleModifier;     // Tactical Spread: \+30

    public float resistanceBonus;         // Phalanx: \+0.2

    \[Header("Special Effects")\]

    public PerkSpecial specialEffect;

}

public enum PerkCategory {

    HeavyOrdinance,

    Tactical,

    Medic,

    Hardware,

    Cursed

}

public enum PerkRarity {

    Common,   // 70% chance

    Rare,     // 25% chance

    Epic      // 5% chance

}

\[Flags\]

public enum PerkSpecial {

    None            \= 0,

    ClusterBombs    \= 1 \<\< 0,   // Гранати розпадаються на 3

    LastStand       \= 1 \<\< 1,   // \+200% damage коли 1 солдат

    Berserker       \= 1 \<\< 2,   // \+5% speed per kill

    AdrenalineRush  \= 1 \<\< 3,   // \+50% speed при \<30% HP

    CarpetBomber    \= 1 \<\< 4,   // AirStrike кожні 60 сек

}

### 8.2 PerkEngine (Runtime)

// PerkEngine.cs

public class PerkEngine : MonoBehaviour {

    \[SerializeField\] PerkDefinition\[\] \_allPerks; // 15 на MVP

    List\<PerkInstance\> \_activePerks \= new();

    StatModifiers \_cachedModifiers;

    // ═══ При level-up: показати 3 карти вибору ═══

    public PerkDefinition\[\] GetChoices(int count \= 3\) {

        var available \= \_allPerks

            .Where(p \=\> CanTake(p))

            .ToList();

        // Weighted random за rarity

        var weights \= available.Select(p \=\> p.rarity switch {

            PerkRarity.Common \=\> 70f,

            PerkRarity.Rare   \=\> 25f,

            PerkRarity.Epic   \=\> 5f,

            \_ \=\> 70f

        }).ToList();

        var chosen \= new List\<PerkDefinition\>();

        for (int i \= 0; i \< count && available.Count \> 0; i++) {

            int idx \= WeightedRandom(weights);

            chosen.Add(available\[idx\]);

            available.RemoveAt(idx);

            weights.RemoveAt(idx);

        }

        // Гарантуємо різні категорії якщо можливо

        // (опціонально, покращує різноманітність)

        return chosen.ToArray();

    }

    bool CanTake(PerkDefinition def) {

        int stacks \= \_activePerks.Count(p \=\> p.Def \== def);

        if (\!def.isStackable && stacks \> 0\) return false;

        if (stacks \>= def.maxStacks) return false;

        return true;

    }

    public void ApplyPerk(PerkDefinition def) {

        \_activePerks.Add(new PerkInstance(def));

        RecalculateModifiers();

        EventBus.Publish(new PerkSelectedEvent { Perk \= def });

    }

    void RecalculateModifiers() {

        var mods \= new StatModifiers();

        foreach (var p in \_activePerks) {

            mods.damageMultiplier      \+= p.Def.damageMultiplier;

            mods.hpMultiplier          \+= p.Def.hpMultiplier;

            mods.speedMultiplier       \+= p.Def.speedMultiplier;

            mods.accuracyBonus         \+= p.Def.accuracyBonus;

            mods.fireRateMultiplier    \+= p.Def.fireRateMultiplier;

            mods.resistanceBonus       \+= p.Def.resistanceBonus;

            mods.healPerSecond         \+= p.Def.healPerSecond;

            mods.healOnKill            \+= p.Def.healOnKill;

            mods.spreadAngleModifier   \+= p.Def.spreadAngleModifier;

            mods.activeSpecials        |= p.Def.specialEffect;

        }

        \_cachedModifiers \= mods;

        Services.Get\<ISquadManager\>().ApplyModifiers(mods);

    }

    // ═══ Query methods ═══

    public float GetDamageMultiplier() \=\> \_cachedModifiers.damageMultiplier;

    public bool HasSpecial(PerkSpecial special) \=\>

        (\_cachedModifiers.activeSpecials & special) \!= 0;

    public float GetHealPerSecond() \=\> \_cachedModifiers.healPerSecond;

    public float GetHealOnKill() \=\> \_cachedModifiers.healOnKill;

}

public class PerkInstance {

    public PerkDefinition Def { get; }

    public float TimeAcquired { get; }

    public int Stacks { get; set; } \= 1;

    public PerkInstance(PerkDefinition def) {

        Def \= def;

        TimeAcquired \= Time.time;

    }

}

### 8.3 MVP: 15 перків

| Категорія | Перк | Ефект | Рідкість |
| :---- | :---- | :---- | :---- |
| Heavy Ordinance | Grenade Spam | \+1 граната, \-25% cooldown | Common |
| Heavy Ordinance | Cluster Bombs | Гранати розпадаються на 3 | Rare |
| Heavy Ordinance | Carpet Bomber | AirStrike кожні 60 сек | Epic |
| Tactical | Tactical Spread | \+30° конус стрільби загону | Common |
| Tactical | Phalanx | \-20% урон, солдати впритул | Rare |
| Tactical | Disperse | Збільшена дистанція, \-AoE вразливість | Common |
| Medic | Medic Pack | \+1 HP/сек поза боєм | Common |
| Medic | Adrenaline | \+50% speed при \<30% HP | Rare |
| Medic | Combat Stim | \+5 HP за кожне вбивство | Common |
| Hardware | Armor Piercing | \+20% урон по техніці | Common |
| Hardware | Hollow Points | \+35% урон по піхоті, \-20% по техніці | Common |
| Hardware | Extended Mag | \+50% боєкомплект | Common |
| Cursed | Glass Cannon | \+100% damage, \-50% HP | Rare |
| Cursed | Last Stand | \+200% damage для останнього солдата | Epic |
| Cursed | Berserker | \+5% speed за кожне вбивство | Rare |

---

## 9\. Система ворогів та AI

### 9.1 EnemyConfig (ScriptableObject)

// EnemyConfig.cs

\[CreateAssetMenu(menuName \= "LPF/Enemy Config")\]

public class EnemyConfig : ScriptableObject {

    \[Header("Identity")\]

    public string enemyName;

    public EnemyType type;

    \[Header("Stats")\]

    public float hp \= 50f;

    public float damage \= 10f;

    public float moveSpeed \= 3f;

    public float attackRange \= 8f;

    public float attackCooldown \= 1f;

    public float detectionRange \= 12f;

    \[Header("Rewards")\]

    public float xpReward \= 10f;

    public int coinReward \= 5;

    public float ammoDropChance \= 0.3f;

    public float grenadeDropChance \= 0.1f;

    public float healthDropChance \= 0.15f;

    \[Header("Audio")\]

    public AudioClip\[\] voiceLines;

    public AudioClip deathSound;

    public AudioClip attackSound;

    \[Header("Visual")\]

    public GameObject deathVFX;

    public Color outlineColor \= Color.red;

}

public enum EnemyType {

    Grunt,

    Sniper,

    Rusher,

    RocketSoldier,

    LightTank,

    // POST-MVP:

    // HeavyTank, Helicopter, AmbulanceTank

}

### 9.2 AI State Machine

Кожен ворог використовує простий FSM (Finite State Machine) з 4–6 станами. На MVP не потрібен Behavior Tree — FSM достатньо для 5 типів ворогів.

| Тип ворога | HP | Damage | Speed | Стани AI | Особлива поведінка |
| :---- | ----: | ----: | ----: | :---- | :---- |
| Grunt (Salaga) | 50 | 8 | 3.0 | Idle → Patrol → Chase → Attack → Die | Стріляє з зупинкою, погана точність (60%) |
| Sniper | 40 | 35 | 1.5 | Idle → Overwatch → Aim(1.5s) → Fire → Relocate | Лазерний приціл попереджає гравця, одна куля |
| Rusher (Бомбер) | 30 | 80 (AoE) | 6.0 | Idle → Detect → Sprint → Explode | Біжить на загін, вибухає при контакті або 3 сек |
| Rocket Soldier | 60 | 40 (AoE) | 2.0 | Idle → Patrol → AimRocket(2s) → Fire → Cooldown(3s) | Повільна ракета з AoE, помітна траєкторія |
| Light Tank | 200 | 25 | 1.5 | Patrol(slow) → Engage → Fire(burst) → Cooldown | Імунітет до куль\! Тільки гранати/спеціальне |

// ═══ EnemyAI.cs (базовий клас) ═══

public abstract class EnemyAI : MonoBehaviour, IDamageable {

    \[SerializeField\] protected EnemyConfig \_config;

    protected EnemyState \_currentState \= EnemyState.Idle;

    protected Transform \_target;

    protected float \_currentHP;

    protected float \_stateTimer;

    public bool IsAlive \=\> \_currentHP \> 0;

    public float HPPercent \=\> \_currentHP / \_config.hp;

    public EnemyType Type \=\> \_config.type;

    protected virtual void Start() {

        \_currentHP \= \_config.hp;

    }

    protected virtual void Update() {

        if (\!IsAlive) return;

        \_stateTimer \+= Time.deltaTime;

        UpdateTargeting();

        switch (\_currentState) {

            case EnemyState.Idle:    OnIdle();    break;

            case EnemyState.Patrol:  OnPatrol();  break;

            case EnemyState.Chase:   OnChase();   break;

            case EnemyState.Attack:  OnAttack();  break;

            case EnemyState.Custom:  OnCustom();  break;

        }

    }

    // Targeting: оновлюємо кожні 0.2 сек (не кожен кадр)

    float \_targetUpdateTimer;

    void UpdateTargeting() {

        \_targetUpdateTimer \+= Time.deltaTime;

        if (\_targetUpdateTimer \< 0.2f) return;

        \_targetUpdateTimer \= 0;

        var colliders \= Physics.OverlapSphere(

            transform.position, \_config.detectionRange,

            LayerMask.GetMask("Soldier"));

        float minDist \= float.MaxValue;

        \_target \= null;

        foreach (var col in colliders) {

            var soldier \= col.GetComponent\<SoldierController\>();

            if (soldier \== null || \!soldier.IsAlive) continue;

            float dist \= Vector3.Distance(transform.position, col.transform.position);

            if (dist \< minDist) {

                minDist \= dist;

                \_target \= col.transform;

            }

        }

    }

    protected void TransitionTo(EnemyState newState) {

        \_currentState \= newState;

        \_stateTimer \= 0f;

    }

    public virtual void TakeDamage(float damage, string source \= "") {

        \_currentHP \-= damage;

        VFXManager.Play(VFXType.EnemyHit, transform.position);

        if (\_currentHP \<= 0\) Die();

    }

    protected virtual void Die() {

        // XP \+ rewards

        EventBus.Publish(new EnemyKilledEvent {

            EnemyId \= GetInstanceID(),

            Type \= \_config.type,

            XPReward \= \_config.xpReward

        });

        // Drop loot

        TryDropLoot();

        // VFX \+ SFX

        VFXManager.Play(VFXType.EnemyDeath, transform.position);

        AudioManager.PlaySFX(SFXType.EnemyDeath, transform.position);

        // Return to pool

        gameObject.SetActive(false);

    }

    void TryDropLoot() {

        if (Random.value \< \_config.ammoDropChance)

            ItemSpawner.Spawn(ItemType.Ammo, transform.position);

        if (Random.value \< \_config.grenadeDropChance)

            ItemSpawner.Spawn(ItemType.Grenade, transform.position);

        if (Random.value \< \_config.healthDropChance)

            ItemSpawner.Spawn(ItemType.Health, transform.position);

    }

    // Абстрактні стани — реалізуються в дочірніх класах

    protected abstract void OnIdle();

    protected abstract void OnPatrol();

    protected abstract void OnChase();

    protected abstract void OnAttack();

    protected virtual void OnCustom() {} // Для спеціальних станів

}

// ═══ GruntAI.cs ═══

public class GruntAI : EnemyAI {

    Vector3 \_patrolTarget;

    float \_attackCooldown;

    protected override void OnIdle() {

        if (\_target \!= null) TransitionTo(EnemyState.Chase);

        else if (\_stateTimer \> 2f) TransitionTo(EnemyState.Patrol);

    }

    protected override void OnPatrol() {

        if (\_target \!= null) { TransitionTo(EnemyState.Chase); return; }

        // Рух до рандомної точки

        MoveTowards(\_patrolTarget);

        if (Vector3.Distance(transform.position, \_patrolTarget) \< 0.5f)

            \_patrolTarget \= GetRandomPatrolPoint();

    }

    protected override void OnChase() {

        if (\_target \== null) { TransitionTo(EnemyState.Idle); return; }

        float dist \= Vector3.Distance(transform.position, \_target.position);

        if (dist \<= \_config.attackRange) TransitionTo(EnemyState.Attack);

        else MoveTowards(\_target.position);

    }

    protected override void OnAttack() {

        if (\_target \== null) { TransitionTo(EnemyState.Idle); return; }

        // Зупинитись і стріляти

        LookAt(\_target.position);

        \_attackCooldown \-= Time.deltaTime;

        if (\_attackCooldown \<= 0\) {

            Fire(\_target.position, accuracy: 0.6f); // Погана точність

            \_attackCooldown \= \_config.attackCooldown;

        }

        float dist \= Vector3.Distance(transform.position, \_target.position);

        if (dist \> \_config.attackRange \* 1.2f) TransitionTo(EnemyState.Chase);

    }

}

// ═══ RusherAI.cs ═══

public class RusherAI : EnemyAI {

    \[SerializeField\] float \_explosionRadius \= 2.5f;

    \[SerializeField\] float \_sprintSpeed \= 6f;

    bool \_hasExploded;

    protected override void OnIdle() {

        if (\_target \!= null) TransitionTo(EnemyState.Chase);

    }

    protected override void OnPatrol() \=\> OnIdle();

    protected override void OnChase() {

        if (\_target \== null) { TransitionTo(EnemyState.Idle); return; }

        // Біжить прямо на ціль

        MoveTowards(\_target.position, \_sprintSpeed);

        // Крик «За вітчизну\!»

        if (\_stateTimer \> 0.5f && \!\_hasPlayedVoice)

            PlayVoiceLine();

        float dist \= Vector3.Distance(transform.position, \_target.position);

        if (dist \< 1.5f) Explode();

        if (\_stateTimer \> 3f) Explode(); // Таймер вибуху

    }

    protected override void OnAttack() \=\> Explode();

    void Explode() {

        if (\_hasExploded) return;

        \_hasExploded \= true;

        // AoE як граната

        VFXManager.Play(VFXType.GrenadeExplosion, transform.position);

        AudioManager.PlaySFX(SFXType.GrenadeExplosion, transform.position);

        var colliders \= Physics.OverlapSphere(transform.position,

            \_explosionRadius, LayerMask.GetMask("Soldier"));

        foreach (var col in colliders) {

            col.GetComponent\<IDamageable\>()

                ?.TakeDamage(\_config.damage, "Rusher explosion");

        }

        Die();

    }

}

// ═══ LightTankAI.cs ═══

public class LightTankAI : EnemyAI {

    // Override TakeDamage для імунітету до куль

    public override void TakeDamage(float damage, string source \= "") {

        if (source \== "Bullet") {

            // Візуальний фідбек «рикошет»

            VFXManager.Play(VFXType.BulletRicochet, transform.position);

            AudioManager.PlaySFX(SFXType.Ricochet, transform.position);

            return; // Нуль урону\!

        }

        base.TakeDamage(damage, source);

    }

}

### 9.3 Enemy Spawner (Wave System)

// WaveSpawner.cs

public class WaveSpawner : MonoBehaviour {

    \[SerializeField\] WaveConfig\[\] \_waves;

    int \_currentWave \= 0;

    int \_aliveEnemies \= 0;

    public void StartNextWave() {

        if (\_currentWave \>= \_waves.Length) {

            EventBus.Publish(new MissionCompleteEvent { Success \= true });

            return;

        }

        var wave \= \_waves\[\_currentWave\];

        EventBus.Publish(new WaveStartedEvent {

            WaveNumber \= \_currentWave \+ 1,

            EnemyCount \= wave.TotalEnemies

        });

        StartCoroutine(SpawnWave(wave));

        \_currentWave++;

    }

    IEnumerator SpawnWave(WaveConfig wave) {

        foreach (var group in wave.groups) {

            for (int i \= 0; i \< group.count; i++) {

                var pos \= GetSpawnPoint(group.spawnArea);

                var enemy \= EnemyPool.Get(group.enemyType);

                enemy.transform.position \= pos;

                \_aliveEnemies++;

                yield return new WaitForSeconds(group.spawnInterval);

            }

        }

    }

    void OnEnemyKilled(EnemyKilledEvent evt) {

        \_aliveEnemies--;

        if (\_aliveEnemies \<= 0 && \_currentWave \< \_waves.Length) {

            // Пауза між хвилями: drop loot crate \+ 3 sec

            StartCoroutine(InterWavePause());

        }

    }

}

// ═══ MVP Wave Configuration (6 хвиль) ═══

// Wave 1: 4 Grunt                                         (easy start)

// Wave 2: 6 Grunt \+ 1 Sniper                              (introduce Sniper)

// Wave 3: 4 Grunt \+ 2 Rusher                              (introduce Rusher)

// Wave 4: 3 Grunt \+ 1 Rocket Soldier \+ 1 Sniper           (introduce Rocket)

// Wave 5: 2 Grunt \+ 2 Rusher \+ 1 Light Tank               (introduce Tank)

// Wave 6: Light Tank \+ 4 Grunt \+ 2 Sniper (FINAL)         (everything)

//

// Total enemies: \~30

// Очікуваний час: 3–5 хвилин

// Object Pool: pre-warm 20 ворогів (recycled між хвилями)

---

## 10\. Процедурна генерація рівнів

### 10.1 Tile-based System

Рівні будуються з модульних tiles розміром **10x10 Unity units**. Кожен tile — prefab з тегами, що визначають його роль.

// TileConfig.cs (ScriptableObject)

\[CreateAssetMenu(menuName \= "LPF/Tile Config")\]

public class TileConfig : ScriptableObject {

    public string tileName;

    public GameObject prefab;

    public TileType type;          // Start, End, Combat, Corridor, Rest

    public BiomeType biome;        // Jungle (MVP)

    public int coverDensity;       // 0–5 (кількість укриттів)

    public int openness;           // 0–5 (відкритість для обстрілу)

    public bool hasElevation;      // Висота для снайперів

    public Direction\[\] exits;      // Напрямки з'єднань (N/S/E/W)

}

public enum TileType { Start, End, Combat, Corridor, Rest }

public enum Direction { North, South, East, West }

### 10.2 Алгоритм генерації (MVP)

// LevelGenerator.cs

public class LevelGenerator : MonoBehaviour {

    \[SerializeField\] TileConfig\[\] \_tilePool;

    \[SerializeField\] int \_minTiles \= 8;

    \[SerializeField\] int \_maxTiles \= 12;

    public GeneratedLevel Generate(int seed) {

        Random.InitState(seed); // Детерміністичний для replay

        var level \= new GeneratedLevel();

        int tileCount \= Random.Range(\_minTiles, \_maxTiles \+ 1);

        // ═══ Крок 1: Start tile ═══

        level.PlaceTile(Vector2Int.zero,

            GetTileOfType(TileType.Start));

        // ═══ Крок 2: Random walk для основного шляху ═══

        var pos \= Vector2Int.zero;

        for (int i \= 1; i \< tileCount \- 1; i++) {

            var dir \= GetRandomDirection(pos, level);

            pos \+= DirectionToOffset(dir);

            // Ранні tiles: більше Corridor, пізніші: більше Combat

            float combatWeight \= (float)i / tileCount;

            var tileType \= Random.value \< combatWeight

                ? TileType.Combat : TileType.Corridor;

            level.PlaceTile(pos, GetWeightedTile(tileType));

        }

        // ═══ Крок 3: End tile (Boss / Extraction) ═══

        var endDir \= GetRandomDirection(pos, level);

        level.PlaceTile(pos \+ DirectionToOffset(endDir),

            GetTileOfType(TileType.End));

        // ═══ Крок 4: Відгалуження (1–2 loot rooms) ═══

        AddBranch(level, Random.Range(1, 3));

        // ═══ Крок 5: Populate ═══

        PopulateEnemies(level);

        PopulateItems(level);

        // ═══ Крок 6: Easter egg (1% tombstone) ═══

        if (Random.value \< 0.01f)

            PlaceVeteranTombstone(level);

        return level;

    }

    void PopulateEnemies(GeneratedLevel level) {

        // Для кожного Combat tile:

        // \- Визначити складність на основі відстані від Start

        // \- Вибрати mix ворогів (більше elite далі від старту)

        // \- Розмістити на SpawnPoints tile-а

    }

    void PopulateItems(GeneratedLevel level) {

        // Ammo crate: кожні 2–3 tiles

        // Health kit: кожні 3–4 tiles

        // Grenade: кожні 4–5 tiles

        // Правило: ніколи 2 однакових item підряд

    }

}

### 10.3 Tile Prefab Structure

TileRoot (GameObject, 10x10 units)

├── Ground (MeshRenderer)          ← Площина з текстурою біому

├── Cover/                           ← Об'єкти укриття

│   ├── Cover\_01 (BoxCollider)      ← Камінь / бочка / стіна

│   ├── Cover\_02 (BoxCollider)

│   └── Cover\_03 (BoxCollider)

├── Props/                           ← Декоративні (дерева, трава)

│   ├── Tree\_01 (no collider)       ← Не блокують рух

│   └── Grass\_cluster

├── SpawnPoints/                     ← Empty GameObjects

│   ├── EnemySpawn\_01 (tag: "EnemySpawn")

│   ├── EnemySpawn\_02

│   └── ItemSpawn\_01 (tag: "ItemSpawn")

├── Exits/                           ← Маркери з'єднань

│   ├── Exit\_North

│   ├── Exit\_South

│   ├── Exit\_East

│   └── Exit\_West

└── NavMesh (NavMeshSurface)         ← Baked per tile

// Вимоги до tile prefab:

// \- Вхід/вихід через exits повинні з'єднуватись з сусідніми tiles

// \- Cover об'єкти: Layer "Cover", BoxCollider

// \- SpawnPoints на відстані мін. 3 units від входу (щоб вороги не спавнились на гравцеві)

// \- NavMesh bake з AgentType "Enemy" (radius 0.4, height 1.8)

---

## 11\. Камера та рендеринг (URP Pipeline)

### 11.1 Camera Setup

// CameraController.cs

public class CameraController : MonoBehaviour {

    \[Header("Follow")\]

    \[SerializeField\] float \_followSpeed \= 8f;

    \[SerializeField\] Vector3 \_offset \= new(0, 15, \-8);

    \[SerializeField\] float \_cameraAngle \= 60f;

    \[Header("Shake")\]

    \[SerializeField\] float \_maxShakeOffset \= 0.3f;

    \[SerializeField\] AnimationCurve \_shakeFalloff;

    \[Header("Landscape Bounds")\]

    \[SerializeField\] float \_boundsMargin \= 5f;

    Transform \_target; // SquadManager.SquadCenter

    float \_shakeIntensity;

    float \_shakeDuration;

    float \_shakeTimer;

    void LateUpdate() {

        if (\_target \== null) return;

        // Follow

        var desiredPos \= \_target.position \+ \_offset;

        // Clamp до меж рівня (динамічно з LevelGenerator)

        var bounds \= Services.Get\<LevelGenerator\>().GetLevelBounds();

        desiredPos.x \= Mathf.Clamp(desiredPos.x,

            bounds.min.x \+ \_boundsMargin,

            bounds.max.x \- \_boundsMargin);

        desiredPos.z \= Mathf.Clamp(desiredPos.z,

            bounds.min.z \+ \_boundsMargin,

            bounds.max.z \- \_boundsMargin);

        transform.position \= Vector3.Lerp(

            transform.position, desiredPos,

            \_followSpeed \* Time.deltaTime);

        transform.rotation \= Quaternion.Euler(\_cameraAngle, 0, 0);

        // Shake

        if (\_shakeTimer \> 0\) {

            \_shakeTimer \-= Time.deltaTime;

            float t \= 1f \- (\_shakeTimer / \_shakeDuration);

            float falloff \= \_shakeFalloff.Evaluate(t);

            var offset \= Random.insideUnitSphere \* \_shakeIntensity \* falloff;

            transform.position \+= offset;

        }

    }

    public void Shake(float intensity, float duration) {

        \_shakeIntensity \= Mathf.Min(intensity, \_maxShakeOffset);

        \_shakeDuration \= duration;

        \_shakeTimer \= duration;

    }

    public void SetTarget(Transform target) \=\> \_target \= target;

}

### 11.2 URP Configuration

| Параметр URP | Значення (MVP) | Чому |
| :---- | :---- | :---- |
| Render Scale | 1.0 (high), 0.75 (low) | Dynamic resolution fallback |
| Shadow Type | Main Light Only, Soft Shadows OFF | Економія GPU |
| Shadow Resolution | 512 (low), 1024 (high) | Достатньо для top-down |
| Shadow Distance | 20 units | Далі камера не бачить |
| MSAA | 2x (high), Off (low) | Anti-aliasing для low-poly |
| Post Processing | OFF на MVP | Bloom/vignette пізніше |
| SRP Batcher | **ON** | Найбільший performance win |
| GPU Instancing | **ON** для ворогів та куль | Batch identical meshes |
| Depth Texture | OFF | Не потрібна без post-proc |
| Opaque Texture | OFF | Не потрібна |
| Max Additional Lights | 0 | Тільки directional \+ ambient |

### 11.3 Visual Style Implementation

// Low-poly Brawl Stars стиль:

//

// 1\. МОДЕЛІ: 300–800 tris на персонажа

//    Порівняння: Brawl Stars \~500–1000 tris per character

//

// 2\. ТЕКСТУРИ: solid color atlas (1 текстура на все)

//    Розмір: 256x256 per biome atlas

//

// 3\. ШЕЙДЕР: URP/Lit з мінімальним Smoothness (0.1)

//    \+ кастомний outline pass (inverted hull method)

//    Outline width: 0.02 (world space)

//    Outline color: darkened base color \* 0.3

//

// 4\. АНІМАЦІЯ: «bouncy» easing (overshoot на jump/land)

//    Idle:   легке покачування, 0.5 сек loop

//    Run:    exaggerated stride, head bob

//    Shoot:  recoil \+ flash

//    Death:  ragdoll-like curve \+ puff VFX (0.5 sec)

//    Wound:  шкутильгання, нахил, повільніший рух

//

// 5\. ОСВІТЛЕННЯ:

//    1 Directional Light (warm white, rotation 50,30,0)

//    Ambient: gradient sky (blue top → green/brown bottom)

//    NO realtime shadows on low-end

//

// 6\. PALETTE (Jungle biome):

//    Ground:  \#5C8A4D, \#8B6E4A (dirt patches)

//    Foliage: \#3A7D44, \#5BA870

//    Rocks:   \#7A7A7A, \#5A5A5A

//    Water:   \#4A90B8 (якщо є)

//    Shadow:  \#2D5A3A

---

## 12\. UI/UX архітектура

### 12.1 UI Framework

UI побудований на **Unity UI (uGUI)** з Canvas у Screen Space — Overlay для HUD та Screen Space — Camera для world-space елементів (HP bars, damage numbers).

UI Toolkit розглядається для Post-MVP

### 12.2 Canvas Structure (Landscape)

Canvas (Screen Space — Overlay, Sort Order 0\)

├── SafeArea (SafeAreaHandler.cs)

│   ├── HUD\_Layer (always visible during gameplay)

│   │   ├── TopLeft/

│   │   │   └── MissionObjective (Text \+ Icon)

│   │   ├── TopCenter/

│   │   │   └── SquadBar (4x SoldierPortrait with HP bar)

│   │   ├── TopRight/

│   │   │   └── MiniMap (RawImage, RT 128x128)

│   │   ├── BottomLeft/

│   │   │   └── MoveStick (TouchStickController)

│   │   ├── BottomCenter/

│   │   │   ├── AmmoDisplay (30/120)

│   │   │   └── GrenadeDisplay (icon x2)

│   │   └── BottomRight/

│   │       ├── AimStick (TouchStickController)

│   │       ├── GrenadeButton (60x60)

│   │       └── AutoFireToggle

│   │

│   ├── Popup\_Layer (perk selection, pause)

│   │   ├── PerkSelectionPanel (3 PerkCards horizontal)

│   │   └── PauseMenu

│   │

│   └── Overlay\_Layer (damage flash, wave announcement)

│       ├── DamageFlash (fullscreen red, 0.1s fade)

│       └── WaveAnnouncement ("WAVE 3\!" big text, 1.5s)

Canvas (World Space — attached to Camera)

└── WorldUI\_Pool/

    ├── EnemyHPBar (pool of 20\)

    ├── DamageNumber (pool of 30, float-up \+ fade)

    └── SoldierNameplate (pool of 4\)

### 12.3 Ключові UI елементи та розміри (Landscape)

| Елемент | Мін. розмір (dp) | Behavior |
| :---- | :---: | :---- |
| Joystick base | 100dp radius | Floating, з'являється при touch |
| Joystick knob | 40dp radius | Слідує за пальцем у межах base |
| Grenade button | 48x48dp | Тап \= кидок, haptic feedback |
| Auto-fire toggle | 36x36dp | ON/OFF, стан зберігається |
| Ammo display | 18sp шрифт | Мигає червоним при \<10 куль |
| Perk card | 180x240dp | 3 карти горизонтально |
| Mini-map | 96x96dp | Квадрат, fog of war |
| Squad HP bar | 36x36dp кожний | 4 іконки, сіріє при смерті |
| Pause button | 36x36dp | Верхній кут, subtle |

### 12.4 Perk Selection UI (Landscape Layout)

┌──────────────────────────────────────────────────────────┐

│                                                          │

│                   ╔═══ LEVEL UP\! ═══╗                    │

│                   ║ Choose your perk ║                    │

│                   ╚══════════════════╝                    │

│                                                          │

│   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │

│   │  \[ICON\]     │  │  \[ICON\]     │  │  \[ICON\]     │     │

│   │             │  │             │  │             │     │

│   │ Grenade     │  │ Phalanx     │  │ Glass       │     │

│   │ Spam        │  │             │  │ Cannon      │     │

│   │             │  │ \-20% damage │  │ \+100% dmg   │     │

│   │ \+1 grenade  │  │ taken       │  │ \-50% HP     │     │

│   │ \-25% CD     │  │             │  │             │     │

│   │  \[COMMON\]   │  │  \[RARE\]     │  │  \[RARE\]     │     │

│   └─────────────┘  └─────────────┘  └─────────────┘     │

│                                                          │

│          ═════ Tap a card to select ═════                 │

└──────────────────────────────────────────────────────────┘

// Ефект: гра на паузі (Time.timeScale \= 0\)

// Анімація: карти «висуваються» знизу (0.3 sec, bounce ease)

// Тап на карту: підсвічення → 0.5 сек → apply → resume

---

## 13\. Аудіо-система

### 13.1 AudioManager Architecture

// AudioManager.cs — через ServiceLocator

public class AudioManager : MonoBehaviour {

    \[SerializeField\] AudioSource \_musicSource;

    \[SerializeField\] int \_sfxPoolSize \= 16;

    \[SerializeField\] SFXLibrary \_sfxLibrary; // ScriptableObject

    AudioSource\[\] \_sfxPool;

    int \_sfxIndex;

    void Awake() {

        // Створюємо пул AudioSources

        \_sfxPool \= new AudioSource\[\_sfxPoolSize\];

        for (int i \= 0; i \< \_sfxPoolSize; i++) {

            var go \= new GameObject($"SFX\_{i}");

            go.transform.SetParent(transform);

            \_sfxPool\[i\] \= go.AddComponent\<AudioSource\>();

            \_sfxPool\[i\].playOnAwake \= false;

        }

    }

    public void PlaySFX(SFXType type, Vector3? position \= null) {

        var clip \= \_sfxLibrary.GetClip(type);

        if (clip \== null) return;

        var source \= GetNextSource();

        source.clip \= clip;

        source.volume \= \_sfxLibrary.GetVolume(type);

        if (position.HasValue) {

            source.transform.position \= position.Value;

            source.spatialBlend \= 0.7f; // Частково 3D

        } else {

            source.spatialBlend \= 0f; // 2D (UI sounds)

        }

        source.Play();

    }

    AudioSource GetNextSource() {

        // Round-robin з перевіркою зайнятості

        for (int i \= 0; i \< \_sfxPoolSize; i++) {

            int idx \= (\_sfxIndex \+ i) % \_sfxPoolSize;

            if (\!\_sfxPool\[idx\].isPlaying) {

                \_sfxIndex \= (idx \+ 1\) % \_sfxPoolSize;

                return \_sfxPool\[idx\];

            }

        }

        // Всі зайняті — перезаписуємо найстаріший

        \_sfxIndex \= (\_sfxIndex \+ 1\) % \_sfxPoolSize;

        return \_sfxPool\[\_sfxIndex\];

    }

    public void PlayMusic(AudioClip clip, float fadeTime \= 1f) {

        StartCoroutine(CrossfadeMusic(clip, fadeTime));

    }

    IEnumerator CrossfadeMusic(AudioClip newClip, float duration) {

        float startVol \= \_musicSource.volume;

        // Fade out

        for (float t \= 0; t \< duration / 2; t \+= Time.unscaledDeltaTime) {

            \_musicSource.volume \= Mathf.Lerp(startVol, 0, t / (duration / 2));

            yield return null;

        }

        \_musicSource.clip \= newClip;

        \_musicSource.Play();

        // Fade in

        for (float t \= 0; t \< duration / 2; t \+= Time.unscaledDeltaTime) {

            \_musicSource.volume \= Mathf.Lerp(0, startVol, t / (duration / 2));

            yield return null;

        }

    }

}

// ═══ SFX Types (MVP) ═══

public enum SFXType {

    // Зброя

    RifleShot, ReloadStart, ReloadEnd,

    // Гранати

    GrenadeThrow, GrenadeExplosion,

    // Солдати

    SoldierHurt, SoldierDeath, SoldierVoice,

    // Вороги

    EnemyHurt, EnemyDeath, EnemyAlert, Ricochet,

    // UI

    ButtonClick, PerkSelect, LevelUp, WaveStart,

    // Пікапи

    AmmoPickup, HealthPickup, GrenadePickup

}

### 13.2 Формати та оптимізація

| Тип аудіо | Формат | Sample Rate | Channels | Load Type |
| :---- | :---- | :---: | :---: | :---- |
| SFX (short, \<1s) | .ogg | 22050 Hz | Mono | Decompress On Load |
| SFX (medium, 1–3s) | .ogg | 44100 Hz | Mono | Compressed In Memory |
| Music (BGM loops) | .ogg | 44100 Hz | Stereo | Streaming |
| Voice Lines | .ogg | 22050 Hz | Mono | Compressed In Memory |

---

## 14\. Мета-прогресія: Boot Camp та економіка

**MVP**: мета-прогресія мінімальна — екран вибору місії, Hill of Fame (список надгробків), збереження загону між забігами.

### 14.1 SaveSystem

// SaveData.cs — серіалізується в JSON

\[System.Serializable\]

public class SaveData {

    // Гравець

    public int playerLevel;

    public int totalMissions;

    public int totalKills;

    // Валюти (MVP: тільки coins)

    public int coins;

    // Загін (поточний склад)

    public SoldierSaveData\[\] squad; // 4 елементи

    // Hill of Fame (всі загиблі за всю історію)

    public List\<TombstoneData\> tombstones \= new();

    // Налаштування

    public float musicVolume \= 0.7f;

    public float sfxVolume \= 1.0f;

    public bool autoAimEnabled \= true;

    public bool hapticEnabled \= true;

    public int controlSize \= 1; // 0=S, 1=M, 2=L

    public bool leftHandMode \= false;

    // POST-MVP fields:

    // public BootCampData bootCamp;

    // public List\<string\> unlockedPerks;

    // public BattlePassData battlePass;

    // public int medals;

    // public int dogTags;

}

\[System.Serializable\]

public class SoldierSaveData {

    public string name;

    public int helmetColorIndex;

    public int missionsCompleted;

    public int totalKills;

    public bool isVeteran; // 5+ missions

}

// SaveManager.cs

public class SaveManager : ISaveProvider {

    const string SAVE\_FILE \= "save\_v1.json";

    string Path \=\> System.IO.Path.Combine(

        Application.persistentDataPath, SAVE\_FILE);

    public SaveData Load() {

        if (\!File.Exists(Path)) return CreateDefault();

        var json \= File.ReadAllText(Path);

        return JsonUtility.FromJson\<SaveData\>(json);

    }

    public void Save(SaveData data) {

        var json \= JsonUtility.ToJson(data, prettyPrint: false);

        File.WriteAllText(Path, json);

    }

    SaveData CreateDefault() {

        var data \= new SaveData();

        data.squad \= new SoldierSaveData\[4\];

        var names \= NameGenerator.GetUniqueNames(4);

        for (int i \= 0; i \< 4; i++) {

            data.squad\[i\] \= new SoldierSaveData {

                name \= names\[i\],

                helmetColorIndex \= i,

                missionsCompleted \= 0,

                totalKills \= 0,

                isVeteran \= false

            };

        }

        return data;

    }

    // Auto-save triggers:

    // \- Після завершеної місії

    // \- Після смерті солдата (tombstone)

    // \- OnApplicationPause(true) / OnApplicationQuit

}

### 14.2 Boot Camp UI Flow (MVP)

┌──────────────────────────────────────────────────────────┐

│  BOOT CAMP (landscape)                                    │

│                                                          │

│  ┌───────────────┐  ┌──────────────────┐  ┌───────────┐ │

│  │ YOUR SQUAD    │  │  MISSION \#42     │  │ HILL OF   │ │

│  │               │  │  "Liberate the   │  │ FAME      │ │

│  │ \[Sgt\] Stoo    │  │   things that    │  │           │ │

│  │  ★ 12 kills   │  │   don't want to  │  │ RIP       │ │

│  │ \[Pvt\] Boris   │  │   be freed"      │  │ Sgt.Bob   │ │

│  │  ☆ 3 kills    │  │                  │  │ 47 kills  │ │

│  │ \[Pvt\] Alina   │  │  Biome: Jungle   │  │           │ │

│  │  ☆ 0 kills    │  │  Waves: 6        │  │ RIP       │ │

│  │ \[Pvt\] Dima    │  │                  │  │ Pvt.Oleg  │ │

│  │  ☆ 1 kill     │  │  ╔════════════╗  │  │ 3 kills   │ │

│  │               │  │  ║  START\!    ║  │  │           │ │

│  │               │  │  ╚════════════╝  │  │ \[scroll\]  │ │

│  └───────────────┘  └──────────────────┘  └───────────┘ │

│                         \[Settings\]                        │

└──────────────────────────────────────────────────────────┘

---

## 15\. Серверна архітектура та бекенд

**MVP**: бекенд не потрібен. Гра працює повністю offline із локальним збереженням. Архітектура клієнта проєктується з урахуванням майбутнього бекенду через інтерфейси.

### 15.1 Бекенд Roadmap

| Фаза | Бекенд | Що дає |
| :---- | :---- | :---- |
| **MVP (0–3 міс)** | Без бекенду. Local JSON save. | Швидка ітерація, нуль інфра-витрат |
| **Alpha (3–6 міс)** | Firebase: Auth \+ Firestore \+ Remote Config | Cloud saves, A/B тести, feature flags |
| **Soft-launch (6–9 міс)** | Firebase \+ Cloud Functions (Node.js) | IAP validation, anti-cheat, leaderboards |
| **Global (9–12 міс)** | Міграція на Go \+ PostgreSQL | Повний контроль, масштабування, кооп |

### 15.2 Backend-Ready Client Architecture

// Інтерфейси для backend abstraction

public interface ISaveProvider {

    SaveData Load();

    void Save(SaveData data);

}

public interface IAnalyticsProvider {

    void LogEvent(string name, Dictionary\<string, object\> parameters);

}

public interface IAuthProvider {

    Task\<AuthResult\> SignInAnonymouslyAsync();

}

public interface IRemoteConfigProvider {

    Task\<T\> GetValueAsync\<T\>(string key);

}

// ═══ MVP реалізації ═══

// Services.Register\<ISaveProvider\>(new LocalSaveProvider());

// ═══ Post-MVP заміна ═══

// Services.Register\<ISaveProvider\>(new FirebaseSaveProvider());

// Services.Register\<IAuthProvider\>(new FirebaseAuthProvider());

// Services.Register\<IRemoteConfigProvider\>(new FirebaseConfigProvider());

---

## 16\. Аналітика та Live-Ops

### 16.1 Ключові події для трекінгу (MVP)

Навіть на MVP критично трекати базові події для валідації core loop.

| Подія | Параметри | Навіщо |
| :---- | :---- | :---- |
| `session_start` / `session_end` | duration, missions\_played | D1/D7/D30 retention |
| `mission_start` | mission\_id, squad\_size, biome | Скільки забігів? |
| `mission_end` | result, duration\_sec, kills, soldiers\_alive | Win rate, складність |
| `soldier_died` | name, cause, wave\_number, time\_in\_mission | Де гравці втрачають солдатів? |
| `perk_selected` | perk\_id, alternatives\_shown, wave | Які перки популярні? |
| `level_up` | new\_level, time\_in\_mission | Темп прогресії |
| `grenade_used` | hit\_count, friendly\_fire | Як часто \+ FF? |
| `tutorial_step` | step\_id, completed | Де відвалюються новачки? |

### 16.2 Реалізація (MVP)

// AnalyticsService.cs — фасад

public class AnalyticsService {

    List\<IAnalyticsProvider\> \_providers \= new();

    public void Init() {

        // MVP: тільки Unity Analytics (безкоштовний)

        \_providers.Add(new UnityAnalyticsProvider());

        // Post-MVP:

        // \_providers.Add(new FirebaseAnalyticsProvider());

        // \_providers.Add(new GameAnalyticsProvider());

    }

    public void LogEvent(string name,

        params (string key, object val)\[\] parameters) {

        var dict \= parameters.ToDictionary(p \=\> p.key, p \=\> p.val);

        foreach (var provider in \_providers)

            provider.LogEvent(name, dict);

    }

}

// Виклик:

// analytics.LogEvent("soldier\_died",

//     ("name", "Stoo Cameron"),

//     ("cause", "Rusher explosion"),

//     ("wave", 3),

//     ("time", 127.5f));

---

## 17\. Оптимізація та цільові пристрої

### 17.1 Performance Tiers

| Tier | Приклади пристроїв | FPS | Render Scale | Shadow | Max Enemies |
| :---- | :---- | :---: | :---: | :---: | :---: |
| **High** | iPhone 13+, Galaxy S21+, Pixel 6+ | 60 | 1.0 | 1024px | 40 |
| **Medium** | iPhone 8–12, Galaxy A52, Poco X3 | 30–60 | 0.85 | 512px | 30 |
| **Low** | iPhone 7, Galaxy A14, Redmi 9 | 30 stable | 0.7 | Off | 20 |

### 17.2 Ключові оптимізації

| Техніка | Що оптимізує | Реалізація |
| :---- | :---- | :---- |
| **Object Pooling** | Кулі, VFX, вороги, UI numbers | `ObjectPool<T>` з pre-warm \+ auto-expand |
| **SRP Batcher** | Draw calls (same material) | Увімкнути в URP Asset, shared materials |
| **GPU Instancing** | Identical mesh rendering | Enable на матеріалах ворогів |
| **LOD (спрощений)** | Далекі об'єкти | 2 LOD: mesh \+ billboard для props |
| **Texture Atlasing** | Текстурна пам'ять | 1 atlas per biome (256x256) |
| **Physics Layers** | Collision checks | 6 layers з collision matrix |
| **Frame Budget AI** | CPU load розподіл | AI через 1 кадр, pathfinding через Jobs |
| **Audio Pooling** | AudioSource instances | 16 pooled, priority-based |
| **Shader Warmup** | First-render stuttering | `ShaderVariantCollection.WarmUp()` при завантаженні |
| **Sprite Atlas** | UI draw calls | 1 atlas для всього HUD |

### 17.3 Memory Budget

| Категорія | Budget (MB) | Деталі |
| :---- | :---: | :---- |
| 3D Models | 15–20 | All characters \+ environment (low-poly \= tiny) |
| Textures | 20–30 | Atlases compressed (ETC2 / ASTC) |
| Audio | 10–15 | Streaming BGM, compressed SFX |
| Scripts/Code | 5–10 | IL2CPP compiled |
| UI Sprites | 5–8 | Atlas compressed |
| NavMesh \+ Physics | 3–5 | Per-tile NavMesh data |
| Object Pools | 5–10 | Pre-warmed projectiles, VFX, enemies |
| **TOTAL** | **\< 100** | **Дозволяє грати на 3GB RAM пристроях** |

### 17.4 Build Size

| Компонент | Оцінка | Техніка зменшення |
| :---- | :---: | :---- |
| APK/IPA base | \< 80 MB | Strip engine code, IL2CPP |
| 3D Art | \~20 MB | Low-poly \= малий розмір файлів |
| Audio | \~10 MB | .ogg compression, short clips |
| Textures | \~15 MB | ETC2/ASTC compression, small atlases |
| Code | \~5 MB | IL2CPP \+ stripping |
| **TOTAL** | **\< 150 MB** | **Cellular install limit** |

---

## 18\. Кооперативний режим

**POST-MVP** — але архітектура закладається з самого початку.

### 18.1 Підготовка в MVP

// ═══ Command Pattern для всіх ігрових дій ═══

// Замість прямого виклику soldier.Move(dir),

// створюємо команди, які можна серіалізувати для мережі

public interface IGameCommand {

    int PlayerId { get; }

    float Timestamp { get; }

    void Execute(GameContext ctx);

}

public struct MoveCommand : IGameCommand {

    public int PlayerId { get; set; }

    public float Timestamp { get; set; }

    public Vector2 Direction;

    public void Execute(GameContext ctx) \=\>

        ctx.GetSquad(PlayerId).Move(Direction);

}

public struct FireCommand : IGameCommand {

    public int PlayerId { get; set; }

    public float Timestamp { get; set; }

    public Vector3 AimDirection;

    public void Execute(GameContext ctx) \=\>

        ctx.GetSquad(PlayerId).Fire(AimDirection);

}

// ═══ Що робимо в MVP: ═══

// 1\. Command Pattern для руху та стрільби (серіалізуються)

// 2\. Deterministic Random (фіксований seed)

// 3\. Player Abstraction: playerId у всій логіці (MVP: завжди 0\)

// 4\. Уникаємо float precision issues (округлення до 0.01)

### 18.2 Post-MVP Network Architecture

| Фаза | Технологія | Що реалізує |
| :---- | :---- | :---- |
| Phase 1: Local | Unity Netcode for GameObjects | LAN/Bluetooth, 2 пристрої |
| Phase 2: Online | Photon Fusion або Netcode \+ Relay | P2P через relay сервер |
| Phase 3: Dedicated | Dedicated server (Go) \+ WebSocket | Для PvP/ranked (якщо буде) |

---

## 19\. CI/CD та білд-пайплайн

### 19.1 Version Control

\# Git \+ Git LFS для великих файлів

\# .gitattributes:

\*.fbx     filter=lfs diff=lfs merge=lfs \-text

\*.png     filter=lfs diff=lfs merge=lfs \-text

\*.ogg     filter=lfs diff=lfs merge=lfs \-text

\*.psd     filter=lfs diff=lfs merge=lfs \-text

\*.wav     filter=lfs diff=lfs merge=lfs \-text

\*.unity   filter=lfs diff=lfs merge=lfs \-text

\# .gitignore: (ключове)

Library/

Temp/

Obj/

Build/

Builds/

Logs/

UserSettings/

\*.csproj

\*.sln

\# Branch strategy:

\# main       ← stable, buildable

\# develop    ← integration branch

\# feature/\*  ← feature branches

\# hotfix/\*   ← critical fixes

### 19.2 Build Pipeline

| Крок | Інструмент | Тригер |
| :---- | :---- | :---- |
| Lint C\# | dotnet format \+ Roslyn analyzers | Кожен PR |
| Unit Tests | Unity Test Framework (EditMode) | Кожен PR |
| Build Android | Unity Cloud Build / GameCI (GitHub Actions) | Мерж в develop |
| Build iOS | Unity Cloud Build \+ Xcode Cloud | Мерж в develop |
| Play Tests | Unity Test Framework (PlayMode) | Nightly |
| Deploy | fastlane → TestFlight / Firebase App Distribution | Мерж в main |

### 19.3 MVP: мінімальний CI

На старті достатньо: **Git \+ GitHub \+ ручні білди через Unity Editor**. CI додаємо коли команда \> 2 розробників.

---

## 20\. MVP vs Post-MVP: повна карта фіч

### 20.1 MVP (8–10 тижнів, 2 розробники)

| Тиждень | Фіча | Люд.-дні | Результат |
| :---: | :---- | :---: | :---- |
| 1–2 | Project setup \+ Input \+ Character Controller | 10 | Загін рухається, twin-stick працює |
| 2–3 | Бойова система (стрільба \+ гранати \+ урон) | 10 | Солдати стріляють, вороги отримують урон |
| 3–4 | Squad System \+ Permadeath \+ Tombstones | 8 | Солдати гинуть, надгробки spawn |
| 4–5 | Enemy AI (5 типів) \+ Wave Spawner | 10 | 5 типів з унікальною поведінкою |
| 5–6 | Procedural Level Gen (1 біом) | 8 | Кожен забіг — унікальна карта |
| 6–7 | Perk System (15 перків) \+ Level-up | 6 | Вибір перків працює |
| 7–8 | HUD \+ Game Flow (start → play → result) | 8 | Повний цикл |
| 8–9 | Camera \+ VFX \+ Audio (placeholder) | 6 | Гра «соковита» |
| 9–10 | Boot Camp \+ Hill of Fame \+ Save | 6 | Мета-loop |
| 10 | Polish \+ Bug Fixes \+ Playtesting | 5 | Стабільна версія |

**Разом: \~77 людино-днів \= \~10 тижнів для 2 розробників**

### 20.2 Post-MVP Roadmap

| Фаза | Фічі | Тижні | Пріоритет |
| :---- | :---- | :---: | :---: |
| Alpha | Tutorial (interactive, 2 хв) | 2 | P0 |
| Alpha | Біом 2 (Сніг) \+ 4 ворогі \+ 2 боси | 4 | P1 |
| Alpha | Boot Camp апгрейди | 3 | P1 |
| Alpha | Розширені перки (30 шт) | 2 | P1 |
| Beta | Firebase integration | 3 | P1 |
| Beta | Battle Pass (60 днів) | 4 | P2 |
| Beta | Магазин \+ IAP \+ Rewarded Video | 3 | P2 |
| Beta | Біом 3 (Пустеля) | 3 | P2 |
| Soft-launch | Local Co-op (Buddy System) | 5 | P2 |
| Soft-launch | Leaderboards \+ Anti-cheat | 3 | P2 |
| Soft-launch | Локалізація (EN, UA, ES, PT-BR) | 2 | P1 |
| Global | Online Co-op | 6 | P3 |
| Global | Steam/Switch порт | 4 | P3 |
| Global | Veteran Mode (pixel-art shader) | 2 | P3 |

---

## 21\. Ризики та мітигація

| Ризик | Ймовір. | Вплив | Мітигація |
| :---- | :---: | :---: | :---- |
| Twin-stick не зручний на mobile | Середня | Крит. | Тестуємо на реальних пристроях з тижня 1\. Fallback: tap-to-move \+ auto-aim (Archero) |
| Permadeath фруструє гравців | Висока | Висок. | Чорний гумор. Hill of Fame. Rewarded revive (1 раз) |
| Performance на low-end Android | Середня | Висок. | Quality tiers. Тест на Galaxy A14 щоспринт. Pools з дня 1 |
| Procgen дає нудні рівні | Середня | Серед. | Hand-crafted tiles \+ рандом-композиція. Playtest щотижня |
| Scope creep | Висока | Висок. | Жорсткий MVP. Перевірка дизайн-стовпами |
| IP-ризик Cannon Fodder | Низька | Серед. | Власна IP. Натхнення \!= копіювання |
| Art bottleneck (1 художник) | Висока | Висок. | Low-poly \= менше деталей. Grey-boxing перші 4 тижні |

---

## 22\. Тижневий план запуску розробки

Перші 4 тижні найкритичніші. Ціль: **playable prototype до кінця тижня 4**, що доводить або спростовує fun-фактор.

### Тиждень 1: Foundation

| День | Задача | Definition of Done |
| :---: | :---- | :---- |
| Пн | Unity-проєкт: URP, folder structure, Git+LFS | Проєкт збирається, пушиться в repo |
| Вт | Input System (New Input System \+ touch sticks) | Два джойстики працюють на пристрої landscape |
| Ср | Grey-box soldier \+ squad formation movement | 4 капсули рухаються формацією |
| Чт | Camera controller (top-down 60°, follow, bounds) | Камера плавно слідує |
| Пт | Safe Area \+ HUD skeleton (landscape) | UI не перекривається notch |

### Тиждень 2: Combat Core

| День | Задача | Definition of Done |
| :---: | :---- | :---- |
| Пн | Weapon system: SO config \+ auto-fire | Солдати стріляють кулями |
| Вт | Projectile pool \+ collision detection | Кулі влучають, 0 GC alloc |
| Ср | Damage pipeline \+ HP \+ death | Можна вбити ворога |
| Чт | Grenade system (arc, AoE, friendly fire) | Гранати вибухають, дамажать усіх |
| Пт | Ammo system (mag \+ reserve \+ reload \+ pickups) | Ammo обмежене, pickups працюють |

### Тиждень 3: Enemies \+ Permadeath

| День | Задача | Definition of Done |
| :---: | :---- | :---- |
| Пн | EnemyAI base \+ Grunt AI | Grunt патрулює, бігає, стріляє |
| Вт | Sniper AI \+ Rusher AI | Лазер \+ вибух працюють |
| Ср | Rocket Soldier \+ Light Tank | Ракета з AoE, танк імунний до куль |
| Чт | Permadeath: tombstone, squad reduction, game over | Солдати гинуть, надгробки spawn |
| Пт | Wave spawner: 6 хвиль з наростанням | Забіг 3–5 хв працює |

### Тиждень 4: Procgen \+ Perks \+ PLAYTEST

| День | Задача | Definition of Done |
| :---: | :---- | :---- |
| Пн | Tile system: 5 grey-box tiles \+ LevelGenerator | Рівень генерується |
| Вт | Perk engine: SO perks \+ selection UI | Level-up → вибір 1 з 3 |
| Ср | 5 стартових перків (по 1 з категорії) | Перки змінюють геймплей |
| Чт | Mission flow: briefing → play → result | Повний цикл |
| **Пт** | **ПЕРШИЙ PLAYTEST на пристрої** | **Go/No-go по fun-фактору** |

---

**"War has never been so much fun."**

---

*Документ створено: квітень 2026* *Версія: 0.1* *Наступний документ: Enemy Bestiary / Boot Camp Economy / Art Bible*  
