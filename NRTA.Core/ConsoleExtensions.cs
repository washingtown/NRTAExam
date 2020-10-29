using System;
using System.Collections.Generic;
using System.Text;

namespace NRTA.Core
{
    public static class ConsoleExtensions
    {
        public static int Input(string message,int? min=0,int? max=1000)
        {
            string erroMessage = "";
            if (min == null && max == null)
                erroMessage = "输入必须为整数";
            else if (min == null)
                erroMessage = string.Format("输入必须为大于{0}的整数", min);
            else if (max == null)
                erroMessage = string.Format("输入必须为小于{0}的整数", max);
            else
                erroMessage = string.Format("输入必须为 {0} 到 {1} 之间的整数", min, max);
            while (true)
            {
                if(string.IsNullOrEmpty(message))
                    Console.WriteLine(message);
                string input = Console.ReadLine();
                int num = 0;
                if (!int.TryParse(input, out num))
                {
                    Console.WriteLine(erroMessage);
                    continue;
                }
                else if((min!=null && num<min)||(max!=num && num > max))
                {
                    Console.WriteLine(erroMessage);
                    continue;
                }
                else
                {
                    return num;
                }
            }
        }
    }
}
