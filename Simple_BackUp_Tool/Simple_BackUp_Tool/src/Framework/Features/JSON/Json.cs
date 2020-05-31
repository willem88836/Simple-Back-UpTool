using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Framework.Utils;

namespace Framework.Features.Json
{
	// TODO: Serializing Arrays does not work. 
	// TODO: Serializing Arrays with multiple dimensions does not work. 
	// TODO: Arrays with no content doesn't work.
	// TODO: Nested strings do not work.
	public static class JsonUtility
	{
		private class Json
		{
			protected const string NULL = "null";
			protected const char OBJPREFIX = '{';
			protected const char OBJSUFFIX = '}';
			protected const char ARRAYPREFIX = '[';
			protected const char ARRAYSUFFIX = ']';
			protected const char OBJDEFINITION = ':';
			protected const char OBJSEPARATOR = ',';
			protected const char STRINGENCAPSULATION = '\"';
			protected const char OBJREFERENCE = '$';
		}

		private class JsonSerializer : Json
		{
			private StringBuilder jsonBuilder = new StringBuilder();
			private List<int> objectHashes = new List<int>();


			public string ToJson(object obj, Type t)
			{
				SerializeObject(obj, t);
				string json = jsonBuilder.ToString();
				return json;
			}

			private void SerializeObject(object obj, Type t)
			{
				if (obj == null)
				{
					jsonBuilder.Append(NULL);
					return;
				}

				int hash = obj.GetHashCode();

				if (objectHashes.Contains(hash))
				{
					int index = objectHashes.IndexOf(hash);
					jsonBuilder.Append(OBJREFERENCE);
					jsonBuilder.Append(index);
					jsonBuilder.Append(OBJREFERENCE);
				}
				else
				{
					int index = objectHashes.Count;
					objectHashes.Add(hash);

					jsonBuilder.Append(OBJPREFIX);

					FieldInfo[] fields = t.GetFields().Where(info => info.GetCustomAttribute(typeof(JsonIgnore)) == null && !info.IsLiteral).ToArray();
					SerializeFields(obj, fields);

					PropertyInfo[] properties = t.GetProperties().Where(info => info.GetCustomAttribute(typeof(JsonIgnore)) == null && info.CanWrite && info.CanRead).ToArray();
					if (properties.Length > 0)
						jsonBuilder.Append(OBJSEPARATOR);
					SerializeProperties(obj, properties);

					jsonBuilder.Append(OBJSUFFIX);
				}
			}

			private void SerializeFields<T>(T obj, FieldInfo[] fields)
			{
				for (int i = 0; i < fields.Length; i++)
				{
					FieldInfo field = fields[i];

					jsonBuilder.Append(field.Name);
					jsonBuilder.Append(OBJDEFINITION);

					object value = field.GetValue(obj);

					if (field.FieldType.IsArray)
						SerializeArray(value);
					else if (value.IsPrimitive())
						SerializePrimitive(value);
					else
						SerializeObject(value, field.FieldType);

					if (i < fields.Length - 1)
						jsonBuilder.Append(OBJSEPARATOR);
				}
			}

			private void SerializeProperties<T>(T obj, PropertyInfo[] properties)
			{
				for (int i = 0; i < properties.Length; i++)
				{
					PropertyInfo property = properties[i];

					jsonBuilder.Append(property.Name);
					jsonBuilder.Append(OBJDEFINITION);

					object value = property.GetValue(obj);

					if (property.PropertyType.IsArray)
						SerializeArray(value);
					else if (value.IsPrimitive())
						SerializePrimitive(value);
					else
						SerializeObject(value, property.PropertyType);

					if (i < properties.Length - 1)
						jsonBuilder.Append(OBJSEPARATOR);
				}
			}

			private void SerializeArray(object obj)
			{
				Type ft = obj.GetType().GetElementType();
				jsonBuilder.Append(ARRAYPREFIX);
				Array range = (Array)obj;
				if (ft.IsPrimitive())
				{
					for (int j = 0; j < range.Length; j++)
					{
						object o = range.GetValue(j);
						SerializePrimitive(o);
						if (j < range.Length - 1)
						{
							jsonBuilder.Append(OBJSEPARATOR);
						}
					}
				}
				else if (ft.IsArray)
				{
					for (int j = 0; j < range.Length; j++)
					{
						object o = range.GetValue(j);
						SerializeArray(o);
						if (j < range.Length - 1)
						{
							jsonBuilder.Append(OBJSEPARATOR);
						}
					}
				}
				else
				{
					for (int j = 0; j < range.Length; j++)
					{
						object o = range.GetValue(j);
						SerializeObject(o, ft);
						if (j < range.Length - 1)
						{
							jsonBuilder.Append(OBJSEPARATOR);
						}
					}
				}
				jsonBuilder.Append(ARRAYSUFFIX);
			}

			private void SerializePrimitive(object obj)
			{
				if (obj.GetType() == typeof(string))
				{
					jsonBuilder.Append(STRINGENCAPSULATION);
					jsonBuilder.Append(obj.ToString());
					jsonBuilder.Append(STRINGENCAPSULATION);
				}
				else
				{
					jsonBuilder.Append(obj.ToString());
				}
			}
		}

		private class JsonDeserializer : Json
		{
			private List<object> chronologicalObjects = new List<object>();


			public object DeserializeObject(string json, ref int i, Type t)
			{
				string nullVal = json.Substring(i, 4);
				if (nullVal == NULL)
				{
					i += 3;
					return null;
				}


				object obj = Activator.CreateInstance(t);

				chronologicalObjects.Add(obj);

				StringBuilder nameBuilder = new StringBuilder();
				StringBuilder valueBuilder = new StringBuilder();

				if (json[i] != OBJPREFIX)
					return null;
				else
					i++;

				for (char c; i < json.Length; i++)
				{
					c = json[i];

					if (c == OBJSUFFIX)
					{
						return obj;
					}

					if (c != OBJDEFINITION)
					{
						nameBuilder.Append(c);
					}
					else
					{
						i++;
						string name = nameBuilder.ToString();
						FieldInfo fieldInfo = t.GetField(name);
						PropertyInfo propertyInfo = t.GetProperty(name);
						Type objectType;
						
						Action<object> setValue;
						if (fieldInfo == null)
						{
							setValue = (object o) => { propertyInfo.SetValue(obj, o); };
							objectType = propertyInfo.PropertyType;
						}
						else
						{
							setValue = (object o) => { fieldInfo.SetValue(obj, o); };
							objectType = fieldInfo.FieldType;
						}

						setValue += delegate
						{
							nameBuilder.Clear();
							valueBuilder.Clear();
						};


						if (objectType.IsPrimitive())
						{
							object o = DeserializePrimitive(json, ref i, objectType);
							setValue.Invoke(o);
						}
						else if (objectType.IsArray)
						{
							object o = DeserializeArray(json, ref i, objectType.GetElementType());
							setValue.Invoke(o);
						}
						else
						{
							object o;
							c = json[i];
							if (c == OBJREFERENCE)
								o = DeserializeReference(json, ref i);
							else
								o = DeserializeObject(json, ref i, objectType);
							setValue.Invoke(o);
							i++;
						}

						c = json[i];
						if (c == OBJSUFFIX)
						{
							return obj;
						}
					}
				}

				return obj;
			}

			private object DeserializePrimitive(string json, ref int i, Type t)
			{
				StringBuilder valueBuilder = new StringBuilder();
				bool isString = false;
				for (char c; i < json.Length; i++)
				{
					c = json[i];
					if (c == STRINGENCAPSULATION)
					{
						isString = !isString;
					}

					
					if (!isString && (c == OBJSEPARATOR || c == OBJSUFFIX || c == ARRAYSUFFIX))
					{
						break;
					}

					valueBuilder.Append(c);
				}

				string v = valueBuilder.ToString();

				if (v == NULL)
					return null;

				if (t == typeof(string))
				{
					v = v.Substring(1, v.Length - 2);
				}

				return Convert.ChangeType(v, t);
			}

			private Array DeserializeArray(string json, ref int i, Type type)
			{
				List<object> arrayElements = new List<object>();

				if (type.IsPrimitive())
				{
					for (char c; i < json.Length; i++)
					{
						c = json[i];
						if (c == OBJSEPARATOR || c == ARRAYPREFIX)
						{
							i++;
							object o = DeserializePrimitive(json, ref i, type);
							arrayElements.Add(o);

							c = json[i];
							if (c == ARRAYSUFFIX)
								break;

							i--;
						}
					}
				}
				else if (type.IsArray)
				{
					for (char c; i < json.Length; i++)
					{
						c = json[i];
						if (c == ARRAYPREFIX)
						{
							i++;
							object o = DeserializeArray(json, ref i, type.GetElementType());
							arrayElements.Add(o);
							i++;
							c = json[i];
						}

						if (c == ARRAYSUFFIX)
						{
							break;
						}
						i--;
					}
				}
				else
				{
					for (char c; i < json.Length; i++)
					{
						c = json[i];
						if (c == OBJPREFIX)
						{
							object o = DeserializeObject(json, ref i, type);
							arrayElements.Add(o);
						}
						else if (c == OBJREFERENCE)
						{
							object o = DeserializeReference(json, ref i);
							arrayElements.Add(o);
						}

						c = json[i];
						if (c == ARRAYSUFFIX)
						{
							break;
						}
					}
				}

				Array array = Array.CreateInstance(type, arrayElements.Count);
				for (int j = 0; j < arrayElements.Count; j++)
				{
					array.SetValue(arrayElements[j], j);
				}

				i++;
				return array;
			}

			private object DeserializeReference(string json, ref int i)
			{
				if (json[i] == OBJREFERENCE)
					i++;

				StringBuilder referenceBuilder = new StringBuilder();

				for (char c; i < json.Length; i++)
				{
					c = json[i];

					if (c == OBJREFERENCE)
						break;
					else
						referenceBuilder.Append(c);
				}

				int j = (int)Convert.ChangeType(referenceBuilder.ToString(), typeof(int));
				return chronologicalObjects[j];
			}
		}


		public static T FromJson<T>(string json) where  T : new()
		{
			try
			{
				JsonDeserializer jsonDeserializer = new JsonDeserializer();
				int i = 0;
				return (T)jsonDeserializer.DeserializeObject(json, ref i, typeof(T));
			}
			catch (Exception ex)
			{
				LoggingUtilities.LogFormat(
					"Json deserialization halted with error message: ({0})\nInner Exception: ({1})\nData: ({2})\nHelplink: ({3})\nHResult: ({4})\nSource: ({5})\nTargetSite: ({6})\nStack Trace: ({7})",
					ex.Message,
					ex.InnerException,
					ex.Data,
					ex.HelpLink,
					ex.HResult,
					ex.Source,
					ex.TargetSite,
					ex.StackTrace);
				throw ex;
			}
		}

		public static object FromJson(string json, Type t)
		{
			try
			{
				JsonDeserializer jsonDeserializer = new JsonDeserializer();
				int i = 0;
				return jsonDeserializer.DeserializeObject(json, ref i, t);
			}
			catch (Exception ex)
			{
				LoggingUtilities.LogFormat(
					"Json deserialization halted with error message: ({0})\nInner Exception: ({1})\nData: ({2})\nHelplink: ({3})\nHResult: ({4})\nSource: ({5})\nTargetSite: ({6})\nStack Trace: ({7})",
					ex.Message,
					ex.InnerException,
					ex.Data,
					ex.HelpLink,
					ex.HResult,
					ex.Source,
					ex.TargetSite,
					ex.StackTrace);
				throw ex;
			}
		}

		public static string ToJson<T>(T obj)
		{
			try
			{
				JsonSerializer jsonSerializer = new JsonSerializer();
				return jsonSerializer.ToJson(obj, typeof(T));
			}
			catch(Exception ex)
			{
				LoggingUtilities.LogFormat(
					"Json serialization halted with error message: ({0})\nInner Exception: ({1})\nData: ({2})\nHelplink: ({3})\nHResult: ({4})\nSource: ({5})\nTargetSite: ({6})\nStack Trace: ({7})",
					ex.Message,
					ex.InnerException,
					ex.Data,
					ex.HelpLink,
					ex.HResult,
					ex.Source,
					ex.TargetSite,
					ex.StackTrace);
				throw ex;
			}
		}

		public static string ToJson(object obj, Type t)
		{
			try
			{
				JsonSerializer jsonSerializer = new JsonSerializer();
				return jsonSerializer.ToJson(obj, t);
			}
			catch (Exception ex)
			{
				LoggingUtilities.LogFormat(
					"Json serialization halted with error message: ({0})\nInner Exception: ({1})\nData: ({2})\nHelplink: ({3})\nHResult: ({4})\nSource: ({5})\nTargetSite: ({6})\nStack Trace: ({7})",
					ex.Message,
					ex.InnerException,
					ex.Data,
					ex.HelpLink,
					ex.HResult,
					ex.Source,
					ex.TargetSite,
					ex.StackTrace);
				throw ex;
			}
		}
	}
}
