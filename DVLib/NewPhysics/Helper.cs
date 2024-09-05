using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;using MathBase;


namespace DVOSLib
{
public static	class Helper
	{
	
		public static unsafe IntPtr ToIntPrt(this int[] obj)
		{
            fixed (int* Ap = obj) return new IntPtr(Ap);
		}
        public static int mixColor(int color, Vector3 colorl)
        {
           double rf= (1 - (100/ (colorl.x + 100))) * 2 - 1f;
            double gf = (1 - (100 / (colorl.y + 100))) * 2 - 1f;
            double bf = (1 - (100 / (colorl.z + 100))) * 2 - 1f;
            int r = (color & 0x00ff0000) >> 16;

            int g = (color & 0x0000ff00) >> 8;
            int b = (color & 0x000000ff);
            if (rf> 0)
            {
                r +=(int)( (255 - r) * rf);
            }
            else
            {
                r += (int)((r) * rf);
            }
            if (gf > 0)
            {
                g+= (int)((255 - g) * gf);
            }
            else
            {
                g += (int)((g) * gf);
            }
            if (bf > 0)
            {
                b += (int)((255 - b) * bf);
            }
            else
            {
                b += (int)((b) * bf);
            }
            return (0xff<<24)|((color >>24<<24) | (r << 16) | (g << 8) | b);

        }

        public static  void sort<T>(T[] arr, Condition<T> condition)
        {
            for (int i = 0; i < arr.Length - 1; i++)
            {
                for (int j = 0; j < arr.Length - i - 1; j++)
                {
                    if (!condition(arr[j], arr[j + 1]))
                    {
                        // 以下三行代码用于交换两个元素
                        T temp = arr[j];
                        arr[j] = arr[j + 1];
                        arr[j + 1] = temp;
                    }
                }
            }
        }

        public static  void sort<T>(List<T> arr, Condition<T> condition)
        {
            int l = arr.Count;
            for (int i = 0; i < l - 1; i++)
            {
                for (int j = 0; j < l - i - 1; j++)
                {
                    if (!condition(arr[j], arr[j + 1]))
                    { 
                        // 以下三行代码用于交换两个元素
                        T temp = arr[j];
                        arr[j]= arr[j + 1];
                        arr[j+1]=temp;
                    }
                }
            }
        }

        public delegate bool Condition<T>(T a, T b);
    }
}
