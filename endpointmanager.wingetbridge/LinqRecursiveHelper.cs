using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace endpointmanager.wingetbridge
{
    /*
    public DataModel()
    {
        using (var dbContext = new DatabaseContext())
        {
            Employees = from e in dbContext.Employee
                        select new EmployeeDto
                        {
                            ID = e.EmployeeID,
                            DepartmentID = e.DepartmentID,
                            AddressID = e.AddressID,
                            FirstName = e.FirstName,
                            LastName = e.LastName,
                            StreetNumber = e.Address.StreetNumber,
                            StreetName = e.Address.StreetName
                        };
        }
    }*/

    /*
    public static class Extensions
    {
        // Y Combinator generic implementation
        private delegate Func<A, R> Recursive<A, R>(Recursive<A, R> r);
        private static Func<A, R> Y<A, R>(Func<Func<A, R>, Func<A, R>> f)
        {
            Recursive<A, R> rec = r => a => f(r(r))(a);
            return rec(rec);
        }

        // Extension method for IEnumerable<Item>
        public static IEnumerable<PathPartsMSIXTable> Traverse(this IEnumerable<PathPartsMSIXTable> source, Func<PathPartsMSIXTable, bool> predicate)
        {
            var traverse = Extensions.Y<IEnumerable<PathPartsMSIXTable>, IEnumerable<PathPartsMSIXTable>>(
            f => items =>
            {
                var r = new List<PathPartsMSIXTable>(items.Where(predicate));
                r.AddRange(items.SelectMany(i => f(i.Children)));
                return r;
            });

            return traverse(source);
        }
    }

    public interface IHierarchical<T>
    {
        T Parent { get; }
        List<T> Children { get; }
    }

    public static class LinqRecursiveHelper
    {
        /// <summary>
        /// Return item and all children recursively.
        /// </summary>
        /// <typeparam name="T">Type of item.</typeparam>
        /// <param name="item">The item to be traversed.</param>
        /// <param name="childSelector">Child property selector.</param>
        /// <returns></returns>
        public static IEnumerable<T> Traverse<T>(this T item, Func<T, T> childSelector)
        {
            var stack = new Stack<T>(new T[] { item });

            while (stack.Any())
            {
                var next = stack.Pop();
                if (next != null)
                {
                    yield return next;
                    stack.Push(childSelector(next));
                }
            }
        }

        /// <summary>
        /// Return item and all children recursively.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="childSelector"></param>
        /// <returns></returns>
        public static IEnumerable<T> Traverse<T>(this T item, Func<T, IEnumerable<T>> childSelector)
        {
            var stack = new Stack<T>(new T[] { item });

            while (stack.Any())
            {
                var next = stack.Pop();
                //if(next != null)
                //{
                yield return next;
                foreach (var child in childSelector(next))
                {
                    stack.Push(child);
                }
                //}
            }
        }

        /// <summary>
        /// Return item and all children recursively.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="childSelector"></param>
        /// <returns></returns>
        public static IEnumerable<T> Traverse<T>(this IEnumerable<T> items,
          Func<T, IEnumerable<T>> childSelector)
        {
            var stack = new Stack<T>(items);
            while (stack.Any())
            {
                var next = stack.Pop();
                yield return next;
                foreach (var child in childSelector(next))
                    stack.Push(child);
            }
        }
    }*/
}
