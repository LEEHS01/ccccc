import math
import numpy as np

class FirFilter:
    def __init__(self, order, cutoff_hz, fs_hz):
        if cutoff_hz <= 0 or fs_hz <= 0:
            raise ValueError("cutoff and fs must be > 0")
        if cutoff_hz >= fs_hz / 2:
            raise ValueError("cutoff must be less than Nyquist (fs/2)")

        self.N = order
        self.fs = fs_hz
        self.fc = cutoff_hz / fs_hz  # 정규화된 주파수로 환산 (0.0 ~ 0.5)
        self.coeffs = self.compute_coefficients()

    def compute_coefficients(self):
        N = self.N
        M = N - 1
        coeffs = []

        for n in range(N):
            if n == M // 2:
                hn = 2 * self.fc
            else:
                k = n - M / 2
                hn = math.sin(2 * math.pi * self.fc * k) / (math.pi * k)

            # Hamming window
            window = 0.54 - 0.46 * math.cos(2 * math.pi * n / M)
            coeffs.append(hn * window)

        return np.array(coeffs, dtype=np.float32)

    def apply(self, input_signal):
        input_signal = np.asarray(input_signal, dtype=np.float32)
        input_length = len(input_signal)

        delay = (self.N - 1) // 2
        padded_input = np.pad(input_signal, (delay, delay), mode='edge')
        output = np.zeros_like(input_signal)

        for i in range(len(input_signal)):
            segment = padded_input[i:i + self.N]
            output[i] = np.dot(self.coeffs, segment)

        return output





import numpy as np
import matplotlib.pyplot as plt
from scipy.signal import firwin, lfilter

# 1. 샘플링 설정
fs = 120  # Hz
t = np.arange(0, 1, 1/fs)
x = np.sin(2*np.pi*5*t) + 0.5*np.sin(2*np.pi*120*t)  # 저주파 + 고주파 노이즈


original_data = 1.2 * np.sin(2* np.pi * 13 * t + 25152156) 
original_data += 0.5 * np.cos(2* np.pi * 29 * t + 31352315)
original_data += -0.7 * np.sin(2* np.pi * 17 * t + 23146879);
original_data += 0.7 * np.cos(2* np.pi * 39 * t + 1342354678);
noised_data = original_data + np.random.normal(0, 0.2, size=t.shape)
noised_data += 1.0 * np.cos(2* np.pi * 41 * t + 12345);
noised_data += -0.6 * np.cos(2* np.pi * 72 * t + 26261);



seed = 1 * 1241 + 1 * 21414512;
time = t * 100;

value = 150 * (np.sin(seed + time) + np.cos((seed + time) * 1.41) + 2 * np.sin((seed + time) / 1.41) +4)/8 +  np.random.normal(0, 5, size=x.shape)



x = value



numtaps = 11  #윈도우 크기
cutoff = 20  # 임계값

# 기존 FIR
b = firwin(numtaps, cutoff, fs=fs)

y = lfilter(b, 1.0, x)

delay = (numtaps - 1) // 2
y_corrected = y[delay:]  # 지연만큼 앞으로 당김
x_trimmed = x[:len(y_corrected)]  # 길이 맞추기




myfir = FirFilter(numtaps, cutoff,fs)

myY = myfir.apply(x)

print("len(y_corrected)", len(y_corrected))

# 5. 시각화
plt.figure(figsize=(10, 5))
plt.plot(t, x, label='Original', alpha=0.5)
# plt.plot(t, y, label='Filtered (with delay)', linestyle='dashed')
plt.plot(t[:len(y_corrected)], y_corrected, label='Filtered (delay corrected)', linewidth=2)
plt.plot(t[:len(myY)], myY, label='myFire Filtered', linewidth=2)
plt.legend()
plt.title('FIR Filtering and Phase Delay Compensation')
plt.xlabel('Time [s]')
plt.ylabel('Amplitude')
plt.grid(True)
plt.tight_layout()
plt.show()