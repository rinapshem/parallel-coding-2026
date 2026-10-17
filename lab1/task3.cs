using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.Write("Введите количество потоков: ");
        int threadCount = int.Parse(Console.ReadLine() ?? "8");

        Console.Write("Выберите метод (1-5): ");
        int method = int.Parse(Console.ReadLine() ?? "1");

        Console.WriteLine($"Запуск метода {method} с {threadCount} потоками.");
        Console.WriteLine();

        switch (method)
        {
            case 1: Method1(threadCount); break;
            case 2: Method2(threadCount); break;
            case 3: Method3(threadCount); break;
            case 4: Method4(threadCount); break;
            case 5: Method5(threadCount); break;
            default: Console.WriteLine("Неверный метод"); break;
        }

        Console.WriteLine();
        Console.WriteLine("Готово.");
    }

    // ============================================================
    // Метод 1: Собрать идентификаторы в массив, затем напечатать
    // их в обратном порядке из главного потока.
    // ============================================================
    static void Method1(int threadCount)
    {
        int[] ids = new int[threadCount];
        Thread[] threads = new Thread[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            int id = i;
            threads[i] = new Thread(() => { ids[id] = id; });
            threads[i].Start();
        }

        foreach (var t in threads) t.Join();

        // Печатаем в обратном порядке
        for (int i = threadCount - 1; i >= 0; i--)
        {
            Console.WriteLine($"Поток {ids[i]}");
        }
    }

    // ============================================================
    // Метод 2: Общий счётчик + lock/Monitor.
    // Поток с номером threadCount-1 печатает первым, затем
    // будит следующий и т.д.
    // ============================================================
    static void Method2(int threadCount)
    {
        int nextToPrint = threadCount - 1;
        object lockObj = new object();

        Thread[] threads = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int id = i;
            threads[i] = new Thread(() =>
            {
                lock (lockObj)
                {
                    while (nextToPrint != id)
                    {
                        Monitor.Wait(lockObj);
                    }
                    Console.WriteLine($"Поток {id}");
                    nextToPrint--;
                    Monitor.PulseAll(lockObj);
                }
            });
            threads[i].Start();
        }

        foreach (var t in threads) t.Join();
    }

    // ============================================================
    // Метод 3: Цепочка семафоров.
    // Поток i ждёт семафор от потока i+1.
    // ============================================================
    static void Method3(int threadCount)
    {
        SemaphoreSlim[] semaphores = new SemaphoreSlim[threadCount];
        for (int i = 0; i < threadCount; i++)
            semaphores[i] = new SemaphoreSlim(0);

        Thread[] threads = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int id = i;
            threads[i] = new Thread(() =>
            {
                if (id < threadCount - 1)
                    semaphores[id + 1].Wait();

                Console.WriteLine($"Поток {id}");

                if (id > 0)
                    semaphores[id].Release();
            });
            threads[i].Start();
        }

        foreach (var t in threads) t.Join();
    }

    // ============================================================
    // Метод 4: Цепочка ManualResetEvent.
    // Аналогично семафорам, но через события.
    // ============================================================
    static void Method4(int threadCount)
    {
        ManualResetEvent[] events = new ManualResetEvent[threadCount];
        for (int i = 0; i < threadCount; i++)
            events[i] = new ManualResetEvent(false);

        Thread[] threads = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int id = i;
            threads[i] = new Thread(() =>
            {
                if (id < threadCount - 1)
                    events[id + 1].WaitOne();

                Console.WriteLine($"Поток {id}");

                if (id > 0)
                    events[id].Set();
            });
            threads[i].Start();
        }

        foreach (var t in threads) t.Join();
    }

    // ============================================================
    // Метод 5: Цепочка Task (ContinueWith).
    // Задачи выполняются последовательно в обратном порядке.
    // ============================================================
    static void Method5(int threadCount)
    {
        // Создаём первую задачу для самого старшего потока
        Task task = Task.Run(() => Console.WriteLine($"Поток {threadCount - 1}"));

        // Прицепляем продолжения в обратном порядке
        for (int i = threadCount - 2; i >= 0; i--)
        {
            int id = i;
            task = task.ContinueWith(_ => Console.WriteLine($"Поток {id}"));
        }

        task.Wait();
    }
}
