using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DataMaskingTemplate
{
    /// <summary>
    /// 資料遮罩 Service
    /// </summary>
    public class DataMaskingService
    {
        private readonly Char MaskChar;
        private readonly Dictionary<MaskingWayEnum, Func<string, string>> MaskingWayFunctions;
        private readonly HashSet<MaskingWayEnum> DataTypes;

        public DataMaskingService()
        {
            MaskChar = '○';
            MaskingWayFunctions = new Dictionary<MaskingWayEnum, Func<string, string>>
            {
                { MaskingWayEnum.MaskAllCharacters, MaskAllCharacters },
                { MaskingWayEnum.MaskLastOneCharacter, new Func<string, string>(data => MaskLastFewCharacters(data, 1)) },
                { MaskingWayEnum.MaskLastTwoCharacters, new Func<string, string>(data => MaskLastFewCharacters(data, 2)) },
                { MaskingWayEnum.MaskLastFourCharacters, new Func<string, string>(data => MaskLastFewCharacters(data, 4)) },
                { MaskingWayEnum.MaskExceptFirstOneCharacter, new Func<string, string>(data => MaskExceptFirstFewCharacters(data, 1)) },
                { MaskingWayEnum.MaskExceptLastFourCharacters, new Func<string, string>(data => MaskExceptLastFewCharacters(data, 4)) },
                { MaskingWayEnum.MaskTaiwanDate, MaskTaiwanDate },
                { MaskingWayEnum.MaskNumberCharacter, MaskNumberCharacter },
                { MaskingWayEnum.ReturnEmailDomain, ReturnEmailDomain },
            };
            DataTypes = Enum.GetValues(typeof(MaskingWayEnum)).Cast<MaskingWayEnum>().ToHashSet();

            CheckAllMaskingWaysHasMaskingFunction();
        }

        /// <summary>
        /// 檢查是否所有的遮罩方式皆設定了相對應的實作方法
        /// </summary>
        private void CheckAllMaskingWaysHasMaskingFunction()
        {
            IEnumerable<MaskingWayEnum> missingTypes = DataTypes.Except(MaskingWayFunctions.Select(x => x.Key));
            if (missingTypes.Any())
            {
                throw new InvalidOperationException($"缺少以下遮罩方式的實作方法：{string.Join(", ", missingTypes)}");
            }
        }

        /// <summary>
        /// 從遮罩方式中找出並使用相對應的實作方法並且回應結果
        /// </summary>
        /// <param name="data">要做遮罩的資料</param>
        /// <param name="maskingWay">遮罩方式</param>
        /// <returns></returns>
        private string ApplyMaskedData(string data, MaskingWayEnum maskingWay)
        {
            return MaskingWayFunctions.TryGetValue(maskingWay, out var maskFunction) ? maskFunction(data) : data;
        }

        /// <summary>
        /// 回應遮罩過的資料
        /// </summary>
        /// <typeparam name="T">資料的類別</typeparam>
        /// <param name="data">資料</param>
        /// <param name="maskingWayPairs">標示資料的某些屬性套用何種遮罩方式</param>
        /// <returns></returns>
        public T GetMaskedResponse<T>(T data, Dictionary<string, MaskingWayEnum> maskingWayPairs)
        {
            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                object value = property.GetValue(data);
                if (value == null)
                {
                    continue;
                }

                if (value is string s && string.IsNullOrEmpty(s))
                {
                    continue;
                }

                string valueString = value.ToString();
                if (maskingWayPairs.ContainsKey(property.Name) && MaskingWayFunctions.Any(x => x.Key == maskingWayPairs[property.Name]))
                {
                    string maskedValue = ApplyMaskedData(valueString, maskingWayPairs[property.Name]);
                    property.SetValue(data, maskedValue);
                    continue;
                }
            }

            return data;
        }

        /// <summary>
        /// 回應遮罩過的資料
        /// </summary>
        /// <typeparam name="T">資料的類別</typeparam>
        /// <param name="datas">資料</param>
        /// <param name="maskingWayPairs">標示資料的某些屬性套用何種遮罩方式</param>
        /// <returns></returns>
        public List<T> GetMaskedResponse<T>(List<T> datas, Dictionary<string, MaskingWayEnum> maskingWayPairs)
        {
            foreach (T data in datas)
            {
                GetMaskedResponse(data, maskingWayPairs);
            }

            return datas;
        }

        #region 遮罩方式
        /// <summary>
        /// 遮罩全部資料
        /// </summary>
        /// <param name="data">資料</param>
        /// <returns></returns>
        private string MaskAllCharacters(string data)
        {
            return new string(MaskChar, data.Length);
        }

        /// <summary>
        /// 遮罩資料末幾碼
        /// </summary>
        /// <param name="data">資料</param>
        /// <param name="maskCharacters">遮罩幾碼</param>
        /// <returns></returns>
        private string MaskLastFewCharacters(string data, int maskCharacters)
        {
            if (data.Length < maskCharacters)
            {
                return new string(MaskChar, maskCharacters);
            }

            return data.Substring(0, data.Length - maskCharacters) + new string(MaskChar, maskCharacters);
        }

        /// <summary>
        /// 遮罩資料除了前幾碼以外的部分
        /// </summary>
        /// <param name="data">資料</param>
        /// <param name="holdCharacters">保留幾碼</param>
        /// <returns></returns>
        private string MaskExceptFirstFewCharacters(string data, int holdCharacters)
        {
            if (data.Length < holdCharacters)
            {
                return data;
            }

            return data.Substring(0, holdCharacters) + new string(MaskChar, data.Length - holdCharacters);
        }

        /// <summary>
        /// 遮罩資料除了末幾碼以外的部分
        /// </summary>
        /// <param name="data">資料</param>
        /// <param name="holdCharacters">保留幾碼</param>
        /// <returns></returns>
        private string MaskExceptLastFewCharacters(string data, int holdCharacters)
        {
            if (data.Length < holdCharacters)
            {
                return data;
            }

            return new string(MaskChar, data.Length - holdCharacters) + data.Substring(data.Length - holdCharacters);
        }

        /// <summary>
        /// 遮罩民國年月日的民國年的個位數以及月跟日
        /// </summary>
        /// <param name="date">民國年月日</param>
        /// <returns></returns>
        private string MaskTaiwanDate(string date)
        {
            string pattern = @"^(\d{2})(\d)/(\d{2})/(\d{2})$";
            MatchEvaluator evaluator = x => $"{x.Groups[1].Value}{MaskChar}/{new string(MaskChar, 2)}/{new string(MaskChar, 2)}";

            return Regex.Replace(date, pattern, evaluator);
        }

        /// <summary>
        /// 遮罩資料中數字的部分
        /// </summary>
        /// <param name="data">資料</param>
        /// <returns></returns>
        private string MaskNumberCharacter(string data)
        {
            string pattern = @"([一二三四五六七八九十\d]+)(鄉|鎮|區|路|段|巷|弄|號|樓|之\d+)?";
            MatchEvaluator evaluator = x =>
            {
                if (string.IsNullOrEmpty(x.Groups[2].Value))
                {
                    return x.Value;
                }

                return MaskChar + x.Groups[2].Value;
            };

            return Regex.Replace(data, pattern, evaluator);
        }

        /// <summary>
        /// 回應電子郵件中@及其後面的部分
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns></returns>
        private string ReturnEmailDomain(string email)
        {
            if (!email.Contains('@'))
            {
                return new string(MaskChar, 4);
            }

            return '@' + email.Split('@')[1];
        }
        #endregion
    }
}
