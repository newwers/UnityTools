using System;

public class CurrencyManager : BaseSingleClass<CurrencyManager>, IDataSaver
{
    private int fieldCoin;
    private int token;

    public event Action<CurrencyType, int> CurrencyChanged;

    public int FieldCoin => fieldCoin;
    public int Token => token;

    public void AddCurrency(CurrencyType type, int amount)
    {
        if (amount <= 0) return;

        switch (type)
        {
            case CurrencyType.FieldCoin:
                fieldCoin += amount;
                break;
            case CurrencyType.Token:
                token += amount;
                break;
        }

        CurrencyChanged?.Invoke(type, amount);
    }

    public bool RemoveCurrency(CurrencyType type, int amount)
    {
        if (amount <= 0) return false;

        switch (type)
        {
            case CurrencyType.FieldCoin:
                if (fieldCoin < amount) return false;
                fieldCoin -= amount;
                break;
            case CurrencyType.Token:
                if (token < amount) return false;
                token -= amount;
                break;
        }

        CurrencyChanged?.Invoke(type, -amount);
        return true;
    }

    public bool HasEnoughCurrency(Price price)
    {
        return fieldCoin >= price.fieldCoin && token >= price.token;
    }

    public bool TryPurchase(Price price)
    {
        if (!HasEnoughCurrency(price)) return false;

        if (price.fieldCoin > 0)
            RemoveCurrency(CurrencyType.FieldCoin, price.fieldCoin);

        if (price.token > 0)
            RemoveCurrency(CurrencyType.Token, price.token);

        return true;
    }

    public void OnSave(SaveData saveData)
    {
        saveData.Set("Currency_FieldCoin", fieldCoin);
        saveData.Set("Currency_Token", token);
    }

    public void OnLoad(SaveData saveData)
    {
        fieldCoin = saveData.Get<int>("Currency_FieldCoin", 0);
        token = saveData.Get<int>("Currency_Token", 0);
    }
}