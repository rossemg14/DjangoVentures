using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;
using System.Numerics;

namespace Django_Ventures {
    public partial class DjangoVentures : Form {
        int[,] mazeGrid;
        int tileSize = 38;
        private int playerX = 1, playerY = 0;
        private System.Windows.Forms.Timer timer;
        bool redSlimeUsage = true, blueSlimeusage = true, greenSlimeUsage = true;

        private Bitmap mazeBitmap;
        private Bitmap playerBitmap;

        private bool isMovingUp = false;
        private bool isMovingDown = false;
        private bool isMovingLeft = false;
        private bool isMovingRight = false;
        bool FinnHasKey = false;

        Bitmap moveUp;
        Bitmap moveDown;
        Bitmap moveLeft;
        Bitmap moveRight;

        private Bitmap healthIcon;
        private Bitmap strengthIcon;
        private Bitmap speedIcon;
        public DjangoVentures() {
            InitializeComponent();
            InitializeResources();
            InitializeGame();
        }

        public void InitializeResources() {
            healthIcon = Properties.Resources.health_icon;
            strengthIcon = Properties.Resources.strength_icon;
            speedIcon = Properties.Resources.speed_icon;

            mazeBitmap = Properties.Resources.maze_stage1_transparent;

            moveUp = new Bitmap(Properties.Resources.player_idle);
            moveDown = new Bitmap(Properties.Resources.player_idle);
            moveLeft = new Bitmap(Properties.Resources.player_idle);
            moveRight = new Bitmap(Properties.Resources.player_idle);
        }

        public void InitializeGame() {
            if (mazeGrid == null) {
                mazeGrid = ConvertToGrid(mazeBitmap);
            }

            mazePictureBox.Location = new Point((ClientSize.Width - mazePictureBox.Width) / 2, (ClientSize.Height - mazePictureBox.Height) / 2);


            playerBitmap = new Bitmap(Properties.Resources.player_idle);

            playerPictureBox.Image = playerBitmap;
            Point spawnPoint = GetStartingPoint();
            playerPictureBox.Location = spawnPoint;

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 16;
            timer.Tick += GameLoop;
            timer.Start();
        }

        private int[,] ConvertToGrid(Bitmap bitmap) {
            int rows = bitmap.Height / tileSize;
            int cols = bitmap.Width / tileSize;
            int[,] grid = new int[rows, cols];

            for (int y = 0; y < rows; y++) {
                for (int x = 0; x < cols; x++) {
                    int pixelX = x * tileSize + tileSize / 2;
                    int pixelY = y * tileSize + tileSize / 2;

                    // Clamp to bitmap boundaries
                    pixelX = Math.Min(pixelX, bitmap.Width - 1);
                    pixelY = Math.Min(pixelY, bitmap.Height - 1);

                    Color pixelColor = bitmap.GetPixel(pixelX, pixelY);
                    grid[y, x] = (pixelColor.A < 10) ? 1 : 0; // Transparent = walkable
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

                        int pixelX = mazeStartX + (x * tileSize) + (tileSize - playerPictureBox.Width) / 2;
                        int pixelY = mazeStartY + (y * tileSize) + (tileSize - playerPictureBox.Height) / 2;

                        return new Point(pixelX, pixelY);
                    }
                }
            }

            throw new Exception("No valid starting position found in the maze!");
        }

        private void GameLoop(object sender, EventArgs e) {
            this.Invalidate();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            int deltaX = 0, deltaY = 0;

            switch (keyData) {
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

            CheckCollision();

            int updatedX = playerX + deltaX;
            int updatedY = playerY + deltaY;

            if (IsWalkable(updatedX, updatedY)) {
                playerX = updatedX;
                playerY = updatedY;
                UpdatePlayerVisual();
            }

            return true;
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            base.OnKeyUp(e);

            switch (e.KeyCode) {
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

            if (!isMovingUp && !isMovingDown && !isMovingLeft && !isMovingRight) {
                playerPictureBox.Image = Properties.Resources.player_idle;
                playerPictureBox.BringToFront();
            }
        }

        private void UpdatePlayerVisual() {
            int startX = mazePictureBox.Location.X;
            int startY = mazePictureBox.Location.Y;

            playerPictureBox.Location = new Point(
                startX + playerX * tileSize + (tileSize - playerPictureBox.Width) / 2,
                startY + playerY * tileSize + (tileSize - playerPictureBox.Height) / 2
            );
        }

        private bool IsWalkable(int x, int y) {
            if (x < 0 || y < 0 || y >= mazeGrid.GetLength(0) || x >= mazeGrid.GetLength(1)) {
                return false;
            }
            return mazeGrid[y, x] == 1;
        }

        private List<PictureBox> heartPictureBoxes;
        private List<PictureBox> strengthPictureBoxes;
        private List<PictureBox> speedPictureBoxes;

        private void InitializeHealthPanel() {
            heartPictureBoxes = new List<PictureBox> { heartPictureBox1, heartPictureBox2, heartPictureBox3,
                                                       heartPictureBox4, heartPictureBox5, heartPictureBox6 };
        }

        private void InitializeStengthPanel() {
            strengthPictureBoxes = new List<PictureBox> { strengthPictureBox1, strengthPictureBox2, strengthPictureBox3,
                                                          strengthPictureBox4, strengthPictureBox5, strengthPictureBox6 };
        }

        private void InitializeSpeedPanel() {
            speedPictureBoxes = new List<PictureBox> { speedPictureBox1, speedPictureBox2, speedPictureBox3,
                                                       speedPictureBox4, speedPictureBox5, speedPictureBox6 };
        }

        public void addStrength() {
            foreach (var strength in strengthPictureBoxes) {
                if (!strength.Visible) {
                    strength.Visible = true;
                    return;
                }
            }
        }

        public void addHealth() {
            foreach (var heart in heartPictureBoxes) {
                if (!heart.Visible) {
                    heart.Visible = true;
                    return;
                }
            }
        }

        public void addSpeed() {
            foreach (var speed in speedPictureBoxes) {
                if (!speed.Visible) {
                    speed.Visible = true;
                    return;
                }
            }
        }

        private void CheckCollision() {
            int mazeStartX = mazePictureBox.Location.X;
            int mazeStartY = mazePictureBox.Location.Y;

            if (playerPictureBox.Bounds.IntersectsWith(trueDoorPictureBox.Bounds)) {
                //LabelStatus.Text = "You have found the right door!";
                if (FinnHasKey == true) {
                    //LabelStatus.Text = "You may enter!!";
                } else {
                    KeyDoorStatus.Text = "You forgot to bring the key...";
                }
            }
            if (playerPictureBox.Bounds.IntersectsWith(falseDoorPictureBoxOne.Bounds) || playerPictureBox.Bounds.IntersectsWith(falseDoorPictureBoxTwo.Bounds)) {
                //LabelStatus.Text = "You have found the right door!!";
                if (FinnHasKey == true) {
                    //KeyDoorStatus.Text = "You may enter..SIKE";
                    Point spawnPoint = GetStartingPoint();
                    playerPictureBox.Location = spawnPoint;        // this is the block that resets the player to the spawn point
                    KeyStatus.Text = "You lost the Key!";
                    KeyToDoor.Visible = true;
                    FinnHasKey = false;
                } else {
                    KeyDoorStatus.Text = "You forgot to bring the key...";
                }
            }
            if (playerPictureBox.Bounds.IntersectsWith(KeyToDoor.Bounds)) {
                KeyStatus.Text = "You now have the Key!";
                FinnHasKey = true;
                KeyToDoor.Visible = false;
                KeyDoorStatus.Text = "";
            }

            if (playerPictureBox.Bounds.IntersectsWith(redSlimePictureBox.Bounds)) {
                if (redSlimeUsage == true) {
                    addHealth();
                    redSlimePictureBox.Image = Properties.Resources.Slime_2_hurt;
                    redSlimeUsage = false;
                }
            }
            if (playerPictureBox.Bounds.IntersectsWith(greenSlimePictureBox.Bounds)) {
                if (greenSlimeUsage == true) {
                    addStrength();
                    greenSlimePictureBox.Image = Properties.Resources.Slime_1_hurt;
                    greenSlimeUsage = false;
                }
            }
            if (playerPictureBox.Bounds.IntersectsWith(blueSlimePictureBox.Bounds)) {
                if (blueSlimeusage == true) {
                    addSpeed();
                    blueSlimePictureBox.Image = Properties.Resources.Slime_3_hurt;
                    blueSlimeusage = false;
                }
            }
        }

        private void DjangoVentures_Load(object sender, EventArgs e) {
            // Hello World :VV
            InitializeHealthPanel();
            InitializeStengthPanel();
            InitializeSpeedPanel();
        }
    }
}
