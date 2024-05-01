using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace DropBear.Codex.StateManagement.DeepCloning;

public static class CollectionCloner
{
    public static Expression CloneCollection(Expression collection, Type collectionType, Expression track)
{
    var elementType = collectionType.IsArray
        ? collectionType.GetElementType() ?? throw new InvalidOperationException("Array has no element type.")
        : collectionType.GetGenericArguments().FirstOrDefault() ??
          throw new InvalidOperationException("Generic collection has no element type.");

    var countProperty = collectionType.GetProperty("Count") ??
                        throw new InvalidOperationException(
                            $"No 'Count' property found on type {collectionType.Name}.");
    var countExpression = Expression.Property(collection, countProperty);

    var addMethod = collectionType.GetMethod("Add") ??
                    throw new InvalidOperationException($"No 'Add' method found on type {collectionType.Name}.");

    var result = Expression.Variable(collectionType, "clonedCollection");
    var index = Expression.Variable(typeof(int), "i");
    var element = Expression.Variable(elementType, "element");
    var assignNewCollection = Expression.Assign(result, CreateNewInstanceExpression(collectionType));
    var loopBreak = Expression.Label("loopBreak");

    var elementClone = ExpressionCloner.BuildCloneExpression(elementType, Expression.Property(collection, "Item", index), track);
    var loop = Expression.Block(new[] { index, element },
        Expression.Assign(index, Expression.Constant(0)),
        Expression.Loop(
            Expression.IfThenElse(
                Expression.LessThan(index, countExpression),
                Expression.Block(
                    Expression.Assign(element, elementClone),
                    Expression.IfThen(
                        Expression.TypeIs(element, elementType),
                        Expression.Call(result, addMethod, element)
                    ),
                    Expression.PreIncrementAssign(index)
                ),
                Expression.Break(loopBreak)
            ),
            loopBreak
        )
    );

    return Expression.Block(new[] { result },
        assignNewCollection,
        loop,
        result
    );
}


    public static Expression CloneArray(Expression array, Type arrayType, Expression track)
    {
        var elementType = arrayType.GetElementType();
        var lengthExpr = Expression.ArrayLength(array);
        var newArrayExpr =
            Expression.NewArrayBounds(elementType ?? throw new InvalidOperationException("Element Type is null."), lengthExpr);
        return CreateElementCloneLoop(array, newArrayExpr, elementType, track);
    }

    public static bool IsImmutableCollection(Type type) =>
        type.Namespace?.StartsWith("System.Collections.Immutable", StringComparison.OrdinalIgnoreCase) is true;

    public static Expression CloneImmutableCollection(Expression collection, Type collectionType, Expression track)
    {
        var elementType = collectionType.GetGenericArguments()[0];
        var listType = typeof(List<>).MakeGenericType(elementType);
        var tempListExpr = Expression.Variable(listType, "tempList");

        var toImmutableMethod = collectionType.GetMethod("ToImmutable", BindingFlags.Static | BindingFlags.Public) ??
                                throw new InvalidOperationException("ToImmutable method not found.");

        var convertToImmutable = Expression.Call(null, toImmutableMethod, tempListExpr);

        return Expression.Block(new[] { tempListExpr },
            Expression.Assign(tempListExpr, Expression.New(listType)),
            PopulateWithClonedElements(collection, tempListExpr, elementType, track),
            convertToImmutable
        );
    }

    private static Type GetConcreteType(Type abstractOrInterfaceType)
    {
        // This dictionary maps interfaces and abstract collection types to their concrete implementations.
        var map = new Dictionary<Type, Type>
        {
            { typeof(IEnumerable<>), typeof(List<>) },
            { typeof(ICollection<>), typeof(List<>) },
            { typeof(IList<>), typeof(List<>) },
            { typeof(ISet<>), typeof(HashSet<>) },
            { typeof(IDictionary<,>), typeof(Dictionary<,>) },
            { typeof(IReadOnlyList<>), typeof(List<>) },
            { typeof(IReadOnlyCollection<>), typeof(List<>) },
            { typeof(IReadOnlyDictionary<,>), typeof(ReadOnlyDictionary<,>) },
            { typeof(ConcurrentDictionary<,>), typeof(ConcurrentDictionary<,>) },
            { typeof(ReadOnlyCollection<>), typeof(List<>) },
            { typeof(ReadOnlyDictionary<,>), typeof(Dictionary<,>) }
        };

        if (map.TryGetValue(abstractOrInterfaceType, out var concreteType))
        {
            return concreteType;
        }

        throw new InvalidOperationException(
            $"No concrete type mapped for interface or abstract class {abstractOrInterfaceType.Name}.");
    }

    public static Expression CreateNewInstanceExpression(Type type)
    {
        if (type.IsInterface || type.IsAbstract) type = GetConcreteType(type);

        return Expression.New(type);
    }

    private static BlockExpression CreateElementCloneLoop(Expression sourceArray, Expression newArray, Type elementType,
        Expression track)
    {
        var index = Expression.Variable(typeof(int), "index");
        var loopLabel = Expression.Label("loopEnd");

        var element = Expression.ArrayIndex(sourceArray, index);
        var clonedElement = ExpressionCloner.BuildCloneExpression(elementType, element, track);
        var assignElement = Expression.Assign(Expression.ArrayAccess(newArray, index), clonedElement);

        var loop = Expression.Loop(
            Expression.IfThenElse(
                Expression.LessThan(index, Expression.ArrayLength(sourceArray)),
                Expression.Block(
                    assignElement,
                    Expression.PostIncrementAssign(index)
                ),
                Expression.Break(loopLabel)
            ),
            loopLabel
        );

        return Expression.Block(new[] { index },
            Expression.Assign(index, Expression.Constant(0)),
            loop,
            newArray
        );
    }

    private static BlockExpression PopulateWithClonedElements(Expression sourceCollection, Expression targetCollection,
        Type elementType, Expression track)
    {
        var enumeratorType = typeof(IEnumerator<>).MakeGenericType(elementType);
        var enumeratorVar = Expression.Variable(enumeratorType, "enumerator");
        var getEnumeratorCall = Expression.Call(sourceCollection,
            typeof(IEnumerable<>).MakeGenericType(elementType).GetMethod("GetEnumerator") ?? throw new InvalidOperationException("GetEnumerator method not found."));
        var moveNextCall = Expression.Call(enumeratorVar, typeof(IEnumerator).GetMethod("MoveNext") ?? throw new InvalidOperationException("MoveNext method not found."));
        var currentElement = Expression.Property(enumeratorVar, "Current");

        var loopLabel = Expression.Label("loopEnd");

        var clonedElement = ExpressionCloner.BuildCloneExpression(elementType, currentElement, track);
        var addMethod = targetCollection.Type.GetMethod("Add");
        var addCall = Expression.Call(targetCollection, addMethod ?? throw new InvalidOperationException("Add Method is null."), clonedElement);

        var loopBlock = Expression.Loop(
            Expression.IfThenElse(
                Expression.Equal(moveNextCall, Expression.Constant(true)),
                addCall,
                Expression.Break(loopLabel)
            ),
            loopLabel
        );

        return Expression.Block(new[] { enumeratorVar },
            Expression.Assign(enumeratorVar, getEnumeratorCall),
            loopBlock
        );
    }
}
