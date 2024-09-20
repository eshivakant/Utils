using System;
using System.Collections;
using System.Reflection;

public class DeepComparer
{
    public static bool DeepCompare(object obj1, object obj2)
    {
        // If both are null, they're equal
        if (obj1 == null && obj2 == null) return true;
        
        // If one is null and the other is not, they're not equal
        if (obj1 == null || obj2 == null) return false;
        
        // If types are not the same, they're not equal
        if (obj1.GetType() != obj2.GetType()) return false;

        // Handle primitive types and strings
        if (obj1 is string || obj1.GetType().IsPrimitive)
        {
            return obj1.Equals(obj2);
        }

        // Handle collections (arrays, lists, dictionaries, etc.)
        if (obj1 is IEnumerable && obj2 is IEnumerable)
        {
            return CompareEnumerables((IEnumerable)obj1, (IEnumerable)obj2);
        }

        // Use reflection to compare each property and field
        Type type = obj1.GetType();
        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            object value1 = property.GetValue(obj1);
            object value2 = property.GetValue(obj2);

            if (!DeepCompare(value1, value2)) return false;
        }

        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            object value1 = field.GetValue(obj1);
            object value2 = field.GetValue(obj2);

            if (!DeepCompare(value1, value2)) return false;
        }

        return true;
    }

    private static bool CompareEnumerables(IEnumerable enum1, IEnumerable enum2)
    {
        var enumerator1 = enum1.GetEnumerator();
        var enumerator2 = enum2.GetEnumerator();

        while (enumerator1.MoveNext())
        {
            if (!enumerator2.MoveNext() || !DeepCompare(enumerator1.Current, enumerator2.Current))
            {
                return false;
            }
        }

        // Ensure both enumerators are exhausted
        if (enumerator2.MoveNext()) return false;

        return true;
    }
}

class Program
{
    static void Main()
    {
        var obj1 = new TestClass
        {
            Name = "John",
            Age = 30,
            Address = new Address { City = "New York", Street = "5th Avenue" },
            Numbers = new int[] { 1, 2, 3 }
        };

        var obj2 = new TestClass
        {
            Name = "John",
            Age = 30,
            Address = new Address { City = "New York", Street = "5th Avenue" },
            Numbers = new int[] { 1, 2, 3 }
        };

        bool isEqual = DeepComparer.DeepCompare(obj1, obj2);
        Console.WriteLine($"Objects are {(isEqual ? "equal" : "not equal")}");
    }
}

class TestClass
{
    public string Name { get; set; }
    public int Age { get; set; }
    public Address Address { get; set; }
    public int[] Numbers { get; set; }
}

class Address
{
    public string City { get; set; }
    public string Street { get; set; }
}
