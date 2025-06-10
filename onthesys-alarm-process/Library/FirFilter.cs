using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace onthesys_alarm_process.Library
{
    public class FirFilter
    {
        private readonly int N;           // 필터 차수  
        private readonly float fc;        // 컷오프  
        //private readonly float fs; // 샘플링 주파수  
        private readonly float[] coeffs;  // 필터 계수  

        public FirFilter(int order, float cutoff_hz, float fs_hz)
        {
            if (cutoff_hz <= 0 || fs_hz <= 0)
                throw new Exception("cutoff and fs must be > 0");
            if (cutoff_hz >= fs_hz / 2)
                throw new Exception("cutoff must be less than Nyquist (fs/2)");

            N = order;
            //fs = fs_hz;
            fc = cutoff_hz / fs_hz;
            coeffs = new float[N];

            ComputeCoefficients();
        }

        private void ComputeCoefficients()
        {
            int M = N - 1;
            for (int n = 0; n < N; n++)
            {
                float hn;
                if (n == M / 2)
                {
                    hn = 2 * fc;
                }
                else
                {
                    float k = n - M / 2f;
                    hn = (float)(Math.Sin(2 * Math.PI * fc * k) / (Math.PI * k));
                }

                // Hamming window  
                float window = (float)(0.54 - 0.46 * Math.Cos(2 * Math.PI * n / M));
                coeffs[n] = hn * window;
            }
        }

        public List<float> ApplyFilter(List<float> input)
        {
            int inputLength = input.Count;

            int delay = (N - 1);

            List<float> paddedInput = PadEdge(input, delay);
            List<float> output = input.Select(i => 0f).ToList();

            for (int i = 0; i < inputLength; i++)
            {
                float sum = 0;
                for (int j = 0; j < N; j++)
                {
                    sum += coeffs[j] * paddedInput[i + j];
                }
                output[i] = sum;
            }

            return output;
        }

        List<float> PadEdge(List<float> input, int pad)
        {
            int n = input.Count;
            List<float> padded = new List<float>(n + 2 * pad);

            // 앞쪽 padding: input[0] 반복  
            for (int i = 0; i < pad; i++)
                padded.Add(input[0]);

            // 본문 복사  
            padded.AddRange(input);

            // 뒤쪽 padding: input[n-1] 반복  
            for (int i = 0; i < pad; i++)
                padded.Add(input[n - 1]);

            return padded;
        }
    }
}
