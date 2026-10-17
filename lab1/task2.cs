using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        const int N = 16000;

        // Создаём и заполняем исходный массив: a[i] = i
        double[] a = new double[N];
        for (int i = 0; i < N; i++)
            a[i] = i;

        // Результирующий массив
        double[] b = new double[N];

        int threadCount = 8;

        Console.WriteLine($"Массив из {N} элементов, потоков: {threadCount}");
        Console.WriteLine();

        // Пробуем три типа распределения работ
        RunSchedule("static",  a, b, N, threadCount);
        RunSchedule("dynamic", a, b, N, threadCount);
        RunSchedule("guided",  a, b, N, threadCount);

        Console.WriteLine();
        Console.WriteLine("Проверка результата (несколько элементов массива b):");
        Console.WriteLine($"b[0]    = {b[0]:F3}");
        Console.WriteLine($"b[1]    = {b[1]:F3}");
        Console.WriteLine($"b[8000] = {b[8000]:F3}");
        Console.WriteLine($"b[15999]= {b[15999]:F3}");
    }

    static void RunSchedule(string schedule, double[] a, double[] b, int N, int threadCount)
    {
        var sw = Stopwatch.StartNew();

        var options = new ParallelOptions { MaxDegreeOfParallelism = threadCount };

        switch (schedule)
        {
            case "static":
            {
                // Делим итерации заранее поровну между потоками
                int blockSize = (N + threadCount - 1) / threadCount;

                Parallel.For(0, threadCount, options, t =>
                {
                    int start = t * blockSize;
                    int end = Math.Min(start + blockSize, N);

                    for (int i = start; i < end; i++)
                        b[i] = ComputeB(a, i, N);
                });
                break;
            }

            case "dynamic":
            {
                // Каждый поток берёт следующую порцию (по 1 элементу)
                int next = 0;

                Parallel.For(0, threadCount, options, _ =>
                {
                    while (true)
                    {
                        int i = Interlocked.Increment(ref next) - 1;
                        if (i >= N) break;

                        b[i] = ComputeB(a, i, N);
                    }
                });
                break;
            }

            case "guided":
            {
                // Порции сначала большие, потом уменьшаются
                int next = 0;
                object lockObj = new object();

                Parallel.For(0, threadCount, options, _ =>
                {
                    while (true)
                    {
                        int start;
                        int size;

                        lock (lockObj)
                        {
                            int remaining = N - next;
                            if (remaining <= 0) break;

                            size = Math.Max(1, remaining / (2 * threadCount));
                            start = next;
                            next = Math.Min(next + size, N);
                        }

                        int end = Math.Min(start + size, N);
                        for (int i = start; i < end; i++)
                            b[i] = ComputeB(a, i, N);
                    }
                });
                break;
            }
        }

        sw.Stop();
        Console.WriteLine($"schedule = {schedule,-8}  время = {sw.ElapsedMilliseconds} мс");
    }

    // Формула b[i] = (a[i-1] + a[i] + a[i+1]) / 3.0
    // Крайние элементы копируем из a
    static double ComputeB(double[] a, int i, int N)
    {
        if (i == 0) return a[0];
        if (i == N - 1) return a[N - 1];
        return (a[i - 1] + a[i] + a[i + 1]) / 3.0;
    }
}
