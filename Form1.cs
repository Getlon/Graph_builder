using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Graph_builder
{
    public partial class Form1 : Form
    {
        protected Panel drawingPanel;
        protected Graphics g;
        protected bool isDirected = false; 
        protected int[,] matrix = { }; // Матрица весов
        protected PointF[] vertexPositions = new PointF[0]; // Массив координат вершин
        protected List<int> selectedVertices = new List<int>(); // Список выбранных вершин (индексы для matrix)
        protected float vertexRadius = 14;   // Радиус вершины
        protected string str; // От куда, куда

        public Form1()
        {
            InitializeComponent();
        }

        protected void DrawGraph()
        {
            g = drawingPanel.CreateGraphics();
            g.Clear(Color.White);

            int vertexCount = matrix.GetLength(0);
            vertexPositions = new PointF[vertexCount];
            float radius = Math.Min(drawingPanel.Width, drawingPanel.Height) / 2 - 25;
            PointF center = new PointF(drawingPanel.Width / 2, drawingPanel.Height / 2);

            float ellipseX = drawingPanel.Width / 2 - 25;
            float ellipseY = drawingPanel.Height / 2 - 25;

            // Рассчитать позиции вершин по эллипсу
            for (int i = 0; i < vertexCount; i++)
            {
                float angle = (float)(2 * Math.PI * i / vertexCount);
                vertexPositions[i] = new PointF(
                    center.X + ellipseX * (float)Math.Cos(angle),
                    center.Y + ellipseY * (float)Math.Sin(angle)
                );
            }

            // Рисование рёбер
            if (isDirected)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    for (int j = 0; j < vertexCount; j++)
                    {
                        if (matrix[i, j] != 0)
                        {
                            DrawEdge(vertexPositions[i], vertexPositions[j], Color.Black);
                            DrawWeight(vertexPositions[i], vertexPositions[j], matrix[i, j]);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    for (int j = i + 1; j < vertexCount; j++)
                    {
                        if (matrix[i, j] != 0)
                        {
                            DrawEdge(vertexPositions[i], vertexPositions[j], Color.Black);
                            DrawWeight(vertexPositions[i], vertexPositions[j], matrix[i, j]);
                        }
                    }
                }
            }

            // Рисование вершин
            for (int i = 0; i < vertexCount; i++)
            {
                DrawVertex(vertexPositions[i], i + 1);
            }
        }

        protected void DrawVertex(PointF position, int label)
        {
            float size = vertexRadius * 2; // Размер вершины
            RectangleF rect = new RectangleF(position.X - size / 2, position.Y - size / 2, size, size);
            Brush fillBrush = selectedVertices.Contains(label - 1) ? Brushes.Yellow : Brushes.LightBlue;
            g.FillEllipse(fillBrush, rect);
            g.DrawEllipse(Pens.Black, rect);

            // Подпись вершины
            string text = (label).ToString(); // !!!!!!!!!!!!!!!!!!!! label - 1
            Font vertexFont = new Font("Arial", 16, FontStyle.Bold);
            SizeF textSize = g.MeasureString(text, vertexFont);
            PointF textPosition = new PointF(
                position.X - textSize.Width / 2,
                position.Y - textSize.Height / 2
            );
            g.DrawString(text, vertexFont, Brushes.Black, textPosition);
        }
        protected void DrawEdge(PointF start, PointF end, Color color)
        {
            Pen pen = new Pen(color, 2);
            AdjustableArrowCap arrowCap = new AdjustableArrowCap(6, 6);
            if (isDirected)
            {
                pen.CustomEndCap = arrowCap;

                float dx = end.X - start.X;
                float dy = end.Y - start.Y;
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                // Корректировка конечной точки
                PointF newEnd = new PointF(
                    end.X - dx / distance * vertexRadius,
                    end.Y - dy / distance * vertexRadius
                );
                g.DrawLine(pen, start, newEnd);
            }
            else { g.DrawLine(pen, start, end); }
        }

        protected void DrawWeight(PointF start, PointF end, int weight)
        {
            // Подпись веса
            PointF middle = new PointF((start.X + end.X) / 2, (start.Y + end.Y) / 2);
            string text = weight.ToString();
            Font edgeFont = new Font("Arial", 16, FontStyle.Bold);
            SizeF textSize = g.MeasureString(text, edgeFont);
            PointF textPosition = new PointF(
                middle.X - textSize.Width / 2,
                middle.Y - textSize.Height / 2
            );
            g.DrawString(text, edgeFont, Brushes.Red, textPosition);
        }

        private void Panel1_Paint(object sender, PaintEventArgs e)
        {
            //if (matrix.Length != 0) { DrawGraph(); }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            drawingPanel = panel1;
            g = drawingPanel.CreateGraphics();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            selectedVertices.Clear();

            string str = richTextBox1.Text;
            string[] rows = str.Trim().Split(new[] { '\n' }); // Разбиваем входную строку на строки

            // Определяем размерность массива
            int rowCount = rows.Length;
            int colCount = rows[0].Split(new[] { ',' }).Length - 1; // Последний элемент ""
            if (colCount != rowCount)
            {
                g.Clear(Color.White); label2.ForeColor = Color.Red; label2.Text = "Ошибка!"; return;
            }
            matrix = new int[rowCount, colCount];

            // Заполняем массив
            for (int i = 0; i < rowCount; i++)
            {
                string[] elements = rows[i].Split(new[] { ',' });
                for (int j = 0; j < colCount; j++)
                {
                    try { matrix[i, j] = int.Parse(elements[j]); }
                    catch (System.FormatException) { g.Clear(Color.White); label2.ForeColor = Color.Red; label2.Text = "Ошибка!"; return; }
                    catch (System.IndexOutOfRangeException) { g.Clear(Color.White); label2.ForeColor = Color.Red; label2.Text = "Ошибка!"; return; }
                    
                }
            }

            if (checkBox1.Checked) { isDirected = true; }
            else { isDirected = false; }
            DrawGraph();
            label2.ForeColor = Color.Green;
            label2.Text = "Успех";
        }

        private void Panel1_MouseClick(object sender, MouseEventArgs e)
        {
            str = "";
            // Проверяем, попал ли клик на вершину
            for (int i = 0; i < vertexPositions.Length; i++)
            {
                PointF vertex = vertexPositions[i];
                float dx = e.X - vertex.X;
                float dy = e.Y - vertex.Y;
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                if (distance <= vertexRadius) // Клик попал на вершину
                {
                    if (selectedVertices.Contains(i))
                    {
                        selectedVertices.Remove(i); // Снять выбор, если уже выбрана
                        DrawVertex(vertex, i + 1);
                        break;
                    }
                    else
                    {
                        if (selectedVertices.Count == 2) { return; }
                        selectedVertices.Add(i); // Выбрать вершину
                        DrawVertex(vertex, i + 1);
                        break;
                    }
                }
            }

            foreach (int vertex in selectedVertices)
            {
                str += " " + (vertex + 1); // !!!!!!!!!!!!!!!! vertex
            }
            label3.Text = str.Trim().Replace(" ", " -> ");

        }

        protected string FindShortestPath()
        {
            int start = selectedVertices[0]; // От куда
            int end = selectedVertices[1]; // Куда
            int n = matrix.GetLength(0); // Количество вершин
            int[] distance = Enumerable.Repeat(int.MaxValue, n).ToArray(); // Расстояния
            int[] previous = new int[n]; // Для восстановления пути
            bool[] visited = new bool[n]; // Посещенные вершины
            distance[start] = 0;

            DrawGraph(); // Чтобы убрать нарисованные пути от пред. вызова

            // Алгоритм Дейкстры
            for (int i = 0; i < n; i++)
            {
                int u = -1;

                // Ищем вершину с минимальным расстоянием среди не посещённых
                for (int j = 0; j < n; j++)
                {
                    if (!visited[j] && (u == -1 || distance[j] < distance[u]))
                    {
                        u = j;
                    }
                }

                if (distance[u] == int.MaxValue) break; // Если минимальное расстояние бесконечно завершаем цикл

                visited[u] = true; // Помечаем вершину как посещённую

                // Обновление расстояний до соседей вершины u
                for (int v = 0; v < n; v++)
                {
                    if (matrix[u, v] > 0 && distance[u] + matrix[u, v] < distance[v])
                    {
                        distance[v] = distance[u] + matrix[u, v];
                        previous[v] = u; // Предшественником вершины v является вершина u
                    }
                }
            }

            // Если до конечной вершины нет пути
            if (distance[end] == int.MaxValue)
                return "Путь не существует";

            // Восстановление пути
            var path = new List<int>();
            for (int at = end; at != start; at = previous[at]) { path.Add(at + 1); }
            path.Add(start + 1); // !!!!!!!!!!!!!!! start
            path.Reverse();

            // Визуал
            for (int i = 0; i < path.Count(); i++)
            {
                int j = i + 1;
                if (i != path.Count() - 1)
                {
                    DrawEdge(vertexPositions[path[i] - 1], vertexPositions[path[j] - 1], Color.Yellow);
                    DrawWeight(vertexPositions[path[i] - 1], vertexPositions[path[j] - 1], matrix[path[i] - 1, path[j] - 1]);
                }
                DrawVertex(vertexPositions[path[i] - 1], path[i]);
            }

            /*for (int i = 0; i < path.Count(); i++) // !!!!!!!!!!!!!!!! если надо сменить нумерацию вершин с нуля
            {
                path[i] = path[i] - 1;
            }*/

            return $"Путь: {string.Join(" -> ", path)}\nДлина: {distance[end]}";
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (selectedVertices.Count() != 2)
            {
                label3.Text = "Должно быть выбрано две вершины!";
                return;
            }
            label3.Text = FindShortestPath();
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            DrawGraph();
        }
    }
}
