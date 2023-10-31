//Async Await sample
using System.Diagnostics;

class Program
{
    class MyClass
    {
        //асинхронный метод PrintAsync будет объявлен в начале метода Main, но запущен 
        //лишь в конце командой await. Метод Main дождется его завершения.
        public async Task PrintAsync()
        {
            await Task.Run(async () =>
            {
                Console.WriteLine($"Step 4. Task id: {Task.CurrentId}; " +
                    $"Thread id: {Thread.CurrentThread.ManagedThreadId}");
                for (int i = 0; i < 4; i++)
                {
                    Console.Write("Signal_");
                    await Task.Delay(100);
                }
            });
        }

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
                await Console.Out.WriteLineAsync("Press q to quit");
                for (int i = 0; i < 10; i++)                                //цикл
                {
                    if (!token.IsCancellationRequested)                     //проверка на отмену
                    {
                        await Task.Delay(400);                              //неблокирующая задержка выполнения
                        Console.Write(i);
                    }
                    else
                    {
                        Console.WriteLine(" cancelled"); break;             //при отмене
                    }
                }
                if(!token.IsCancellationRequested)
                                await Console.Out.WriteLineAsync("\nPress any key to continue");
                return 3;                                                   //возврат значения
            });
        }
    }
    static async Task Main()
    {
        CancellationTokenSource tokenSource = new CancellationTokenSource();            //создается CancellationTokenSource
        CancellationToken token = tokenSource.Token;                                    //извлекается токен

        MyClass instance = new MyClass();                                               //инстанцируется класс, 

        Console.WriteLine($"Step 0. Task id:  {Task.CurrentId}; " +
            $"Thread id: {Thread.CurrentThread.ManagedThreadId}");

        Task<int> task = instance.OperationAsync(token);                                //запуск асинхронной операции
        var task2 = instance.PrintAsync;                                                //объявление асинхронной операции (без передачи параметров)

        char quit = Console.ReadKey(true).KeyChar;                                          //нажать q для отмены
        if (quit == 'q') { tokenSource.Cancel(); }                                      //асинхронная операция завершится сразу после следующего шага (если она не была завершена ранее).

        await task.ContinueWith(t =>                                                    //дождаться окончания выполнения продолжения операции
        {
            Console.WriteLine($"Step {t.Result}. " +                                    //выполнить действие над результатом задачи
            $"Task id: {Task.CurrentId}; " +
            $"Thread id: {Thread.CurrentThread.ManagedThreadId}");
        });
        await Task.Delay(1000);

        await task2();                                                                    //запустить и дождаться окончания операции
    }
}
//Результат при отмене
//Step 0. Task id:  ; Thread id: 1
//Step 1. Task id: 1; Thread id: 7
//Step 2. Task id: 2; Thread id: 4
//Press q to quit
//01234 cancelled
//Step 3. Task id: 3; Thread id: 6
//Step 4. Task id: 4; Thread id: 6
//Signal_Signal_Signal_Signal_
//
//Результат при завершении
//Step 0. Task id:  ; Thread id: 1
//Step 1. Task id: 1; Thread id: 7
//Step 2. Task id: 2; Thread id: 4
//Press q to quit
//0123456789
//Press any key to continue
//Step 3. Task id: 3; Thread id: 8
//Step 4. Task id: 4; Thread id: 8
//Signal_Signal_Signal_Signal_