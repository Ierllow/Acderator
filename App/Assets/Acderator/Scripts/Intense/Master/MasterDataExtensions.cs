using MasterMemory;
using System;
using ZLinq;
using ZLinq.Linq;

namespace Intense.Master
{
    public static class MasterDataExtensions
    {
        public static ValueEnumerable<Where<FromEnumerable<TElement>, TElement>, TElement> Where<TElement>(this TableBase<TElement> source, Func<TElement, bool> predicate) => source.All.AsValueEnumerable().Where(predicate);
        public static TElement First<TElement>(this TableBase<TElement> source) => source.All.AsValueEnumerable().First();
        public static TElement First<TElement>(this TableBase<TElement> source, Func<TElement, bool> predicate) => source.All.AsValueEnumerable().First(predicate);
        public static TElement Last<TElement>(this TableBase<TElement> source) => source.All.AsValueEnumerable().Last();
        public static TElement Last<TElement>(this TableBase<TElement> source, Func<TElement, bool> predicate) => source.All.AsValueEnumerable().Last(predicate);
        public static TElement FirstOrDefault<TElement>(this TableBase<TElement> source) => source.All.AsValueEnumerable().FirstOrDefault();
        public static TElement FirstOrDefault<TElement>(this TableBase<TElement> source, Func<TElement, bool> predicate) => source.All.AsValueEnumerable().FirstOrDefault(predicate);
        public static TElement LastOrDefault<TElement>(this TableBase<TElement> source) => source.All.AsValueEnumerable().LastOrDefault();
        public static TElement LastOrDefault<TElement>(this TableBase<TElement> source, Func<TElement, bool> predicate) => source.All.AsValueEnumerable().LastOrDefault(predicate);
        public static ValueEnumerable<Select<FromEnumerable<TElement>, TElement, TResult>, TResult> Select<TElement, TResult>(this TableBase<TElement> source, Func<TElement, TResult> selector) => source.All.AsValueEnumerable().Select(selector);
        public static ValueEnumerable<OrderBy<FromEnumerable<TElement>, TElement, TResult>, TElement> OrderBy<TElement, TResult>(this TableBase<TElement> source, Func<TElement, TResult> selector) => source.All.AsValueEnumerable().OrderBy(selector);
    }
}