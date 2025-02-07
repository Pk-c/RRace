using System;
using System.Collections;
using System.Collections.Generic;

namespace Game
{
    public class SceneManager
    {
        private static Dictionary<Type, IList> SceneObjects = new Dictionary<Type, IList>();

        public static void Register<T>(T obj)
        {
            Type type = typeof(T);
            if (!SceneObjects.ContainsKey(type))
            {
                SceneObjects[type] = new List<T>();
            }
            ((List<T>)SceneObjects[type]).Add(obj);
        }

        public static bool Remove<T>(T obj)
        {
            Type type = typeof(T);
            if (SceneObjects.ContainsKey(type))
            {
                List<T> list = (List<T>)SceneObjects[type];
                bool removed = list.Remove(obj);
                if (list.Count == 0)
                {
                    SceneObjects.Remove(type);
                }
                return removed;
            }
            return false;
        }

        public static IEnumerable<T> EnumerateObjects<T>()
        {
            Type type = typeof(T);
            if (SceneObjects.TryGetValue(type, out IList objects))
            {
                foreach (T obj in objects)
                {
                    yield return obj;
                }
            }
        }

        public static bool GetObject<T>(out T result)
        {
            Type type = typeof(T);
            if (SceneObjects.TryGetValue(type, out IList objects) && objects.Count > 0)
            {
                result = (T)objects[0];
                return true;
            }
            result = default;
            return false;
        }

        public static T GetObject<T>() where T : class
        {
            Type type = typeof(T);
            if (SceneObjects.TryGetValue(type, out IList objects) && objects.Count > 0)
            {
                return (T)objects[0];
            }
            return null;
        }

        public static int GetObjectsCount<T>() where T : class
        {
            Type type = typeof(T);
            return SceneObjects.TryGetValue(type, out IList objects) ? objects.Count : 0;
        }

        public static void CleanUp()
        {
            SceneObjects.Clear();
        }
    }
}