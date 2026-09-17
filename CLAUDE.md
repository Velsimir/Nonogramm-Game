# Match3-Game — контекст проекта

Мобильный match-3 / puzzle под Android и iOS, 2D. В проекте лежит только архитектурный
фундамент: геймплея, экономики и контента нет.

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
│   ├── Configs/    GameConfigs, PagesConfig, PlatformSdkConfig, ConfigsInstaller (.asset)
│   ├── Prefabs/    Curtain.prefab, UI/HudPage.prefab
│   ├── Resources/  ProjectContext.prefab, DOTweenSettings.asset
│   ├── Scenes/     Boot.unity (индекс 0), Game.unity (индекс 1)
│   └── Scripts/    корневой namespace G
│       ├── Core/     Common, DI, Boot, Curtain, Scenes, Save, UI, Tick, Factory, Pool
│       ├── Platform/ фасад IPlatformSdk + StubPlatformSdk
│       ├── Configs/  GameConfigs
│       ├── Meta/     пусто — сюда кладутся механики
│       └── Gameplay/ UI/HudPage.cs — заглушка стартовой страницы
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

**Редакторных валидаторов и меню `Tools → …` в проекте нет.** Раньше здесь стояли три
(`ArchitectureValidator`, `ScenesNameValidator`, `SceneSingletonValidator`), их убрали
2026-09-15 вслед за обновлением агента `unity-foundation`: он теперь проверяет фундамент сам
при сборке, а не оставляет постоянно шумящий в консоли инструмент. Если правила ниже
(имена сцен, единственные `AudioListener`/`EventSystem`, `[Inject]` во `View`, `UI` не ссылается
на `Gameplay`) когда-нибудь массово нарушатся — проверяй руками, автоматики для этого нет.

## Конвейер загрузки

`Boot` → `GameBootstrapper` → шторка → операции (инициализация SDK → загрузка сейва →
восстановление прогресса → загрузка `Game`) → `Game` становится активной → шторка убирается →
`Boot` выгружается. Стартовая страница `HudPage` открывается на слое `Hud`.

Каждому элементу `ScenesName` обязана соответствовать сцена в Build Settings. Добавил сцену —
правь и enum, и `SceneNameMap`, и Build Settings; автоматической проверки нет, сверяй руками.

## UI

Один Canvas на каждый `UILayer`, `sortingOrder` равен значению enum: Hud 0, Window 100,
Popup 200, Overlay 300, Loading 400, System 500. Плюс отдельный Canvas 1000 с `InputBlocker`.
Корни слоёв собраны в `UILayerRoots` на объекте `UI` в сцене `Game`.

Новая страница: префаб с наследником `Page`/`PayloadPage`, слой выставляется в инспекторе,
префаб добавляется в `PagesConfig` (есть контекстное меню «Собрать все префабы страниц»).

## Сейвы

`persistentDataPath/Save/save.json`, то есть
`%USERPROFILE%\AppData\LocalLow\<CompanyName>\<ProductName>\Save\save.json` на Windows.
Пишется по `Paused` и `Quitting` из `ApplicationLifecycleWatcher` — на Android
`OnApplicationQuit` не гарантирован. Формат — конверт с версией и словарём секций:

```json
{"version":1,"savedAtUtc":"...","sections":{}}
```

Секции пустые, пока нет ни одного `ISaveParticipant`.

**Company Name и Product Name задают путь сейвов.** Менять их после релиза нельзя — игроки
потеряют прогресс.

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

- `AudioListener` — только на `Main Camera` сцены `Game`.
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

## История

- 2026-09-14 — фундамент собран агентом `unity-foundation` в проекте `TestAgenLogic`
  (`Assets/Scripts/`, namespace `Scripts`), три редакторных валидатора, R3/Addressables/
  Localization/DOTween заведены руками из-за EPERM на распаковке пакетов.
- 2026-09-15 — проект переименован в `Match3-Game`. Код перенесён под `Assets/G/`,
  namespace `Scripts.*` → `G.*` (GUID-ы скриптов и все ссылки в сценах/префабах/ассетах
  сохранены — переезд через `AssetDatabase.MoveAsset`, без потери связей). Три валидатора
  удалены вслед за новой версией агента, которая проверяет фундамент сама на шаге 8,
  а не оставляет постоянный `Tools →` инструмент.
