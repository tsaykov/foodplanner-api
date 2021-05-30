using System;

// Тази функция обновява датата и час на обработените данни на всички домакинства
// което ще предизвика обновяване на списъците със съвместими рецепти

public static void Run(
            IEnumerable<object> inputDocument, 
            ICollector<object> outputDocument, 
            TimerInfo myTimer)
{

    foreach (var input in inputDocument)
    {
        outputDocument.Add(input);
    }

}
