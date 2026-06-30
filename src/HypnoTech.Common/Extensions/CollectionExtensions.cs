using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace HypnoTech.Common.Extensions
{
    public static class CollectionExtensions
    {
        public static ICollection<T> Shuffle<T>(this ICollection<T> items)
        {
            return items.ToList().Shuffle();
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> items)
        {
            return items.ToList().Shuffle();
        }

        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            using (RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider())
            {
                int n = list.Count;
                while (n > 1)
                {
                    byte[] box = new byte[1];
                    do provider.GetBytes(box);
                    while (!(box[0] < n * (byte.MaxValue / n)));
                    int k = box[0] % n;
                    n--;
                    T value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
                return list;
            }
        }
    }
}
