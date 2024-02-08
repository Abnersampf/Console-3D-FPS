using System.Drawing;

class Game
{
    // You can customize the map by placing numbers 1 as walls or objects.
    // 0 = space where you can walk;
    // 1 = walls you can't go through.
    // IMPORTANT: The horizontal side always has to be the larger one
    static int[,] Map = { { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                          { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                          { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                          { 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 1 },
                          { 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                          { 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                          { 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                          { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                          { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 1 },
                          { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                          { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 } };

    static int MapWidth = Map.GetLength(0), MapLength = Map.GetLength(1);
    // Here you define how many times you want to increase the size of the map (only for visualization, the map doesn't actually increase).
    static int Upscale = 3;
    static int WindowWidth = MapLength * Upscale, WindowHeight = MapWidth * Upscale;
    // You can change "ViewDistance" variable.
    static int ViewDistance = MapLength;
    static int TotalAngles = (MapLength * 2) + (MapWidth - 2) * 2;
    // You can change "FOV" variable.
    static int FOV = 90;
    int _45Angle = ((FOV / 2) * TotalAngles) / 360, _90Angle = (FOV * TotalAngles) / 360;
    // You can change X's and Y's variables.
    // x1 and y1 are the coordinates of the camera position
    // x2 and y2 are the coordinates of where the camera is looking
    int X1 = MapWidth / 2, Y1 = MapLength / 2, X2 = 0, Y2 = 20;
    int NextX1, NextY1, LastObjX = -1, LastObjY = -1;
    string LineView = "";

    ConsoleKey Key;

    static void Main()
    {
        Console.CursorVisible = false;
        Game game = new Game();
        while (true)
        {
            // Draws the game on the screen, turns 2D into "3D".
            game.DrawGame();

            // Draw a 'O' where the camera is.
            Console.SetCursorPosition(game.Y1 + WindowWidth, game.X1);
            Console.Write('O');
            // Draw a 'v' where the camera is looking.
            Console.SetCursorPosition(game.Y2 + WindowWidth, game.X2);
            Console.Write('+');

            // The next screen update will only happen when the user moves the camera (by pressing the right, left or up arrow).
            game.Key = Console.ReadKey().Key;
            game.MoveCamera();

            Console.Clear();
        }
    }
    void Vision()
    {
        int tempX2 = X2, tempY2 = Y2;
        int differenceX = Math.Max(X1, X2) - Math.Min(X1, X2);
        int differenceY = Math.Max(Y1, Y2) - Math.Min(Y1, Y2);
        int max = Math.Max(differenceX, differenceY);
        max = (max > _45Angle) ? _45Angle : max;

        // Turn the camera 45° counterclockwise (or another value depending on the angle chosen)
        for (int skip = 0; skip < max; skip++)
        {
            Key = ConsoleKey.LeftArrow;
            MoveCamera();
        }

        max = (max * 2) + 1;
        max = (max > _90Angle) ? _90Angle : max;

        // This loop is responsible for drawing muiltiple lines that form the complete view of the camera.
        for (int line = 0; line < max; line++)
        {
            // Variables to set up the equation of the line.
            // m is the slope/gradient.
            double m = (double)(Y2 - Y1) / (X2 - X1);
            double b = Y1 - (m * X1);
            // b is the Y Intercept.
            bool negX = X1 > X2, negY = Y1 > Y2;

            // The number with the biggest difference will be i.
            differenceX = Math.Max(X1, X2) - Math.Min(X1, X2);
            differenceY = Math.Max(Y1, Y2) - Math.Min(Y1, Y2);

            // Here we define i, stop (when the i stops), and the step (if is negative or not) variables.
            int i = X1, j = Y1;
            int stop = (negX) ? X2 - 1 : X2 + 1;
            int stepI = (negX) ? -1 : 1;
            int stepJ = (negY) ? -1 : 1;

            int left = Y1, top = X1;

            if (differenceY > differenceX)
            {
                i = Y1;
                j = X1;

                stop = (negY) ? Y2 - 1 : Y2 + 1;
                stepI = (negY) ? -1 : 1;
                stepJ = (negX) ? -1 : 1;

                left = X1; top = Y1;
            }

            int c = 0;

            // This loop is responsible for drawing a single line between x1, y1 and x2, y2.
            for (; i != stop; i += stepI)
            {
                // Checks if "left" and "top" variables should be i or j respectively.  
                if (differenceX > differenceY)
                {
                    if (differenceX > 0)
                        j = (int)Math.Round((m * i) + b);
                    left = j; top = i;
                }
                else if (differenceY > differenceX)
                {
                    if (differenceX > 0)
                        j = (int)Math.Round((i - b) / m);
                    left = i; top = j;
                }
                else
                {
                    if (c > 0)
                    {
                        j += stepJ;
                        if (negX | negY || !negX | !negY)
                        {
                            left = j; top = i;
                        }
                    }
                }

                // Saves the coordinates of the next camera MOVEMENT (NOT ROTATION).
                if (Y2 == tempY2 && X2 == tempX2 && c == 1)
                {
                    NextX1 = top; NextY1 = left;
                }

                // Makes the loop stop drawing the line according to the viewing distance or if it finds an object on the map.  
                if (Map[top, left] == 1)
                {
                    if (top != LastObjX || left != LastObjY)
                        // Creates a string with the distance of each line from the wall    
                        LineView += $"{c} ";
                    LastObjX = top; LastObjY = left;
                    break;
                }
                else if (c == ViewDistance)
                {
                    LineView += $"{c} ";
                    break;
                }

                Console.SetCursorPosition(left + WindowWidth, top);
                Console.Write('.');

                c++;
            }
            // Moves x2 and y2 to form a new line.
            Key = ConsoleKey.RightArrow;
            MoveCamera();
        }
        X2 = tempX2; Y2 = tempY2;
    }

    void DrawGame()
    {
        LineView = "";
        Vision();
        LineView = LineView.Substring(0, LineView.Length - 1);
        int[] LineViewArray = LineView.Split(' ').Select(int.Parse).ToArray();
        //Console.SetCursorPosition(0, 0);
        //Console.WriteLine(LineView);
        double c = 0;
        double change = (double)LineViewArray.Length / WindowWidth;
        int piece, middle = WindowHeight / 2, max = WindowHeight - (LineViewArray.Min() * Upscale);
        // Draw each of the columns
        for (int i = 0; i < WindowWidth; i++)
        {
            piece = WindowHeight - (LineViewArray[(int)c] * Upscale);
            // Draw each pixel in the column
            for (int j = middle - (piece / 2); j < middle + (max / 2); j++)
            {
                // Color the column according to its distance from the camera
                if (j < middle + (piece / 2))
                {
                    if (piece < WindowHeight / 3)
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                    else if (piece < WindowHeight / 2)
                        Console.BackgroundColor = ConsoleColor.Gray;
                    else if (piece < WindowHeight)
                        Console.BackgroundColor = ConsoleColor.White;
                    Console.SetCursorPosition(i, j);
                    Console.Write(' ');
                }
                // Draw the floor
                else
                {
                    Console.SetCursorPosition(i, j);
                    Console.Write('.');
                }
                Console.BackgroundColor = ConsoleColor.Black;
            }
            c += change;
        }
    }

    // The name of this method speaks for itself
    void MoveCamera()
    {
        int step = -1;
        if (Key == ConsoleKey.RightArrow || Key == ConsoleKey.LeftArrow)
        {
            if (Key == ConsoleKey.RightArrow)
                step = 1;

            if (X2 == 0 && Y2 == 0)
            {
                if (step == 1)
                    Y2++;
                else
                    X2++;
            }
            else if (X2 == 0 && Y2 == MapLength - 1)
            {
                if (step == 1)
                    X2 += step;
                else
                    Y2 += step;
            }
            else if (X2 == MapWidth - 1 && Y2 == MapLength - 1)
            {
                if (step == 1)
                    Y2--;
                else
                    X2--;
            }
            else if (X2 == MapWidth - 1 && Y2 == 0)
            {
                if (step == 1)
                    X2 -= step;
                else
                    Y2 -= step;
            }
            else if (X2 == 0 && Y2 > 0 && Y2 < MapLength - 1)
                Y2 += step;
            else if (Y2 == MapLength - 1 && X2 > 0 && X2 < MapWidth - 1)
                X2 += step;
            else if (X2 == MapWidth - 1 && Y2 > 0 && Y2 < MapLength - 1)
                Y2 -= step;
            else if (Y2 == 0 && X2 > 0 && X2 < MapWidth - 1)
                X2 -= step;
        }
        else if (Key == ConsoleKey.UpArrow)
            if (Map[NextX1, NextY1] != 1)
            {
                X1 = NextX1; Y1 = NextY1;
            }
    }
}
