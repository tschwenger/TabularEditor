﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TOM = Microsoft.AnalysisServices.Tabular;

namespace TabularEditor.TOMWrapper
{
    public static class TabularObjectHelper
    {
        public static string GetLinqPath(this TabularNamedObject obj)
        {
            if(obj is Column || obj is Hierarchy || obj is Measure || obj is Partition)
            {
                var o = obj as ITabularTableObject;
                var colType = obj.GetTypeName(true);
                return string.Format("{0}.{1}[\"{2}\"]", o.Table.GetLinqPath(), colType, obj.Name);
            }

            switch (obj.ObjectType)
            {
                case ObjectType.Model:
                    return "Model";
                case ObjectType.Table:
                    return string.Format("{0}.Tables[\"{1}\"]", obj.Model.GetLinqPath(), obj.Name);
                case ObjectType.Level:
                    return string.Format("{0}.Levels[\"{1}\"]", (obj as Level).Hierarchy.GetLinqPath(), obj.Name);
                case ObjectType.Perspective:
                    return string.Format("{0}.Perspectives[\"{1}\"]", obj.Model.GetLinqPath(), obj.Name);
                case ObjectType.Culture:
                    return string.Format("{0}.Cultures[\"{1}\"]", obj.Model.GetLinqPath(), obj.Name);
                default:
                    throw new NotSupportedException();
            }
        }

        public static string GetObjectPath(this TOM.MetadataObject obj)
        {

            var name = (obj as TOM.NamedMetadataObject)?.Name ?? obj.ObjectType.ToString();

            if (obj.Parent != null)
                return obj.Parent.GetObjectPath() + "." + name;
            else
                return name;
        }

        public static string GetObjectPath(this TabularObject obj)
        {
            return GetObjectPath(obj.MetadataObject);
        }

        public static string SplitCamelCase(this string str)
        {
            return Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }

        public static string GetTypeName(this ObjectType objType, bool plural = false)
        {
            var result = SplitCamelCase(objType.ToString());
            if (plural && result.EndsWith("chy")) result = result.Substring(0, result.Length - 3) + "chies";
            else if (plural && !result.EndsWith("data")) result = result + "s";
            return result;
        }

        public static string GetTypeName(this ITabularObject obj, bool plural = false)
        {
            if (obj is DataColumn) return "Column" + (plural ? "s" : "");
            if (obj is CalculatedColumn) return "Calculated Column" + (plural ? "s" : "");
            if (obj is CalculatedTableColumn) return "Calculated Table Column" + (plural ? "s" : "");
            else return obj.ObjectType.GetTypeName(plural);
        }

        public static string GetName(this ITabularNamedObject obj, Culture culture)
        {
            // Folders are not culture aware:
            if (culture == null || obj is Folder) return obj.Name;

            // Other objects must take culture into account for their name:
            if (obj is TabularNamedObject)
            {
                var name = (obj as TabularNamedObject).TranslatedNames[culture];

                // Return base name if there was no translated name:
                if (string.IsNullOrEmpty(name)) name = obj.Name;
                return name;
            }
            throw new ArgumentException("This object does not have a Name property.");
        }

        public static bool SetName(this ITabularNamedObject obj, string newName, Culture culture)
        {
            if (culture == null || obj is Folder)
            {
                if (string.IsNullOrEmpty(newName)) return false;
                obj.Name = newName;
                return true;
            }
            if (obj is TabularNamedObject)
            {
                var tObj = obj as TabularNamedObject;
                tObj.TranslatedNames[culture] = newName;
                return true;
            }
            throw new ArgumentException("This object does not have a Name property.");
        }

        
    }

}