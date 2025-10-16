using UnityEngine;


namespace ModularEventArchitecture
{
    public class TestModule : ModuleBase
    {
        public override void Initialize()
        {
            // Подписка на тестовые события которое без колбэка
            Entity.SubscribeLocalEvent<MassageDTO>(TestLocal);

            // Подписка на тестовое событие которое с колбэком
            Entity.SubscribeLocalEvent<MassageDTO, ResponseMassageDTO>(OnTestLocalResponse);

            // Подписка на глобальные тестовые события которое без колбэка
            Entity.SubscribeGlobalEvent<MassageDTO>(OnTestGlobal);

            // Подписка на глобальные тестовые события которое с колбэком
            Entity.SubscribeGlobalEvent<MassageDTO, ResponseMassageDTO>(OnTestGlobalResponse);
        }

        private void TestLocal(MassageDTO eventBase)
        {
            Debug.Log($"<color=green> Получено тестовое ЛОКАЛЬНОЕ событие</color> на объекте {Entity.name}");
        }
        private ResponseMassageDTO OnTestLocalResponse(MassageDTO eventBase)
        {
            Debug.Log($"<color=green> Получено тестовое ЛОКАЛЬНОЕ событие</color> на объекте {Entity.name} <color=green> и направлен ответ</color>");
            return new ResponseMassageDTO { Result = "Колбэк" };
        }
        private void OnTestGlobal(MassageDTO eventBase)
        {
            Debug.Log($"<color=red> Получено тестовое ГЛОБАЛЬНОЕ событие</color> на объекте {Entity.name}");
        }
        private ResponseMassageDTO OnTestGlobalResponse(MassageDTO eventBase)
        {
            Debug.Log($"<color=red> Получено тестовое ГЛОБАЛЬНОЕ событие</color> на объекте {Entity.name} <color=red> и направлен ответ</color>");
            return new ResponseMassageDTO { Result = "Колбэк" };
        }

        public override void UpdateMe()
        {
            // Здесь можно реализовать логику обновления модуля, если требуется
            // Debug.Log($"<color=blue>Обновление модуля {GetType().Name} на объекте {Entity.name}</color>");
        }
    }
}