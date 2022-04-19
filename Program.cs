// See httusing System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

internal static class Program
{
    private struct Solution
    {
        public Solution(double y, double z)
        {
            this.y = y;
            this.z = z;
        }
        public double y;
        public double z;
    }

    private struct ToSave
    {
        public double[] X1 { get; set; }
        public double[] Y1 { get; set; }
        public double[] Z1 { get; set; }
        public double[] X2 { get; set; }
        public double[] Y2 { get; set; }
        public double[] Z2 { get; set; }
        public double[] X3 { get; set; }
        public double[] Y3 { get; set; }
        public double[] Z3 { get; set; }
    };

    private static void Main()
    {
        Console.WriteLine("Введите N0");
        var N = Convert.ToInt32(Console.ReadLine());

        //Console.WriteLine("Введите a");
        var a = 1;/*Convert.ToDouble(Console.ReadLine());*/

        Console.WriteLine("Введите b");
        var b = Convert.ToDouble(Console.ReadLine());

        //Console.WriteLine("Введите y0");
        var y0 = Math.E;/*Convert.ToDouble(Console.ReadLine());*/

        //Console.WriteLine("Введите z0");
        var z0 = 2 * Math.E;/*Convert.ToDouble(Console.ReadLine());*/

        Console.WriteLine("Введите требуемую точность");
        var eps0 = Convert.ToDouble(Console.ReadLine());

        Solve(N, a, b, y0, z0, eps0);
    }

    private static double Y(double x, double y, double z)
    {
        return z;
    }

    private static double Z(double x, double y, double z)
    {
        return z * z / y + z / x;
    }

    private static void Solve(int initN, double a, double b, double y0, double z0, double eps0)
    {
        Solution prevSolution = new Solution(0, 0);
        var currentN = initN;
        var currentPointsQuantity = currentN + 1;

        var yi = new double[currentN];
        var zi = new double[currentN];
        double[] yiprev;
        double[] ziprev;

        double prevEpsZ = 0;
        double currentEpsZ = 0;
        double prevEpsY = 0;
        double currentEpsY = 0;

        var h = (b - a) / currentPointsQuantity;
        var x = GetUniformPoints(currentPointsQuantity, a, b);
        var solution = GetSolution(h, y0, z0, x, currentPointsQuantity, ref yi, ref zi);
        var iterationsCount = 0;

        var x1 = x;
        var y1 = yi;
        var z1 = zi;

        var xprev = x;
        do
        {
            iterationsCount++;
            prevSolution = solution;
            currentN *= 2;
            currentPointsQuantity = currentN + 1;
            h = (b - a) / currentPointsQuantity;
            if (currentN > 64e6)
            {
                throw new ApplicationException("Шаг стал недопустимо маленьким, n = " + currentN);
            }

            xprev = x;
            yiprev = yi;
            ziprev = zi;
            x = GetUniformPoints(currentPointsQuantity, a, b);
            solution = GetSolution(h, y0, z0, x, currentPointsQuantity, ref yi, ref zi);
            prevEpsZ = currentEpsZ;
            prevEpsY = currentEpsY;
            currentEpsZ = Math.Abs(prevSolution.z - solution.z) / 7;
            currentEpsY = Math.Abs(prevSolution.y - solution.y) / 7;
            if (Math.Abs(currentEpsZ) - Math.Abs(prevEpsZ) == 0)
            {
                throw new ApplicationException("С уменьшением шага погрешность перестала уменьшаться, h = " + h);
            }
        } while (currentEpsZ > eps0 || currentEpsY > eps0);

        PrintToFile("yn.txt", xprev, yiprev, currentN / 2 + 1);
        PrintToFile("y2n.txt", x, yi, currentN + 1);
        PrintToFile("zn.txt", xprev, ziprev, currentN / 2 + 1);
        PrintToFile("z2n.txt", x, zi, currentN + 1);

        Console.WriteLine($"Всего было произведено {iterationsCount} итераций");
        Console.WriteLine($"y: {b} : {solution.y}");
        Console.WriteLine($"z: {b} : {solution.z}");
        Console.WriteLine($"Решение получено при n = {currentN}, шаг {h}");

        drawingGraph(x1, y1, z1, xprev, yiprev, ziprev, x, yi, zi);
    }

    static void drawingGraph(double[] x1, double[] y1, double[] z1, double[] x2, double[] y2, double[] z2, double[] x3, double[] y3, double[] z3)
    {
        try
        {
            ToSave data = new ToSave();
            data.X1 = x1;
            data.Y1 = y1;
            data.Z1 = z1;
            data.X2 = x2;
            data.Y2 = y2;
            data.Z2 = z2;
            data.X3 = x3;
            data.Y3 = y3;
            data.Z3 = z3;

            string json = JsonSerializer.Serialize(data);
            File.WriteAllText(@"D:\лабы\6 семестр\ЧМ\laba2.2\Save\temp.json", json);
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(@"D:\лабы\6 семестр\ЧМ\laba2.2\visualizationGraphs\visualizationGraphs.py")
            {
                UseShellExecute = true
            };
            p.Start();
            p.WaitForExit();
        }
        catch (Win32Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    private static void PrintToFile(string filename, double[] x, double[] y, int n)
    {
        var xValues = "";
        var yValues = "";

        for (var i = 0; i < n; i++)
        {
            xValues += x[i] + " ";
            yValues += y[i] + " ";
        }
        if (File.Exists(filename))
        {
            File.Delete(filename);
        }

        using var fstream = new FileStream(filename, FileMode.OpenOrCreate);
        fstream.Write(Encoding.Default.GetBytes(xValues + "\n"));
        fstream.Write(Encoding.Default.GetBytes(yValues + "\n"));
        fstream.Close();
    }

    private static Solution GetSolution(double h, double y0, double z0, double[] x, int pointsQuantity, ref double[] yi, ref double[] zi)
    {
        var yprev = y0;
        var zprev = z0;

        zi = new double[pointsQuantity];
        yi = new double[pointsQuantity];

        yi[0] = y0;
        zi[0] = z0;

        for (var i = 1; i < pointsQuantity; i++)
        {
            var k1 = h * Y(x[i - 1], yprev, zprev);
            var l1 = h * Z(x[i - 1], yprev, zprev);

            var k2 = h * Y(x[i - 1] + h, yprev + k1, zprev + l1);
            var l2 = h * Z(x[i - 1] + h, yprev + k1, zprev + l1);

            yprev = yprev + (k1 + k2) / 2;
            zprev = zprev + (l1 + l2) / 2;
            yi[i] = yprev;
            zi[i] = zprev;
        }
        return new Solution(yprev, zprev);
    }

    private static double[] GetUniformPoints(int pointsNumber, double a, double b)
    {
        var result = new double[pointsNumber];
        var step = (b - a) / (pointsNumber - 1);
        for (var i = 0; i < pointsNumber; i++)
        {
            result[i] = a + step * i;
        }

        return result;
    }

    private static double Fsource(double x)
    {
        return x;
    }
}