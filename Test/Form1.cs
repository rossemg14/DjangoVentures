using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace Test
{
    public partial class DjangoVentures : Form
    {
        int[,] mazeGrid;
        int tileSize = 38;
        private int playerX = 1, playerY = 0;
        private System.Windows.Forms.Timer timer;

        private Bitmap mazeBitmap;
        private Bitmap playerBitmap;

        private bool isMovingUp = false;
        private bool isMovingDown = false;
        private bool isMovingLeft = false;
        private bool isMovingRight = false;

        Bitmap moveUp;
        Bitmap moveDown;
        Bitmap moveLeft;
        Bitmap moveRight;

        private Bitmap healthIcon;
        private Bitmap strengthIcon;
        private Bitmap speedIcon;
        public DjangoVentures()
        {
            InitializeComponent();
            InitializeResources();
            InitializeGame();
        }

        public void InitializeResources()
        {
            healthIcon = Properties.Resources.health_icon;
            strengthIcon = Properties.Resources.strength_icon;
            speedIcon = Properties.Resources.speed_icon;

            mazeBitmap = new Bitmap(650, 650);
            mazeBitmap = Properties.Resources.maze_stage1_transparent;

            moveUp = new Bitmap(Properties.Resources.player_run_up);
            moveDown = new Bitmap(Properties.Resources.player_run_down);
            moveLeft = new Bitmap(Properties.Resources.player_run_left);
            moveRight = new Bitmap(Properties.Resources.player_run_right);
        }

        public void InitializeGame()
        {

            //mazePictureBox.Location = new Point((ClientSize.Width - mazePictureBox.Width) / 2, (ClientSize.Height - mazePictureBox.Height) / 2);

            if (mazeGrid == null)
            {
                mazeGrid = ConvertToGrid(mazeBitmap);
            }
            mazePictureBox.Image = mazeBitmap;

            playerBitmap = new Bitmap(Properties.Resources.player_idle);
            playerPictureBox.Image = playerBitmap;
            //mazePictureBox.Controls.Add(playerPictureBox);
            //this.Controls.Add(playerPictureBox);

            //Point spawnPoint = GetStartingPoint();
            //playerPictureBox.Location = new Point(tileSize, tileSize);
            playerPictureBox.Location = GetStartingPoint();

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 16;
            timer.Tick += GameLoop;
            timer.Start();
        }

        private int[,] ConvertToGrid(Bitmap bitmap)
        {
            int rows = bitmap.Height / tileSize;
            int cols = bitmap.Width / tileSize;
            int[,] grid = new int[rows, cols];

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    int pixelX = x * tileSize + tileSize / 2;
                    int pixelY = y * tileSize + tileSize / 2;

                    // Clamp to bitmap boundaries
                    pixelX = Math.Min(pixelX, bitmap.Width - 1);
                    pixelY = Math.Min(pixelY, bitmap.Height - 1);

                    Color pixelColor = bitmap.GetPixel(pixelX, pixelY);
                    grid[y, x] = (pixelColor.A < 10) ? 1 : 0;
                }
            }

            return grid;
        }

        private Point GetStartingPoint() {
            int mazeStartX = mazePictureBox.Location.X;
            int mazeStartY = mazePictureBox.Location.Y;

            for (int y = 0; y < mazeGrid.GetLength(0); y++) {
                for (int x = 0; x < mazeGrid.GetLength(1); x++) {
                    if (mazeGrid[y, x] == 1) {
                        playerX = x;
                        playerY = y;

                        int pixelX = mazeStartX + x * tileSize + (tileSize - playerPictureBox.Width) / 2;
                        int pixelY = mazeStartY + y * tileSize + (tileSize - playerPictureBox.Height) / 2;

                        return new Point(pixelX, pixelY);
                    }
                }
            }

            throw new Exception("No valid starting position found in the maze!");
        }

        private void GameLoop(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            int deltaX = 0, deltaY = 0;

            switch (keyData)
            {
                case Keys.W:
                    deltaY = -1;
                    isMovingUp = true;
                    playerPictureBox.Image = moveUp;
                    break;
                case Keys.S:
                    deltaY = 1;
                    isMovingDown = true;
                    playerPictureBox.Image = moveDown;
                    break;
                case Keys.A:
                    deltaX = -1;
                    isMovingLeft = true;
                    playerPictureBox.Image = moveLeft;
                    break;
                case Keys.D:
                    deltaX = 1;
                    isMovingRight = true;
                    playerPictureBox.Image = moveRight;
                    break;
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }

            int updatedX = playerX + deltaX;
            int updatedY = playerY + deltaY;

            if (IsWalkable(updatedX, updatedY))
            {
                playerX = updatedX;
                playerY = updatedY;
                UpdatePlayerVisual();
            }

            return true;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            switch (e.KeyCode)
            {
                case Keys.W:
                    isMovingUp = false;
                    break;
                case Keys.S:
                    isMovingDown = false;
                    break;
                case Keys.A:
                    isMovingLeft = false;
                    break;
                case Keys.D:
                    isMovingRight = false;
                    break;
            }

            if (!isMovingUp && !isMovingDown && !isMovingLeft && !isMovingRight)
            {
                playerPictureBox.Image = Properties.Resources.player_idle;
            }
        }

        private void UpdatePlayerVisual()
        {
            int startX = mazePictureBox.Location.X;
            int startY = mazePictureBox.Location.Y;

            playerPictureBox.Location = new Point(
                startX + playerX * tileSize + (tileSize - playerPictureBox.Width) / 2,
                startY + playerY * tileSize + (tileSize - playerPictureBox.Height) / 2
            );
        }

        private bool IsWalkable(int x, int y)
        {
            if (x < 0 || y < 0 || y >= mazeGrid.GetLength(0) || x >= mazeGrid.GetLength(1))
            {
                return false;
            }
            return mazeGrid[y, x] == 1;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Hello World :VV
        }

    }
}
