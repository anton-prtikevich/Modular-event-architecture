using UnityEngine;

namespace ModularEventArchitecture
{
    public class DebugModule : ModuleBase
    {
        public override void Initialize()
        {
            Entity.PublishGlobalEvent(new MassageDTO(), true);
        }

        [Button("Вывести в консоль события LocalEventBus")]
        private void DebugLocalEventBus()
        {
            Entity.LogLocalEvents();
        }

        [Button("Вывести в консоль события GlobalEventBus")]
        private void DebugGlobalEventBus()
        {
            Entity.LogGlobalEvents();
        }

        [Button("Вызвать тестовое ЛОКАЛЬНОЕ событие")]
        private void TestLocalEvent()
        {
            Debug.Log("Отправлено ЛОКАЛЬНОЕ событие без ответа");

            //вызвать событие без ответа
            Entity.PublishLocalEvent(new MassageDTO());
        }

        [Button("Вызвать тестовое ЛОКАЛЬНОЕ событие с ответом")]
        private void TestLocalResponceEvent()
        {
            //вызвать событие с ответом
            ResponseMassageDTO response = Entity.PublishLocalEvent<MassageDTO, ResponseMassageDTO>(new MassageDTO());

            Debug.Log($"Получен ответ на локальный запрос: {response.Result}");
        }

        [Button("Вызвать тестовое ГЛОБАЛЬНОЕ событие")]
        private void TestGlobalEvent()
        {
            Debug.Log("Отправлено глобальное событие без ответа");

            Entity.PublishGlobalEvent(new MassageDTO());
        }

        [Button("Вызвать тестовое ГЛОБАЛЬНОЕ событие с ответом")]
        private void TestGlobalResponceEvent()
        {
            ResponseMassageDTO response = Entity.PublishGlobalEvent<MassageDTO, ResponseMassageDTO>(new MassageDTO());
            Debug.Log($"Получен ответ на глобальный запрос: {response.Result}");
        }
    }
}
