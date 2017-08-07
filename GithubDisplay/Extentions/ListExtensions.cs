using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace GithubDisplay.Extentions
{
    public static class ListExtensions
    {
        public static IList<T> KeepEvery<T>(this IList<T> list, int keepEvery = 1)
        {
            if (keepEvery < 1) { return new List<T>(); }
            if (keepEvery == 1) { return list; }

            var newList = new List<T>();

            for (int i = 0; i <= list.Count - 2; i += keepEvery)
            {
                newList.Add(list[i]);
            }

            newList.Add(list.Last());
            return newList;
        }

        public static bool ContainsFile(this IEnumerable<Tuple<StorageFile, BitmapImage>> list, StorageFile file)
        {
            return list.Any(elem => elem?.Item1.Name == file?.Name && elem?.Item1?.Path == file?.Path);
        }

        public static IList<T> AddIfNotNull<T>(this IList<T> list, T value)
        {
            return list.AddIf(value, l => l != null);
        }

        public static IList<T> AddIf<T>(this IList<T> list, T value, Func<T, bool> condition)
        {
            if (condition.Invoke(value)) { list.Add(value); }
            return list;
        }

        public static IDictionary<T,V> AddIfNotNull<T, V>(this IDictionary<T, V> dictionary, T key, V value)
        {
            return dictionary.AddIf(key, value, l => l != null);
        }

        public static IDictionary<T, V> AddIf<T, V>(this IDictionary<T, V> dictionary, T key, V value, Func<V, bool> condition)
        {
            if (condition.Invoke(value)) { dictionary.Add(key, value); }
            return dictionary;
        }

        public static void RemoveWhere<T>(this IList<T> iList, Func<T, bool> selector)
        {
            var filter = iList.Where(selector);

            // https://stackoverflow.com/questions/18027575/most-efficient-way-to-remove-multiple-items-from-a-ilistt/18028605#18028605
            var set = new HashSet<T>(filter);

            var list = filter as List<T>;
            if (list == null)
            {
                int i = 0;
                while (i < iList.Count)
                {
                    if (set.Contains(iList[i])) iList.RemoveAt(i);
                    else i++;
                }
            }
            else
            {
                list.RemoveAll(set.Contains);
            }
        }
        
        /// <summary>
        /// Update the first list to make it look like the second list by
        /// performing additions, deletions and moves
        /// </summary>
        /// <typeparam name="T">The type of list items</typeparam>
        /// <param name="existingList">The original list to update</param>
        /// <param name="newList">The new list to merge to the original list</param>
        /// <param name="comparer">Custom comparer. Default comparer used if null</param>
        /// <exception cref="NullReferenceException">Throws if existingList is null</exception>
        public static void UpdateAndSortFromList<T>(this IList<T> existingList, IList<T> newList, IEqualityComparer<T> comparer = null)
        {
            if (comparer == null) { comparer = EqualityComparer<T>.Default; }
            if (existingList == null) { throw new NullReferenceException("Expecting value in existingList"); }
            if (newList == null)
            {
                existingList.Clear();
                return;
            }

            var removedItems = existingList.Except(newList, comparer).ToList();

            foreach (var item in removedItems)
            {
                existingList.Remove(item);
            }

            for (var i = 0; i < newList.Count; i++)
            {
                if (i < existingList.Count && comparer.Equals(existingList[i], newList[i]))
                {
                    continue;
                }

                if (existingList.Contains(newList[i], comparer))
                {
                    existingList.Remove(existingList.First(item => comparer.Equals(item, newList[i])));
                }

                existingList.Insert(i, newList[i]);
            }
        }
    }
}
