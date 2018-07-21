using System.Collections.Generic;
using System.Threading.Tasks;
using Fasolib.Helpers;
using Fasolib.Managers;
using Infogare.Classes.Presenters;

namespace Infogare.Classes.Managers
{
    class BinaryManager
    {
        public static async Task Insert<T>(string filename, T obj)
        {
            var stack = (List<T>)await ObjectManager.DeserializeToObject<List<T>>(filename);
            var retstack = new List<T>();
            if (stack != null) retstack.AddRange(stack);

            if (stack != null)
            {
                var found = false;
                foreach (var element in stack)
                {
                    found = false;
                    if (typeof(T) == typeof(GareSuggestionPresenter))
                    {
                        var target = obj as GareSuggestionPresenter;
                        var item = element as GareSuggestionPresenter;

                        if (item != null &&
                            (target != null &&
                             (target.GareName == item.GareName && target.Trigramme == item.Trigramme &&
                              target.Logo == item.Logo)))
                        {
                            found = true;
                        }
                    }

                    if (!found) continue;

                    retstack.Remove(element);
                    retstack.Insert(0, obj);
                    break;
                }
                if (!found) retstack.Insert(0, obj);
            }
            else
            {
                retstack = new List<T> { obj };
            }
            await ObjectHelper.SerializeToFile(retstack, filename);
        }
    }
}
