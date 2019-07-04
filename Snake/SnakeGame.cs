using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Snake {
    public partial class SnakeGame : Form {
        #region Variables
        private const Keys up = Keys.Up;
        private const Keys right = Keys.Right;
        private const Keys down = Keys.Down;
        private const Keys left = Keys.Left;

        private const int test = 0;

        private int posX;
        private int posY;
        private const int xMax = 69;
        private const int xMin = 0;
        private const int yMax = 46;
        private const int yMin = 0;

        private bool lastKeyProcessed = true;
        private int multiplier = 11;
        private int gamePoint = 0;
        private DirectionEnum direction;
        private Point bait;
        private List<Point> snakePosition = new List<Point>();

        //Sabit duvarlar için
        private List<Point> brickOnTheWall = new List<Point>();

        //Hareketli duvarlar için, multilevel dizi, içine girip -1 leyecek yöne göre
        private List<Point[]> itIsAlive = new List<Point[]>();
        private List<bool> moveIt = new List<bool>();
        private List<bool> directionIt = new List<bool>();


        private List<Point[]> ouroborus = new List<Point[]>();
        private int ouroborusStartX, ouroborusEndX, ouroborusStartY, ouroborusEndY;

        //Hareketli karenin olabileceği pozisyonların listesi, buraya diğer duvarlar gelmeyecek
        private List<Point[]> sittingDead = new List<Point[]>();
        #endregion

        #region Constructor And Loader
        public SnakeGame() {
            InitializeComponent();
        }

        private void SnakeGame_Load(object sender, EventArgs e) {
            speedSelection.SelectedIndex = 2;
        }
        #endregion

        #region Events
        private void startButton_Click(object sender, EventArgs e) {
            startGame();
        }

        private void gameTimer_Tick(object sender, EventArgs e) {
            playGame();
        }

        private void resetButton_Click(object sender, EventArgs e) {
            resetGame();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (gameTimer.Enabled && lastKeyProcessed) {
                lastKeyProcessed = false;
                determineDirection(keyData);
            }

            pauseGame(keyData);
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion

        #region Methods
        private void startGame() {
            speedSelection.Enabled = false;
            startButton.Enabled = false;
            setGameSpeed();
            gameTimer.Enabled = true;
        }

        private void playGame() {
            setPositionValues();
            bool isGameEnded = isGameOver();

            if (isGameEnded) {
                gameTimer.Enabled = false;
                MessageBox.Show(String.Format("Oyun Bitti!\n\nPuanınız: {0}", gamePoint), "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            snakePosition.Insert(0, new Point(posX, posY));
            snakePosition.RemoveAt(snakePosition.Count - 1);

            if (bait.X == posX * multiplier && bait.Y == posY * multiplier) {
                eatBait();
            }

            drawSnake();
            lastKeyProcessed = true;
        }

        private bool checkPosition(int x, int y) {
            if (x == bait.X / multiplier || y == bait.Y / multiplier) {
                return true;
            }

            for (int i = 0; i < sittingDead.Count; i++) {
                for (int j = 0; j < 11; j++) {
                    if (sittingDead[i][j].X == x || sittingDead[i][j].Y == y) {
                        return true;
                    }
                }
            }

            for (int i = 0; i < itIsAlive.Count; i++) {
                if (itIsAlive[i].Any(t => t.X == x && t.Y == y)) {
                    return true;
                }
            }

            for (int i = 0; i < ouroborus.Count; i++) {
                if (ouroborus[i].Any(t => t.X == x && t.Y == y)) {
                    return true;
                }
            }

            if (snakePosition.Any(t => t.X == x && t.Y == y)) {
                return true;
            }


            return false;
        }

        private bool checkPosition(int x, int y, bool direction, int step) {
            for (int i = direction ? x : y; i < (direction ? x : y) + step; i++) {
                if (checkPosition(direction ? i : x, direction ? y : i)) {

                    return true;
                }
            }

            return false;
        }

        private void iLikeToMoveIt() {

        }

        private void anotherBrickInTheWall() {
            brickOnTheWall.Clear();
            itIsAlive.Clear();
            sittingDead.Clear();
            moveIt.Clear();
            directionIt.Clear();
            ouroborus.Clear();

            Random random = new Random(DateTime.Now.TimeOfDay.Milliseconds);
            int x = 0;
            int y = 0;

            //Kolay
            if (speedSelection.SelectedIndex == 0 || speedSelection.SelectedIndex == 1) {
                int z = speedSelection.SelectedIndex == 0 ? 2 : 3;

                for (int i = 0; i < z; i++) {
                    while (true) {
                        x = random.Next(xMin, xMax - 4);
                        y = random.Next(yMin, yMax + 1);

                        if (!checkPosition(x, y, true, 5)) {
                            brickOnTheWall.Add(new Point(x, y));
                            brickOnTheWall.Add(new Point(x + 1, y));
                            brickOnTheWall.Add(new Point(x + 2, y));
                            brickOnTheWall.Add(new Point(x + 3, y));
                            brickOnTheWall.Add(new Point(x + 4, y));

                            break;
                        }
                    }
                }
            }

            if (speedSelection.SelectedIndex == 2 || speedSelection.SelectedIndex == 3 || speedSelection.SelectedIndex == 4) {
                for (int i = 0; i < 2; i++) {
                    while (true) {
                        x = random.Next(xMin, xMax + 1);  //3 + 3 + 5  == 11 )
                        y = random.Next(yMin, yMax + -10); // 3 hareket yukarı gitek

                        if (!checkPosition(x, y, false, 11)) {
                            insertBrick(x, y, false);

                            break;
                        }
                    }
                }
            }

            if (speedSelection.SelectedIndex == 3 || speedSelection.SelectedIndex == 4) {
                int z = speedSelection.SelectedIndex == 0 ? 1 : 2;

                for (int i = 0; i < z; i++) {
                    while (true) {
                        x = random.Next(xMin, xMax - 9);// 3 hareket sağa sola gitek
                        y = random.Next(yMin, yMax + 1);

                        if (!checkPosition(x, y, true, 8)) {
                            insertBrick(x, y, true);

                            break;
                        }
                    }
                }
            }

            if (speedSelection.SelectedIndex == 5) {
                while (true) {
                    x = random.Next(xMin + 11, xMax - 10);
                    y = random.Next(yMin + 11, yMax - 10);

                    if (!checkPosition(x, y, true, 11)) {
                        if (!checkPosition(x, y, false, 11)) {
                            if (!checkPosition(x + 10, y, false, 11)) {
                                if (!checkPosition(x, y + 10, true, 11)) {
                                    ouroborus.Add(new Point[]{ new Point(x + 3, y),
                                        new Point(x + 4, y),
                                            new Point(x + 5, y),
                                     new Point(x + 6, y),
                                     new Point(x + 7, y)});

                                    ouroborus.Add(new Point[] { new Point(x + 10, y + 3),
                                    new Point(x + 10, y + 4),
                                    new Point(x + 10, y + 5),
                                    new Point(x + 10, y + 6),
                                    new Point(x + 10, y + 7)});

                                    ouroborus.Add(new Point[] { new Point(x + 3, y + 10),
                                    new Point(x + 4, y + 10),
                                    new Point(x + 5, y + 10),
                                    new Point(x + 6, y + 10),
                                    new Point(x + 7, y + 10)});

                                    ouroborus.Add(new Point[] { new Point(x, y + 3),
                                    new Point(x, y + 4),
                                    new Point(x, y + 5),
                                     new Point(x, y + 6),
                                     new Point(x, y + 7)});

                                    ouroborusStartX = x;
                                    ouroborusStartY = y;
                                    ouroborusEndX = x + 10;
                                    ouroborusEndY = y + 10;

                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void insertBrick(int x, int y, bool direction) {
            if (direction) {
                itIsAlive.Add(new Point[] {
                            new Point(x+ 3, y),
                            new Point(x + 4, y),
                            new Point(x + 5, y),
                            new Point(x + 6, y),
                            new Point(x + 7, y),
            });


                sittingDead.Add(new Point[] {
                    new Point(x, y),
                    new Point(x + 1, y),
                    new Point(x + 2, y),
                    new Point(x + 3, y),
                    new Point(x + 4, y),
                    new Point(x + 5, y),
                    new Point(x + 6, y),
                    new Point(x + 7, y),
                    new Point(x + 8, y),
                    new Point(x + 9, y),
                    new Point(x + 10, y)
                });
            } else {
                itIsAlive.Add(new Point[] {
                            new Point(x, y+3),
                            new Point(x , y+4),
                            new Point(x , y+5),
                            new Point(x , y+6),
                            new Point(x , y+7),
            });


                sittingDead.Add(new Point[] {
                    new Point(x, y),
                    new Point(x , y+1),
                    new Point(x , y+2),
                    new Point(x , y+3),
                    new Point(x , y+4),
                    new Point(x , y+5),
                    new Point(x , y+6),
                    new Point(x , y+7),
                    new Point(x , y+8),
                    new Point(x , y+9),
                    new Point(x , y+10)
                });
            }


            moveIt.Add(direction);
            directionIt.Add(direction);
        }

        private void resetGame() {
            gameTimer.Enabled = false;
            startButton.Enabled = true;
            speedSelection.Enabled = true;

            createNewSnake();
            resetVariables();
            createBait();
            anotherBrickInTheWall();
            drawSnake();

        }

        private void pauseGame(Keys keyData) {
            if (keyData == Keys.P) {
                gameTimer.Enabled = !gameTimer.Enabled;
            }
        }

        private void setGameSpeed() {
            switch (speedSelection.SelectedIndex) {
                case 0:
                    gameTimer.Interval = 100;
                    break;
                case 1:
                    gameTimer.Interval = 75;
                    break;
                case 2:
                default:
                    gameTimer.Interval = 50;
                    break;
                case 3:
                    gameTimer.Interval = 40;
                    break;
                case 4:
                    gameTimer.Interval = 25;
                    break;
                case 5:
                    gameTimer.Interval = 10;
                    break;
            }
        }

        private void resetVariables() {
            posX = 12;
            posY = 20;
            gamePoint = 0;
            direction = DirectionEnum.Right;
            printStat();
        }

        private void createNewSnake() {
            snakePosition.Clear();
            snakePosition.Add(new Point(12, 20));
            snakePosition.Add(new Point(11, 20));
            snakePosition.Add(new Point(10, 20));
        }

        private void setPositionValues() {
            switch (direction) {
                case DirectionEnum.Down:
                    posY++;
                    break;
                case DirectionEnum.Up:
                    posY--;
                    break;
                case DirectionEnum.Left:
                    posX--;
                    break;
                case DirectionEnum.Right:
                default:
                    posX++;
                    break;
            }
        }

        private bool isGameOver() {
            //Check limits
            if (posX > xMax || posX < xMin || posY > yMax || posY < yMin) {
                return true;
            }

            for (int i = 0; i < itIsAlive.Count; i++) {
                for (int j = 0; j < 5; j++) {
                    if (snakePosition.Any(t => t.X == itIsAlive[i][j].X && t.Y == itIsAlive[i][j].Y)) {
                        return true;
                    }
                }
            }

            for (int i = 0; i < ouroborus.Count; i++) {
                for (int j = 0; j < 5; j++) {
                    if (snakePosition.Any(t => t.X == ouroborus[i][j].X && t.Y == ouroborus[i][j].Y)) {
                        return true;
                    }
                }
            }

            if (brickOnTheWall.Any(t => t.X == posX && t.Y == posY)) {
                return true;
            }

            //Eat itself
            if (snakePosition.Any(t => t.X == posX && t.Y == posY)) {
                return true;
            }

            return false;
        }

        private void createBait() {
            Random random = new Random(DateTime.Now.TimeOfDay.Milliseconds);
            int x = 0;
            int y = 0;
            bool contains = true;

            while (contains) {
                x = random.Next(xMin, xMax + 1) * multiplier;
                y = random.Next(yMin, yMax + 1) * multiplier;

                contains = snakePosition.Any(t => t.X == x && t.Y == y);
            }

            bait = new Point(x, y);
        }

        private void eatBait() {
            Point lastPoint = snakePosition[snakePosition.Count - 1];
            snakePosition.Add(new Point(lastPoint.X, lastPoint.Y));
            gamePoint += (speedSelection.SelectedIndex + 1) * 10;
            printStat();
            createBait();
        }

        private void printStat() {
            scoreLabel.Text = gamePoint.ToString();
            baitLabel.Text = (snakePosition.Count - 3).ToString();
        }

        private void determineDirection(Keys keyData) {
            switch (keyData) {
                case up:
                    if (direction != DirectionEnum.Down)
                        direction = DirectionEnum.Up;
                    break;
                case down:
                    if (direction != DirectionEnum.Up)
                        direction = DirectionEnum.Down;
                    break;
                case left:
                    if (direction != DirectionEnum.Right)
                        direction = DirectionEnum.Left;
                    break;
                case right:
                default:
                    if (direction != DirectionEnum.Left)
                        direction = DirectionEnum.Right;
                    break;
            }
        }

        #endregion

        #region Drawing Methods
        private void drawSnake() {
            gameArea.Refresh();
            drawBait();
            drawBricks();
            foreach (Point item in snakePosition) {
                int xVal = item.X * multiplier;
                int yVal = item.Y * multiplier;

                drawPoint(xVal, yVal);
            }
        }

        private void drawBricks() {
            foreach (Point item in brickOnTheWall) {
                drawPoint(item.X * multiplier, item.Y * multiplier);
            }

            if (speedSelection.SelectedIndex == 2 || speedSelection.SelectedIndex == 3 || speedSelection.SelectedIndex == 4) {
                for (int i = 0; i < itIsAlive.Count; i++) {
                    if (moveIt[i] && (itIsAlive[i][0].X == sittingDead[i][0].X || itIsAlive[i][4].X == sittingDead[i][10].X)) {
                        directionIt[i] = !directionIt[i];
                    } else if (!moveIt[i] && (itIsAlive[i][0].Y == sittingDead[i][0].Y || itIsAlive[i][4].Y == sittingDead[i][10].Y)) {
                        directionIt[i] = !directionIt[i];
                    }

                    for (int j = 0; j < 5; j++) {
                        if (moveIt[i]) {
                            if (directionIt[i]) {
                                itIsAlive[i][j].X--;
                            } else {
                                itIsAlive[i][j].X++;
                            }
                        } else {
                            if (directionIt[i]) {
                                itIsAlive[i][j].Y--;
                            } else {
                                itIsAlive[i][j].Y++;
                            }

                        }

                        drawPoint(itIsAlive[i][j].X * multiplier, itIsAlive[i][j].Y * multiplier);
                    }
                }
            }

            if (speedSelection.SelectedIndex == 5) {
                for (int i = 0; i < 4; i++) {
                    for (int j = 0; j < 5; j++) {
                        //ÜSTTE DÜZ
                        if (ouroborus[i][j].X != ouroborusEndX && ouroborus[i][j].Y == ouroborusStartY) {
                            ouroborus[i][j].X++;
                        } else if (ouroborus[i][j].X == ouroborusEndX && ouroborus[i][j].Y != ouroborusEndY) {
                            //EN Sağda, Aşağıya inmemiş
                            ouroborus[i][j].Y++;
                        } else if (ouroborus[i][j].X != ouroborusStartX && ouroborus[i][j].Y == ouroborusEndY) {
                            //Aşağıda, başa dönememiş
                            ouroborus[i][j].X--;
                        } else if (ouroborus[i][j].X == ouroborusStartX && ouroborus[i][j].Y != ouroborusStartY) {
                            //Solda, yukarı çıkmamış
                            ouroborus[i][j].Y--;
                        }

                        drawPoint(ouroborus[i][j].X * multiplier, ouroborus[i][j].Y * multiplier);
                    }
                }
            }
        }

        private void drawPoint(int x, int y, bool isBlack = true) {
            using (Graphics g = this.gameArea.CreateGraphics()) {
                Color penColor = isBlack ? Color.Black : Color.Red;
                Pen pen = new Pen(penColor, 5);
                g.DrawRectangle(pen, x, y, 5, 5);
                pen.Dispose();
            }
        }

        private void drawBait() {
            drawPoint(bait.X, bait.Y, false);
        }
        #endregion
    }

    public enum DirectionEnum {
        Undefined,
        Up,
        Right,
        Down,
        Left
    }
}
