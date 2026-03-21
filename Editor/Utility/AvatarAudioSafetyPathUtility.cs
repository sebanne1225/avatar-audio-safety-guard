using System.Collections.Generic;
using UnityEngine;

namespace Sebanne.AvatarAudioSafetyGuard.Editor
{
    internal static class AvatarAudioSafetyPathUtility
    {
        public static bool IsDescendantOrSelf(Transform root, Transform target)
        {
            if (root == null || target == null)
            {
                return false;
            }

            Transform current = target;
            while (current != null)
            {
                if (current == root)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        public static string GetRelativePath(Transform root, Transform target)
        {
            if (root == null || target == null)
            {
                return "./";
            }

            if (root == target)
            {
                return "./";
            }

            List<string> names = new List<string>();
            Transform current = target;

            while (current != null && current != root)
            {
                names.Add(current.name);
                current = current.parent;
            }

            if (current == null)
            {
                return "./" + target.name;
            }

            names.Reverse();
            return "./" + string.Join("/", names.ToArray());
        }

        public static Transform ResolveRelativePath(Transform root, string path)
        {
            if (root == null)
            {
                return null;
            }

            string normalized = Normalize(path);
            if (string.IsNullOrEmpty(normalized) || normalized == "./")
            {
                return root;
            }

            string relative = normalized.Substring(2);
            return root.Find(relative);
        }

        public static string Normalize(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            string normalized = path.Trim().Replace('\\', '/');

            if (normalized == "." || normalized == "/" || normalized == "./")
            {
                return "./";
            }

            if (normalized.StartsWith("./"))
            {
                return normalized;
            }

            if (normalized.StartsWith("/"))
            {
                return "." + normalized;
            }

            return "./" + normalized.TrimStart('.');
        }
    }
}
