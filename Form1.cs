using System.IO;
using System.Threading;
using System.Resources;
using System.Drawing;

namespace ThreadDemo
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }
        Stream myStream;
        Mutex[,] mutList;
        MyThread[] threadLst;
        const int xCell = 30;
        const int yCell = 30;
        const int xMax = 25;
        const int yMax = 20;
        //tạo ₫ối tượng sinh số ngẫu nhiên
        public Random rnd = new Random();
        //₫ịnh nghĩa hàm giả lập hành vi của thread
        void MySleep(long count)
        {
            long i, j, k = 0;
            for (i = 0; i < count; i++)
                for (j = 0; j < 64000; j++) k = k + 1;
        }
        //₫ịnh nghĩa hàm mà mỗi thread sẽ chạy
        void Running(object obj)
        {
            //ép kiểu tham số về MyThread theo yêu cầu xử lý
            MyThread p = (MyThread)obj;
            //tạo ₫ối tượng vẽ
            Graphics g = this.CreateGraphics();
            //tạo chổi màu ₫en ₫ể xóa cell cũ
            Brush brush = new SolidBrush(Color.FromArgb(0, 0, 0));
            //xin khóa truy xuất cell (x1,y1)
            mutList[p.Pos.Y, p.Pos.X].WaitOne();
            int x1, y1;
            int x2, y2;
            int x, y;
            bool kq = true;
            try
            {
                while (p.start)
                { //lặp trong khi chưa có yêu cầu kết thúc
                  //xác ₫ịnh tọa ₫ộ hiện hành của thread
                    x1 = p.Pos.X; y1 = p.Pos.Y;
                    //hiển thị logo của thread ở (x1,y1)
                    g.DrawImage(p.Pic, xCell * x1, yCell * y1);
                    Color c = p.Pic.GetPixel(1, 1);
                    int yR, yG, yB;
                    if (c.R > 128) yR = 0; else yR = 255;
                    if (c.G > 128) yG = 0; else yG = 255;
                    if (c.B > 128) yB = 0; else yB = 255;
                    Pen pen = new Pen(Color.FromArgb(yR, yG, yB), 2);
                    if (p.tx >= 0 && p.ty >= 0)
                    { //hiện mũi tên góc dưới phải
                        x = xCell * x1 + xCell - 2;
                        y = yCell * y1 + yCell - 2;
                        g.DrawLine(pen, x, y, x - 10, y);
                        g.DrawLine(pen, x, y, x, y - 10);
                    }
                    else if (p.tx >= 0 && p.ty < 0)
                    { //hiện mũi tên góc trên phải
                        x = xCell * x1 + xCell - 2;
                        y = yCell * y1 + 2;
                        g.DrawLine(pen, x, y, x - 10, y);
                        g.DrawLine(pen, x, y, x, y + 10);
                    }
                    else if (p.tx < 0 && p.ty >= 0)
                    { //hiện mũi tên góc dưới trái
                        x = xCell * x1 + 2;
                        y = yCell * y1 + yCell - 2;
                        g.DrawLine(pen, x, y, x + 10, y);
                        g.DrawLine(pen, x, y, x, y - 10);
                    }
                    else
                    {//hiện mũi tên góc trên trái
                        x = xCell * x1 + 2;
                        y = yCell * y1 + 2;
                        g.DrawLine(pen, x, y, x + 10, y);
                        g.DrawLine(pen, x, y, x, y + 10);
                    }
                    //giả lập thực hiện công việc của thread tốn 500ms
                    MySleep(500);
                    //xác ₫ịnh vị trí mới của thread
                    p.HieuchinhVitri();
                    x2 = p.Pos.X; y2 = p.Pos.Y;
                    //xin khóa truy xuất cell (x2,y2)
                    mutList[y2, x2].WaitOne();  
                    // Xóa vị trí cũ
                    g.FillRectangle(brush, xCell * x1, yCell * y1, xCell, yCell);
                    //trả cell (x1,y1) cho các thread khác truy xuất
                    mutList[y1, x1].ReleaseMutex();
                    if (kq == false && p.start == false)
                    { //xoa thread
                      // dừng Thread
                        p.t.Abort();
                        p.stop = true;
                        return;
                    }
                }
            }
            catch (Exception e) { p.t.Abort(); }
            //dọn dẹp thread trước khi ngừng
            x1 = p.Pos.X; y1 = p.Pos.Y;
            g.FillRectangle(brush, xCell * x1, yCell * y1, xCell, yCell);
            //trả cell (x1,y1) cho các thread khác truy xuất
            mutList[y1, x1].ReleaseMutex(); 
            // dừng Thread
            p.stop = true;
            p.t.Abort();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Reflection.Assembly myAssembly =
        System.Reflection.Assembly.GetExecutingAssembly();

            threadLst = new MyThread[26];
            mutList = new Mutex[yMax, xMax];

            // Khoi tao Mutex cho tung o
            for (int y = 0; y < yMax; y++)
            {
                for (int x = 0; x < xMax; x++)
                {
                    mutList[y, x] = new Mutex();
                }
            }

            // Khoi tao 26 Thread tu A den Z
            for (int i = 0; i < 26; i++)
            {
                threadLst[i] = new MyThread(rnd, xMax, yMax);

                threadLst[i].stop = true;
                threadLst[i].suspend = false;
                threadLst[i].start = false;

                char c = (char)(i + 65);

                string resourceName =
                    "ThreadDemo.Resources.Image" + c + ".bmp";

                using (Stream myStream =
                    myAssembly.GetManifestResourceStream(resourceName))
                {
                    if (myStream != null)
                    {
                        using (Bitmap temp = new Bitmap(myStream))
                        {
                            threadLst[i].Pic = new Bitmap(temp);
                        }
                    }
                    else
                    {
                        // Tao anh thay the neu khong tim thay tai nguyen
                        Bitmap bmp = new Bitmap(30, 30);

                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.Clear(Color.DarkGray);
                            g.DrawString(
                                c.ToString(),
                                new Font("Arial", 12),
                                Brushes.White,
                                5, 5);
                        }

                        threadLst[i].Pic = bmp;
                    }
                }
            }

            ClientSize = new Size(xMax * xCell, yMax * yCell);
            Location = new Point(0, 0);
            BackColor = Color.Black;

            // Nhan phim tren Form
            KeyPreview = true;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            int index = e.KeyValue - 65;

            if (index < 0 || index >= 26)
                return;

            MyThread p = threadLst[index];

            // Ctrl + Shift: yeu cau dung Thread
            if (e.Control && e.Shift)
            {
                p.start = false;
            }
            // Ctrl + Alt: tam dung
            else if (e.Control && e.Alt)
            {
                MessageBox.Show(
                    "Nen thay Suspend/Resume bang co che tam dung an toan.");
            }
            // Ctrl: uu tien thap
            else if (e.Control)
            {
                if (p.t != null && p.t.IsAlive)
                {
                    p.t.Priority = ThreadPriority.Lowest;
                }
            }
            // Alt: tiep tuc
            else if (e.Alt)
            {
                MessageBox.Show(
                    "Can bo sung co che tiep tuc Thread an toan.");
            }
            // Shift: uu tien cao
            else if (e.Shift)
            {
                if (p.t != null && p.t.IsAlive)
                {
                    p.t.Priority = ThreadPriority.Highest;
                }
            }
            // Khong co phim dieu khien: chay Thread
            else
            {
                if (!p.start && (p.t == null || !p.t.IsAlive))
                {
                    p.start = true;
                    p.stop = false;

                    p.t = new Thread(Running);
                    p.t.IsBackground = true;

                    p.t.Priority = index == 0
                        ? ThreadPriority.Highest
                        : ThreadPriority.Lowest;

                    p.t.Start(p);
                }
            }

            e.Handled = true;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            int i;
            //lặp kiểm tra xem có thread con nào còn chạy không, nếu có thì xóa nó
            for (i = 0; i < 26; i++)
                if (threadLst[i].start)
                {
                    threadLst[i].start = false;
                    while (!threadLst[i].stop) ;
                }
        }
    }
}
