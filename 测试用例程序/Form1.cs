using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 测试用例程序
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
          
            this.richTextBox2.Text = string.Empty;
            //中文处理
            var voictTextDateTimeHelper = new VoiceTextDateTimeHelper();

         // var tempdate=  voictTextDateTimeHelper.CurWeekDay(DayOfWeek.Monday, DateTime.Now, 1);
            var needDealStr = this.richTextBox1.Text;
            needDealStr = voictTextDateTimeHelper.DateChineseToNum(needDealStr);
            ///冒号处理
            needDealStr = voictTextDateTimeHelper.DateTimeHourseWhiteSpanFilterDeal(needDealStr);
            //中文处理
            needDealStr = voictTextDateTimeHelper.DateTimeWhiteSpanFilterDeal(needDealStr);

         


            this.richTextBox2.Text = needDealStr+"\n\r";
            Regex reg = new Regex(string.Format(@"{0}", this.textBox1.Text ));
            var matches = reg.Matches(needDealStr);
            foreach (var match in matches)
            {
                this.richTextBox2.Text = this.richTextBox2.Text+ match.ToString()+"\n";
            }
            ///空格缩进 冒号处理
            
         
            var result = voictTextDateTimeHelper.GetDateTimeStrForAlert(this.richTextBox1.Text);
            this.richTextBox2.Text= this.richTextBox2.Text  + result;



        }

        #region 语音字符串 语意日期提取 中文转化数字，零散时间补齐 正则时间提取
        public static string chineseWord = "零一二三四五六七八九1234567890";

        /// <summary>
        ///转化时间字符串钱空格过滤正则表达式，
        /// </summary>
        public const string dateTimeWhiteSpanFilter = @"(\w*[年,-,/])|(\w*[月,-,/])|(\w*[日,-,/, 号])|(\w*[:,点])|(\w*[:,分])|(\w*[秒])";
        //用于小时前面:的空格生成
        public const string dateTimeHourseWhiteSpanFilter = @"\s*((\d{1,2})|[一,二,三,四,五,六,七,八,九,十]{2,3})[:,点,时][\d{1,2}]*[:,分]?";
        /// <summary>
        /// 时间提取正则表达式
        /// </summary>
        public const string DateTimeFilter = @"(\d{2,4}[-,年,/,\.])?(\d{1,2}[-,/,月,\.])?(\d{1,2}[-,/,日,号]?)?\s*(\d{1,2}[:,点]?)?(\d{1,2}[:,分]?)?(\d{1,2})?";


        public Dictionary<string, string> CNStrDic = new Dictionary<string, string>() {
            //{ "十", "1" } ,{ "二十", "2" },{ "三十", "3" },
            //{ "四十", "4" }, { "五十", "5" },{ "六十", "6" },
            { "零", "0" }, { "一", "1" }, { "二", "2" }, { "三", "3" }, { "四", "4" },
            { "五", "5" }, { "六", "6" }, { "七", "7" }, { "八", "8" },
            { "九", "9" }
        };



        /// <summary>
        /// 几十几 十几 过滤
        /// </summary>
        public List<CTNCls> CTNStrList = new List<CTNCls>() {
            //只有一个十
            new CTNCls() { key="十",ruler=string.Format("[^{0}]十[^{0}]",chineseWord),value="10"},
            //只有整十 ，二十 三十 ....
            new CTNCls() { key="十",ruler=string.Format("[{0}]十[^{0}]",chineseWord),value="0"},
            //只有十N ，十一 十二 ....
            new CTNCls() { key="十",ruler=string.Format("[^{0}]十[{0}]",chineseWord),value="1"},
            //只有N十N ，二十一 二十二 ....
            new CTNCls() { key="十",ruler=string.Format("[{0}]十[{0}]",chineseWord),value=""},
          new CTNCls() { key="半",ruler=string.Format("[{0}]点半",chineseWord),value="30"},
         };

        public class CTNCls
        {
            public string key { get; set; }
            public string value { get; set; }
            public string ruler { get; set; }
        }

        /// <summary>
        /// 时间格式化
        /// </summary>
        public Dictionary<string, string> CNDateStrDic = new Dictionary<string, string>() {
            { "号", "日" },
            { "点", ":" }, { "时", ":" },{ "分", ":" },{ "秒", "" },

        };
        public string[] DateStrFormat = new string[] { "年", "月", "日", "-", "/", "." };

        /// <summary>
        /// 正则过滤十
        /// </summary>
        /// <param name="needDealStr"></param>
        /// <returns></returns>
        private string ReplaceTenStr(string needDealStr)
        {

            foreach (var dic in CTNStrList)
            {
                Regex reg = new Regex(string.Format(@"{0}", dic.ruler));
                foreach (var result in reg.Matches(needDealStr))
                {
                    var otherResult = result.ToString().Replace(dic.key, dic.value);
                    needDealStr = needDealStr.Replace(result.ToString(), otherResult);
                }

            }
            return needDealStr;

        }

        /// <summary>
        /// 转换时间为可以直接parse的时间
        /// </summary>
        /// <param name="dateStr"></param>
        /// <returns></returns>
        private string ReplaceDateStr(Dictionary<string, string> replaceDic, string needDealStr)
        {

            needDealStr = ReplaceTenStr(needDealStr);//整十过滤
            foreach (var dic in replaceDic)
            {
                if (needDealStr.Contains(dic.Key))
                {
                    needDealStr = needDealStr.Replace(dic.Key, dic.Value);
                }

            }

            return needDealStr.Trim('-').Trim(':');
        }
        /// <summary>
        /// 时间格式化
        /// </summary>
        /// <param name="needDealStr"></param>
        /// <returns></returns>
        public string DateToFormat(string needDealStr)
        {
            //最后返回可能有：
            return ReplaceDateStr(CNDateStrDic, needDealStr).Trim(':');
        }

        /// <summary>
        /// 中文数字格式化
        /// </summary>
        /// <param name="needDealStr"></param>
        /// <returns></returns>
        public string DateChineseToNum(string needDealStr)
        {
            needDealStr=ChineseTimeFix(needDealStr);
            ReplaceDateStr(CNStrDic, needDealStr);
           return ReplaceDateStr(CNStrDic, needDealStr);
        }


        /// <summary>
        /// 转化时间前进行冒号空格处理，进行空格缩进 冒号前置空格
        /// </summary>
        /// <param name="needDealStr"></param>
        /// <param name="regexStr"></param>
        /// <returns></returns>
        private string DateTimeWhiteSpanFilterDeal(string needDealStr)
        {
            //if (!string.IsNullOrEmpty(needDealStr) && !string.IsNullOrEmpty(dateTimeWhiteSpanFilter))
            //{
            //    Regex reg = new Regex(string.Format(@"{0}", dateTimeWhiteSpanFilter));
            //    var match = reg.Matches(needDealStr);
            //    foreach (var m in match)
            //    {
            //        var temp = m.ToString().Trim().Replace(" ","");
            //        needDealStr = needDealStr.Replace(m.ToString(), temp);
            //    }
            //    return needDealStr;
            //}
            //else
            //{
            //    return needDealStr;
            //}
            return needDealStr.Replace(" ", "");

        }

        /// <summary>
        ///  冒号前置空格
        /// </summary>
        /// <param name="needDealStr"></param>
        /// <param name="regexStr"></param>
        /// <returns></returns>
        private string DateTimeHourseWhiteSpanFilterDeal(string needDealStr)
        {
            if (!string.IsNullOrEmpty(needDealStr) && !string.IsNullOrEmpty(dateTimeHourseWhiteSpanFilter))
            {
                Regex reg = new Regex(string.Format(@"{0}", dateTimeHourseWhiteSpanFilter));
                var match = reg.Matches(needDealStr);
                foreach (var m in match)
                {
                    var temp = "|Y|" + m.ToString().Trim();
                    needDealStr = needDealStr.Replace(m.ToString(), temp);
                }
                return needDealStr.Replace(" ", "").Replace("|Y|", " ");
            }
            else
            {
                return needDealStr;
            }

        }
        /// <summary>
        /// 将残缺的日期进行修正展示
        /// </summary>
        /// <param name="dtStr"></param>
        /// <returns></returns>
        public string DateTimeStrFix(string dtStr)
        {
            // 传入的对象需要处理11年1日 12:20（可用）日期与时间拆分
            // 传入的对象需要处理        12:20（可用）日期与时间拆分
            // 传入的对象需要处理11年1日 12 （不可用）日期与时间拆分,需要加载对应的值
            // 10号只有该字段可能导致DateTime.TryParse转化为2015/1月10号
            try
            {
                DateTime date;
                Regex reg = new Regex(@"^\d{1,2}[号,日]$");
                if (reg.IsMatch(dtStr))
                {
                    dtStr = string.Format("{0}月{1}", DateTime.Now.Month, dtStr);
                }
                
                if (DateTime.TryParse(dtStr, out date))//转化成功
                {
                    return date.ToString();
                }
                else//转化失败
                {
                    var curNewDTStr = string.Empty;
                    var DateStrArray = dtStr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (DateStrArray.Count() >= 2)
                    {
                        var dateStr = DateStrArray[0];
                        if (!string.IsNullOrEmpty(dateStr))
                        {
                            curNewDTStr += DateStrTryFix(dateStr);

                        }
                        var timeStr = DateStrArray[1];
                        if (!string.IsNullOrEmpty(timeStr))
                        {
                            curNewDTStr += string.Format(" {0}", TimeStrTryFix(timeStr));
                        }
                        if (DateTime.TryParse(curNewDTStr, out date))//转化成功
                        {
                            return date.ToString();
                        }
                    }
                    else if (DateStrArray.Count() == 1)//单个需要翻译为 10点小事
                    {
                        if (DateStrFormat.Any(c => dtStr.Contains(c)))
                        {
                            curNewDTStr += DateStrTryFix(dtStr);
                         }
                        else
                        {
                          curNewDTStr += TimeStrTryFix(dtStr);
                         
                        }
                        if (DateTime.TryParse(curNewDTStr, out date))//转化成功
                        {
                            return date.ToString();
                        }

                    }


                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }

        }
        /// <summary>
        /// 尝试日期修复 2010-10-10  10-10 10 
        /// </summary>
        /// <returns></returns>
        private string DateStrTryFix(string dateStr)
        {
            DateTime tempDate;
            if (DateTime.TryParse(dateStr, out tempDate))//转化成功 17号
            {
                return tempDate.ToString("yyyy-MM-dd");//返回正确格式测试用1.1 12:20
            }
            else//失败一般为 如下10-10系统无法识别需要补齐,原则为由后往前补齐 15-12 需要识别为年而非月日
            {
                var temDateStr = dateStr;
                foreach (var format in DateStrFormat.Where(c => c != "-"))
                {
                    temDateStr = temDateStr.Replace(format, "-");
                }
                var splitArray = temDateStr.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (splitArray.Count() == 3)//年月日
                {
                    var fullDateStr = string.Format("{0}-{1}-{2}", splitArray[0], splitArray[1], splitArray[2]);
                    if (DateTime.TryParse(fullDateStr, out tempDate))//转化成功
                    {
                        return tempDate.ToString("yyyy-MM-dd");//返回正确格式测试用1.1 12:20
                    }
                }
                else if (splitArray.Count() == 2)//可能出错因为年2-4位 月12 日30最高
                {
                    try {
                        //年月 //月日 此处只考虑中国情况按循序年月日
                        var firstNum = int.Parse(splitArray[0]);
                        var secdNum = int.Parse(splitArray[1]);
                        var MonthDayStr = string.Format("{0}-{1}-{2}", DateTime.Now.Year, firstNum, secdNum);

                        if (firstNum <= 12 && DateTime.TryParse(MonthDayStr, out tempDate))//月日优先
                        {
                            return tempDate.ToString("yyyy-MM-dd");//返回正确格式测试用1.1 12:20
                        }
                        else//年月
                        {
                            MonthDayStr = string.Format("{0}-{1}-{2}", firstNum, secdNum, DateTime.Now.Day);
                            if (DateTime.TryParse(MonthDayStr, out tempDate))
                            {
                                return tempDate.ToString("yyyy-MM-dd");//返回正确格式测试用1.1 12:20
                            }
                        }
                    } catch (InvalidCastException ex)
                    {
                        return string.Empty;
                    }
                    catch (Exception ex)
                    {
                        return string.Empty;
                    }

                }

                return string.Empty;
            }

        }

        /// <summary>
        /// 尝试日期修复 12点需要转化成12:00
        /// </summary>
        /// <returns></returns>
        private string TimeStrTryFix(string timeStr)
        {
            DateTime tempDate;
            if (DateTime.TryParse(timeStr, out tempDate))//转化成功
            {
                return tempDate.ToString("HH:mm:ss");//返回正确格式测试用1.1 12:20
            }
            else//失败一般为单个 补齐即可
            {
                if (!timeStr.Contains(":"))
                {
                    timeStr = string.Format("{0}:00", timeStr);
                    if (DateTime.TryParse(timeStr, out tempDate))//转化成功
                    {
                        return tempDate.ToString("HH:mm:ss");//返回正确格式测试用1.1 12:20
                    }
                }

                return string.Empty;
            }

        }

        #endregion

        #region 中文语意时间替换 早上 中午 下午 晚上 凌晨 提前N天 早N天 N天后
        //早上1-5点是凌晨，
        //5-8点是早晨，
        //9-13点是中午，
        //14-18点是下午，
        //19-24点是晚上。
        /// <summary>
        /// 获取周一至周五对应天数
        /// </summary>
        /// <param name="week"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public string CurWeekDay(DayOfWeek week, DateTime now,int Round)
        {
            var curWeekDay = DateTime.Parse(string.Format("{0}-{1}-{2}", now.Year, now.Month, now.Day));
         
            if ((int)week >= (int)now.DayOfWeek)
            {
                var daySpan = Math.Abs((int)now.DayOfWeek - (int)week);
                if (daySpan > 0)
                {
                    curWeekDay= curWeekDay.AddDays(daySpan);
                }
            }
            else
            {
                var daySpan = ((int)week+7* Round)-(int)now.DayOfWeek;
                curWeekDay= curWeekDay.AddDays(daySpan);
            }

            return curWeekDay.ToString(" yyyy-MM-dd ");
        }
        /// <summary>
        /// 中文语意替换
        /// </summary>
        /// <returns></returns>
        public string ChineseTimeFix(string dealStrl)
        {
            var curDateTime = DateTime.Now;
            var year = curDateTime.Year;
            var month = curDateTime.Month;
            var day = curDateTime.Day;
           
            //周一至周末日数统计

            Dictionary<string, string> ChineseTimeStrDic = new Dictionary<string, string>()
            {
                 { "今年",string.Format("{0}年",year) }, { "本年",string.Format("{0}年",year) }, { "该年",string.Format("{0}年",year) },{ "这一年",string.Format("{0}年",year) },
                 { "下年",string.Format("{0}年",year+1) }, { "明年",string.Format("{0}年",year+1) }, { "下一年",string.Format("{0}年",year+1) },
                 { "后年",string.Format("{0}年",year+2) }, { "后一年",string.Format("{0}年",year+2) }, { "大后年",string.Format("{0}年",year+3) },
                 { "这个月",string.Format("{0}月",month) }, { "本月",string.Format("{0}月",month) }, { "该月",string.Format("{0}月",month) },
                 { "下个月",string.Format("{0}月",month+1) }, { "下月",string.Format("{0}月",month+1) },{ "下一月",string.Format("{0}月",month+1) },
                 { "今天",string.Format("{0}日",day) },  { "今早",string.Format("{0}日",day) }, { "今晚",string.Format("{0}日",day) },
                 { "明天",string.Format("{0}日",day+1) },  { "明早",string.Format("{0}日",day+1) }, { "明晚",string.Format("{0}日",day+1) },
                 { "大后天",string.Format("{0}日",day+3) },{ "后天",string.Format("{0}日",day+2) },
                  {"星期","周" },
                 { "下下周一",string.Format("{0}", CurWeekDay(DayOfWeek.Monday,curDateTime,2))},{ "下周一",string.Format("{0}", CurWeekDay(DayOfWeek.Monday,curDateTime,1))},{ "周一",string.Format("{0}", CurWeekDay(DayOfWeek.Monday,curDateTime,0))},
                 { "下下周二",string.Format("{0}", CurWeekDay(DayOfWeek.Tuesday,curDateTime,2))},{ "下周二",string.Format("{0}", CurWeekDay(DayOfWeek.Tuesday,curDateTime,1))},{ "周二",string.Format("{0}", CurWeekDay(DayOfWeek.Tuesday,curDateTime,0))},
                 { "下下周三",string.Format("{0}", CurWeekDay(DayOfWeek.Wednesday,curDateTime,2))},{ "下周三",string.Format("{0}", CurWeekDay(DayOfWeek.Wednesday,curDateTime,1))},{ "周三",string.Format("{0}", CurWeekDay(DayOfWeek.Wednesday,curDateTime,0))},
                 { "下下周四",string.Format("{0}", CurWeekDay(DayOfWeek.Thursday,curDateTime,2))},{ "下周四",string.Format("{0}", CurWeekDay(DayOfWeek.Tuesday,curDateTime,1))},{ "周四",string.Format("{0}", CurWeekDay(DayOfWeek.Tuesday,curDateTime,0))},
                 { "下下周五",string.Format("{0}", CurWeekDay(DayOfWeek.Friday,curDateTime,2))},{ "下周五",string.Format("{0}", CurWeekDay(DayOfWeek.Friday,curDateTime,1))},{ "周五",string.Format("{0}", CurWeekDay(DayOfWeek.Friday,curDateTime,0))},
                 { "下下周六",string.Format("{0}", CurWeekDay(DayOfWeek.Saturday,curDateTime,2))},{ "下周六",string.Format("{0}", CurWeekDay(DayOfWeek.Saturday,curDateTime,1))},{ "周六",string.Format("{0}", CurWeekDay(DayOfWeek.Saturday,curDateTime,0))},
                 { "下下周日",string.Format("{0}", CurWeekDay(DayOfWeek.Sunday,curDateTime,3))},{ "下周日",string.Format("{0}", CurWeekDay(DayOfWeek.Sunday,curDateTime,2))},{ "周日",string.Format("{0}", CurWeekDay(DayOfWeek.Sunday,curDateTime,1))},

            };
            return ReplaceDateStr(ChineseTimeStrDic, dealStrl);
             

            
        }
        #endregion

        private void button2_Click(object sender, EventArgs e)
        {
             var needDealStr = this.richTextBox1.Text;
             var regexStr = this.textBox2.Text;
            this.richTextBox2.Text = string.Empty;
            if (!string.IsNullOrEmpty(needDealStr)&&!string.IsNullOrEmpty(regexStr))
            {
                Regex reg = new Regex(string.Format(@"{0}", regexStr));
                var match = reg.Matches(needDealStr);
                foreach (var m in match)
                {
                      //var curDateFormat = DateToFormat(m.ToString());
                    var temp = "|Y|"+ m.ToString().Trim();
                     if(!string.IsNullOrEmpty(m.ToString()))
                    { needDealStr = needDealStr.Replace(m.ToString(), temp); } 
                    
                    this.richTextBox2.Text += string.Format("old:{0} new:{1} \n", m.ToString(),temp);
                }
                //this.richTextBox2.Text = needDealStr.Replace(" ","").Replace("|Y|"," ");
                
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
             
           var dateStr = this.textBox3.Text.Trim();
           dateStr = ChineseTimeFix(dateStr);
           var foramtDateStr = DateToFormat(dateStr);//将年月日转化为对应的值
            //this.richTextBox2.Text = DateTimeStrFix(foramtDateStr);
            this.richTextBox2.Text = Convert.ToDateTime(foramtDateStr).ToString();
           
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var needDealStr = this.richTextBox1.Text;
            var regexStr = this.textBox2.Text;
            this.richTextBox2.Text = string.Empty;
            Regex reg = new Regex(string.Format(@"{0}", regexStr));
            var match = reg.Matches(needDealStr);
            foreach (var m in match)
            {
                //var curDateFormat = DateToFormat(m.ToString());
            
                this.richTextBox2.Text += m.ToString();
            }
        }

        public bool Find(int[][] array, int target)
        {
            var str = string.Empty;
            var strArray = str.Split(' ');
            string.Join("%20", strArray);
        }
    }
}
