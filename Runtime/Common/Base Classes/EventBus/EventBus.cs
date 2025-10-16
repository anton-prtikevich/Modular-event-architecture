        
using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace ModularEventArchitecture
{
    // Message-pipe стиль: события определяются только по типу DTO
    public abstract class EventBus : IDisposable
    {
        // Для каждого типа сообщения храним Subject<T>
        private readonly Dictionary<Type, object> _subjects = new Dictionary<Type, object>();

        // Для запросов с ответом
        private readonly Dictionary<Type, object> _requestSubjects = new Dictionary<Type, object>();
        private readonly Dictionary<Type, object> _responseSubjects = new Dictionary<Type, object>();

        protected EventBus() { }

        // Подписка на событие
        public IDisposable Subscribe<T>(Action<T> handler)
        {
            var subject = GetOrCreateSubject<T>(_subjects);
            return ((ISubject<T>)subject).Subscribe(handler);
        }

        // Публикация события
        public void Publish<T>(T message)
        {
            if (_subjects.TryGetValue(typeof(T), out var subject))
            {
                ((ISubject<T>)subject).OnNext(message);
            }
        }

        // Подписка на запрос с ответом (Request/Response)
        public IDisposable SubscribeRequest<TRequest, TResponse>(Func<TRequest, TResponse> handler)
        {
            var requestSubject = GetOrCreateSubject<TRequest>(_requestSubjects);
            var responseSubject = GetOrCreateResponseSubject<TResponse>(_responseSubjects);
            return ((ISubject<TRequest>)requestSubject).Subscribe(request =>
            {
                var response = handler(request);
                ((ISubject<TResponse>)responseSubject).OnNext(response);
            });
        }

        // Публикация запроса и получение ответа
        public IObservable<TResponse> PublishRequest<TRequest, TResponse>(TRequest request)
        {
            var requestSubject = GetOrCreateSubject<TRequest>(_requestSubjects);
            var responseSubject = GetOrCreateResponseSubject<TResponse>(_responseSubjects);
            ((ISubject<TRequest>)requestSubject).OnNext(request);
            return ((IObservable<TResponse>)responseSubject).Take(1); // только первый ответ
        }

        // Вспомогательный метод для получения/создания Subject<T>
        private object GetOrCreateSubject<T>(Dictionary<Type, object> dict)
        {
            if (!dict.TryGetValue(typeof(T), out var subject))
            {
                subject = new Subject<T>();
                dict[typeof(T)] = subject;
            }
            return subject;
        }

        // Для responseSubject используем ReplaySubject, чтобы не терять первый ответ
        private object GetOrCreateResponseSubject<T>(Dictionary<Type, object> dict)
        {
            if (!dict.TryGetValue(typeof(T), out var subject))
            {
                subject = new ReplaySubject<T>(1);
                dict[typeof(T)] = subject;
            }
            return subject;
        }

        public void Dispose()
        {
            foreach (var subject in _subjects.Values)
            {
                (subject as IDisposable)?.Dispose();
            }
            _subjects.Clear();
            // Можно добавить очистку request/response subjects при необходимости
        }

        // Вывести все типы сообщений и количество подписчиков (для отладки)
        public void ShowAllEvents()
        {
            Debug.Log(_subjects.Count == 0 ? "Нет подписок" : $"Подписок всего: {_subjects.Count}");
        }
    }
}