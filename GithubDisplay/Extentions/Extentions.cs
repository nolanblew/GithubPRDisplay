using System.Linq;
using System.Reflection;

namespace GithubDisplay.Extentions
{
    public static class MergeExtentions
    {
        /// <summary>
        /// Merges a new entity into an existing entity
        /// </summary>
        /// <typeparam name="T">The type we are merging</typeparam>
        /// <param name="originalEntity">The original entity to be merged into</param>
        /// <param name="newEntity">The new entity that we want to merge into the original entity</param>
        /// <returns>True if anything was changed, false if the original entity is unchanged.</returns>
        public static bool Merge<T>(this T originalEntity, T newEntity)
        {
            return Merge(originalEntity, newEntity, new string[] { });
        }

        /// <summary>
        /// Merges a new entity into an existing entity
        /// </summary>
        /// <typeparam name="T">The type we are merging</typeparam>
        /// <param name="originalEntity">The original entity to be merged into</param>
        /// <param name="newEntity">The new entity that we want to merge into the original entity</param>
        /// <param name="propertiesToMerge">Properties to perform the merge on. Leave empty or use overload to use all properties</param>
        /// <returns>True if anything was changed, false if the original entity is unchanged.</returns>
        public static bool Merge<T>(this T originalEntity, T newEntity, params string[] propertiesToMerge)
        {
            var properties = typeof(T).GetProperties();

            var hasChanges = false;

            foreach (var property in properties.Where(p => p.CanWrite && p.CanRead))
            {
                if (!propertiesToMerge.Any() || propertiesToMerge.Contains(property.Name))
                {
                    var originalValue = property.GetValue(originalEntity);
                    var newValue = property.GetValue(newEntity);

                    if (!originalValue.Equals(newValue))
                    {
                        property.SetValue(originalEntity, newValue);
                        hasChanges = true;
                    }
                }
            }
            return hasChanges;
        }
    }
}
