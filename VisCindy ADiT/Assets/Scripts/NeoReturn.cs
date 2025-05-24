public interface IReturnStrategy
{
    string Return(object value);
}


public class ChainReturn : IReturnStrategy{
    public string Return(object value){
        return "";
    }
}


public class QueryReturn : IReturnStrategy
{
    public string Return(object value){
        return "ddd";
    }
}
