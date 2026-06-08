[System.Serializable]
public struct CurrencyAmount
{
    public CurrencyType type;
    public int amount;
}

[System.Serializable]
public struct Price
{
    public int fieldCoin;
    public int token;
}