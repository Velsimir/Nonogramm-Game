# Nonogram-Game — контекст проекта

Мобильный puzzle под Android и iOS, 2D. Кор-геймплей — цветные нонограммы (пикросс),
см. раздел «Нонограммы». Экономики и меты пока нет.

## Стек

- Unity **6000.4.3f1**, URP 17.4.0 (настройки рендера — `Assets/Settings/`, шаблонные ассеты
  `Mobile_RPAsset` / `PC_RPAsset` оставлены как есть).
- DI — Zenject (Extenject), реактивность — R3, асинхронность — UniTask, шина — MessagePipe,
  сериализация — Newtonsoft.Json.
- Ввод — Input System (новый), `Active Input Handling = Input System Package (New)`.
  В сценах стоит `InputSystemUIInputModule`, не `StandaloneInputModule`.

## Пакеты

Git-URL'ы и версии — в `Packages/manifest.json`. Значимое:

| Пакет | Примечание |
|---|---|
| `com.svermeulen.extenject` | git |
| `com.cysharp.r3` | git, **только Unity-интеграция**, ядро отдельно — см. ниже |
| `com.cysharp.unitask` | git |
| `com.cysharp.messagepipe` + `.zenject` | git |
| `com.unity.nuget.newtonsoft-json` | 3.2.1 |
| `com.unity.addressables` | **2.10.3** — нужен и Extenject'у (`AssetReference`/`AsyncOperationHandle` в его Runtime), не только под контент |
| `com.unity.localization` | **1.5.13** |
| `com.unity.inputsystem` | 1.19.0 |
| `com.github-glitchenzo.nugetforunity` | git, нужен для ядра R3 |
| `com.coplaydev.unity-mcp` | git, `#v10.2.0`, мост для агента |

### Ядро R3 поставлено вручную — не трогать

Git-пакет `com.cysharp.r3` содержит только слой `R3.Unity`. Само ядро (`Observable<>`,
`ReactiveProperty<>`, `TimeProvider`, `R3.Collections`) — это NuGet-пакет. Без него проект
не компилируется: было 2988 ошибок CS вида `The type or namespace name 'Observable<>'
could not be found`.

Ядро лежит в `Assets/Packages/` и описано в `Assets/packages.config` в формате NuGetForUnity:

- R3 **1.3.1**
- Microsoft.Bcl.TimeProvider 8.0.0
- Microsoft.Bcl.AsyncInterfaces 6.0.0
- System.ComponentModel.Annotations 5.0.0
- System.Runtime.CompilerServices.Unsafe 6.0.0
- System.Threading.Channels 8.0.0

**Не удаляй `Assets/Packages/` и не «восстанавливай» эти пакеты через окно NuGet вслепую** —
рассинхрон версии ядра с версией `R3.Unity` ломает компиляцию всего проекта. Менять версию R3
только вместе с обновлением git-пакета `com.cysharp.r3` и с проверкой компиляции.

### DOTween установлен

Импортирован из Asset Store в `Assets/Plugins/Demigiant/DOTween/`, сетап пройден:
сгенерированы `Modules/*.cs` и `Assets/G/Resources/DOTweenSettings.asset`.
Scripting Define Symbols на Android, iOS и Standalone: `DOTWEEN;UNITASK_DOTWEEN_SUPPORT` —
второй дефайн включает сборку `UniTask.DOTween`, без него не работает `tween.ToUniTask()`.
Код самого фундамента твинов не использует.

DOTween не ставится из manifest и не восстанавливается автоматически: после `rm -rf Library`
он переживёт, а после чистки `Assets/Plugins/Demigiant` — нет, переимпортировать руками.

## Структура

```
Assets/
├── G/
│   ├── Configs/    GameConfigs, PagesConfig, MenuPagesConfig, PlatformSdkConfig,
│   │               ConfigsInstaller, Nonogram/ (Rule, Levels, Skin, Levels/*.asset)
│   ├── Prefabs/    Curtain.prefab, UI/HudPage.prefab,
│   │               UI/Nonogram/ (Cell, Clue, NonogramPage), UI/Menu/ (LevelTile, LevelSelectPage)
│   ├── Resources/  ProjectContext.prefab, DOTweenSettings.asset
│   ├── Scenes/     Boot.unity (0), Game.unity (1), Menu.unity (2)
│   ├── Textures/   Nonogram/ — исходные картинки уровней
│   └── Scripts/    корневой namespace G
│       ├── Core/     Common, DI, Boot, Curtain, Scenes, Save, UI, Tick, Factory, Pool
│       ├── Platform/ фасад IPlatformSdk + StubPlatformSdk
│       ├── Configs/  GameConfigs
│       ├── Meta/     LevelFlow, LevelProgress, LevelSelect, DI
│       └── Gameplay/ Nonogram/ (Data, Level, Config, Board, Presentation, UI),
│                     GameSceneEntry, DI, UI/HudPage.cs
├── Packages/       ядро R3 из NuGet — не трогать
├── Plugins/        Demigiant/DOTween
├── Settings/       URP
└── TextMesh Pro/   TMP Essentials
```

Корневой namespace — **`G`** (`G.Core.UI`, `G.Platform` и так далее). Всё под `Assets/G/`,
а не разбросано по корню `Assets/` — это дефолт фундамента, начиная с 2026-09-14.

**asmdef в проекте нет намеренно** — компенсация описана в базе агента `unity-foundation`
(`ARCHITECTURE.md`). Правил, которые без asmdef не форсит компилятор, всего два: во `View`
не должно быть `[Inject]`, слой `Core/UI/` не должен ссылаться на `Gameplay`. Автоматических
проверок в проекте нет — это сознательное решение (см. ниже), проверяй руками при ревью.

**Постоянно работающих редакторных валидаторов в проекте нет.** Раньше здесь стояли три
(`ArchitectureValidator`, `ScenesNameValidator`, `SceneSingletonValidator`), их убрали
2026-09-15 вслед за обновлением агента `unity-foundation`: он теперь проверяет фундамент сам
при сборке, а не оставляет постоянно шумящий в консоли инструмент.

Правило именно про фоновые проверки, а не про редакторные инструменты вообще: **разовая
команда по кнопке разрешена** — она ничего не сторожит и в консоль сама не пишет. Такое меню
в проекте одно, `Tools → Сейвы` (см. раздел «Сейвы»).

Если правила ниже (имена сцен, единственные `AudioListener`/`EventSystem`, `[Inject]` во
`View`, `UI` не ссылается на `Gameplay`) когда-нибудь массово нарушатся — проверяй руками, автоматики для этого нет.

## Конвейер загрузки

`Boot` → `GameBootstrapper` → шторка → операции (инициализация SDK → загрузка сейва →
восстановление прогресса → загрузка `Menu`) → `Menu` становится активной → шторка убирается →
`Boot` выгружается.

Дальше сцены меняет `LevelFlowService` (`Meta/LevelFlow/`, проектный контейнер, переживает
смену сцен): `StartLevelAsync` — шторка, грузит `Game`, делает активной, выгружает `Menu`;
`ReturnToMenuAsync` — зеркально. Шторку убирает не сам сервис, а вошедшая сцена через
`CompleteTransitionAsync` — иначе игрок увидит пустую доску до того, как откроется страница.

Точка входа каждой сцены — свой `Service` в сценовом инсталлере: `MenuSceneEntry` только
досматривает переход, `GameSceneEntry` берёт `PendingLevel` и открывает `NonogramPage`.
Если `PendingLevel` пуст (сцену `Game` запустили напрямую из редактора), берётся первый
уровень конфига и пишется предупреждение.

Каждому элементу `ScenesName` обязана соответствовать сцена в Build Settings. Добавил сцену —
правь и enum, и `SceneNameMap`, и Build Settings; автоматической проверки нет, сверяй руками.

## UI

Один Canvas на каждый `UILayer`, `sortingOrder` равен значению enum: Hud 0, Window 100,
Popup 200, Overlay 300, Loading 400, System 500. Плюс отдельный Canvas 1000 с `InputBlocker`.
Корни слоёв собраны в `UILayerRoots` на объекте `UI` в сцене `Game`.

Новая страница: префаб с наследником `Page`/`PayloadPage`, слой выставляется в инспекторе,
префаб добавляется в `PagesConfig` (есть контекстное меню «Собрать все префабы страниц»).
Контекстное меню собирает **все** префабы страниц проекта — после него выкинь из списка
чужие страницы руками, иначе в конфиге сцены окажется страница соседней сцены.

**`PagesConfig` — сценовый, а не проектный.** Поле стоит на `UIInstaller` в сцене и
перекрывает проектный биндинг из `ConfigsInstaller`. Так у каждой сцены своя стартовая
страница: `Menu` открывает `LevelSelectPage` из `MenuPagesConfig`, `Game` — `HudPage` из
`PagesConfig`. Забыл назначить поле — `UIInstaller` громко ругается и не биндит UI.

**Страницы инжектятся из проектного контейнера, не из сценового.** `PagesProvider` создаёт их
через `GameFactory`, а тот живёт в `ProjectContext`, поэтому сабконтейнер страницы видит
только проектные биндинги. Всё, что нужно странице, биндится в `MetaInstaller` /
`GameplayInstaller`, а не в сценовом инсталлере — на этом уже спотыкались с
`LevelPreviewFactory`.

## Сейвы

`persistentDataPath/Save/save.json`, то есть
`%USERPROFILE%\AppData\LocalLow\<CompanyName>\<ProductName>\Save\save.json` на Windows.
Пишется по `Paused` и `Quitting` из `ApplicationLifecycleWatcher` — на Android
`OnApplicationQuit` не гарантирован. Формат — конверт с версией и словарём секций:

```json
{"version":1,"savedAtUtc":"...","sections":{}}
```

Сейчас секций две: `nonogram` (состояние доски) и `levelProgress` (пройденные уровни).

**Company Name и Product Name задают путь сейвов.** Менять их после релиза нельзя — игроки
потеряют прогресс.

### Очистка сейва

`Tools → Сейвы`: «Удалить сохранение», «Показать папку», «Вывести в консоль»
(`Core/Save/Editor/SaveDebugMenu.cs`).

**Удаление сносит папку `Save` целиком, и это не перестраховка.** Рядом с `save.json`
`LocalFileSaveStorage` держит `save.bak` и `save.tmp`: если основной файл пуст или удалён,
чтение молча поднимает резервную копию, и «очищенный» прогресс возвращается. Удалять по
одному файлу бессмысленно.

Пункт удаления заблокирован в Play Mode: `SaveService` держит конверт в памяти и перезапишет
файл по `Paused`/`Quitting`. Чистить надо на остановленном редакторе.

## Платформенные SDK

Биндится `StubPlatformSdk` (`PlatformSdkConfig.Kind = EditorStub`): реклама, покупки,
лидерборды и облако имитируются, capabilities выключены. Реальный адаптер площадки
пишется под `IPlatformSdk` и биндится в `PlatformInstaller`.

## Правки, внесённые в шаблонный код

Фундамент собран из шаблонов, но несколько мест пришлось починить. Если шаблоны будут
накатываться поверх заново — эти правки надо сохранить.

1. `G/Scripts/Core/Tick/TickService.cs` — свойства `Update`/`FixedUpdate`/`LateUpdate`
   конфликтовали с магическими методами MonoBehaviour (CS0102). Переведены на явную
   реализацию `ITickService`, сам интерфейс не менялся.
2. `G/Scripts/Core/Pool/ObjectPoolService.cs` — добавлен `using Object = UnityEngine.Object;`
   (CS0104, неоднозначность с `System.Object`).
3. `G/Scripts/Core/Common/IsExternalInit.cs` — создан полифилл, без него не компилируется
   позиционный `record PageId` (CS0518).
4. **`LoadingService` перенесён из проектного контейнера в сценовый.** Было:
   `CoreInstaller.InstallLoading()` биндил его в ProjectContext как `NonLazy`, при этом его
   зависимость `ILoadingErrorHandler` → `LoadingErrorHandler` → `LoadingScreenView` живёт
   только в сцене `Boot`. Zenject резолвит зависимости от ребёнка к родителю, но не наоборот,
   и игра падала на старте с `ZenjectException: Unable to resolve 'ILoadingErrorHandler'`.
   Теперь биндинг стоит в `BootSceneInstaller` рядом с обработчиком ошибок.

## Настройки проекта

- Enter Play Mode Options включены, Reload Domain и Reload Scene выключены — вход в Play Mode
  быстрый, но **статика между запусками не сбрасывается**. Для статических полей есть
  `StaticStateResetter` и атрибут `[ResetOnPlay]`, пользуйся ими, а не рассчитывай на domain reload.
- Android: IL2CPP, ARM64, minSdk 25 (Unity 6000.4 не поддерживает ниже). iOS: IL2CPP.
- Scripting Define Symbols на Android, iOS и Standalone: `DOTWEEN;UNITASK_DOTWEEN_SUPPORT`.

## AudioListener и EventSystem: ровно по одному

- `AudioListener` — дочерним объектом `ProjectContext.prefab`, в `DontDestroyOnLoad`.
  Переехал туда 2026-09-17 вместе с появлением сцены `Menu`: слушатель на камере сцены
  означал бы тишину в меню и дубль в момент, когда `Menu` и `Game` сосуществуют. Игра 2D,
  пространственный звук не нужен, поэтому один слушатель на всю игру — верное решение.
  **В сценах `AudioListener` нет ни одного.**
- `EventSystem` (+ `InputSystemUIInputModule`) — только дочерним объектом `ProjectContext.prefab`,
  то есть в `DontDestroyOnLoad`. В сценах его нет: один на всю игру, переживает смену сцен,
  кнопки панели ошибки в `Boot` работают через него.

Почему так: сцены в конвейере загрузки какое-то время сосуществуют (`Game` уже загружена,
`Boot` ещё не выгружен). Дубли в этот момент дают «There are 2 audio listeners» и «There are
2 event systems» **каждый кадр**, звук берётся с произвольного слушателя, а клики уходят в тот
EventSystem, который проснулся первым. Ноль — тишина в билде и мёртвый UI; в редакторе и то,
и другое замечают поздно. Пока идёт `Boot`, слушателей ноль — это нормально, звука там нет.

**Автоматической проверки больше нет** (валидатор `SceneSingletonValidator` убран
2026-09-15) — соблюдай правило при создании новых камер и объектов, а не полагайся на то,
что консоль тебя предупредит.

## Куда писать код

- Игровая логика — `G/Scripts/Gameplay/`, биндинги в `GameplayInstaller`.
- Мета (кошелёк, магазин, ежедневки и прочее) — `G/Scripts/Meta/`, биндинги в `MetaInstaller`.
- Инфраструктуру в `G/Scripts/Core/` без необходимости не трогай.
- Ни корутин, ни `async void` — только UniTask.
- Сервис биндится через `BindService` / `BindServiceInterfacesOnly`, презентер — через
  `BindPresenter`. Простой `Bind<T>()` молча не вызовет `IInitializable` и `IDisposable`.
- Молчаливый `return` при ошибке конфигурации запрещён — ошибка обязана быть громкой
  (`GameDebug.LogConfigurationError`).
- **Комментариев в коде нет** — см. `CODE-STYLE.md` в базе агента `unity-foundation`.

## Нонограммы

Кор-механика проекта. Весь код — `G/Scripts/Gameplay/Nonogram/`, namespace `G.Gameplay.Nonogram.*`.

```
Gameplay/Nonogram/
├── Data/         CellState, CellAddress, NonogramCell, NonogramClue, LineAxis, LineAddress,
│                 PaintTool, PaintOutcome, MistakeMode — структуры без зависимостей
├── Level/        NonogramLevelAsset, NonogramLevelImport (весь под #if UNITY_EDITOR),
│                 NonogramClueBuilder, NonogramLevelsConfig
├── Config/       NonogramRuleConfig
├── Board/        INonogramBoardService + NonogramBoardService — правило целиком,
│                 NonogramBoardSaveData, NonogramMessages (MessagePipe)
├── Presentation/ NonogramSkin, NonogramCellView, NonogramClueView, NonogramBoardInput,
│                 NonogramBoardView, NonogramHudView, NonogramColorSwatchView,
│                 NonogramPresenter
└── UI/           NonogramPage
```

Папки называются `Board` и `Presentation`, а не `Service` / `View` / `Presenter`: последний
сегмент namespace, совпадающий с именем базового класса ядра, даёт CS0118.

### Модель

Клетка хранит `CellState` (Empty / Filled / Marked) и `byte ColorIndex`: 0 — пусто, 1..N —
индекс в палитре уровня. Чёрно-белая нонограмма — это палитра из одного цвета, отдельного
кода под неё нет. Доска не больше 40x40 (ограничение импорта), палитра не больше 16 цветов.

### Ч/б режим и раскраска после победы

Флаг `PlayMonochrome` на `NonogramLevelAsset` (по умолчанию включён, сейчас стоит у всех
уровней). При загрузке сервис держит **два** решения: рабочее, где все ненулевые цвета
схлопнуты в `1`, и исходные цвета уровня — их отдаёт `GetRevealColor(address)`.

Это не косметика, а другая головоломка: в ч/б соседние блоки разных цветов сливаются в один,
поэтому и подсказки у ч/б версии другие. Их строит тот же `NonogramClueBuilder`, но уже по
схлопнутому решению. Само правило про ч/б не знает — для него это обычная одноцветная доска,
`PaletteSize` возвращает 1, палитра в HUD прячется.

По `BoardSolved` вью прогоняет `PlayRevealAsync`: DOTween-твин цвета от чернил
(`NonogramSkin.MonochromeInkColor`) к настоящему цвету клетки, волной по диагонали
(`RevealCellDuration`, `RevealWaveDelay`). Флаг `_isRevealed` во вью нужен, чтобы пересборка
доски после раскраски не вернула чернила: `Build` его сбрасывает, а пересборка по смене
размера вьюпорта — нет.

**Твины раскраски обязаны умирать вместе с клеткой.** `NonogramCellView` держит твин в поле
и убивает его в `OnDestroy`, а `PlayRevealAsync` связывает переданный токен с `DestroyToken`
вью. Без этого выход в меню сразу после победы даёт пачку `DOTWEEN ► Target or field is
missing/null` — твины продолжают тикать по уничтоженным `Image`. В этой версии DOTween
`SetLink` недоступен, поэтому чистим руками.

### Два режима ошибок

Флаг `MistakeMode` в `NonogramRuleConfig`:

- `Validated` — закраска не своей клетки считается ошибкой: минус жизнь, клетка сама
  принимает верное состояние, стереть верную закраску нельзя. Линия считается решённой по
  счётчику верных клеток.
- `Free` — классический пикросс: закрашивать можно что угодно, жизни не тратятся, линия
  решена, когда её цвета сходятся с подсказками (`NonogramClueBuilder.AreEqual`).

Там же `Lives`, `Hints`, `AllowMarks`, `AutoMarkCompletedLines` (автозачёркивание
завершённой линии).

### Конфиги и ассеты

| Ассет | Путь |
|---|---|
| Правило | `Assets/G/Configs/Nonogram/NonogramRuleConfig.asset` |
| Список уровней | `Assets/G/Configs/Nonogram/NonogramLevelsConfig.asset` |
| Скин (весь арт) | `Assets/G/Configs/Nonogram/NonogramSkin.asset` |
| Уровни | `Assets/G/Configs/Nonogram/Levels/*.asset` |
| Картинки уровней | `Assets/G/Textures/Nonogram/*.png` |
| Префабы | `Assets/G/Prefabs/UI/Nonogram/` — `NonogramCell`, `NonogramClue`, `NonogramHeart`, `NonogramColorSwatch`, `NonogramPage` |

В `NonogramSkin` лежат спрайты, цвета, `MinCellSize`/`MaxCellSize`, шаг сетки (`BlockSize`,
`BlockSpacing`, `CellSpacing`), `ClueFontRatio`, `ClueChipPadding`, цвет чернил ч/б режима и
тайминги раскраски.

**Цифра подсказки всегда сидит на чипе-подложке, и это не украшение.** Пары цветов заданы
явно: `ClueChipColor` / `ClueTextColor` и `ClueSolvedChipColor` / `ClueSolvedTextColor`.
Раньше решённая линия гасилась множителем альфы, а в монохроме чип вообще отключался — на
тёмном фоне страницы тёмный текст давал контраст 1.1:1, то есть цифры пропадали. Гашение
альфой на произвольном фоне выкинуто намеренно: подложка и явная пара цветов не могут
случайно слиться с фоном. Меняешь цвета — проверь контраст текста к чипу, а не к фону. Сейчас там спрайты-заглушки из встроенного
UI-скина — их меняют на настоящий арт, код при этом не трогают.

### Новый уровень из картинки

1. Положи PNG в `Assets/G/Textures/Nonogram/`. Один пиксель — одна клетка, прозрачный
   пиксель (alpha < 128) — пустая клетка.
2. В импортере обязательно: **Read/Write Enabled = On**, Filter Mode = Point,
   Compression = None. Без `isReadable` сборка уровня падает с громкой ошибкой.
3. Создай `Configs/Nonogram/Level` (меню Create), задай `Id`, положи текстуру в
   `Source Texture`, вызови контекстное меню **«Собрать из текстуры»** — палитра и решение
   соберутся сами. Строка 0 решения — верх картинки.
4. Добавь ассет уровня в `NonogramLevelsConfig`. Дубликаты `Id` конфиг ловит в `OnValidate`.

Близкие цвета схлопываются в один индекс палитры с допуском 0.08 по каналу — сводить
палитру нужно в графическом редакторе, а не надеяться на импортёр.

### Экран уровня

**Вся страница уровня — один экран `NonogramPage`.** Внутри неё доска (`NonogramBoardView`) и
HUD (`NonogramHudView`): сверху кнопка «Назад» и жизни сердечками, снизу палитра цветов и
переключатель «Закрасить» / «Пометить». `HudPage` — пустая заглушка и стартовая страница
сцены, своего содержимого у неё нет.

Так вышло не из вкусовщины: корень `NonogramPage` — непрозрачный полноэкранный `Image`, и
страницы одного слоя лежат сиблингами на одном Canvas в порядке открытия. Всё, что положить
на `HudPage`, доска просто закроет собой. Не клади элементы уровня в `HudPage`.

Палитра прячется целиком, когда цвет один, — то есть на всех ч/б уровнях. Код выбора цвета
живой и включится сам, как только появится уровень с `PlayMonochrome = false`.

### Экран проигрыша

Жизни кончились → `BoardFailed` → `LevelFailedPage` на слое `Popup` с двумя кнопками:
«Продолжить» и «В меню». Страница реализует `IResultPage<LevelFailedResult>` из фундамента,
поэтому показ и ожидание ответа — это один вызов `ShowForResultAsync`.

Открывает её **`GameSceneEntry`, а не `NonogramPage`**, и причина не стилистическая:
`IPageService` биндится в сценовом контейнере, а страницы инжектятся из проектного (см.
раздел «UI»), так что страница физически не может его получить. `GameSceneEntry` сценовый и
уже отвечает за то, какой экран открыт.

`Continue` зовёт `INonogramBoardService.Revive()`: жизни возвращаются к значению из
`NonogramRuleConfig`, флаг поражения снимается, закрашенные клетки остаются на месте.
`Menu` уходит в `LevelFlowService.ReturnToMenuAsync`. Первый элемент `LevelFailedResult` —
`Menu`, потому что при отмене `ShowForResultAsync` возвращает `default`: безопаснее увести
в меню, чем молча воскресить.

**Продолжение сейчас бесплатное и без лимита** — рекламы и экономики в проекте нет.
Точка подключения одна: вызов `Revive()` в `GameSceneEntry.ShowFailedAsync`, туда и встанет
`IPlatformSdk.Ads` или списание валюты.

Посредник ровно один — `NonogramPresenter`: `Attach(boardView, hudView)` связывает сообщения
доски с обоими вью и дёргает `SetTool` / `SetColor`. Навигацию презентер не знает: подписку
на `BackClicked` держит сама страница и зовёт `LevelFlowService.ReturnToMenuAsync`.

### Рендер и ввод

`NonogramBoardView` считает раскладку сам (без `LayoutGroup`): размер клетки подбирается под
`viewport` и зажимается в `[MinCellSize, MaxCellSize]`, полосы подсказок стоят слева и сверху,
все корни с pivot (0,1). Клетки и цифры берутся из внутренних списков-пулов самого вью —
не из `ObjectPoolService` (тот мировой и живёт в `DontDestroyOnLoad`).

У клеток и цифр `raycastTarget = false`. Ввод ловит **один** прозрачный `Image` с
`NonogramBoardInput` поверх сетки, адрес клетки считается из локальной точки; там же осевой
замок протяжки. `NonogramPresenter` держит `BeginBatch()` на всё время штриха, поэтому
протяжка по линии — одна пачка изменений и одна пометка сейва.

### Точки интеграции

- `GameConfigs` — поля `NonogramRule` и `NonogramLevels`, оба биндятся в `ConfigsInstaller`.
- `GameplayInstaller` — `BindService<NonogramBoardService>()` и
  `BindPresenter<NonogramPresenter>()` (проектный контейнер, ставится из `ProjectInstaller`).
- `NonogramMessages.Install` подхватывается рефлексией из `InstallAllMessageBrokers`,
  руками регистрировать брокеры не нужно.
- Сейв — секция **`nonogram`**, `Order = 20`. Пишется прогресс доски, жизни и подсказки;
  если `LevelId` в сейве не совпал с загруженным уровнем, доска начинается заново.
- UI — `NonogramPage : PayloadPage<NonogramLevelAsset>` на слое `Hud`, префаб прописан
  в `PagesConfig`. Открывается через
  `IPageService.ShowAsync<NonogramPage, NonogramLevelAsset>(level)`.

Игрового входа в уровень (карта, кнопка на HUD, выбор инструмента и цвета) пока нет:
`NonogramPresenter.SetTool` / `SetColor` ждут своего UI.

## Меню и прогресс уровней

Сцена `Menu` — копия каркаса `Game` (камера, объект `UI` со слоями, `SceneContext`), отличается
только конфигом страниц и вторым инсталлером. Стартовая страница — `LevelSelectPage`
(`Meta/LevelSelect/`): `ScrollRect` с `GridLayoutGroup` и пулом плиток `LevelTileView`.

`LevelProgressService` (`Meta/LevelProgress/`, секция сейва `levelProgress`) сам подписан на
`NonogramMessages.BoardSolved` — доска ничего не знает про прогресс, связь идёт через шину.
Гейтинг линейный: уровень открыт, если он первый в `NonogramLevelsConfig` или предыдущий
пройден. **Порядок уровней в конфиге и есть порядок прохождения** — переставил строки,
переставил и гейтинг.

Плитка показывает превью картинки **только у пройденного** уровня, у остальных «?» или замок.
Превью рисует `LevelPreviewFactory` из решения уровня в `Texture2D` с точечной фильтрацией и
кэширует по `Id`; показывать его у непройденного — значит выдать ответ.

Новый уровень в меню появляется сам, как только попадает в `NonogramLevelsConfig`.

## История

- 2026-09-14 — фундамент собран агентом `unity-foundation` в проекте `TestAgenLogic`
  (`Assets/Scripts/`, namespace `Scripts`), три редакторных валидатора, R3/Addressables/
  Localization/DOTween заведены руками из-за EPERM на распаковке пакетов.
- 2026-09-15 — проект переименован в `Match3-Game`. Код перенесён под `Assets/G/`,
  namespace `Scripts.*` → `G.*` (GUID-ы скриптов и все ссылки в сценах/префабах/ассетах
  сохранены — переезд через `AssetDatabase.MoveAsset`, без потери связей). Три валидатора
  удалены вслед за новой версией агента, которая проверяет фундамент сама на шаге 8,
  а не оставляет постоянный `Tools →` инструмент.
- 2026-09-17 — экран проигрыша `LevelFailedPage` с продолжением и выходом в меню,
  `Revive()` на сервисе доски.
- 2026-09-17 — подсказки вернулись на чипы-подложки с явными парами цветов вместо гашения
  альфой: в ч/б они сливались с фоном. Плюс Bold и `ClueFontRatio` 0.5 → 0.62.
- 2026-09-17 — ч/б режим уровней с раскраской по победе, HUD уровня (жизни, палитра,
  переключатель инструмента, «Назад») перенесён на `NonogramPage`, `HudPage` снова пустой.
  Product Name → `Nonogram-Game`, путь сейвов сменился.
- 2026-09-17 — добавлено меню `Tools → Сейвы` (`Core/Save/Editor/` — первая `Editor`-папка
  в проекте). Формулировка правила про редакторные инструменты сужена до фоновых валидаторов.
- 2026-09-17 — добавлена сцена `Menu` с выбором уровня, `LevelFlowService` и
  `LevelProgressService` с линейным гейтингом. `PagesConfig` стал сценовым, `AudioListener`
  переехал на `ProjectContext`, проект переименован в `Nonogram-Game`.
- 2026-09-17 — встроена кор-механика нонограмм (`G/Scripts/Gameplay/Nonogram/`): правило,
  импорт уровней из картинок, uGUI-рендер и ввод, страница `NonogramPage`, секция сейва
  `nonogram`. Заведены три тестовых уровня (5x5 ч/б, 10x10 и 15x15 цветные) и скин со
  спрайтами-заглушками. Проверено в Play Mode: доска строится, протяжка и режим ошибок
  `Validated` работают, решённая линия гасит цифры, `BoardSolved` публикуется,
  секция сейва пишется на выходе из Play Mode.
