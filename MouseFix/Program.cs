using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseFixer
{
    internal class MouseFixerApp
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        static bool isActive = false;
        static bool wasKeyPressed = false;

        const int F2KeyCode = 0x71;

        private static async Task Main(string[] args)
        {
            Task keyCheck = Task.Run(() =>
            {
                while (true)
                {
                    bool f2KeyPressed = IsKeyPressed(F2KeyCode);

                    if (f2KeyPressed && !wasKeyPressed)
                    {
                        ToggleFixer();
                    }

                    wasKeyPressed = f2KeyPressed;
                    Thread.Sleep(100);
                }
            });

            await Task.Delay(500);

            Task mouseFix = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(500);

                    IntPtr hWnd = FindWindow(null, "Roblox");

                    if (hWnd == IntPtr.Zero)
                    {
                        ShowClosedMsg();
                        continue;
                    }
                    else
                    {
                        ShowOpenMsg();
                    }

                    if (isActive)
                    {
                        SetForegroundWindow(hWnd);

                        while (isActive)
                        {
                            IntPtr currentForegroundWindow = GetForegroundWindow();
                            if (currentForegroundWindow == hWnd)
                            {
                                RECT rect;

                                if (!GetWindowRect(hWnd, out rect))
                                {
                                    Console.WriteLine("No se pudo encontrar las dimensiones de la ventana. Por favor, abre Roblox.");
                                    Thread.Sleep(5000);
                                    continue;
                                }

                                int x = rect.Left + (rect.Right - rect.Left) / 2;
                                int y = rect.Top + (rect.Bottom - rect.Top) / 2;

                                Cursor.Position = new Point(x, y);
                            }

                            Thread.Sleep(10);
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            });

            await Task.WhenAll(keyCheck, mouseFix);
        }

        static void ToggleFixer()
        {
            isActive = !isActive;
        }

        static void ShowClosedMsg()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No se pudo encontrar Roblox.");
            Console.WriteLine("Por favor, abre Roblox desde la Microsoft Store.");
        }

        static void ShowOpenMsg()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("MouseFixer Roblox");
            Console.WriteLine("Presiona F2 para activar/desactivar");
            Console.WriteLine($"Estado: {(isActive ? "Activado" : "Desactivado")}");
            Console.WriteLine("\nhttps://discord.gg/tgx");
            Console.WriteLine("#TGXGANG");

        }

        static bool IsKeyPressed(int keyCode)
        {
            return (GetAsyncKeyState(keyCode) & 0x8000) != 0;
        }

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);
    }
}
