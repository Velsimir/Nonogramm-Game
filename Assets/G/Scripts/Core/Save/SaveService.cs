using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using R3;
using UnityEngine;
using Zenject;
using G.Core.Common;

namespace G.Core.Save
{
    public class SaveService : Service, ISaveService
    {
        private const float ROUTINE_THROTTLE_SECONDS = 15f;

        private readonly List<ISaveParticipant> _participants;
        private readonly ISaveStorage _localStorage;
        private readonly ISaveStorage _cloudStorage;
        private readonly IApplicationLifecycle _lifecycle;

        private readonly SerialDisposable _pendingFlush = new();

        private SaveEnvelope _envelope = SaveEnvelope.CreateDefault();
        private SaveReason _pendingReason;
        private bool _isDirty;
        private bool _isWriting;
        private float _lastRoutineWriteTime = float.NegativeInfinity;

        public SaveService(
            [InjectOptional] List<ISaveParticipant> participants,
            [Inject(Id = SaveStorageId.LOCAL)] ISaveStorage localStorage,
            [Inject(Id = SaveStorageId.CLOUD)] ISaveStorage cloudStorage,
            IApplicationLifecycle lifecycle)
        {
            _participants = participants == null
                ? new List<ISaveParticipant>()
                : participants.OrderBy(participant => participant.Order).ToList();

            _localStorage = localStorage;
            _cloudStorage = cloudStorage;
            _lifecycle = lifecycle;
        }

        protected override void OnInitialize()
        {
            _pendingFlush.AddTo(Disposables);

            _lifecycle.Paused
                .Where(isPaused => isPaused)
                .Subscribe(_ => SaveNowAsync(includeCloud: true, DisposeToken).Forget())
                .AddTo(Disposables);

            _lifecycle.Quitting
                .Subscribe(_ => SaveNowAsync(includeCloud: true, CancellationToken.None).Forget())
                .AddTo(Disposables);
        }

        public async UniTask LoadAsync(CancellationToken cancellationToken = default)
        {
            string localJson = await _localStorage.ReadAsync(cancellationToken);
            SaveEnvelope local = Deserialize(localJson, _localStorage.Id);

            if (_cloudStorage == null || _cloudStorage.IsAvailable == false)
            {
                _envelope = local ?? SaveEnvelope.CreateDefault();
                return;
            }

            string cloudJson = await _cloudStorage.ReadAsync(cancellationToken);
            SaveEnvelope cloud = Deserialize(cloudJson, _cloudStorage.Id);

            _envelope = ResolveConflict(local, cloud);
        }

        public void RestoreAll()
        {
            foreach (ISaveParticipant participant in _participants)
            {
                try
                {
                    participant.Restore(_envelope.GetSection(participant.Key));
                }
                catch (Exception exception)
                {
                    GameDebug.LogError(
                        $"[Save] Участник «{participant.Key}» не смог восстановиться: {exception.Message}");
                }
            }
        }

        public void MarkDirty(SaveReason reason)
        {
            _isDirty = true;

            if (reason > _pendingReason)
                _pendingReason = reason;

            bool isThrottled = reason == SaveReason.Routine
                && Time.realtimeSinceStartup - _lastRoutineWriteTime < ROUTINE_THROTTLE_SECONDS;

            if (isThrottled)
                return;

            ScheduleFlush();
        }

        public async UniTask SaveNowAsync(bool includeCloud, CancellationToken cancellationToken = default)
        {
            if (_isWriting)
                return;

            _isWriting = true;

            try
            {
                CaptureAll();

                string json = JsonConvert.SerializeObject(_envelope, Formatting.None);

                await _localStorage.WriteAsync(json, cancellationToken);

                if (includeCloud && _cloudStorage != null && _cloudStorage.IsAvailable)
                    await _cloudStorage.WriteAsync(json, cancellationToken);

                _isDirty = false;
                _pendingReason = SaveReason.Routine;
                _lastRoutineWriteTime = Time.realtimeSinceStartup;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                GameDebug.LogError($"[Save] Запись не удалась: {exception.Message}");
            }
            finally
            {
                _isWriting = false;
            }
        }

        private void ScheduleFlush()
        {
            _pendingFlush.Disposable = Observable.NextFrame()
                .Take(1)
                .Subscribe(_ =>
                {
                    if (_isDirty == false)
                        return;

                    bool includeCloud = _pendingReason == SaveReason.Critical;
                    SaveNowAsync(includeCloud, DisposeToken).Forget();
                });
        }

        private void CaptureAll()
        {
            foreach (ISaveParticipant participant in _participants)
            {
                try
                {
                    object data = participant.Capture();

                    if (data == null)
                        continue;

                    _envelope.Sections[participant.Key] = JToken.FromObject(data);
                }
                catch (Exception exception)
                {
                    GameDebug.LogError($"[Save] Участник «{participant.Key}» не отдал снимок: {exception.Message}");
                }
            }

            _envelope.Version = SaveEnvelope.CURRENT_VERSION;
            _envelope.SavedAtUtc = DateTime.UtcNow;
        }

        private static SaveEnvelope Deserialize(string json, string storageId)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<SaveEnvelope>(json);
            }
            catch (JsonException exception)
            {
                GameDebug.LogError($"[Save] Сохранение «{storageId}» не разобрано: {exception.Message}");
                return null;
            }
        }

        private static SaveEnvelope ResolveConflict(SaveEnvelope local, SaveEnvelope cloud)
        {
            if (local == null && cloud == null)
                return SaveEnvelope.CreateDefault();

            if (local == null)
                return cloud;

            if (cloud == null)
                return local;

            return cloud.SavedAtUtc > local.SavedAtUtc ? cloud : local;
        }
    }
}
