using System;
using System.Threading;
class Program
{
    static void Main(string[] args)
    {
        // Чтобы русские буквы нормально отображались
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Спрашиваем у пользователя число потоков
        Console.Write("Введите количество потоков: ");
        string input = Console.ReadLine();

        // Преобразуем строку в число
        if (!int.TryParse(input, out int threadCount) || threadCount <= 0)
        {
            Console.WriteLine("Ошибка: нужно ввести положительное целое число.");
            return;
        }

        Console.WriteLine($"Запускаем {threadCount} потоков.");

        // Массив для объектов потоков
        Thread[] threads = new Thread[threadCount];
        Console.WriteLine("Hello, World!");
        // Создаём и запускаем каждый поток
        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i; // локальная копия, чтобы каждый поток запомнил свой номер
            threads[i] = new Thread(() =>
            {
                Console.WriteLine($"Поток {threadId}: всего потоков {threadCount}");
            });

            threads[i].Start(); // запускаем поток
        }

        // Ждём завершения всех потоков
        foreach (Thread t in threads)
        {
            t.Join();
        }

        Console.WriteLine("Все потоки завершены.");
    }
}
