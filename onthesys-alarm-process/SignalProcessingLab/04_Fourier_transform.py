import numpy as np
import matplotlib.pyplot as plt

# 1. 시간 축 설정 (1초간 1000 샘플)
t = np.linspace(0, 1, 1000, endpoint=False)
sampling_rate = 1000  # 샘플링 주파수 [Hz]

# 2. 여러 주파수의 사인파 합성
f1, f2, f3 = 8, 26, 61  # 주파수들 [Hz]
signal = (
    1.0 * np.sin(2 * np.pi * f1 * t) +  # 5Hz, 세기 1.0
    0.5 * np.sin(2 * np.pi * f2 * t) +  # 20Hz, 세기 0.5
    0.2 * np.sin(2 * np.pi * f3 * t)    # 60Hz, 세기 0.2
)

# 3. 푸리에 변환 (FFT)
fft_result = np.fft.fft(signal)
fft_magnitude = np.abs(fft_result) / len(t)  # 정규화
fft_freq = np.fft.fftfreq(len(t), d=1/sampling_rate)

# 4. 양의 주파수만 보기 (대칭 제거)
positive_freqs = fft_freq > 0
freqs = fft_freq[positive_freqs]
magnitudes = 2 * fft_magnitude[positive_freqs]  # 에너지가 양/음 반쪽에 나뉘므로 ×2

# 5. 그래프 출력
plt.figure(figsize=(10, 5))
plt.plot(freqs, magnitudes)
plt.title('Fourier Transform of Mixed Frequency Signal')
plt.xlabel('Frequency [Hz]')
plt.ylabel('Amplitude')
plt.grid(True)
plt.xlim(0, 100)  # 100Hz까지 보기
plt.show()
