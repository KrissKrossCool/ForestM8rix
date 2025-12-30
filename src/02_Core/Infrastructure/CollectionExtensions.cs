using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ForestM8rix.Infrastructure
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Синхронный итератор.
        /// Возвращает элементы по одному, не создавая промежуточных списков.
        /// </summary>
        public static IEnumerable<object> EnsureEnumerable(this object input)
        {
            if (input == null) yield break;

            // Строка - это IEnumerable<char>, но логически это один объект данных
            if (input is IEnumerable collection && input is not string)
            {
                foreach (var item in collection) yield return item;
            }
            else
            {
                yield return input;
            }
        }

        /// <summary>
        /// Асинхронный итератор.
        /// Поддерживает распаковку Task и Task<T> перед перечислением.
        /// </summary>
        public static async IAsyncEnumerable<object> EnsureEnumerableAsync(this object input)
        {
            if (input == null) yield break;

            object data = input;

            // Если на входе задача - дожидаемся её завершения
            if (input is Task task)
            {
                await task.ConfigureAwait(false);

                // Хаки reflection для получения Result из Task<T> (так как T неизвестен)
                // В реальном коде лучше использовать dynamic или явный Task<object>
                var prop = task.GetType().GetProperty("Result");
                if (prop != null)
                {
                    data = prop.GetValue(task);
                }
                else
                {
                    // Если это просто Task без результата, данных нет
                    yield break;
                }
            }

            if (data == null) yield break;

            // Логика перечисления (такая же, как в синхронном)
            if (data is IEnumerable collection && data is not string)
            {
                foreach (var item in collection) yield return item;
            }
            else
            {
                yield return data;
            }
        }
    }
}
