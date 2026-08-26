using Newtonsoft.Json;
using System.ComponentModel;

public sealed class EnumDefinition
{
    public enum VisionConfigStatus
    {
        NotExist = 0,
        Initializing = 1,
        Completed = 2
    }

    public enum VisionConfigNetworkStatus
    {
        Error = -1,
        NotConnected = 0,
        NoInternet = 1,
        Connected = 2
    }

    public enum VisionConfigThirdPartyDeviceStatus
    {
        Offline = 0,
        Online = 1
    }

    public enum SecurityType
    {
        [Description("Open")]
        Open = 1,

        [Description("WPA2 Personal")]
        WPA2Personal = 2
    }

    /// <summary>Gets the enum value.</summary>
    /// <param name="enumObjectName">Name of the enum object.</param>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    public static string GetEnumValue(string enumObjectName, string key)
    {
        var content = LocalizerHelper.LocalizerResource[enumObjectName];
        var enumObject = JsonConvert.DeserializeObject<dynamic>(content);
        if (enumObject != null)
        {
            return enumObject[key].ToString();
        }

        return string.Empty;
    }

    /// <summary>Gets the enum name by key.</summary>
    /// <param name="enumObjectName">Name of the enum object.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static string GetEnumNameByKey(string enumObjectName, int value)
    {
        Type typeName = GetTypeName(enumObjectName);
        string returnValue = string.Empty;
        var content = LocalizerHelper.LocalizerResource[enumObjectName];
        var enumObject = JsonConvert.DeserializeObject<dynamic>(content);
        foreach (object o in Enum.GetValues(typeName))
        {
            string key = Enum.Format(typeName, o, "D");
            if (value.ToString() == key)
            {
                returnValue = (enumObject == null) ? string.Empty : enumObject[key].ToString();
                break;
            }
        }

        return returnValue;
    }

    /// <summary>Gets the enum name by value.</summary>
    /// <param name="enumObjectName">Name of the enum object.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static string GetEnumNameByValue(string enumObjectName, string value)
    {
        Type typeName = GetTypeName(enumObjectName);
        string returnValue = string.Empty;
        var content = LocalizerHelper.LocalizerResource[enumObjectName];
        var enumObject = JsonConvert.DeserializeObject<dynamic>(content);
        foreach (object o in Enum.GetValues(typeName))
        {
            if (value == o.ToString())
            {
                string key = Enum.Format(typeName, o, "D");
                returnValue = (enumObject == null) ? string.Empty : enumObject[key].ToString();
                break;
            }
        }

        return returnValue;
    }

    /// <summary>Gets the enums.</summary>
    /// <param name="enumObjectName">Name of the enum object.</param>
    /// <returns></returns>
    public static List<KeyValuePair<string, string>> GetEnums(string enumObjectName)
    {
        Type typeName = GetTypeName(enumObjectName);
        List<KeyValuePair<string, string>> returnList = new();
        var content = LocalizerHelper.LocalizerResource[enumObjectName];
        var enumObject = JsonConvert.DeserializeObject<dynamic>(content);
        foreach (object o in Enum.GetValues(typeName))
        {
            string key = Enum.Format(typeName, o, "D");
            returnList.Add(new KeyValuePair<string, string>(key, ((enumObject == null) ? string.Empty : enumObject[key].ToString())));
        }
        return returnList;
    }

    /// <summary>
    /// Gets the description.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static string GetDescription(Enum value)
    {
        var field = value.GetType().GetField(value.ToString());

        if (field == null) return string.Empty;

        var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);

        if (attributes.Length == 0) return string.Empty;

        return ((DescriptionAttribute)attributes[0]).Description;
    }

    /// <summary>Gets the name of the type.</summary>
    /// <param name="enumObjectName">Name of the enum object.</param>
    /// <returns></returns>
    private static Type GetTypeName(string enumObjectName)
    {
        Type typeName = typeof(object);
        switch (enumObjectName)
        {
            case "Enum_SecurityType":
                typeName = typeof(SecurityType);
                break;
        }

        return typeName;
    }
}
