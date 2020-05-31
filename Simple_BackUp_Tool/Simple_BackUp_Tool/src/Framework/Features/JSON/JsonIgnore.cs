using System;

namespace Framework.Features.Json
{
	/// <summary>
	/// Variables with this attribute will be ignored when converted to json.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class JsonIgnore : Attribute { }
}
