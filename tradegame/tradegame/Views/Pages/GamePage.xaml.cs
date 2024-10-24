using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Controls;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using tradegame.Models;
using tradegame.Utilities;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace tradegame.Views.Pages
{
    public partial class GamePage : Page
    {
        private List<Point> dots = new List<Point>();
        private List<Line> lines = new List<Line>();
        private int player1Score = 0;
        private int player2Score = 0;
        private Point? lastDotClicked = null;
        private ClientWebSocket _clientWebSocket;
        private SolidColorBrush BoxColor { get; set; }
        private bool IsChance { get; set; }

        private List<PointInfo> PointsInfo { get; set; }
        public GamePage(ClientWebSocket clientWebSocket)
        {
            InitializeComponent();
            DrawDots();
            UpdateCurrentPlayer();
            _clientWebSocket = clientWebSocket;
            PointsInfo = new List<PointInfo>();
            WebSocketUtil.OnMessageReceived += OnMessageReceived;
            GameCompletionCheck();
            
        }
        private async void OnMessageReceived(string message)
        {
            if (message == "yeschance")
            {
                IsChance = true;
            }
            else if (message == "nochance")
            {
                IsChance = false;
            }
            else
            {
                Message msg = JsonConvert.DeserializeObject<Message>(message);
                List<Point> points = msg.points;
                BoxColor = new SolidColorBrush(Colors.Red); 
                await DrawLineBetweenDotsAsync(points[0], points[1]);
            }
        }
        private async void WaitMessage_Click(object sender, RoutedEventArgs e)
        {
            await WebSocketUtil.SendWaitingMessageAsync("waiting");
            StartGame.Visibility = Visibility.Hidden;
            UpdateCurrentPlayer();
        }

        private void DrawDots()
        {
            for (int i = 0; i < 40; i++) // 10 dots for example
            {
                for (int j = 0; j < 40; j++)
                {
                    Ellipse dot = new Ellipse
                    {
                        Fill = Brushes.Black,
                        Width = 10,
                        Height = 10
                    };

                    dot.Margin = new Thickness(i * 60 + 30, j * 60 + 30, 0, 0);
                    GameCanvas.Children.Add(dot);
                    dots.Add(new Point(i * 60 + 30, j * 60 + 30));
                }
            }
        }

        private async void GameCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsChance == false)
            {
                MessageBox.Show("Not your Chance, Please wait for opponent to play");
                return;
            }
            Point clickPosition = e.GetPosition(GameCanvas);
            
            if (lastDotClicked.HasValue)
            {
                Point secondDotClicked = FindClosestDot(clickPosition);
                BoxColor = new SolidColorBrush(Colors.LightBlue);
                await DrawLineBetweenDotsAsync(lastDotClicked.Value, secondDotClicked);
                
                lastDotClicked = null; // Reset for next click
            }
            else
            {
                lastDotClicked = FindClosestDot(clickPosition);
            }
        }

        private Point FindClosestDot(Point clickPosition)
        {
            Point closestDot = dots[0];
            double closestDistance = double.MaxValue;

            foreach (Point dot in dots)
            {
                double distance = Distance(dot, clickPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDot = dot;
                }
            }

            return closestDot;
        }

        private double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        private async Task DrawLineBetweenDotsAsync(Point startPoint, Point endPoint)
        {
            if (startPoint.X == endPoint.X)
            {
                if (Math.Abs(startPoint.Y - endPoint.Y) > 60)
                {
                    return;
                }
            }
            else
            {
                if (Math.Abs(startPoint.X - endPoint.X) > 60)
                {
                    return;
                }
            }
            if (startPoint == endPoint) return;
            if (IsValidLine(startPoint, endPoint))
            {
                Line line = new Line
                {
                    X1 = startPoint.X,
                    Y1 = startPoint.Y,
                    X2 = endPoint.X,
                    Y2 = endPoint.Y,
                    Stroke = Brushes.Blue,
                    StrokeThickness = 2
                };
                GameCanvas.Children.Add(line);

                if (IsChance == true)
                {
                    List<Point> points = new List<Point>();
                    points.Add(startPoint);
                    points.Add(endPoint);
                    await WebSocketUtil.SendGameMessageAsync(points);
                    
                }


                lines.Add(line);


                // Check if a square is formed and update scores
                int score = CheckForCompletedSquare(startPoint, endPoint);
                if (score>0)
                {
                    if (IsChance)
                    {
                        player1Score+=score;
                    }
                    else
                    {
                        player2Score+=score;
                    }
                    
                }
                else IsChance = !IsChance;
                UpdateCurrentPlayer();
                MakeLineBlink(line);
            }
            
        }

        private bool IsValidLine(Point startPoint, Point endPoint)
        {
            // Check if the points are aligned vertically or horizontally
            bool isSameRow = Math.Abs(startPoint.Y - endPoint.Y) < 10; // Y coordinates should be the same (allowing for a small margin)
            bool isSameColumn = Math.Abs(startPoint.X - endPoint.X) < 10; // X coordinates should be the same (allowing for a small margin)

            // Check if the distance is valid (to ensure the line is between the dots)
            bool isDistanceValid = Math.Abs(startPoint.X - endPoint.X) <= 60 || Math.Abs(startPoint.Y - endPoint.Y) <= 60;

            return (isSameRow || isSameColumn) && isDistanceValid;
        }

        private int CheckForCompletedSquare(Point startPoint, Point endPoint)
        {
            int score = 0;
            // Calculate the horizontal line 
            
            if (startPoint.Y == endPoint.Y)
            {
                if (startPoint.X > endPoint.X)
                {
                    Point temp = startPoint;
                    startPoint = endPoint;
                    endPoint = temp;
                }
                double startx = startPoint.X;
                double starty = startPoint.Y;
                double endx = endPoint.X;
                double endy = endPoint.Y;
                bool topLine = LineExists(new Point(startx, starty+60), new Point(endx, endy+60));
                bool bottomLine = LineExists(new Point(startx,starty), new Point(endx, endy));
                bool leftLine = LineExists(new Point(startx, starty+60), new Point(endx-60, endy));
                bool rightLine = LineExists(new Point(startx+60, starty), new Point(endx, endy+60));

                if(topLine && bottomLine && leftLine && rightLine)
                {
                    FillSquare(startx, starty, 60, 60);
                    score++;
                }
                topLine = LineExists(new Point(startx, starty - 60), new Point(endx, endy - 60));
                bottomLine = LineExists(new Point(startx, starty), new Point(endx, endy));
                leftLine = LineExists(new Point(startx, starty - 60), new Point(endx - 60, endy));
                rightLine = LineExists(new Point(startx + 60, starty), new Point(endx, endy - 60));
                if (topLine && bottomLine && leftLine && rightLine)
                {
                    FillSquare(startx, starty-60, 60, 60);
                    score++;
                }
            }
            else
            {

                if (startPoint.Y > endPoint.Y)
                    {
                        Point temp = startPoint;
                        startPoint = endPoint;
                        endPoint = temp;
                    }
                double startx = startPoint.X;
                double starty = startPoint.Y;
                double endx = endPoint.X;
                double endy = endPoint.Y;
                bool topLine = LineExists(new Point(startx, starty + 60), new Point(endx-60, endy));
                bool bottomLine = LineExists(new Point(startx-60, starty), new Point(endx, endy-60));
                bool leftLine = LineExists(new Point(startx-60, starty), new Point(endx - 60, endy));
                bool rightLine = LineExists(new Point(startx , starty), new Point(endx, endy ));

                if (topLine && bottomLine && leftLine && rightLine)
                {
                    FillSquare(startx-60, starty, 60, 60);
                    score++;
                }
                topLine = LineExists(new Point(startx, starty +60), new Point(endx+60, endy));
                bottomLine = LineExists(new Point(startx+60, starty), new Point(endx, endy-60));
                leftLine = LineExists(new Point(startx+60, starty), new Point(endx + 60, endy));
                rightLine = LineExists(new Point(startx , starty), new Point(endx, endy ));
                if (topLine && bottomLine && leftLine && rightLine)
                {
                    FillSquare(startx, starty, 60, 60);
                    score++;
                }
            }
            return score;
            
        }

        private bool LineExists(Point start, Point end)
        {
            foreach (Line line in lines)
            {
                if ((line.X1 == start.X && line.Y1 == start.Y && line.X2 == end.X && line.Y2 == end.Y) ||
                    (line.X1 == end.X && line.Y1 == end.Y && line.X2 == start.X && line.Y2 == start.Y))
                {
                    return true;
                }
            }
            return false;
        }

        

        private void UpdateCurrentPlayer()
        {
            if (IsChance)
            {
                CurrentPlayerText.Text = $"Your Turn";

            }
            else
            {
                CurrentPlayerText.Text = $"Opponent's Turn";

            }
            string currentPlayer = UserDetaiils.UserName;
            ScoreText.Text = $"{currentPlayer} {player1Score}, Opponent : {player2Score}";
        }

        private void MakeLineBlink(Line line)
        {
            // Create a DispatcherTimer
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500); // Set interval for 500 milliseconds (0.5 seconds)

            int elapsedTime = 0; // Track elapsed time in milliseconds

            // Define the blinking action
            timer.Tick += (sender, args) =>
            {
                // Toggle the line's color between green and blue
                if (line.Stroke == Brushes.Blue)
                {
                    line.Stroke = Brushes.Transparent;
                }
                else
                {
                    line.Stroke = Brushes.Blue;
                }

                // Increment elapsed time
                elapsedTime += 500;

                // Stop the blinking after 5 seconds (5000 milliseconds)
                if (elapsedTime >= 2000)
                {
                    timer.Stop();
                    line.Stroke = Brushes.Blue; // Ensure the line ends up as blue
                }
            };

            // Start the timer
            timer.Start();
        }
        private void FillSquare(double x, double y, double width, double height)
        {
            Rectangle rectangle = new Rectangle
            {
                Fill = BoxColor, // Choose your desired fill color here
                Width = width,
                Height = height,
                Opacity = 0.5 // Set opacity to make it semi-transparent (optional)
            };

            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);
            GameCanvas.Children.Add(rectangle);
        }
        private void GameCompletionCheck()
        {
            if (player1Score + player2Score == 60)
            {
                string winner = player1Score > player2Score ? UserDetaiils.UserName : "opponent";
                MessageBox.Show($"Game is completed and winner is {winner}");
            }
        }

    }
}
