using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Requester<TRequest, TAsker>
{
    public static Dictionary<TAsker, List<TRequest>> DoRequests(
        List<TAsker> askers,
        List<TRequest> available,
        Func<TAsker, float> askerNeed,
        Func<TRequest, float> reinforcePower)
    {
        //should have preference func for asker
        var result = new Dictionary<TAsker, List<TRequest>>();
        foreach (var asker in askers)
        {
            result.Add(asker, new List<TRequest>());
        }
        
        var needs = askers.Select(a => askerNeed(a)).ToList();
        var noNeedCount = needs.Where(n => n <= 0f).Count();
        var needSum = needs.Sum();
        var availCount = available.Count;
        int satisfied = 0;
        int iter = 0;
        while (satisfied < askers.Count - noNeedCount
               && available.Count > 0)
        {
            int index = iter % askers.Count;
            iter++;

            if (needs[index] <= 0)
            {
                continue;
            }
            else
            {
                var avail = available[0];
                available.RemoveAt(0);
                needs[index] -= reinforcePower(avail);
                result[askers[index]].Add(avail);
                if (needs[index] <= 0)
                {
                    satisfied++;
                }
            }
        }

        return result;
    }
}
