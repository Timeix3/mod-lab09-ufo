using ScottPlot;

namespace Lab9
{
    public partial class FormMain : Form
    {
        string projectDirectory = Directory.GetParent(Application.StartupPath).Parent.Parent.Parent.Parent.FullName;
        PointF startPoint = new(10, 10);
        PointF endPoint = new(1550, 850);
        float radius = 10;
        float step = 2;
        float pointSize = 15;
        int[] accuracy = { 2, 3, 4, 5, 6 };
        List<float> closestDistances;
        PointF currentPoint;
        int currentIndex = 0;
        float minDistance = float.MaxValue;
        public FormMain()
        {
            InitializeComponent();
            closestDistances = new();
            timer.Interval = 1;
            currentPoint = new(startPoint.X, startPoint.Y);
            timer.Start();
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.ScaleTransform(0.5f, 0.5f);
            g.FillEllipse(Brushes.Red, startPoint.X, startPoint.Y, pointSize, pointSize);
            g.FillEllipse(Brushes.Green, endPoint.X, endPoint.Y, pointSize, pointSize);
            g.FillEllipse(Brushes.Blue, currentPoint.X, currentPoint.Y, pointSize, pointSize);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            float angle = Atan(Math.Abs(endPoint.Y - startPoint.Y) / Math.Abs(endPoint.X - startPoint.X), accuracy[currentIndex]);
            currentPoint.X += step * Cos(angle, accuracy[currentIndex]);
            currentPoint.Y += step * Sin(angle, accuracy[currentIndex]);
            float distance = CalculateDistance(currentPoint, endPoint);
            if (distance < minDistance) minDistance = distance;
            pictureBox.Refresh();
            if (distance < radius || CalculateDistance(currentPoint, startPoint) > (CalculateDistance(endPoint, startPoint) + radius))
            {
                closestDistances.Add(minDistance);
                minDistance = float.MaxValue;
                currentIndex++;
                currentPoint = new(startPoint.X, startPoint.Y);
                if (currentIndex >= accuracy.Length)
                {
                    timer.Stop();
                    WriteData();
                    DrawGraph();
                }
            }
        }

        private void WriteData()
        {
            string resultFolder = Path.Combine(projectDirectory, "result");
            if (!Directory.Exists(resultFolder))
                Directory.CreateDirectory(resultFolder);
            using (StreamWriter writer = new StreamWriter(Path.Combine(resultFolder, "data.txt")))
            {
                for (int i = 0; i < accuracy.Length; i++)
                {
                    writer.WriteLine(accuracy[i] + " " + closestDistances[i]);
                }
            }
        }

        private void DrawGraph()
        {
            string resultFolder = Path.Combine(projectDirectory, "result");
            Plot myPlot = new();
            var scatter = myPlot.Add.Scatter(closestDistances, accuracy.Select(x => (float)x).ToList(), ScottPlot.Colors.Blue);
            myPlot.XLabel("Радиус зоны попадания");
            myPlot.YLabel("Количество членов ряда");
            myPlot.SavePng(Path.Combine(resultFolder, "plot.png"), 800, 600);
        }

        private float CalculateDistance(PointF point1, PointF point2) 
            => (float)Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));

        private float Factorial(int n)
        {
            if (n <= 1) return 1;
            return n * Factorial(n - 1);
        }

        private float Sin(float x, int n)
        {
            float sum = 0;
            for (int i = 0; i < n; i++)
            {
                sum += (float)(Math.Pow(-1, i) * Math.Pow(x, 2 * i + 1) / Factorial(2 * i + 1));
            }
            return sum;
        }

        private float Cos(float x, int n)
        {
            float sum = 0;
            for (int i = 0; i < n; i++)
            {
                sum += (float)(Math.Pow(-1, i) * Math.Pow(x, 2 * i) / Factorial(2 * i));
            }
            return sum;
        }

        private float Atan(float x, int n)
        {
            if (Math.Abs(x) > 1)
            {
                return (float)(Math.Sign(x) * Math.PI / 2 - Atan(1 / x, n));
            }

            float sum = 0;
            for (int i = 0; i < n; i++)
            {
                sum += (float)(Math.Pow(-1, i) * Math.Pow(x, 2 * i + 1) / (2 * i + 1));
            }
            return sum;
        }
    }
}
