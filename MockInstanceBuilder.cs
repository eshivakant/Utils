using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Moq;

public class InstanceBuilder
{
    /// <summary>
    /// Creates an instance of T with mocked constructor parameters.
    /// Allows customization of specific mocks via setup expressions.
    /// </summary>
    /// <typeparam name="T">The type to create an instance of.</typeparam>
    /// <param name="mockSetups">Optional setup actions for specific mocks.</param>
    /// <returns>A tuple containing the created instance and a dictionary of all mocks.</returns>
    public static (T Instance, Dictionary<Type, object> Mocks) CreateInstanceWithMocks<T>(
        params (Type MockType, Action<object> Setup)[] mockSetups) where T : class
    {
        var type = typeof(T);
        var constructor = type.GetConstructors()
                              .OrderByDescending(c => c.GetParameters().Length)
                              .FirstOrDefault();

        if (constructor == null)
            throw new InvalidOperationException($"No public constructors found for type {type.Name}");

        var constructorParameters = constructor.GetParameters();
        var parameterMocks = new object[constructorParameters.Length];
        var mocksDictionary = new Dictionary<Type, object>();

        // Create and optionally set up mocks for each constructor parameter.
        for (int i = 0; i < constructorParameters.Length; i++)
        {
            var parameterType = constructorParameters[i].ParameterType;

            // Create a Mock<> object dynamically using reflection.
            var mockType = typeof(Mock<>).MakeGenericType(parameterType);
            var mockInstance = Activator.CreateInstance(mockType);

            // Store the mock in the dictionary for future reference.
            mocksDictionary[parameterType] = mockInstance;

            // Apply setup if provided for this type.
            var setup = mockSetups.FirstOrDefault(s => s.MockType == parameterType);
            setup.Setup?.Invoke(mockInstance);

            // Retrieve the mocked object from the Mock<> instance.
            var mockValue = mockType.GetProperty("Object").GetValue(mockInstance);

            parameterMocks[i] = mockValue;
        }

        // Create the instance using the constructor with mocked parameters.
        var instance = (T)constructor.Invoke(parameterMocks);

        return (instance, mocksDictionary);
    }
}

// Example Interfaces and Class
public interface IServiceA
{
    string GetData();
}

public interface IServiceB
{
    void Execute();
}

public class MyClass
{
    private readonly IServiceA _serviceA;
    private readonly IServiceB _serviceB;

    public MyClass(IServiceA serviceA, IServiceB serviceB)
    {
        _serviceA = serviceA;
        _serviceB = serviceB;
    }

    public void PrintData()
    {
        Console.WriteLine($"Data from ServiceA: {_serviceA.GetData()}");
    }

    public void ExecuteServiceB()
    {
        _serviceB.Execute();
    }
}

// Usage in a Console Application
public class Program
{
    public static void Main()
    {
        // Create an instance of MyClass with mocked dependencies, and set up specific mocks.
        var (myClassInstance, mocks) = InstanceBuilder.CreateInstanceWithMocks<MyClass>(
            (typeof(IServiceA), mock =>
            {
                var serviceAMock = (Mock<IServiceA>)mock;
                serviceAMock.Setup(s => s.GetData()).Returns("Mocked Data");
            }),
            (typeof(IServiceB), mock =>
            {
                var serviceBMock = (Mock<IServiceB>)mock;
                serviceBMock.Setup(s => s.Execute()).Callback(() => Console.WriteLine("ServiceB executed"));
            })
        );

        // Use the instance and verify behavior.
        myClassInstance.PrintData();
        myClassInstance.ExecuteServiceB();

        // Access a specific mock to perform further assertions.
        var serviceAMock = (Mock<IServiceA>)mocks[typeof(IServiceA)];
        serviceAMock.Verify(s => s.GetData(), Times.Once);
    }
}
