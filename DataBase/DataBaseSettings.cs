namespace SMarket.DataBase;

public class DataBaseSettings
{
    [XmlAttribute]
    public string
        ConnectionString = "Server=127.0.0.1;Database=test;Uid=root;",
        Table = "SMarket";
}
