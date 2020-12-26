namespace Gtt.CodeWorks.DataAnnotations
{
    public interface ICollectionPropertyNamingStrategy
    {
        string CreateName(string collectionProperty, int index, string memberProperty);
    }

    public class DottedNumberCollectionPropertyNamingStrategy : ICollectionPropertyNamingStrategy
    {
        public string CreateName(string collectionProperty, int index, string memberProperty)
        {
            return $"{collectionProperty}.{index}.{memberProperty}";
        }
    }

    public class ArrayCollectionPropertyNamingStrategy : ICollectionPropertyNamingStrategy
    {
        public string CreateName(string collectionProperty, int index, string memberProperty)
        {
            return $"{collectionProperty}[{index}].{memberProperty}";
        }
    }

    public class IgnoreIndexCollectionPropertyNamingStrategy : ICollectionPropertyNamingStrategy
    {
        public string CreateName(string collectionProperty, int index, string memberProperty)
        {
            return $"{collectionProperty}.{memberProperty}";
        }
    }
}