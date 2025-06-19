using System;
using System.Collections.Generic;

namespace DataMaskingTemplate
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("請輸入想要遮罩的資料：");
            string value = Console.ReadLine();

            Console.WriteLine("請輸入想要遮罩的格式：");
            Console.WriteLine("1.遮罩全部資料；2.遮罩資料末一碼；3.遮罩資料除了第一碼以外的部分");
            int type = Convert.ToInt32(Console.ReadLine());

            MaskingWayEnum maskingWay;
            switch (type)
            {
                case 1:
                    maskingWay = MaskingWayEnum.MaskAllCharacters;
                    break;
                case 2:
                    maskingWay = MaskingWayEnum.MaskLastOneCharacter;
                    break;
                case 3:
                    maskingWay = MaskingWayEnum.MaskExceptFirstOneCharacter;
                    break;
                default:
                    maskingWay = MaskingWayEnum.MaskAllCharacters;
                    break;
            }

            TestDataViewModel data = new TestDataViewModel { Value = value };
            Dictionary<string, MaskingWayEnum> maskingWayPairs = new Dictionary<string, MaskingWayEnum> { ["Value"] = maskingWay };
            TestDataViewModel maskedData = new DataMaskingService().GetMaskedResponse(data, maskingWayPairs);

            Console.WriteLine("顯示遮罩後的結果：" + maskedData.Value);
            Console.ReadLine();
        }
    }
}
