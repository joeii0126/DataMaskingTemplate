namespace DataMaskingTemplate
{
    public enum MaskingWayEnum
    {
        /// <summary> 遮罩全部資料 </summary>
        MaskAllCharacters,

        /// <summary> 遮罩資料末一碼。例如：114 → 11○ </summary>
        MaskLastOneCharacter,

        /// <summary> 遮罩資料末兩碼。例如：2025 → 20○○ </summary>
        MaskLastTwoCharacters,

        /// <summary> 遮罩資料末四碼。例如：F123456789 → F12345○○○○ </summary>
        MaskLastFourCharacters,

        /// <summary> 遮罩資料除了第一碼以外的部分。例如：王小明 → 王○○ </summary>
        MaskExceptFirstOneCharacter,

        /// <summary> 遮罩資料除了末四碼以外的部分。例如：1234567812345678 → ○○○○○○○○○○○○5678 </summary>
        MaskExceptLastFourCharacters,

        /// <summary> 遮罩民國年月日的民國年的個位數以及月跟日。例如：114/06/12 → 11○/○○/○○ </summary>
        MaskTaiwanDate,

        /// <summary> 遮罩資料中數字的部分。例如：新北市新莊區中平路一號南棟4樓 → 新北市新莊區中平路○號南棟○樓 </summary>
        MaskNumberCharacter,

        /// <summary> 回應電子郵件中@及其後面的部分。例如：test@mail.turbotech.com.tw → @mail.turbotech.com.tw </summary>
        ReturnEmailDomain,
    }
}
