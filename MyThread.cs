using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;

namespace ThreadDemo
{
    internal class MyThread
    {
        const double PI = 3.1416;

        public Thread t;
        public Boolean start;
        public Boolean stop;
        public Boolean suspend;
        public Boolean WaitOne = false;

        public Bitmap Pic;

        internal int Xmax;
        internal int Ymax;

        public Point Pos;

        double dblGocChay;
        internal double tx, ty;

        // Khoi tao Thread
        public MyThread(Random rnd, int xMax, int yMax)
        {
            if (rnd == null)
                throw new ArgumentNullException(nameof(rnd));

            if (xMax < 2 || yMax < 2)
                throw new ArgumentException(
                    "Vung di chuyen phai co chieu rong va cao >= 2.");

            Xmax = xMax;
            Ymax = yMax;

            Pos.X = rnd.Next(0, Xmax);
            Pos.Y = rnd.Next(0, Ymax);

            dblGocChay = ChinhGocChay(rnd.Next(0, 360));
        }

        // Chuan hoa goc di chuyen
        double ChinhGocChay(double goc)
        {
            goc = ((goc % 360) + 360) % 360;

            if (goc < 90) return 45;
            if (goc < 180) return 135;
            if (goc < 270) return 225;

            return 315;
        }

        // Phan xa khi cham canh trai hoac phai
        double DoiGocChayX(double goc)
        {
            return ChinhGocChay(180 - goc);
        }

        // Phan xa khi cham canh tren hoac duoi
        double DoiGocChayY(double goc)
        {
            return ChinhGocChay(360 - goc);
        }

        // Cap nhat vi tri Thread
        public void HieuchinhVitri()
        {
            // Tinh buoc di chuyen theo hai truc
            tx = 2 * Math.Cos(dblGocChay * PI / 180);
            ty = 2 * Math.Sin(dblGocChay * PI / 180);

            int dx = Math.Sign(tx);
            int dy = Math.Sign(ty);

            // Kiem tra va cham theo truc X
            int nextX = Pos.X + dx;

            if (nextX < 0 || nextX >= Xmax)
            {
                dblGocChay = DoiGocChayX(dblGocChay);

                tx = 2 * Math.Cos(dblGocChay * PI / 180);
                ty = 2 * Math.Sin(dblGocChay * PI / 180);

                dx = Math.Sign(tx);
                dy = Math.Sign(ty);
            }

            // Kiem tra va cham theo truc Y
            int nextY = Pos.Y + dy;

            if (nextY < 0 || nextY >= Ymax)
            {
                dblGocChay = DoiGocChayY(dblGocChay);

                tx = 2 * Math.Cos(dblGocChay * PI / 180);
                ty = 2 * Math.Sin(dblGocChay * PI / 180);

                dx = Math.Sign(tx);
                dy = Math.Sign(ty);
            }

            // Cap nhat vi tri moi
            int x = Pos.X + dx;
            int y = Pos.Y + dy;

            // Dam bao vi tri nam trong vung di chuyen
            x = Math.Max(0, Math.Min(x, Xmax - 1));
            y = Math.Max(0, Math.Min(y, Ymax - 1));

            Pos.X = x;
            Pos.Y = y;
        }
    }
}

