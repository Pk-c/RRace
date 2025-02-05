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

        public static List<T> GetObjects<T>()
        {
            Type type = typeof(T);

            if (SceneObjects.ContainsKey(type))
            {
                return (List<T>)SceneObjects[type];
            }

            return null;
        }

        public static bool GetObject<T>(out T result)
        {
            Type type = typeof(T);

            if (SceneObjects.ContainsKey(type) && SceneObjects[type].Count > 0)
            {
                result = (T)(SceneObjects[type])[0];
                return true;
            }

            result = default;
            return false;
        }

        public static T GetObject<T>() where T : class
        {
            Type type = typeof(T);

            if (SceneObjects.ContainsKey(type) && SceneObjects[type].Count > 0)
            {
                return (T)(SceneObjects[type])[0];
            }

            return null;
        }

        public static int GetObjectsCount<T>() where T : class
        {
            Type type = typeof(T);

            if (SceneObjects.ContainsKey(type))
            {
                return SceneObjects[type].Count;
            }

            return 0;
        }

        public static void CleanUp()
        {
            SceneObjects.Clear();
        }
    }
}