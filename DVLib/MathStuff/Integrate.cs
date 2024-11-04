using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathBase
{
	 public class GaussQuadrature
	{
		// 计算勒让德多项式 P_n(x) 的值
		static double LegendrePolynomial(int n, double x)
		{
			if (n == 0) return 1;
			if (n == 1) return x;

			double Pn_2 = 1;  // P_0(x)
			double Pn_1 = x;  // P_1(x)
			double Pn = 0;

			for (int i = 2; i <= n; i++)
			{
				Pn = ((2 * i - 1) * x * Pn_1 - (i - 1) * Pn_2) / i;
				Pn_2 = Pn_1;
				Pn_1 = Pn;
			}

			return Pn;
		}

		// 计算勒让德多项式 P_n(x) 的导数
		static double LegendrePolynomialDerivative(int n, double x)
		{
			return (n * (x * LegendrePolynomial(n, x) - LegendrePolynomial(n - 1, x))) / (x * x - 1);
		}

		// 使用牛顿-拉夫森法寻找勒让德多项式的根（高斯点）
		static double NewtonRaphson(double initialGuess, int n, double tolerance = 1e-12, int maxIterations = 100)
		{
			double x = initialGuess;
			for (int i = 0; i < maxIterations; i++)
			{
				double Pn = LegendrePolynomial(n, x);
				double Pn_prime = LegendrePolynomialDerivative(n, x);

				double x_new = x - Pn / Pn_prime;

				if (Math.Abs(x_new - x) < tolerance)
					return x_new;

				x = x_new;
			}

			throw new Exception("Newton-Raphson method did not converge.");
		}

		public static (double[] points, double[] weight) findPoints2(int n)
		{
			double[] points = new double[n+1];
			double[] weights = new double[n+1];
			double pin = Math.PI / n;
			double w,sin;
			
			for (int i = 0; i <= n; i++)
			{
				points[i] = Math.Cos(i * pin);
				w = 0.5;
				for(int j = 1;j<n; j++)
				{
					sin= Math.Sin(j* pin);
					w += 1 / (sin * sin) * Math.Cos(i * j * pin);
				}
				weights[i] =w* pin;
			}
			return (points, weights);
		}

		public static (double[] points, double[] weight) findPoints(int n)
		{
			double[] points = new double[n];
			double[] weights = new double[n];

			for (int i = 0; i < n; i++)
			{
				// 使用初始猜测值为 cos(pi * (i + 0.75) / (n + 0.5))
				double initialGuess = Math.Cos(Math.PI * (i + 0.75) / (n + 0.5));
				double xi = NewtonRaphson(initialGuess, n);

				points[i] = xi;
				weights[i] = 2 / ((1 - xi * xi) * Math.Pow(LegendrePolynomialDerivative(n, xi), 2));
			}
			return(points,weights);
		}
		// 寻找高斯点和权重
		static void GaussLegendre(int n, double a, double b)
		{
			double[] points = new double[n];
			double[] weights = new double[n];

			for (int i = 0; i < n; i++)
			{
				// 使用初始猜测值为 cos(pi * (i + 0.75) / (n + 0.5))
				double initialGuess = Math.Cos(Math.PI * (i + 0.75) / (n + 0.5));
				double xi = NewtonRaphson(initialGuess, n);

				points[i] = xi;
				weights[i] = 2 / ((1 - xi * xi) * Math.Pow(LegendrePolynomialDerivative(n, xi), 2));
			}

			// 进行区间变换并计算积分
			double integral = 0;
			for (int i = 0; i < n; i++)
			{
				double t = points[i];
				double x = (b - a) / 2 * t + (b + a) / 2;
				double f_x = x * x;  // 被积函数是 f(x) = x^2

				integral += weights[i] * f_x;
			}

			integral *= (b - a) / 2;

			Console.WriteLine($"积分结果: {integral}");
		}

		public static double integrate((double[] p,double[] w) helper,Func<double,double>f,double start,double end)
		{
			double[] p = helper.p;
			double[] w = helper.w;
			double sum = 0;
			double halfLength=(end-start)*0.5;
			double midPoint = halfLength + start;
			int l = p.Length;
			double x;
			for(int i=0; i<l; i++)
			{
				sum += f(halfLength * p[i] + midPoint)*w[i];

			}
			sum *= halfLength;
			return sum;
		}
	
	}

	// 测试

}
