using MasterMemory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Intense.Master
{
    public static class MasterDataExtensions
    {
        public static IEnumerable<TElement> Where<TElement>(this TableBase<TElement> source, Func<TElement, bool> predicate) => source.All.Where(predicate);
        public static TElement First<TElement>(this TableBase<TElement> source) => source.All[0];
        public static TElement First<TElement>(this TableBase<TElement> source, Func<TElement, bool> predicate) => source.All.First(predicate);
        public static TElement Last<TElement>(this TableBase<TElement> source) => source.All[^1];
        public static TElement Last<TElement>(this TableBase<TElement> source, Func<TElement, bool> predicate) => source.All.Last(predicate);
        public static TElement FirstOrDefault<TElement>(this TableBase<TElement> source) => source.All.FirstOrDefault();
        public static TElement FirstOrDefault<TElement>(this TableBase<TElement> source, Func<TElement, bool> predicate) => source.All.FirstOrDefault(predicate);
        public static TElement LastOrDefault<TElement>(this TableBase<TElement> source) => source.All.LastOrDefault();
        public static TElement LastOrDefault<TElement>(this TableBase<TElement> source, Func<TElement, bool> predicate) => source.All.LastOrDefault(predicate);
        public static IEnumerable<TResult> Select<TElement, TResult>(this TableBase<TElement> source, Func<TElement, TResult> selector) => source.All.Select(selector);
        public static IOrderedEnumerable<TElement> OrderBy<TElement, TResult>(this TableBase<TElement> source, Func<TElement, TResult> selector) => source.All.OrderBy(selector);
    }
}