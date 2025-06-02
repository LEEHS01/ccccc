import numpy as np
import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation

# 데이터 설정
sampling_rate = 1000
t = np.linspace(0, 1, sampling_rate, endpoint=False)

# 합성 신호
f1, f2, f3 = 8, 26, 61
signal = (
    1.0 * np.sin(2 * np.pi * f1 * t + 14215.1251) +
    0.5 * np.sin(2 * np.pi * f2 * t + 215412521.214214) +
    0.2 * np.sin(2 * np.pi * f3 * t +21366.12421)
)

# 주파수 범위
freq = np.linspace(1, 100, 1000)
dt = 1 / sampling_rate
X_f = np.array([
    np.sum(signal * np.exp(-2j * np.pi * f * t)) * dt
    for f in freq
])
lValues = np.abs(X_f)

# 유의미한 성분만 필터링
mask = lValues > 0
valuable_freqs = freq[mask]
valuable_coeffs = lValues[mask]
# 복원
reconstructed = np.zeros_like(t, dtype=np.complex128)
for X, f in zip(valuable_coeffs, valuable_freqs):
    reconstructed += X * np.exp(2j * np.pi * f * t)
reconstructed = reconstructed.real

# 3. 그래프 그리기
plt.plot(t, signal, label='original', color='blue')  
plt.plot(t, reconstructed, label='Inverted Fourier', color='orange', linestyle='dashed')  
plt.title('Difference Between Original & Inverted Fourier')
plt.xlabel('x')
plt.ylabel('sin(x)')
plt.grid(True)
plt.legend()
plt.show()