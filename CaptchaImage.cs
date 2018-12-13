using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;

namespace TCaptcha
{
    public class CaptchaImage
    {
        private int _height;

        private int _width;

        private Random _rand;

        private DateTime _generatedAt;

        private string _randomText;

        private int _randomTextLength;

        private string _randomTextChars;

        private string _fontFamilyName;

        private CaptchaImage.FontWarpFactor _fontWarp;

        private CaptchaImage.BackgroundNoiseLevel _backgroundNoise;

        private CaptchaImage.LineNoiseLevel _lineNoise;

        private string _guid;

        private string _fontWhitelist;

        private Color _backColor = Color.White;

        private Color _fontColor = Color.Black;

        private Color _noiseColor = Color.Black;

        private Color _lineColor = Color.Black;

        public Color BackColor
        {
            get
            {
                return this._backColor;
            }
            set
            {
                this._backColor = value;
            }
        }

        public CaptchaImage.BackgroundNoiseLevel BackgroundNoise
        {
            get
            {
                return this._backgroundNoise;
            }
            set
            {
                this._backgroundNoise = value;
            }
        }

        public string Font
        {
            get
            {
                return this._fontFamilyName;
            }
            set
            {
                Font font = null;
                try
                {
                    try
                    {
                        font = new Font(value, 12f);
                        this._fontFamilyName = value;
                    }
                    catch (Exception)
                    {
                        this._fontFamilyName = FontFamily.GenericSerif.Name;
                    }
                }
                finally
                {
                    font.Dispose();
                }
            }
        }

        public Color FontColor
        {
            get
            {
                return this._fontColor;
            }
            set
            {
                this._fontColor = value;
            }
        }

        public CaptchaImage.FontWarpFactor FontWarp
        {
            get
            {
                return this._fontWarp;
            }
            set
            {
                this._fontWarp = value;
            }
        }

        public string FontWhitelist
        {
            get
            {
                return this._fontWhitelist;
            }
            set
            {
                this._fontWhitelist = value;
            }
        }

        public int Height
        {
            get
            {
                return this._height;
            }
            set
            {
                if (value <= 20)
                {
                    throw new ArgumentOutOfRangeException("height", (object)value, "height must be greater than 20.");
                }
                this._height = value;
            }
        }

        public Color LineColor
        {
            get
            {
                return this._lineColor;
            }
            set
            {
                this._lineColor = value;
            }
        }

        public CaptchaImage.LineNoiseLevel LineNoise
        {
            get
            {
                return this._lineNoise;
            }
            set
            {
                this._lineNoise = value;
            }
        }

        public Color NoiseColor
        {
            get
            {
                return this._noiseColor;
            }
            set
            {
                this._noiseColor = value;
            }
        }

        public DateTime RenderedAt
        {
            get
            {
                return this._generatedAt;
            }
        }

        public string Text
        {
            get
            {
                return this._randomText;
            }
        }

        public string TextChars
        {
            get
            {
                return this._randomTextChars;
            }
            set
            {
                this._randomTextChars = value;
                this._randomText = this.GenerateRandomText();
            }
        }

        public int TextLength
        {
            get
            {
                return this._randomTextLength;
            }
            set
            {
                this._randomTextLength = value;
                this._randomText = this.GenerateRandomText();
            }
        }

        public string UniqueId
        {
            get
            {
                return this._guid;
            }
        }

        public int Width
        {
            get
            {
                return this._width;
            }
            set
            {
                if (value <= 60)
                {
                    throw new ArgumentOutOfRangeException("width", (object)value, "width must be greater than 60.");
                }
                this._width = value;
            }
        }

        public CaptchaImage()
        {
            this._rand = new Random();
            this._fontWarp = CaptchaImage.FontWarpFactor.Low;
            this._backgroundNoise = CaptchaImage.BackgroundNoiseLevel.Low;
            this._lineNoise = CaptchaImage.LineNoiseLevel.None;
            this._width = 180;
            this._height = 40;
            this._randomTextLength = 5;            
            this._fontFamilyName = "";
            this._fontWhitelist = "arial;courier new;estrangelo edessa;lucida console;lucida sans unicode;mangal;palatino linotype;sylfaen;tahoma;trebuchet ms;verdana";
            this._randomText = this.GenerateRandomText();
            this._generatedAt = DateTime.Now;
            this._guid = Guid.NewGuid().ToString();
        }

        private void AddLine(Graphics graphics1, Rectangle rect)
        {
            int num = 0;
            float single = 1f;
            int num1 = 0;
            switch (this._lineNoise)
            {
                case CaptchaImage.LineNoiseLevel.None:
                    {
                        return;
                    }
                case CaptchaImage.LineNoiseLevel.Low:
                    {
                        num = 4;
                        single = Convert.ToSingle((double)this._height / 33.25);
                        num1 = 1;
                        break;
                    }
                case CaptchaImage.LineNoiseLevel.Medium:
                    {
                        num = 5;
                        single = Convert.ToSingle((double)this._height / 29.7777);
                        num1 = 1;
                        break;
                    }
                case CaptchaImage.LineNoiseLevel.High:
                    {
                        num = 3;
                        single = Convert.ToSingle(this._height / 27);
                        num1 = 2;
                        break;
                    }
                case CaptchaImage.LineNoiseLevel.Extreme:
                    {
                        num = 3;
                        single = Convert.ToSingle((double)this._height / 24.7272);
                        num1 = 3;
                        break;
                    }
            }
            PointF[] pointFArray = new PointF[num + 1];
            using (Pen pen = new Pen(this._lineColor, single))
            {
                for (int i = 1; i <= num1; i++)
                {
                    for (int j = 0; j <= num; j++)
                    {
                        pointFArray[j] = this.RandomPoint(rect);
                    }
                    graphics1.DrawCurve(pen, pointFArray, 1.75f);
                }
            }
        }

        private void AddNoise(Graphics graphics1, Rectangle rect)
        {
            int num = 0;
            int num1 = 0;
            switch (this._backgroundNoise)
            {
                case CaptchaImage.BackgroundNoiseLevel.None:
                    {
                        return;
                    }
                case CaptchaImage.BackgroundNoiseLevel.Low:
                    {
                        num = 30;
                        num1 = 40;
                        break;
                    }
                case CaptchaImage.BackgroundNoiseLevel.Medium:
                    {
                        num = 18;
                        num1 = 40;
                        break;
                    }
                case CaptchaImage.BackgroundNoiseLevel.High:
                    {
                        num = 16;
                        num1 = 39;
                        break;
                    }
                case CaptchaImage.BackgroundNoiseLevel.Extreme:
                    {
                        num = 12;
                        num1 = 38;
                        break;
                    }
            }
            using (SolidBrush solidBrush = new SolidBrush(this._noiseColor))
            {
                int num2 = Convert.ToInt32(Math.Max(rect.Width, rect.Height) / num1);
                for (int i = 0; i <= Convert.ToInt32(rect.Width * rect.Height / num); i++)
                {
                    graphics1.FillEllipse(solidBrush, this._rand.Next(rect.Width), this._rand.Next(rect.Height), this._rand.Next(num2), this._rand.Next(num2));
                }
            }
        }

        private Bitmap GenerateImagePrivate()
        {
            Font font = null;
            Bitmap bitmap = new Bitmap(this._width, this._height, PixelFormat.Format32bppArgb);
            using (Graphics graphic = Graphics.FromImage(bitmap))
            {
                graphic.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rectangle = new Rectangle(0, 0, this._width, this._height);
                SolidBrush solidBrush = new SolidBrush(this._backColor);
                Brush brush = solidBrush;
                using (solidBrush)
                {
                    graphic.FillRectangle(brush, rectangle);
                }
                int num = 0;
                double num1 = (this._width / this._randomTextLength);
                SolidBrush solidBrush1 = new SolidBrush(this._fontColor);
                brush = solidBrush1;
                using (solidBrush1)
                {
                    string str = this._randomText;
                    for (int i = 0; i < str.Length; i++)
                    {
                        char chr = str[i];
                        Font font1 = this.GetFont();
                        font = font1;
                        using (font1)
                        {
                            Rectangle rectangle1 = new Rectangle(Convert.ToInt32((double)num * num1), 0, Convert.ToInt32(num1), this._height);
                            using (GraphicsPath graphicsPath = this.TextPath(chr.ToString(), font, rectangle1))
                            {
                                this.WarpText(graphicsPath, rectangle1);
                                graphic.FillPath(brush, graphicsPath);
                            }
                        }
                        num++;
                    }
                }
                this.AddNoise(graphic, rectangle);
                this.AddLine(graphic, rectangle);
            }
            return bitmap;
        }

        private string GenerateRandomText()
        {
            return Guid.NewGuid().ToString("n").Substring(0, this._randomTextLength).ToUpper();
        }

        private Font GetFont()
        {
            float num = 0f;
            string str = this._fontFamilyName;
            if (str == "")
            {
                str = this.RandomFontFamily();
            }
            switch (this.FontWarp)
            {
                case CaptchaImage.FontWarpFactor.None:
                    {
                        num = Convert.ToInt32((double)this._height * 0.7);
                        break;
                    }
                case CaptchaImage.FontWarpFactor.Low:
                    {
                        num = Convert.ToInt32((double)this._height * 0.8);
                        break;
                    }
                case CaptchaImage.FontWarpFactor.Medium:
                    {
                        num = Convert.ToInt32((double)this._height * 0.85);
                        break;
                    }
                case CaptchaImage.FontWarpFactor.High:
                    {
                        num = Convert.ToInt32((double)this._height * 0.9);
                        break;
                    }
                case CaptchaImage.FontWarpFactor.Extreme:
                    {
                        num = Convert.ToInt32((double)this._height * 0.95);
                        break;
                    }
            }
            return new Font(str, num, FontStyle.Bold);
        }

        private string RandomFontFamily()
        {
            string[] strArrays = null;
            if (strArrays == null)
            {
                strArrays = this._fontWhitelist.Split(new char[] { ';' });
            }
            return strArrays[this._rand.Next(0, strArrays.Length)];
        }

        private PointF RandomPoint(int xmin, int xmax, int ymin, int ymax)
        {
            return new PointF((float)this._rand.Next(xmin, xmax), (float)this._rand.Next(ymin, ymax));
        }

        private PointF RandomPoint(Rectangle rect)
        {
            return this.RandomPoint(rect.Left, rect.Width, rect.Top, rect.Bottom);
        }

        public Bitmap RenderImage()
        {
            return this.GenerateImagePrivate();
        }

        private GraphicsPath TextPath(string s, Font f, Rectangle r)
        {
            StringFormat stringFormat = new StringFormat()
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near
            };
            GraphicsPath graphicsPath = new GraphicsPath();
            graphicsPath.AddString(s, f.FontFamily, (int)f.Style, f.Size, r, stringFormat);
            return graphicsPath;
        }

        private void WarpText(GraphicsPath textPath, Rectangle rect)
        {
            float single = 1f;
            float single1 = 1f;
            switch (this._fontWarp)
            {
                case CaptchaImage.FontWarpFactor.None:
                    {
                        return;
                    }
                case CaptchaImage.FontWarpFactor.Low:
                    {
                        single = 6f;
                        single1 = 1f;
                        break;
                    }
                case CaptchaImage.FontWarpFactor.Medium:
                    {
                        single = 5f;
                        single1 = 1.3f;
                        break;
                    }
                case CaptchaImage.FontWarpFactor.High:
                    {
                        single = 4.5f;
                        single1 = 1.4f;
                        break;
                    }
                case CaptchaImage.FontWarpFactor.Extreme:
                    {
                        single = 4f;
                        single1 = 1.5f;
                        break;
                    }
            }
            RectangleF rectangleF = new RectangleF(Convert.ToSingle(rect.Left), 0f, Convert.ToSingle(rect.Width), (float)rect.Height);
            int num = Convert.ToInt32((float)rect.Height / single);
            int num1 = Convert.ToInt32((float)rect.Width / single);
            int left = rect.Left - Convert.ToInt32((float)num1 * single1);
            int top = rect.Top - Convert.ToInt32((float)num * single1);
            int width = rect.Left + rect.Width + Convert.ToInt32((float)num1 * single1);
            int height = rect.Top + rect.Height + Convert.ToInt32((float)num * single1);
            if (left < 0)
            {
                left = 0;
            }
            if (top < 0)
            {
                top = 0;
            }
            if (width > this.Width)
            {
                width = this.Width;
            }
            if (height > this.Height)
            {
                height = this.Height;
            }
            PointF pointF = this.RandomPoint(left, left + num1, top, top + num);
            PointF pointF1 = this.RandomPoint(width - num1, width, top, top + num);
            PointF pointF2 = this.RandomPoint(left, left + num1, height - num, height);
            PointF pointF3 = this.RandomPoint(width - num1, width, height - num, height);
            PointF[] pointFArray = new PointF[] { pointF, pointF1, pointF2, pointF3 };
            Matrix matrix = new Matrix();
            matrix.Translate(0f, 0f);
            textPath.Warp(pointFArray, rectangleF, matrix, WarpMode.Perspective, 0f);
        }

        public enum BackgroundNoiseLevel
        {
            None,
            Low,
            Medium,
            High,
            Extreme
        }

        public enum FontWarpFactor
        {
            None,
            Low,
            Medium,
            High,
            Extreme
        }

        public enum LineNoiseLevel
        {
            None,
            Low,
            Medium,
            High,
            Extreme
        }
    }
}
