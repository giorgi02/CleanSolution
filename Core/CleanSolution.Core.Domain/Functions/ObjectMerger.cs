﻿using System.Reflection;

namespace CleanSolution.Core.Domain.Functions;
public static class ObjectMerger
{
    public static TDest ApplyTo<TDest, TSrc>(this TDest dest, TSrc src) where TDest : class where TSrc : class
    {
        var destProperties = typeof(TDest).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(x => x.Name).ToList();
        var srcProperties = typeof(TSrc).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(x => x.Name).ToList();
        var commonProperties = destProperties.Intersect(srcProperties).ToList();

        foreach (var prop in commonProperties)
        {
            PropertyInfo destInfo = typeof(TDest).GetProperty(prop);
            PropertyInfo srcInfo = typeof(TSrc).GetProperty(prop);

            if (destInfo?.PropertyType != srcInfo?.PropertyType)
                continue;

            if (destInfo.PropertyType != typeof(string) && !destInfo.PropertyType.IsValueType)
                continue;


            if (destInfo.GetValue(dest) != srcInfo.GetValue(src))
                destInfo.SetValue(dest, srcInfo.GetValue(src));
        }

        return dest;
    }


    // CancellationToken ის ვადის გასვლისას ერორის გასროლა
    public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

        // This disposes the registration as soon as one of the tasks trigger
        await using (cancellationToken.Register(state => ((TaskCompletionSource<object?>)state).TrySetResult(null), tcs))
        {
            var resultTask = await Task.WhenAny(task, tcs.Task);
            if (resultTask == tcs.Task)
            {
                // Operation cancelled
                throw new OperationCanceledException(cancellationToken);
            }

            return await task;
        }
    }
}