using Raylib_cs;
using static Raylib_cs.Raylib;
using System;

class Program
{
    const int screenWidth = 600;
    const int screenHeight = 750;
    const int cellSize = 200;

    static int[,] board = new int[3, 3];

    static int gameState = 0; // 0 = menu, 1 = game, 2 = results
    static int gameMode = 0;  // 0 = PvP, 1 = PvAI
    static int difficulty = 0; // 0 Easy, 1 Medium, 2 Impossible

    static int currentPlayer = 1;
    static int winner = 0;

    static int scoreX = 0;
    static int scoreO = 0;

    static float animProgress = 0f;
    static bool animating = false;
    static int animRow = -1;
    static int animCol = -1;

    static Random random = new Random();

    // ================= RESET =================

    static void ResetBoard()
    {
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                board[r, c] = 0;

        currentPlayer = 1;
        winner = 0;
    }

    // ================= WIN CHECK =================

    static bool CheckWin(int player)
    {
        for (int i = 0; i < 3; i++)
        {
            if (board[i, 0] == player && board[i, 1] == player && board[i, 2] == player)
                return true;

            if (board[0, i] == player && board[1, i] == player && board[2, i] == player)
                return true;
        }

        if (board[0, 0] == player && board[1, 1] == player && board[2, 2] == player)
            return true;

        if (board[0, 2] == player && board[1, 1] == player && board[2, 0] == player)
            return true;

        return false;
    }

    static bool IsBoardFull()
    {
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                if (board[r, c] == 0)
                    return false;

        return true;
    }

    // ================= AI =================

    static void MakeRandomMove()
    {
        int r, c;
        do
        {
            r = random.Next(0, 3);
            c = random.Next(0, 3);
        }
        while (board[r, c] != 0);

        board[r, c] = 2;
        animRow = r;
        animCol = c;
    }

    static void MakeMediumMove()
    {
        // Win if possible
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                if (board[r, c] == 0)
                {
                    board[r, c] = 2;
                    if (CheckWin(2))
                    {
                        animRow = r;
                        animCol = c;
                        return;
                    }
                    board[r, c] = 0;
                }

        // Block player
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                if (board[r, c] == 0)
                {
                    board[r, c] = 1;
                    if (CheckWin(1))
                    {
                        board[r, c] = 2;
                        animRow = r;
                        animCol = c;
                        return;
                    }
                    board[r, c] = 0;
                }

        MakeRandomMove();
    }

    static int Minimax(bool isMax)
    {
        if (CheckWin(2)) return 1;
        if (CheckWin(1)) return -1;
        if (IsBoardFull()) return 0;

        if (isMax)
        {
            int best = -1000;
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    if (board[r, c] == 0)
                    {
                        board[r, c] = 2;
                        int score = Minimax(false);
                        board[r, c] = 0;
                        if (score > best) best = score;
                    }
            return best;
        }
        else
        {
            int best = 1000;
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    if (board[r, c] == 0)
                    {
                        board[r, c] = 1;
                        int score = Minimax(true);
                        board[r, c] = 0;
                        if (score < best) best = score;
                    }
            return best;
        }
    }

    static void MakeImpossibleMove()
    {
        int bestScore = -1000;
        int bestR = 0;
        int bestC = 0;

        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                if (board[r, c] == 0)
                {
                    board[r, c] = 2;
                    int score = Minimax(false);
                    board[r, c] = 0;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestR = r;
                        bestC = c;
                    }
                }

        board[bestR, bestC] = 2;
        animRow = bestR;
        animCol = bestC;
    }

    // ================= GAME LOGIC =================

    static void AfterMove()
    {
        animating = true;
        animProgress = 0f;

        if (CheckWin(currentPlayer))
        {
            winner = currentPlayer;
            if (winner == 1) scoreX++;
            else scoreO++;
            gameState = 2;
        }
        else if (IsBoardFull())
        {
            winner = 0;
            gameState = 2;
        }
        else
        {
            currentPlayer = currentPlayer == 1 ? 2 : 1;
        }
    }

    static void HandleGame()
    {
        if (gameMode == 1 && currentPlayer == 2)
        {
            if (difficulty == 0) MakeRandomMove();
            else if (difficulty == 1) MakeMediumMove();
            else MakeImpossibleMove();

            AfterMove();
            return;
        }

        if (IsMouseButtonPressed(MouseButton.Left))
        {
            int col = GetMouseX() / cellSize;
            int row = GetMouseY() / cellSize;

            if (row < 3 && col < 3 && board[row, col] == 0)
            {
                board[row, col] = currentPlayer;
                animRow = row;
                animCol = col;
                AfterMove();
            }
        }
    }

    // ================= DRAW =================

    static void DrawBoard()
    {
        for (int i = 1; i < 3; i++)
        {
            DrawLine(i * cellSize, 0, i * cellSize, 600, Color.Black);
            DrawLine(0, i * cellSize, 600, i * cellSize, Color.Black);
        }
    }

    static void DrawSymbols()
    {
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                if (board[r, c] != 0)
                {
                    int centerX = c * cellSize + 100;
                    int centerY = r * cellSize + 100;

                    bool isAnim = (r == animRow && c == animCol && animating);
                    float progress = isAnim ? animProgress : 1f;

                    if (board[r, c] == 1)
                    {
                        int size = 60;

                        DrawLine(centerX - size, centerY - size,
                                 centerX - size + (int)(2 * size * progress),
                                 centerY - size + (int)(2 * size * progress),
                                 Color.Blue);

                        DrawLine(centerX + size, centerY - size,
                                 centerX + size - (int)(2 * size * progress),
                                 centerY - size + (int)(2 * size * progress),
                                 Color.Blue);
                    }
                    else
                    {
                        DrawCircleLines(centerX, centerY, 60 * progress, Color.Green);
                    }
                }
            }
        }
    }

    static void DrawHover()
    {
        if (gameState != 1 || winner != 0) return;

        int col = GetMouseX() / cellSize;
        int row = GetMouseY() / cellSize;

        if (row < 3 && col < 3 && board[row, col] == 0)
        {
            Color preview = currentPlayer == 1
                ? new Color(0, 0, 255, 100)
                : new Color(0, 200, 0, 100);

            DrawText(currentPlayer == 1 ? "X" : "O",
                col * cellSize + 80,
                row * cellSize + 60,
                80,
                preview);
        }
    }

    static void DrawGame()
    {
        ClearBackground(Color.RayWhite);
        DrawBoard();
        DrawSymbols();
        DrawHover();

        DrawText("X: " + scoreX, 50, 650, 30, Color.Blue);
        DrawText("O: " + scoreO, 450, 650, 30, Color.Green);
    }

    static void DrawResults()
    {
        ClearBackground(Color.RayWhite);

        DrawText("JATEK VEGE", 180, 150, 40, Color.Black);

        if (winner == 0)
            DrawText("Dontetlen!", 220, 230, 30, Color.Red);
        else
            DrawText("Nyertes: " + (winner == 1 ? "X" : "O"), 200, 230, 30, Color.Red);

        DrawText("Allas: X " + scoreX + " - O " + scoreO,
            170, 300, 30, Color.Blue);

        DrawText("ENTER - Uj jatek", 170, 500, 25, Color.DarkGreen);
        DrawText("M - Menu", 170, 540, 25, Color.DarkGreen);
    }

    static void DrawMenu()
    {
        ClearBackground(Color.RayWhite);

        DrawText("AMOBA", 220, 180, 50, Color.Black);
        DrawText("1 - PvP", 220, 280, 30, Color.Blue);
        DrawText("2 - PvAI", 220, 320, 30, Color.Blue);
        DrawText("E - Easy", 220, 380, 25, Color.DarkGreen);
        DrawText("M - Medium", 220, 410, 25, Color.DarkGreen);
        DrawText("I - Impossible", 220, 440, 25, Color.DarkGreen);
        DrawText("SPACE - Start", 220, 500, 30, Color.Red);
    }

    // ================= UPDATE =================

    static void Update()
    {
        if (animating)
        {
            animProgress += 0.08f;
            if (animProgress >= 1f)
            {
                animProgress = 1f;
                animating = false;
            }
        }

        if (gameState == 0)
        {
            if (IsKeyPressed(KeyboardKey.One)) gameMode = 0;
            if (IsKeyPressed(KeyboardKey.Two)) gameMode = 1;
            if (IsKeyPressed(KeyboardKey.E)) difficulty = 0;
            if (IsKeyPressed(KeyboardKey.M)) difficulty = 1;
            if (IsKeyPressed(KeyboardKey.I)) difficulty = 2;

            if (IsKeyPressed(KeyboardKey.Space))
            {
                ResetBoard();
                gameState = 1;
            }
        }
        else if (gameState == 1)
        {
            HandleGame();
        }
        else
        {
            if (IsKeyPressed(KeyboardKey.Enter))
            {
                ResetBoard();
                gameState = 1;
            }

            if (IsKeyPressed(KeyboardKey.M))
                gameState = 0;
        }
    }

    static void Main()
    {
        InitWindow(screenWidth, screenHeight, "Amoba");
        SetTargetFPS(60);

        while (!WindowShouldClose())
        {
            BeginDrawing();
            Update();

            if (gameState == 0)
                DrawMenu();
            else if (gameState == 1)
                DrawGame();
            else
                DrawResults();

            EndDrawing();
        }

        CloseWindow();
    }
}