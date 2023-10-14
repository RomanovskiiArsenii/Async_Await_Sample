//Async Await sample
class Program
{
    class MyClass
    {
        //асинхронный метод OperationAsync принимает CancellationToken token.
        //этот токен позволяет отменить выполнение операции, если появится такая необходимость.
        //операции, разделенные командой await будут выполняться последовательно
        public async Task<int> OperationAsync(CancellationToken token)      //асинхронный метод
        {
            await Task.Run(() =>                                            //асинхронная операция
            {
                Console.WriteLine($"Step 1. Task id: {Task.CurrentId}; " +
                    $"Thread id: {Thread.CurrentThread.ManagedThreadId}");
            });

            return await Task.Run(async () =>                               //асинхронная операция
            {
                Console.WriteLine($"Step 2. Task id: {Task.CurrentId}; " +
                    $"Thread id: {Thread.CurrentThread.ManagedThreadId}");

                for (int i = 0; i < 10; i++)                                //цикл
                {
                    if (!token.IsCancellationRequested)                     //проверка на отмену
                    {
                        await Task.Delay(100);                              //неблокирующая задержка выполнения
                        Console.Write(i);
                    }
                    else
                    {
                        Console.WriteLine(" cancelled"); break;             //при отмене
                    }
                }

                return 3;                                                   //возврат значения
            });
        }
    }
    static async Task Main()
    {
        CancellationTokenSource tokenSource = new CancellationTokenSource();            //создается CancellationTokenSource
        CancellationToken token = tokenSource.Token;                                    //извлекается токен

        MyClass instance = new MyClass();                                               //инстанцируется класс, 

        Console.WriteLine($"Step 0. Task id:  {Task.CurrentId}, " +
            $"Thread id: {Thread.CurrentThread.ManagedThreadId}");

        Task<int> task = instance.OperationAsync(token);                                //запуск асинхронной операции

        Thread.Sleep(500);                                                              //задержка
        tokenSource.Cancel();                                                           //асинхронная операция завершится сразу после следующего шага (если она не была завершена ранее).

        await task.ContinueWith(t =>
        {
            Console.WriteLine($"Step {t.Result}. " +                                    //выполнить действие над результатом задачи
            $"Task id: {Task.CurrentId}; " +
            $"Thread id: {Thread.CurrentThread.ManagedThreadId}");
        });                  
    }
}
//Результат:
//Step 0. Task id:  , Thread id: 1
//Step 1. Task id: 1; Thread id: 7
//Step 2. Task id: 2; Thread id: 8
//01234 cancelled
//Step 3. Task id: 3; Thread id: 6