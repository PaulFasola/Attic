using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage; 
using Infoécran.Classes.Presenters;

namespace Infoécran.Classes
{
    class ObjectManager
    {
        public static async Task SerializeToFile<T>(T data, string filename)
        {
            var sessionData = new MemoryStream();
            var serializer = new DataContractSerializer(typeof(T));
            serializer.WriteObject(sessionData, data);
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            using (var fileStream = await file.OpenStreamForWriteAsync())
            {
                sessionData.Seek(0, SeekOrigin.Begin);
                await sessionData.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }
        }

        public static async Task<object> DeserializeToObject<T>(string filename)
        {
            object data;
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(filename);

                if (file == null)
                {
                    return null;
                }

                using (var inStream = await file.OpenSequentialReadAsync())
                {
                    var serializer = new DataContractSerializer(typeof(T)); 
                    data = (T)serializer.ReadObject(inStream.AsStreamForRead());
                    inStream.Dispose();
                }

                return (T)data;

            }
            catch (Exception)
            {
                return null;
            } 
        }

        public static async Task Insert<T>(string filename, T obj)
        {
            var stack = (List<T>)await DeserializeToObject<List<T>>(filename);
            var retstack = new List<T>();
             if(stack != null) retstack.AddRange(stack);

            if (stack != null)
            {
                bool found = false;
                foreach (var element in stack)
                {
                    found = false;
                    if (typeof(T) == typeof (GareSuggestionPresenter))
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
            

                    if (found)
                    { 
                        retstack.Remove(element);
                        retstack.Insert(0, obj);
                        break;
                    } 
                }
                if (!found) retstack.Insert(0, obj);    
            }
            else
            {
                retstack = new List<T> { obj };
            }

            await SerializeToFile(retstack, filename);
        }

    }
}
