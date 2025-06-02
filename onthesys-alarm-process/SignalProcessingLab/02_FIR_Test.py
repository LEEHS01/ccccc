import numpy as np
import matplotlib.pyplot as plt

# 1. 시간축 (x축) 생성: 0부터 2파이까지 0.01 간격
x = np.arange(0, 200, 0.01)

# 2. 사인 함수 생성
clean_signal = np.sin(x);
noisy_signal = clean_signal + np.random.normal(0, 0.2, size=x.shape)

# 2. FIR 필터 계수 생성 (단순 이동 평균 필터)
N = 100  # 필터 길이 (탭 수)
fir_coeff = np.ones(N) / N  # 모든 계수가 1/N인 평균 필터

# 3. 필터 적용: numpy의 convolve 사용 (same 크기 유지)
filtered_signal = np.convolve(noisy_signal, fir_coeff, mode='same')

# 4. 시각화
plt.plot(x, noisy_signal, label='Noisy Signal', color='gray', alpha=0.6)
plt.plot(x, filtered_signal, label=f'FIR Filter (N={N})', color='red')
# plt.plot(x, clean_signal, label='Clean sin(x)', color='blue', linestyle='--')
plt.title('FIR Filtering of Noisy Sine Wave')
plt.xlabel('x')
plt.ylabel('Signal')
plt.legend()
plt.grid(True)
plt.show()
