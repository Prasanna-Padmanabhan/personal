using SharpJackApi.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpJackApi.UnitTests
{
    public static class Evaluation
    {
        private static readonly ConcurrentDictionary<int, bool> completions = new ConcurrentDictionary<int, bool>();

        public static void OnEvaluationCompleted(object sender, Game e)
        {
            completions.AddOrUpdate(e.Id, true, (id, val) => true);
        }

        public static async Task CompletedAsync(int id)
        {
            while (!completions.GetValueOrDefault(id))
            {
                await Task.Yield();
            }
            completions.AddOrUpdate(id, false, (id, val) => false);
        }
    }
}