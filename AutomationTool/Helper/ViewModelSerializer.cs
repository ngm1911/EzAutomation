using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace AutomationTool.Helper
{
    public static class ViewModelSerializer
    {
        public static async Task SaveObservableProps(object viewModel, string filePath = null)
        {
            var dict = SerializeObject(viewModel);
            var json = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
            if (string.IsNullOrWhiteSpace(filePath) == false)
            {
                await File.WriteAllTextAsync(filePath, json);
            }
            else
            {
                Clipboard.SetText(json);
            }
        }

        public static async Task LoadObservableProps(object viewModel, string filePath = null)
        {
            string json = string.Empty;
            if (string.IsNullOrWhiteSpace(filePath) == false)
            {
                if (!File.Exists(filePath)) return;
                json = await File.ReadAllTextAsync(filePath);
            }
            else
            {
                json = Clipboard.GetText();
            }

            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            if (dict != null)
                DeserializeObject(viewModel, dict);
        }

        public static object? SerializeObject(object? obj)
        {
            if (obj == null) return null;

            var type = obj.GetType();

            if (type.IsPrimitive || type == typeof(string) || type.IsEnum
                || type == typeof(DateTime) || type == typeof(decimal))
                return obj;

            if (obj is IEnumerable enumerable && !(obj is string))
            {
                var list = new List<object?>();
                foreach (var item in enumerable)
                    list.Add(SerializeObject(item));
                return list;
            }

            var result = new Dictionary<string, object?>();

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (field.GetCustomAttribute<ObservablePropertyAttribute>() != null
                        && field.GetCustomAttribute<Newtonsoft.Json.JsonIgnoreAttribute>() == null)
                {
                    var propName = char.ToUpper(field.Name[0]) + field.Name.Substring(1);
                    var prop = type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null)
                    {
                        var value = SerializeObject(prop.GetValue(obj));
                        if (value != null)
                        {
                            result[propName] = value;
                        }
                    }
                }
            }
            return result;
        }

        public static void DeserializeObject(object? target, Dictionary<string, JsonElement> dict)
        {
            if (target == null) return;
            var type = target.GetType();

            foreach (var kv in dict)
            {
                var prop = type.GetProperty(kv.Key, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null && prop.CanWrite)
                {
                    object? value = null;

                    if (kv.Value.ValueKind == JsonValueKind.Object)
                    {
                        var nestedObj = prop.GetValue(target) ?? Activator.CreateInstance(prop.PropertyType)!;
                        var nestedDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(kv.Value.GetRawText());
                        DeserializeObject(nestedObj, nestedDict!);
                        value = nestedObj;
                    }
                    else if (kv.Value.ValueKind == JsonValueKind.Array)
                    {
                        // Collection
                        if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
                        {
                            var elementType = prop.PropertyType.IsGenericType
                                ? prop.PropertyType.GetGenericArguments()[0]
                                : typeof(object);

                            var listType = typeof(List<>).MakeGenericType(elementType);
                            var listInstance = (IList)JsonSerializer.Deserialize(kv.Value.GetRawText(), listType)!;

                            if (prop.PropertyType.IsGenericType &&
                                prop.PropertyType.GetGenericTypeDefinition() == typeof(ObservableCollection<>))
                            {
                                var ocType = typeof(ObservableCollection<>).MakeGenericType(elementType);
                                var ocInstance = (IList)Activator.CreateInstance(ocType)!;
                                foreach (var item in listInstance) ocInstance.Add(item);
                                value = ocInstance;
                            }
                            else
                            {
                                value = listInstance;
                            }
                        }
                        else
                        {
                            value = JsonSerializer.Deserialize(kv.Value.GetRawText(), prop.PropertyType);
                        }
                    }
                    else
                    {
                        value = JsonSerializer.Deserialize(kv.Value.GetRawText(), prop.PropertyType);
                    }

                    prop.SetValue(target, value);
                }
            }
        }
    }
}
